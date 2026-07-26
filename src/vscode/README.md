# TechLeadTools para Visual Studio Code

Compartilhe uma linha ou um intervalo de código em um formato que outro
desenvolvedor pode usar para abrir o mesmo arquivo diretamente.

## Uso

- No editor, clique com o botão direito e escolha **Copiar com TLT**. Sem
  seleção, a linha do cursor é copiada; com seleção, são copiadas as linhas
  completas abrangidas.
- Para navegar, deixe o bloco recebido na área de transferência e execute
  **Colar com TLT** pelo menu de contexto ou pela Paleta de Comandos.

O bloco inclui um cabeçalho legível, o caminho relativo, a classe detectada e
as linhas. O comando de colar apenas abre e seleciona o trecho; ele nunca
insere ou modifica o código.

Todo o processamento é local, sem telemetria ou chamadas de rede.

## English

Right-click in the editor and choose **Copy with TLT** to copy the current line
or the complete selected lines together with portable location metadata. Put a
received TLT block on the clipboard and run **Paste with TLT** to open and
select the referenced range. The command never edits the document. Processing
is fully local and no telemetry is sent.

