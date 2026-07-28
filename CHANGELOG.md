# Changelog

## 0.1.4

- Registra o VSPackage por `CodeBase` para carregamento a partir do diretório
  privado da extensão.
- Valida o `.pkgdef` efetivamente empacotado no VSIX durante o CI e a release.

## 0.1.3

- Adiciona o submenu **TechLeadTools** diretamente ao menu contextual do editor.
- Organiza os comandos na hierarquia canônica de menus do Visual Studio.

## 0.1.2

- Corrige a exibição dos comandos TLT no menu contextual do editor do Visual Studio.
- Mantém os comandos disponíveis também no menu **Ferramentas**.

## 0.1.1

- Corrige a separação das tags no manifesto VSIX do Visual Studio.
- Ajusta os metadados usados na publicação pelo `VsixPublisher.exe`.
- Documenta o subject OIDC imutável usado pelo GitHub Actions.

## 0.1.0

- Comando **Copiar com TLT** para linha atual ou seleção.
- Cabeçalho humano e metadados interoperáveis no protocolo `TLT/1`.
- Comando **Colar com TLT** para localizar, abrir e selecionar o trecho.
- Suporte inicial a Visual Studio Code, Visual Studio 2022 e Visual Studio 2026.
