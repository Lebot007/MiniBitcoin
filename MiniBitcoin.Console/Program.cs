using System.Globalization;
using MiniBitcoin.Core;

// =====================================================================
//  MiniBitcoin — Carteira de Bitcoin (aplicação de console, .NET 8)
//  Menu interativo: cotação, saldos, compra, venda e extrato.
// =====================================================================

const decimal saldoInicialBRL = 10000m;
const decimal cotacaoInicialBRL = 350000m;

Console.WriteLine("======================================================");
Console.WriteLine("       MiniBitcoin — Carteira de Bitcoin");
Console.WriteLine("======================================================");
Console.WriteLine($"Saldo inicial ....... {FormatarBRL(saldoInicialBRL)}");
Console.WriteLine($"Cotação inicial ..... {FormatarBRL(cotacaoInicialBRL)}");
Console.WriteLine();

var carteira = new Carteira(saldoInicialBRL, 0m);
var servico = new ServicoTransacoes(carteira, cotacaoInicialBRL);
var extrato = new Extrato();

bool executando = true;
while (executando)
{
    ExibirMenu();
    string? opcao = Console.ReadLine();
    Console.WriteLine();

    if (opcao is null)
    {
        // Fim da entrada padrão (Ctrl+D no Linux/macOS, Ctrl+Z no Windows):
        // encerra o programa com segurança em vez de entrar em loop infinito.
        Console.WriteLine("Entrada de dados encerrada. Saindo do MiniBitcoin...");
        break;
    }

    try
    {
        switch (opcao.Trim())
        {
            case "1":
                ConsultarCotacao();
                break;
            case "2":
                ExibirSaldosEPatrimonio();
                break;
            case "3":
                ExecutarCompra();
                break;
            case "4":
                ExecutarVenda();
                break;
            case "5":
                ExibirExtrato();
                break;
            case "0":
                executando = false;
                Console.WriteLine("Obrigado por usar o MiniBitcoin. Até a próxima!");
                break;
            default:
                Console.WriteLine("Opção inválida. Digite um número entre 0 e 5.");
                break;
        }
    }
    catch (Exception ex)
    {
        // Rede de segurança: nenhuma falha inesperada pode derrubar o menu.
        Console.WriteLine($"Ocorreu um erro inesperado: {ex.Message}");
    }

    Console.WriteLine();
}

// ---------------------------------------------------------------------
// Ações do menu (capturam carteira, servico e extrato declarados acima)
// ---------------------------------------------------------------------

/// <summary>Exibe o menu principal de opções.</summary>
static void ExibirMenu()
{
    Console.WriteLine("------------------------- MENU -------------------------");
    Console.WriteLine(" 1) Consultar cotação");
    Console.WriteLine(" 2) Ver saldos e patrimônio consolidado");
    Console.WriteLine(" 3) Comprar Bitcoin");
    Console.WriteLine(" 4) Vender Bitcoin");
    Console.WriteLine(" 5) Exibir extrato completo");
    Console.WriteLine(" 0) Sair");
    Console.Write("Escolha uma opção: ");
}

/// <summary>Exibe a cotação atual e, se o usuário desejar, atualiza o valor simulado.</summary>
void ConsultarCotacao()
{
    Console.WriteLine($"Cotação atual do Bitcoin: {FormatarBRL(servico.CotacaoAtualBRL)}");
    Console.Write("Deseja atualizar a cotação? (s/N): ");
    string? resposta = Console.ReadLine();

    if (!string.Equals(resposta?.Trim(), "s", StringComparison.OrdinalIgnoreCase))
    {
        return;
    }

    Console.Write("Nova cotação do Bitcoin em BRL (ex.: 400000): ");
    if (!TryLerDecimal(Console.ReadLine(), out decimal novaCotacao))
    {
        Console.WriteLine("Valor inválido: informe um número, por exemplo 400000 ou 400000,50.");
        return;
    }

    try
    {
        servico.AtualizarCotacao(novaCotacao);
        Console.WriteLine($"Cotação atualizada para {FormatarBRL(servico.CotacaoAtualBRL)}.");
    }
    catch (ArgumentException)
    {
        Console.WriteLine("Cotação inválida: o valor deve ser maior que zero.");
    }
}

/// <summary>Exibe os saldos em BRL e BTC e o patrimônio consolidado pela cotação atual.</summary>
void ExibirSaldosEPatrimonio()
{
    Console.WriteLine("--- Saldos ---");
    Console.WriteLine($"Saldo em BRL ............. {FormatarBRL(carteira.SaldoBRL)}");
    Console.WriteLine($"Saldo em BTC ............. {FormatarBTC(carteira.SaldoBTC)}");
    Console.WriteLine($"Cotação atual ............ {FormatarBRL(servico.CotacaoAtualBRL)}");
    Console.WriteLine($"Patrimônio consolidado ... {FormatarBRL(extrato.CalcularPatrimonioTotalBRL(carteira, servico.CotacaoAtualBRL))}");
}

/// <summary>Executa uma ordem de compra de Bitcoin e a registra no extrato.</summary>
void ExecutarCompra()
{
    Console.WriteLine("--- Comprar Bitcoin ---");
    Console.Write("Quantidade de BTC a comprar (ex.: 0,01): ");

    if (!TryLerDecimal(Console.ReadLine(), out decimal quantidadeBTC))
    {
        Console.WriteLine("Quantidade inválida: informe um número, por exemplo 0,01.");
        return;
    }

    try
    {
        Transacao transacao = servico.ComprarBitcoin(quantidadeBTC);
        extrato.Registrar(transacao);
        Console.WriteLine($"Compra realizada: {FormatarBTC(transacao.QuantidadeBTC)} por {FormatarBRL(transacao.ValorTotalBRL)} (cotação {FormatarBRL(transacao.PrecoCotacaoBRL)}).");
    }
    catch (ArgumentException)
    {
        Console.WriteLine("Quantidade inválida: o valor deve ser maior que zero.");
    }
    catch (InvalidOperationException)
    {
        Console.WriteLine("Saldo em BRL insuficiente para concluir a compra.");
    }
}

/// <summary>Executa uma ordem de venda de Bitcoin e a registra no extrato.</summary>
void ExecutarVenda()
{
    Console.WriteLine("--- Vender Bitcoin ---");
    Console.Write("Quantidade de BTC a vender (ex.: 0,005): ");

    if (!TryLerDecimal(Console.ReadLine(), out decimal quantidadeBTC))
    {
        Console.WriteLine("Quantidade inválida: informe um número, por exemplo 0,005.");
        return;
    }

    try
    {
        Transacao transacao = servico.VenderBitcoin(quantidadeBTC);
        extrato.Registrar(transacao);
        Console.WriteLine($"Venda realizada: {FormatarBTC(transacao.QuantidadeBTC)} por {FormatarBRL(transacao.ValorTotalBRL)} (cotação {FormatarBRL(transacao.PrecoCotacaoBRL)}).");
    }
    catch (ArgumentException)
    {
        Console.WriteLine("Quantidade inválida: o valor deve ser maior que zero.");
    }
    catch (InvalidOperationException)
    {
        Console.WriteLine("Saldo em BTC insuficiente para concluir a venda.");
    }
}

/// <summary>Exibe o histórico completo de transações e o patrimônio consolidado.</summary>
void ExibirExtrato()
{
    Console.WriteLine("--- Extrato de transações ---");

    if (extrato.Operacoes.Count == 0)
    {
        Console.WriteLine("Nenhuma transação realizada até o momento.");
    }
    else
    {
        Console.WriteLine($"{"Data/Hora",-19} | {"Tipo",-6} | {"Quantidade BTC",14} | {"Cotação",14} | {"Valor",14}");
        Console.WriteLine(new string('-', 84));

        foreach (Transacao transacao in extrato.Operacoes)
        {
            Console.WriteLine(
                $"{transacao.DataHora:dd/MM/yyyy HH:mm:ss} | {NomeDoTipo(transacao.Tipo),-6} | {FormatarBTC(transacao.QuantidadeBTC),14} | {FormatarBRL(transacao.PrecoCotacaoBRL),14} | {FormatarBRL(transacao.ValorTotalBRL),14}");
        }
    }

    Console.WriteLine();
    Console.WriteLine($"Total de transações ..... {extrato.Operacoes.Count}");
    Console.WriteLine($"Patrimônio consolidado .. {FormatarBRL(extrato.CalcularPatrimonioTotalBRL(carteira, servico.CotacaoAtualBRL))}");
}

// ---------------------------------------------------------------------
// Utilidades de apresentação e entrada de dados
// ---------------------------------------------------------------------

/// <summary>Retorna o nome amigável do tipo de transação.</summary>
static string NomeDoTipo(TipoTransacao tipo) => tipo switch
{
    TipoTransacao.Compra => "Compra",
    TipoTransacao.Venda => "Venda",
    _ => tipo.ToString(),
};

/// <summary>Formata um valor em reais com duas casas decimais (padrão brasileiro).</summary>
static string FormatarBRL(decimal valor) => $"R$ {valor.ToString("N2", CulturaBrasileira())}";

/// <summary>Formata uma quantidade de Bitcoin com até oito casas decimais.</summary>
static string FormatarBTC(decimal quantidade) => $"{quantidade.ToString("0.########", CulturaBrasileira())} BTC";

/// <summary>
/// Tenta converter uma linha digitada pelo usuário em <see cref="decimal"/>,
/// aceitando vírgula ou ponto como separador decimal.
/// </summary>
static bool TryLerDecimal(string? texto, out decimal valor)
{
    valor = 0m;

    if (string.IsNullOrWhiteSpace(texto))
    {
        return false;
    }

    texto = texto.Trim();

    // Aceita "1234,56" (vírgula decimal), "1.234,56" (ponto de milhar +
    // vírgula decimal) e "1234.56" (ponto decimal), normalizando sempre
    // para o formato invariante antes do parse.
    if (texto.Contains(',') && texto.Contains('.'))
    {
        texto = texto.Replace(".", string.Empty).Replace(',', '.');
    }
    else if (texto.Contains(','))
    {
        texto = texto.Replace(',', '.');
    }

    return decimal.TryParse(texto, NumberStyles.Number, CultureInfo.InvariantCulture, out valor);
}

/// <summary>
/// Cultura brasileira usada na formatação de valores; caso o ambiente
/// não a possua, usa a cultura invariante como alternativa segura.
/// </summary>
static CultureInfo CulturaBrasileira()
{
    try
    {
        return CultureInfo.GetCultureInfo("pt-BR");
    }
    catch (CultureNotFoundException)
    {
        return CultureInfo.InvariantCulture;
    }
}
