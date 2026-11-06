namespace MiniBitcoin.Console;

/// <summary>Tipo de operação realizada na carteira.</summary>
public enum TipoTransacao
{
    Compra,
    Venda
}

/// <summary>
/// Representa uma operação de compra ou venda de Bitcoin executada
/// com base na cotação vigente no momento da ordem.
/// </summary>
/// <param name="Tipo">Tipo da operação (compra ou venda).</param>
/// <param name="QuantidadeBTC">Quantidade de Bitcoin negociada.</param>
/// <param name="PrecoCotacaoBRL">Cotação unitária do BTC em BRL usada na operação.</param>
/// <param name="ValorTotalBRL">Valor total da operação em BRL.</param>
/// <param name="DataHora">Momento em que a operação foi executada.</param>
public record Transacao(
    TipoTransacao Tipo,
    decimal QuantidadeBTC,
    decimal PrecoCotacaoBRL,
    decimal ValorTotalBRL,
    DateTime DataHora);
