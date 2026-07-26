import * as vscode from "vscode";

export async function findContainingClassName(
  document: vscode.TextDocument,
  range: vscode.Range
): Promise<string> {
  let symbols: vscode.DocumentSymbol[] | undefined;
  try {
    const result = await vscode.commands.executeCommand<
      vscode.DocumentSymbol[] | vscode.SymbolInformation[]
    >("vscode.executeDocumentSymbolProvider", document.uri);

    if (result && result.length > 0 && result[0] instanceof vscode.DocumentSymbol) {
      symbols = result as vscode.DocumentSymbol[];
    }
  } catch {
    return "Global";
  }

  if (!symbols) {
    return "Global";
  }

  return findDeepestContainingClass(symbols, range, []) ?? "Global";
}

function findDeepestContainingClass(
  symbols: readonly vscode.DocumentSymbol[],
  selection: vscode.Range,
  parentClasses: readonly string[]
): string | undefined {
  let best: string | undefined;

  for (const symbol of symbols) {
    if (!contains(symbol.range, selection)) {
      continue;
    }

    const isClass = symbol.kind === vscode.SymbolKind.Class;
    const classPath = isClass ? [...parentClasses, symbol.name] : parentClasses;
    if (isClass) {
      best = classPath.join(".");
    }

    const nested = findDeepestContainingClass(symbol.children, selection, classPath);
    if (nested) {
      best = nested;
    }
  }

  return best;
}

function contains(container: vscode.Range, inner: vscode.Range): boolean {
  return container.start.isBeforeOrEqual(inner.start)
    && container.end.isAfterOrEqual(inner.end);
}

