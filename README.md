# TechLeadTools

Extensões para Visual Studio Code e Visual Studio que facilitam demonstrações,
revisões e compartilhamento de trechos de código com uma equipe técnica.

## Como funciona

1. Selecione uma ou mais linhas, ou deixe o cursor em uma linha.
2. Clique com o botão direito e escolha **Copiar com TLT**.
3. Compartilhe o texto gerado.
4. Em outro checkout do mesmo projeto, copie o bloco recebido para a área de
   transferência e execute **Colar com TLT**.

O comando de colar não insere texto. Ele localiza o arquivo no workspace ou na
solução, abre o documento e seleciona as linhas indicadas.

Exemplo:

```text
PedidoService.cs:PedidoService:42-48
TLT/1 {"workspace":"MeuProjeto","path":"src/Services/PedidoService.cs","file":"PedidoService.cs","class":"PedidoService","startLine":42,"endLine":48}
---
<conteúdo integral das linhas 42 a 48>
```

## Projetos

- `src/vscode`: extensão TypeScript para Visual Studio Code.
- `src/visualstudio`: VSIX C# para Visual Studio 2022/2026.
- `src/shared/TechLeadTools.Protocol`: implementação .NET do protocolo.
- `protocol`: especificação e fixtures compartilhadas.

## Desenvolvimento

### Visual Studio Code

```powershell
cd src\vscode
npm install
npm test
npm run package
```

Pressione `F5` no VS Code para executar a extensão em um Extension Development
Host.

### Visual Studio

Abra `TechLeadTools.sln`, defina `TechLeadTools.VisualStudio` como projeto de
inicialização e execute. O Visual Studio abrirá uma instância experimental.

Também é possível compilar pela linha de comando:

```powershell
msbuild TechLeadTools.sln /restore /p:Configuration=Release
dotnet run --project tests\TechLeadTools.ProtocolTests
```

## Privacidade

O processamento é totalmente local, sem telemetria ou chamadas de rede.
Consulte [PRIVACY.md](PRIVACY.md) e [SECURITY.md](SECURITY.md).

## English

TechLeadTools provides matching Visual Studio Code and Visual Studio extensions
for sharing a code location and snippet in a portable text format. **Copy with
TLT** writes the current line or full selected lines to the clipboard. **Paste
with TLT** reads that block, opens the referenced file and selects the range;
it never edits the document. All processing is local and no telemetry is sent.

## Licença

MIT.
