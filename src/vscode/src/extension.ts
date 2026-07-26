import * as path from "node:path";
import * as vscode from "vscode";
import { parseTltBlock, serializeTltBlock, TltPayload } from "./protocol";
import { normalizeSelectionLines } from "./selection";
import { findContainingClassName } from "./symbols";

const copyCommand = "techLeadTools.copyWithTLT";
const pasteCommand = "techLeadTools.pasteWithTLT";
const searchExclude = "{**/.git/**,**/node_modules/**,**/bin/**,**/obj/**}";

export function activate(context: vscode.ExtensionContext): void {
  context.subscriptions.push(
    vscode.commands.registerCommand(copyCommand, copyWithTlt),
    vscode.commands.registerCommand(pasteCommand, pasteWithTlt)
  );
}

export function deactivate(): void {
  // Nenhum recurso persistente precisa ser liberado.
}

async function copyWithTlt(): Promise<void> {
  const editor = vscode.window.activeTextEditor;
  if (!editor) {
    void vscode.window.showErrorMessage("TechLeadTools: nenhum editor de texto está ativo.");
    return;
  }

  if (editor.selections.length !== 1) {
    void vscode.window.showErrorMessage("TechLeadTools: seleções múltiplas não são suportadas.");
    return;
  }

  const document = editor.document;
  if (document.isUntitled || document.uri.scheme !== "file") {
    void vscode.window.showErrorMessage("TechLeadTools: salve o arquivo antes de copiá-lo.");
    return;
  }

  const folder = vscode.workspace.getWorkspaceFolder(document.uri);
  if (!folder) {
    void vscode.window.showErrorMessage(
      "TechLeadTools: o arquivo precisa pertencer a uma pasta do workspace."
    );
    return;
  }

  const selection = editor.selection;
  const lineRange = normalizeSelectionLines(
    selection.start.line,
    selection.end.line,
    selection.end.character,
    selection.isEmpty
  );
  const fullRange = new vscode.Range(
    lineRange.startLine,
    0,
    lineRange.endLine,
    document.lineAt(lineRange.endLine).range.end.character
  );

  const relativePath = path.relative(folder.uri.fsPath, document.uri.fsPath).split(path.sep).join("/");
  const payload: TltPayload = {
    workspace: folder.name,
    path: relativePath,
    file: path.basename(document.uri.fsPath),
    class: await findContainingClassName(document, fullRange),
    startLine: lineRange.startLine + 1,
    endLine: lineRange.endLine + 1
  };

  try {
    const block = serializeTltBlock(payload, document.getText(fullRange));
    await vscode.env.clipboard.writeText(block);
    const description = payload.startLine === payload.endLine
      ? `linha ${payload.startLine}`
      : `linhas ${payload.startLine}-${payload.endLine}`;
    void vscode.window.showInformationMessage(`TechLeadTools: ${description} copiadas com TLT.`);
  } catch (error) {
    showError(error);
  }
}

async function pasteWithTlt(): Promise<void> {
  let payload: TltPayload;
  try {
    const clipboard = await vscode.env.clipboard.readText();
    payload = parseTltBlock(clipboard).payload;
  } catch (error) {
    showError(error);
    return;
  }

  const folders = vscode.workspace.workspaceFolders;
  if (!folders || folders.length === 0) {
    void vscode.window.showErrorMessage("TechLeadTools: abra uma pasta ou workspace primeiro.");
    return;
  }

  const target = await resolveTarget(payload, folders);
  if (!target) {
    return;
  }

  try {
    const document = await vscode.workspace.openTextDocument(target);
    const lastLine = Math.max(0, document.lineCount - 1);
    const startLine = Math.min(payload.startLine - 1, lastLine);
    const endLine = Math.min(payload.endLine - 1, lastLine);
    const range = new vscode.Range(
      startLine,
      0,
      endLine,
      document.lineAt(endLine).range.end.character
    );
    const editor = await vscode.window.showTextDocument(document, { preview: false });
    editor.selection = new vscode.Selection(range.start, range.end);
    editor.revealRange(range, vscode.TextEditorRevealType.InCenterIfOutsideViewport);

    if (payload.endLine - 1 > lastLine) {
      void vscode.window.showWarningMessage(
        `TechLeadTools: o arquivo tem ${document.lineCount} linhas; o intervalo foi ajustado.`
      );
    }
  } catch (error) {
    showError(error);
  }
}

async function resolveTarget(
  payload: TltPayload,
  folders: readonly vscode.WorkspaceFolder[]
): Promise<vscode.Uri | undefined> {
  const orderedFolders = [
    ...folders.filter(folder => folder.name === payload.workspace),
    ...folders.filter(folder => folder.name !== payload.workspace)
  ];

  const exactMatches: vscode.Uri[] = [];
  for (const folder of orderedFolders) {
    const candidate = vscode.Uri.joinPath(folder.uri, ...payload.path.split("/"));
    try {
      const stat = await vscode.workspace.fs.stat(candidate);
      if ((stat.type & vscode.FileType.File) !== 0) {
        exactMatches.push(candidate);
      }
    } catch {
      // O fallback por nome será tentado abaixo.
    }
  }

  if (exactMatches.length === 1) {
    return exactMatches[0];
  }

  if (exactMatches.length > 1) {
    return chooseTarget(exactMatches, folders, "Mais de um workspace contém o caminho TLT.");
  }

  const fallbackMatches: vscode.Uri[] = [];
  const fileGlob = `**/${escapeGlob(payload.file)}`;
  for (const folder of orderedFolders) {
    const matches = await vscode.workspace.findFiles(
      new vscode.RelativePattern(folder, fileGlob),
      searchExclude,
      50
    );
    fallbackMatches.push(...matches);
  }

  if (fallbackMatches.length === 0) {
    void vscode.window.showErrorMessage(
      `TechLeadTools: não foi possível localizar “${payload.path}” neste workspace.`
    );
    return undefined;
  }

  if (fallbackMatches.length === 1) {
    void vscode.window.showWarningMessage(
      `TechLeadTools: o caminho mudou; “${payload.file}” foi localizado pelo nome.`
    );
    return fallbackMatches[0];
  }

  return chooseTarget(fallbackMatches, folders, "Escolha o arquivo correspondente ao bloco TLT.");
}

async function chooseTarget(
  candidates: readonly vscode.Uri[],
  folders: readonly vscode.WorkspaceFolder[],
  placeHolder: string
): Promise<vscode.Uri | undefined> {
  const items = candidates.map(uri => {
    const folder = folders.find(item => uri.fsPath.startsWith(item.uri.fsPath));
    return {
      label: path.basename(uri.fsPath),
      description: folder ? path.relative(folder.uri.fsPath, uri.fsPath) : uri.fsPath,
      uri
    };
  });

  return (await vscode.window.showQuickPick(items, { placeHolder }))?.uri;
}

function escapeGlob(value: string): string {
  return value.replace(/[[\]{}*?]/g, character => `[${character}]`);
}

function showError(error: unknown): void {
  const message = error instanceof Error ? error.message : String(error);
  void vscode.window.showErrorMessage(`TechLeadTools: ${message}`);
}

