import fs from "node:fs";
import path from "node:path";

const root = path.resolve(import.meta.dirname, "..");
const packageJson = JSON.parse(
  fs.readFileSync(path.join(root, "src/vscode/package.json"), "utf8")
);
const manifest = fs.readFileSync(
  path.join(
    root,
    "src/visualstudio/TechLeadTools.VisualStudio/source.extension.vsixmanifest"
  ),
  "utf8"
);
const assemblyInfo = fs.readFileSync(
  path.join(
    root,
    "src/visualstudio/TechLeadTools.VisualStudio/Properties/AssemblyInfo.cs"
  ),
  "utf8"
);
const packageSource = fs.readFileSync(
  path.join(
    root,
    "src/visualstudio/TechLeadTools.VisualStudio/TechLeadToolsPackage.cs"
  ),
  "utf8"
);
const publishManifest = JSON.parse(
  fs.readFileSync(
    path.join(root, "publish/visualstudio-publish-manifest.json"),
    "utf8"
  )
);

const manifestVersion = /<Identity[^>]*Version="([^"]+)"/s.exec(manifest)?.[1];
const assemblyVersion = /AssemblyVersion\("([^"]+)"\)/.exec(assemblyInfo)?.[1];
const installedProductVersion =
  /InstalledProductRegistration\(\s*"[^"]+",\s*"[^"]+",\s*"([^"]+)"\s*\)/s
    .exec(packageSource)?.[1];
const expectedAssemblyVersion = `${packageJson.version}.0`;
const tagsText = /<Tags>([^<]+)<\/Tags>/.exec(manifest)?.[1] ?? "";
const tags = tagsText.split(";").map((tag) => tag.trim()).filter(Boolean);
const publishIdentityKeys = Object.keys(publishManifest.identity ?? {});
const marketplaceInternalName = publishManifest.identity?.internalName ?? "";

if (
  manifestVersion !== packageJson.version
  || assemblyVersion !== expectedAssemblyVersion
  || installedProductVersion !== packageJson.version
) {
  console.error(
    `Versões divergentes: VS Code=${packageJson.version}, `
      + `VS=${manifestVersion}, assembly=${assemblyVersion}, `
      + `produto instalado=${installedProductVersion}.`
  );
  process.exit(1);
}

if (tagsText.includes(",") || tags.some((tag) => tag.length > 50)) {
  console.error(
    "As tags do VSIX devem ser separadas por ponto e vírgula e ter até 50 caracteres."
  );
  process.exit(1);
}

if (
  publishIdentityKeys.length !== 1
  || publishIdentityKeys[0] !== "internalName"
) {
  console.error(
    "Para um payload VSIX, identity no manifesto de publicação deve conter apenas internalName."
  );
  process.exit(1);
}

if (
  marketplaceInternalName.length >= 63
  || !/^[A-Za-z0-9][A-Za-z0-9-]*$/.test(marketplaceInternalName)
) {
  console.error(
    "O internalName do Visual Studio Marketplace deve ter menos de 63 caracteres e usar apenas letras, números e hífen."
  );
  process.exit(1);
}

console.log(`Versões sincronizadas em ${packageJson.version}.`);
