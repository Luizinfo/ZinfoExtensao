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

const manifestVersion = /<Identity[^>]*Version="([^"]+)"/s.exec(manifest)?.[1];
const assemblyVersion = /AssemblyVersion\("([^"]+)"\)/.exec(assemblyInfo)?.[1];
const expectedAssemblyVersion = `${packageJson.version}.0`;

if (manifestVersion !== packageJson.version || assemblyVersion !== expectedAssemblyVersion) {
  console.error(
    `Versões divergentes: VS Code=${packageJson.version}, `
      + `VS=${manifestVersion}, assembly=${assemblyVersion}.`
  );
  process.exit(1);
}

console.log(`Versões sincronizadas em ${packageJson.version}.`);

