# Protocolo TLT/1

O TechLeadTools usa um formato textual, legível por pessoas e interoperável
entre Visual Studio Code e Visual Studio.

```text
PedidoService.cs:PedidoService:42-48
TLT/1 {"workspace":"MeuProjeto","path":"src/Services/PedidoService.cs","file":"PedidoService.cs","class":"PedidoService","startLine":42,"endLine":48}
---
<conteúdo integral das linhas 42 a 48>
```

## Regras

- As linhas são inclusivas e começam em 1.
- Uma única linha usa `:42`; um intervalo usa `:42-48`.
- `path` é relativo à pasta do workspace/solução e sempre usa `/`.
- Caminhos absolutos e segmentos `..` são inválidos.
- A primeira linha é o cabeçalho humano; a segunda é o contrato de máquina.
- O delimitador `---` separa os metadados do código.
- `class` contém a classe mais interna que abrange toda a seleção. Classes
  aninhadas usam `Externa.Interna`. Quando isso não puder ser determinado,
  usa-se `Global`.
- Consumidores devem rejeitar versões desconhecidas e não podem abrir arquivos
  fora do workspace/solução.

