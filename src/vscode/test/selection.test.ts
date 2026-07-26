import assert from "node:assert/strict";
import { describe, it } from "node:test";
import { normalizeSelectionLines } from "../src/selection";

describe("normalização de seleção", () => {
  it("usa a linha do cursor quando não há seleção", () => {
    assert.deepEqual(normalizeSelectionLines(4, 4, 8, true), { startLine: 4, endLine: 4 });
  });

  it("não inclui a próxima linha quando a seleção termina na coluna zero", () => {
    assert.deepEqual(normalizeSelectionLines(4, 7, 0, false), { startLine: 4, endLine: 6 });
  });

  it("inclui a linha final quando há caracteres selecionados nela", () => {
    assert.deepEqual(normalizeSelectionLines(4, 7, 2, false), { startLine: 4, endLine: 7 });
  });
});

