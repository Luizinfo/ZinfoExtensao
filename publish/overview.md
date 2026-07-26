# TechLeadTools para Visual Studio

O TechLeadTools facilita demonstrações, revisões e compartilhamento de trechos
de código entre integrantes de uma equipe técnica.

## Copiar com TLT

Clique com o botão direito no editor e escolha **Copiar com TLT**. Sem uma
seleção, a extensão copia a linha atual. Com uma seleção, copia todas as linhas
completas abrangidas.

O texto gerado contém:

- nome do arquivo;
- nome da classe, quando disponível;
- intervalo de linhas;
- caminho relativo à solução;
- conteúdo do trecho.

## Colar com TLT

Coloque um bloco TLT recebido na área de transferência e escolha
**Colar com TLT**. A extensão localiza o arquivo, abre o documento e seleciona
as linhas indicadas.

O comando não insere nem modifica código.

## Interoperabilidade

O protocolo `TLT/1` é compatível com a extensão TechLeadTools para Visual
Studio Code, permitindo copiar em uma IDE e navegar na outra.

## Privacidade

Todo o processamento acontece localmente. A extensão não envia telemetria,
código, caminhos ou conteúdo da área de transferência para serviços externos.

Código-fonte e suporte:
https://github.com/Luizinfo/ZinfoExtensao
