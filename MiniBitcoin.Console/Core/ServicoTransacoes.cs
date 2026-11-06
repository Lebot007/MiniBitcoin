namespace MiniBitcoin.Core;

/// <summary>
/// Serviço responsável pela execução de ordens de compra e venda de Bitcoin,
/// utilizando a cotação atual (simulada/atualizável) e validando os saldos
/// disponíveis na carteira antes de cada operação.
/// </summary>
public class ServicoTransacoes
{
    private readonly Carteira _carteira;

    /// <summary>Cotação atual do Bitcoin em BRL.</summary>
    public decimal CotacaoAtualBRL { get; private set; }

    public ServicoTransacoes(Carteira carteira, decimal cotacaoInicialBRL)
    {
        ValidarCotacao(cotacaoInicialBRL);

        _carteira = carteira ?? throw new ArgumentNullException(nameof(carteira));
        CotacaoAtualBRL = cotacaoInicialBRL;
    }

    /// <summary>Atualiza a cotação simulada do Bitcoin em BRL.</summary>
    public void AtualizarCotacao(decimal novaCotacaoBRL)
    {
        ValidarCotacao(novaCotacaoBRL);

        CotacaoAtualBRL = novaCotacaoBRL;
    }

    /// <summary>
    /// Compra a quantidade informada de BTC pelo valor total
    /// (quantidade × cotação atual), debitando o saldo em BRL.
    /// </summary>
    public Transacao ComprarBitcoin(decimal quantidadeBTC)
    {
        ValidarQuantidade(quantidadeBTC);

        decimal valorTotalBRL = quantidadeBTC * CotacaoAtualBRL;

        // O débito em BRL valida internamente o saldo disponível e ocorre
        // antes do crédito em BTC para manter a operação atômica: se não
        // houver fundos, nada é alterado na carteira.
        _carteira.DebitarBRL(valorTotalBRL);
        _carteira.CreditarBTC(quantidadeBTC);

        return new Transacao(TipoTransacao.Compra, quantidadeBTC, CotacaoAtualBRL, valorTotalBRL, DateTime.Now);
    }

    /// <summary>
    /// Vende a quantidade informada de BTC creditando em BRL o valor total
    /// (quantidade × cotação atual).
    /// </summary>
    public Transacao VenderBitcoin(decimal quantidadeBTC)
    {
        ValidarQuantidade(quantidadeBTC);

        decimal valorTotalBRL = quantidadeBTC * CotacaoAtualBRL;

        // O débito em BTC valida internamente o saldo disponível e ocorre
        // antes do crédito em BRL para manter a operação atômica.
        _carteira.DebitarBTC(quantidadeBTC);
        _carteira.CreditarBRL(valorTotalBRL);

        return new Transacao(TipoTransacao.Venda, quantidadeBTC, CotacaoAtualBRL, valorTotalBRL, DateTime.Now);
    }

    private static void ValidarCotacao(decimal cotacaoBRL)
    {
        if (cotacaoBRL <= 0m)
        {
            throw new ArgumentException("A cotação do Bitcoin deve ser maior que zero.");
        }
    }

    private static void ValidarQuantidade(decimal quantidadeBTC)
    {
        if (quantidadeBTC <= 0m)
        {
            throw new ArgumentException("A quantidade de Bitcoin deve ser maior que zero.");
        }
    }
}
