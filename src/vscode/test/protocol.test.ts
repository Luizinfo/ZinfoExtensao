import assert from "node:assert/strict";
import * as fs from "node:fs";
import * as path from "node:path";
import { describe, it } from "node:test";
import {
  createHeader,
  isSafeRelativePath,
  parseTltBlock,
  serializeTltBlock,
  TltPayload
} from "../src/protocol";

interface Fixture extends TltPayload {
  name: string;
  header: string;
  content: string;
}

const fixturePath = path.resolve(__dirname, "../../../../protocol/fixtures.json");
const fixtures = JSON.parse(fs.readFileSync(fixturePath, "utf8")) as Fixture[];

describe("protocolo TLT/1", () => {
  for (const fixture of fixtures) {
    it(`serializa e lê ${fixture.name}`, () => {
      const payload: TltPayload = {
        workspace: fixture.workspace,
        path: fixture.path,
        file: fixture.file,
        class: fixture.class,
        startLine: fixture.startLine,
        endLine: fixture.endLine
      };

      assert.equal(createHeader(payload), fixture.header);
      const block = serializeTltBlock(payload, fixture.content);
      assert.deepEqual(parseTltBlock(block), { payload, content: fixture.content });
      assert.deepEqual(parseTltBlock(block.replace(/\n/g, "\r\n")).payload, payload);
    });
  }

  it("rejeita protocolo desconhecido e cabeçalho adulterado", () => {
    const fixture = fixtures[0];
    const payload = toPayload(fixture);
    const block = serializeTltBlock(payload, fixture.content);
    assert.throws(() => parseTltBlock(block.replace("TLT/1", "TLT/2")));
    assert.throws(() => parseTltBlock(block.replace(fixture.header, "Outro.cs:Global:1")));
  });

  it("rejeita caminhos absolutos e travessia de diretório", () => {
    assert.equal(isSafeRelativePath("src/Service.cs"), true);
    assert.equal(isSafeRelativePath("../Service.cs"), false);
    assert.equal(isSafeRelativePath("src/../Service.cs"), false);
    assert.equal(isSafeRelativePath("C:/src/Service.cs"), false);
    assert.equal(isSafeRelativePath("/src/Service.cs"), false);
    assert.equal(isSafeRelativePath("src\\Service.cs"), false);
  });
});

function toPayload(fixture: Fixture): TltPayload {
  return {
    workspace: fixture.workspace,
    path: fixture.path,
    file: fixture.file,
    class: fixture.class,
    startLine: fixture.startLine,
    endLine: fixture.endLine
  };
}

