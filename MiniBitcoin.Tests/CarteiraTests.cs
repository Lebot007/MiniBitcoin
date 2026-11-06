using MiniBitcoin.Console;
using Xunit;

namespace MiniBitcoin.Tests;

/// <summary>
/// Testes unitários da entidade Carteira (Etapa 1 do roteiro).
/// Garantem que a carteira não pode ser criada com saldos negativos e que
/// créditos e débitos inválidos são rejeitados sem alterar o estado da carteira.
/// </summary>
public class CarteiraTests
{
    // ---------- Construtor ----------

    [Fact]
    public void Construtor_SemParametros_DeveIniciarComSaldosZerados()
    {
        var carteira = new Carteira();

        Assert.Equal(0m, carteira.SaldoBRL);
        Assert.Equal(0m, carteira.SaldoBTC);
    }

    [Fact]
    public void Construtor_ComSaldosPositivos_DeveCriarCarteiraComOsSaldosInformados()
    {
        var carteira = new Carteira(1000m, 0.5m);

        Assert.Equal(1000m, carteira.SaldoBRL);
        Assert.Equal(0.5m, carteira.SaldoBTC);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-1000)]
    public void Construtor_ComSaldoInicialBRLNegativo_DeveLancarExcecao(int saldoInvalido)
    {
        Assert.Throws<ArgumentException>(() => new Carteira((decimal)saldoInvalido, 0m));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-1000)]
    public void Construtor_ComSaldoInicialBTCNegativo_DeveLancarExcecao(int saldoInvalido)
    {
        Assert.Throws<ArgumentException>(() => new Carteira(0m, (decimal)saldoInvalido));
    }

    // ---------- Operações em BRL ----------

    [Fact]
    public void CreditarBRL_ComValorPositivo_DeveAumentarSaldoBRL()
    {
        var carteira = new Carteira(100m, 0m);

        carteira.CreditarBRL(250m);

        Assert.Equal(350m, carteira.SaldoBRL);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-50)]
    public void CreditarBRL_ComValorNaoPositivo_DeveLancarExcecao(int valorInvalido)
    {
        var carteira = new Carteira(100m, 0m);

        Assert.Throws<ArgumentException>(() => carteira.CreditarBRL((decimal)valorInvalido));
    }

    [Fact]
    public void DebitarBRL_ComValorDisponivel_DeveReduzirSaldoBRL()
    {
        var carteira = new Carteira(500m, 0m);

        carteira.DebitarBRL(200m);

        Assert.Equal(300m, carteira.SaldoBRL);
    }

    [Fact]
    public void DebitarBRL_ComValorIgualAoSaldo_DeveZerarSaldoBRL()
    {
        var carteira = new Carteira(500m, 0m);

        carteira.DebitarBRL(500m);

        Assert.Equal(0m, carteira.SaldoBRL);
    }

    [Fact]
    public void DebitarBRL_ComValorMaiorQueSaldo_DeveLancarExcecaoENaoAlterarSaldo()
    {
        var carteira = new Carteira(100m, 0m);

        Assert.Throws<InvalidOperationException>(() => carteira.DebitarBRL(100.01m));

        Assert.Equal(100m, carteira.SaldoBRL);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public void DebitarBRL_ComValorNaoPositivo_DeveLancarExcecao(int valorInvalido)
    {
        var carteira = new Carteira(100m, 0m);

        Assert.Throws<ArgumentException>(() => carteira.DebitarBRL((decimal)valorInvalido));
    }

    // ---------- Operações em BTC ----------

    [Fact]
    public void CreditarBTC_ComQuantidadePositiva_DeveAumentarSaldoBTC()
    {
        var carteira = new Carteira(0m, 0.1m);

        carteira.CreditarBTC(0.05m);

        Assert.Equal(0.15m, carteira.SaldoBTC);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void CreditarBTC_ComQuantidadeNaoPositiva_DeveLancarExcecao(int quantidadeInvalida)
    {
        var carteira = new Carteira(0m, 0.1m);

        Assert.Throws<ArgumentException>(() => carteira.CreditarBTC((decimal)quantidadeInvalida));
    }

    [Fact]
    public void DebitarBTC_ComQuantidadeDisponivel_DeveReduzirSaldoBTC()
    {
        var carteira = new Carteira(0m, 1m);

        carteira.DebitarBTC(0.25m);

        Assert.Equal(0.75m, carteira.SaldoBTC);
    }

    [Fact]
    public void DebitarBTC_ComQuantidadeMaiorQueSaldo_DeveLancarExcecaoENaoAlterarSaldo()
    {
        var carteira = new Carteira(0m, 0.1m);

        Assert.Throws<InvalidOperationException>(() => carteira.DebitarBTC(0.2m));

        Assert.Equal(0.1m, carteira.SaldoBTC);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void DebitarBTC_ComQuantidadeNaoPositiva_DeveLancarExcecao(int quantidadeInvalida)
    {
        var carteira = new Carteira(0m, 1m);

        Assert.Throws<ArgumentException>(() => carteira.DebitarBTC((decimal)quantidadeInvalida));
    }
}
