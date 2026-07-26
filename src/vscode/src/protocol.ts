export const protocolVersion = "TLT/1";

export interface TltPayload {
  workspace: string;
  path: string;
  file: string;
  class: string;
  startLine: number;
  endLine: number;
}

export interface ParsedTltBlock {
  payload: TltPayload;
  content: string;
}

export function createHeader(payload: TltPayload): string {
  const linePart = payload.startLine === payload.endLine
    ? `${payload.startLine}`
    : `${payload.startLine}-${payload.endLine}`;

  return `${payload.file}:${payload.class}:${linePart}`;
}

export function serializeTltBlock(payload: TltPayload, content: string): string {
  validatePayload(payload);
  const normalizedContent = content.replace(/\r\n/g, "\n");
  return `${createHeader(payload)}\n${protocolVersion} ${JSON.stringify(payload)}\n---\n${normalizedContent}`;
}

export function parseTltBlock(text: string): ParsedTltBlock {
  const match = /^(.*?)\r?\nTLT\/1 ([^\r\n]+)\r?\n---(?:\r?\n|$)([\s\S]*)$/.exec(text);
  if (!match) {
    throw new Error("A área de transferência não contém um bloco TLT/1 válido.");
  }

  let payload: TltPayload;
  try {
    payload = JSON.parse(match[2]) as TltPayload;
  } catch {
    throw new Error("Os metadados JSON do bloco TLT/1 são inválidos.");
  }

  validatePayload(payload);
  if (match[1] !== createHeader(payload)) {
    throw new Error("O cabeçalho do bloco TLT/1 não corresponde aos metadados.");
  }

  return { payload, content: match[3] };
}

export function validatePayload(payload: TltPayload): void {
  if (!payload || typeof payload !== "object") {
    throw new Error("Metadados TLT ausentes.");
  }

  for (const key of ["workspace", "path", "file", "class"] as const) {
    if (typeof payload[key] !== "string" || payload[key].trim().length === 0) {
      throw new Error(`Campo TLT inválido: ${key}.`);
    }
  }

  if (!Number.isInteger(payload.startLine) || !Number.isInteger(payload.endLine)
    || payload.startLine < 1 || payload.endLine < payload.startLine) {
    throw new Error("Intervalo de linhas TLT inválido.");
  }

  if (!isSafeRelativePath(payload.path)) {
    throw new Error("O caminho do bloco TLT não é relativo e seguro.");
  }

  const segments = payload.path.split("/");
  if (segments[segments.length - 1] !== payload.file || /[\\/]/.test(payload.file)) {
    throw new Error("O nome do arquivo não corresponde ao caminho TLT.");
  }
}

export function isSafeRelativePath(value: string): boolean {
  if (value.length === 0 || value.startsWith("/") || value.startsWith("\\")
    || /^[A-Za-z]:/.test(value) || value.includes("\\")) {
    return false;
  }

  const segments = value.split("/");
  return segments.every(segment => segment.length > 0 && segment !== "." && segment !== "..");
}

