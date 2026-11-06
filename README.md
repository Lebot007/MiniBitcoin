# MiniBitcoin

Aplicação de console em **.NET 8** que simula uma carteira de Bitcoin: o usuário gerencia saldos em reais (BRL) e em frações de BTC, consulta e atualiza a cotação simulada, executa ordens de compra e venda e acompanha o extrato completo das operações com o patrimônio total consolidado.

Projeto da disciplina **Gestão e Qualidade de Software** (Professor Daniel Henrique Matos de Paiva), desenvolvido em dupla seguindo rigorosamente as práticas de **TDD** (testes escritos antes da implementação) e **Conventional Commits**.

## Requisitos funcionais

1. **Gestão de Carteira (Wallet)** — gerenciamento do saldo em moeda fiduciária (BRL) e do saldo em Satoshis/Bitcoin, com validações que impedem saldos negativos em qualquer operação (inicialização, créditos e débitos).
2. **Cotação e simulação de mercado** — manutenção e consulta do preço atual do Bitcoin (valor simulado, atualizável pelo próprio sistema).
3. **Compra e venda de Bitcoin** — execução de ordens de compra e venda utilizando o saldo em BRL, com validação de fundos disponíveis e atualização automática dos saldos correspondentes.
4. **Extrato de transações** — exibição do histórico completo de operações realizadas (compras e vendas) e do patrimônio total consolidado em BRL com base na cotação atual.

## Arquitetura

A solução `MiniBitcoin.sln` é composta por dois projetos:

| Projeto | Tipo | Responsabilidade |
| --- | --- | --- |
| `MiniBitcoin.Console` | Console (.NET 8) | Núcleo de domínio (namespace `MiniBitcoin.Core`) e interface interativa (`Program.cs`) |
| `MiniBitcoin.Tests` | Biblioteca de testes (xUnit) | Testes unitários das entidades e serviços de domínio |

### Estrutura de arquivos

```text
MiniBitcoin/
├── MiniBitcoin.sln
├── README.md
├── .gitignore
├── MiniBitcoin.Console/
│   ├── Program.cs                    # Menu interativo (Etapa 4)
│   ├── MiniBitcoin.Console.csproj
│   └── Core/                         # Namespace MiniBitcoin.Core (Etapa 5)
│       ├── Carteira.cs               # Entidade de saldos em BRL e BTC (Etapa 1)
│       ├── Transacao.cs              # enum TipoTransacao + record Transacao (Etapa 2)
│       ├── ServicoTransacoes.cs      # Cotação, compra e venda (Etapa 2)
│       └── Extrato.cs                # Histórico e patrimônio consolidado (Etapa 3)
└── MiniBitcoin.Tests/
    ├── CarteiraTests.cs              # Testes da Etapa 1
    ├── TransacaoTests.cs             # Testes da Etapa 2
    ├── ExtratoTests.cs               # Testes da Etapa 3
    └── MiniBitcoin.Tests.csproj
```

### Entidades e fluxo

- **`Carteira`** — mantém `SaldoBRL` e `SaldoBTC`; o construtor rejeita saldos iniciais negativos (`ArgumentException`) e os métodos de crédito/débito rejeitam valores não positivos ou superiores ao saldo disponível (`InvalidOperationException`), garantindo que nenhum saldo fique negativo.
- **`ServicoTransacoes`** — guarda a `CotacaoAtualBRL` (atualizável). `ComprarBitcoin(qtd)` valida e debita o valor total em BRL e credita a quantidade em BTC; `VenderBitcoin(qtd)` faz o caminho inverso. Ambas são atômicas (o débito validado ocorre antes do crédito) e retornam a `Transacao` executada.
- **`Transacao`** — record imutável com tipo (`Compra`/`Venda`), quantidade em BTC, cotação usada, valor total em BRL e data/hora de execução.
- **`Extrato`** — registra cada `Transacao` via `Registrar(...)`; expõe `Operacoes` em ordem cronológica (coleção somente leitura) e calcula o patrimônio total consolidado: `SaldoBRL + (SaldoBTC × cotação atual)`.

Fluxo de uso no console: o menu aciona `ServicoTransacoes` para comprar/vender, a `Transacao` retornada é registrada no `Extrato` e o patrimônio é sempre recalculado com a cotação vigente.

## Tecnologias e práticas

- [.NET 8](https://dotnet.microsoft.com/) / C# 12
- [xUnit](https://xunit.net/) — 47 testes unitários (saldo, transações e extrato)
- **TDD** — ciclo *red → green → refactor*: para cada etapa, os testes foram escritos e commitados antes da implementação
- **Conventional Commits** — histórico padronizado (`chore`, `test`, `feat`, `refactor`, `docs`)
- `decimal` em todos os valores monetários e quantidades de BTC (precisão exata, sem erros de ponto flutuante)

## Como executar

Pré-requisitos: [.NET SDK 8.0](https://dotnet.microsoft.com/download/dotnet/8.0) ou superior (verifique com `dotnet --version`).

```bash
# Clonar o repositório
git clone https://github.com/Lebot007/MiniBitcoin.git
cd MiniBitcoin

# Restaurar dependências e compilar
dotnet restore
dotnet build

# Executar os testes unitários
dotnet test

# Executar a aplicação de console
dotnet run --project MiniBitcoin.Console/MiniBitcoin.Console.csproj
```

### Exemplo de uso

Ao iniciar, a carteira é criada com **R$ 10.000,00** e cotação inicial de **R$ 350.000,00** por BTC. O menu oferece:

```text
 1) Consultar cotação
 2) Ver saldos e patrimônio consolidado
 3) Comprar Bitcoin
 4) Vender Bitcoin
 5) Exibir extrato completo
 0) Sair
```

Exemplo de sessão: comprar `0,01` BTC consome R$ 3.500,00 do saldo; a opção *2* mostra `Saldo em BRL: R$ 6.500,00`, `Saldo em BTC: 0,01 BTC` e `Patrimônio consolidado: R$ 10.000,00`; a opção *5* lista o extrato com data/hora, tipo, quantidade, cotação e valor de cada operação. Entradas inválidas e regras de negócio violadas (saldo insuficiente, valor não positivo) são tratadas com mensagens amigáveis — o menu nunca é encerrado por erro.

## Histórico de commits (Conventional Commits)

| # | Commit | Etapa |
| --- | --- | --- |
| 1 | `chore: initial project structure with console and xunit apps` | Configuração inicial |
| 2 | `test(carteira): add unit tests for wallet balance validation` | Etapa 1 (TDD) |
| 3 | `feat(carteira): implement wallet entity and initial balance rules` | Etapa 1 |
| 4 | `test(transacao): add unit tests for bitcoin buy and sell operations` | Etapa 2 (TDD) |
| 5 | `feat(transacao): implement bitcoin purchase and sale logic with quote validation` | Etapa 2 |
| 6 | `test(extrato): add unit tests for transaction ledger and portfolio valuation` | Etapa 3 (TDD) |
| 7 | `feat(extrato): implement transaction history and portfolio summary` | Etapa 3 |
| 8 | `feat(console): implement interactive menu for bitcoin wallet application` | Etapa 4 |
| 9 | `refactor(core): improve code readability and organize namespaces` | Etapa 5 |
| 10 | `docs: add comprehensive instructions in README.md` | Etapa 5 |

## Equipe

| Papel | Nome | RA |
| --- | --- | --- |
| Dev 1 | João Vitor Alves Rodrigues | 32513480 |
| Dev 2 | Rafael Luiz Ferreira de Souza | 32511503 |
