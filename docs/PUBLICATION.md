# Plano de publicação — TechLeadTools

## Identidades permanentes

Antes do primeiro envio, confirme que o publisher corporativo `luizinfo` está
disponível e reserve-o nos dois marketplaces. Depois da primeira publicação,
não altere:

- VS Code Marketplace: `luizinfo.techleadtools`;
- Visual Studio Marketplace: `TechLeadTools.VisualStudio`.

Se `luizinfo` não estiver disponível, substitua o publisher nos manifests e nos
workflows antes da versão `1.0.0`. Nunca reutilize a identidade ou uma versão
já publicada.

## Canais

1. **Pré-lançamento**: VSIX anexados a um GitHub Release para validação interna.
2. **Público**: Visual Studio Marketplace e VS Code Marketplace.
3. **Offline**: os dois VSIX e seus hashes SHA-256 no GitHub Release.

As extensões usam a mesma versão SemVer. O assembly do Visual Studio acrescenta
um quarto componente zero: `X.Y.Z.0`.

## Preparação das contas

### VS Code Marketplace

1. Crie o publisher e conceda permissão à identidade de automação.
2. Crie um aplicativo Microsoft Entra e configure federação OIDC com este
   repositório GitHub e o environment `marketplace-production`.
3. Cadastre `AZURE_CLIENT_ID`, `AZURE_TENANT_ID` e `AZURE_SUBSCRIPTION_ID` como
   variáveis/segredos do environment.
4. A publicação usa `vsce publish --azure-credential`, sem PAT de longa duração.

### Visual Studio Marketplace

1. Crie o publisher com o mesmo nome público.
2. Cadastre `VS_MARKETPLACE_PAT` no environment protegido
   `marketplace-production`, com o menor escopo e validade possíveis.
3. A publicação usa `VsixPublisher.exe`. Revise a documentação oficial antes de
   cada rotação de credencial e migre para autenticação federada assim que o
   utilitário documentar suporte.

O environment de produção deve exigir aprovação manual e restringir quem pode
aprovar.

## Checklist de release

1. Atualizar `CHANGELOG.md`.
2. Atualizar e sincronizar as três versões:
   `src/vscode/package.json`, `source.extension.vsixmanifest` e
   `Properties/AssemblyInfo.cs`.
3. Executar `node scripts/validate-version.mjs`.
4. Executar `node scripts/check-utf8-no-bom.mjs`.
5. Executar os testes TypeScript e .NET.
6. Fazer smoke test manual:
   - copiar e colar dentro de cada IDE;
   - copiar no VS Code e colar no Visual Studio;
   - copiar no Visual Studio e colar no VS Code;
   - testar caminho alterado, arquivo duplicado e linha fora do intervalo.
7. Criar a tag assinada `vX.Y.Z`.
8. Verificar os artefatos do workflow e instalar ambos em máquinas limpas.
9. Criar o GitHub Release com os dois VSIX e hashes SHA-256.
10. Executar manualmente o workflow **Release** com publicação habilitada e
    aprovar o environment.
11. Conferir listagem, ícone, README, licença e instalação nos marketplaces.

Correções são sempre publicadas em uma nova versão; artefatos já lançados não
devem ser substituídos.

## Critérios para 1.0.0

- smoke test aprovado em VS Code 1.90 ou superior;
- smoke test aprovado em Visual Studio 2022 e Visual Studio 2026;
- interoperabilidade bidirecional aprovada;
- documentação de privacidade, segurança e suporte publicada;
- publisher e URLs definitivos configurados;
- piloto interno concluído sem perda de navegação ou alteração de código.

## Referências oficiais

- VS Code: https://code.visualstudio.com/api/working-with-extensions/publishing-extension
- Entra OIDC: https://learn.microsoft.com/entra/workload-id/workload-identity-federation-create-trust
- Publicação VSIX: https://learn.microsoft.com/visualstudio/extensibility/walkthrough-publishing-a-visual-studio-extension-via-command-line
- Assinatura VSIX: https://learn.microsoft.com/visualstudio/extensibility/signing-vsix-packages

