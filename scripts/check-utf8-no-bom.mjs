import fs from "node:fs";
import path from "node:path";

const root = path.resolve(import.meta.dirname, "..");
const ignoredDirectories = new Set([
  ".git",
  ".vs",
  "bin",
  "node_modules",
  "obj",
  "out",
  "TestResults"
]);
const textExtensions = new Set([
  ".cs",
  ".csproj",
  ".editorconfig",
  ".gitignore",
  ".json",
  ".js",
  ".md",
  ".mjs",
  ".sln",
  ".ts",
  ".vsct",
  ".xml",
  ".yaml",
  ".yml"
]);
const failures = [];
const decoder = new TextDecoder("utf-8", { fatal: true });

walk(root);

if (failures.length > 0) {
  console.error("Arquivos inválidos:");
  for (const failure of failures) {
    console.error(`- ${failure}`);
  }
  process.exit(1);
}

console.log("Todos os arquivos textuais estão em UTF-8 sem BOM.");

function walk(directory) {
  for (const entry of fs.readdirSync(directory, { withFileTypes: true })) {
    if (entry.isDirectory() && ignoredDirectories.has(entry.name)) {
      continue;
    }

    const fullPath = path.join(directory, entry.name);
    if (entry.isDirectory()) {
      walk(fullPath);
      continue;
    }

    if (!textExtensions.has(path.extname(entry.name)) && !entry.name.startsWith(".")) {
      continue;
    }

    const bytes = fs.readFileSync(fullPath);
    const relative = path.relative(root, fullPath);
    if (bytes.length >= 3 && bytes[0] === 0xef && bytes[1] === 0xbb && bytes[2] === 0xbf) {
      failures.push(`${relative}: contém BOM UTF-8`);
      continue;
    }

    try {
      decoder.decode(bytes);
    } catch {
      failures.push(`${relative}: não é UTF-8 válido`);
    }
  }
}

