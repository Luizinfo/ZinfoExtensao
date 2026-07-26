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
const publishManifest = JSON.parse(
  fs.readFileSync(
    path.join(root, "publish/visualstudio-publish-manifest.json"),
    "utf8"
  )
);

const manifestVersion = /<Identity[^>]*Version="([^"]+)"/s.exec(manifest)?.[1];
const assemblyVersion = /AssemblyVersion\("([^"]+)"\)/.exec(assemblyInfo)?.[1];
const expectedAssemblyVersion = `${packageJson.version}.0`;
const tagsText = /<Tags>([^<]+)<\/Tags>/.exec(manifest)?.[1] ?? "";
const tags = tagsText.split(";").map((tag) => tag.trim()).filter(Boolean);
const publishIdentityKeys = Object.keys(publishManifest.identity ?? {});

if (manifestVersion !== packageJson.version || assemblyVersion !== expectedAssemblyVersion) {
  console.error(
    `Versões divergentes: VS Code=${packageJson.version}, `
      + `VS=${manifestVersion}, assembly=${assemblyVersion}.`
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

console.log(`Versões sincronizadas em ${packageJson.version}.`);
