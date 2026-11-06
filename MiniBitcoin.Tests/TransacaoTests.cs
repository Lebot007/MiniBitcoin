using MiniBitcoin.Console;
using Xunit;

namespace MiniBitcoin.Tests;

/// <summary>
/// Testes unitários da lógica de compra e venda de Bitcoin (Etapa 2 do roteiro),
/// incluindo validações de saldo em BRL, de saldo em BTC e da cotação atual.
/// </summary>
public class TransacaoTests
{
    // ---------- Construtor e cotação ----------

    [Fact]
    public void Construtor_ComCotacaoPositiva_DeveInicializarComACotacaoInformada()
    {
        var carteira = new Carteira(1000m, 0m);

        var servico = new ServicoTransacoes(carteira, 350000m);

        Assert.Equal(350000m, servico.CotacaoAtualBRL);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Construtor_ComCotacaoInicialNaoPositiva_DeveLancarExcecao(int cotacaoInvalida)
    {
        var carteira = new Carteira(1000m, 0m);

        Assert.Throws<ArgumentException>(() => new ServicoTransacoes(carteira, (decimal)cotacaoInvalida));
    }

    [Fact]
    public void AtualizarCotacao_ComValorPositivo_DeveSerUsadaNasProximasTransacoes()
    {
        var carteira = new Carteira(70000m, 0m);
        var servico = new ServicoTransacoes(carteira, 350000m);

        servico.AtualizarCotacao(400000m);
        servico.ComprarBitcoin(0.1m);

        Assert.Equal(400000m, servico.CotacaoAtualBRL);
        Assert.Equal(30000m, carteira.SaldoBRL);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void AtualizarCotacao_ComValorNaoPositivo_DeveLancarExcecao(int cotacaoInvalida)
    {
        var carteira = new Carteira(1000m, 0m);
        var servico = new ServicoTransacoes(carteira, 350000m);

        Assert.Throws<ArgumentException>(() => servico.AtualizarCotacao((decimal)cotacaoInvalida));
    }

    // ---------- Compra ----------

    [Fact]
    public void ComprarBitcoin_ComSaldoSuficiente_DeveDebitarBRLCreditarBTCERetornarTransacao()
    {
        var carteira = new Carteira(50000m, 0m);
        var servico = new ServicoTransacoes(carteira, 350000m);

        var transacao = servico.ComprarBitcoin(0.1m);

        Assert.Equal(15000m, carteira.SaldoBRL);
        Assert.Equal(0.1m, carteira.SaldoBTC);
        Assert.Equal(TipoTransacao.Compra, transacao.Tipo);
        Assert.Equal(0.1m, transacao.QuantidadeBTC);
        Assert.Equal(350000m, transacao.PrecoCotacaoBRL);
        Assert.Equal(35000m, transacao.ValorTotalBRL);
    }

    [Fact]
    public void ComprarBitcoin_DeUmUnicoSatoshi_DeveExecutarAOperacao()
    {
        var carteira = new Carteira(100m, 0m);
        var servico = new ServicoTransacoes(carteira, 350000m);

        var transacao = servico.ComprarBitcoin(0.00000001m);

        Assert.Equal(0.00000001m, carteira.SaldoBTC);
        Assert.Equal(99.9965m, carteira.SaldoBRL);
        Assert.Equal(0.0035m, transacao.ValorTotalBRL);
    }

    [Fact]
    public void ComprarBitcoin_ComSaldoBRLInsuficiente_DeveLancarExcecaoENaoAlterarSaldos()
    {
        var carteira = new Carteira(1000m, 0m);
        var servico = new ServicoTransacoes(carteira, 350000m);

        Assert.Throws<InvalidOperationException>(() => servico.ComprarBitcoin(0.1m));

        Assert.Equal(1000m, carteira.SaldoBRL);
        Assert.Equal(0m, carteira.SaldoBTC);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void ComprarBitcoin_ComQuantidadeNaoPositiva_DeveLancarExcecao(int quantidadeInvalida)
    {
        var carteira = new Carteira(50000m, 0m);
        var servico = new ServicoTransacoes(carteira, 350000m);

        Assert.Throws<ArgumentException>(() => servico.ComprarBitcoin((decimal)quantidadeInvalida));
    }

    // ---------- Venda ----------

    [Fact]
    public void VenderBitcoin_ComSaldoBTCSuficiente_DeveCreditarBRLDebitarBTCERetornarTransacao()
    {
        var carteira = new Carteira(0m, 1m);
        var servico = new ServicoTransacoes(carteira, 300000m);

        var transacao = servico.VenderBitcoin(0.25m);

        Assert.Equal(75000m, carteira.SaldoBRL);
        Assert.Equal(0.75m, carteira.SaldoBTC);
        Assert.Equal(TipoTransacao.Venda, transacao.Tipo);
        Assert.Equal(0.25m, transacao.QuantidadeBTC);
        Assert.Equal(300000m, transacao.PrecoCotacaoBRL);
        Assert.Equal(75000m, transacao.ValorTotalBRL);
    }

    [Fact]
    public void VenderBitcoin_ComSaldoBTCInsuficiente_DeveLancarExcecaoENaoAlterarSaldos()
    {
        var carteira = new Carteira(0m, 0.1m);
        var servico = new ServicoTransacoes(carteira, 300000m);

        Assert.Throws<InvalidOperationException>(() => servico.VenderBitcoin(0.5m));

        Assert.Equal(0.1m, carteira.SaldoBTC);
        Assert.Equal(0m, carteira.SaldoBRL);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void VenderBitcoin_ComQuantidadeNaoPositiva_DeveLancarExcecao(int quantidadeInvalida)
    {
        var carteira = new Carteira(0m, 1m);
        var servico = new ServicoTransacoes(carteira, 300000m);

        Assert.Throws<ArgumentException>(() => servico.VenderBitcoin((decimal)quantidadeInvalida));
    }
}
