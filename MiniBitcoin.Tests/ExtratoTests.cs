using MiniBitcoin.Console;
using Xunit;

namespace MiniBitcoin.Tests;

/// <summary>
/// Testes unitários do extrato de transações e da gestão de portfólio
/// (Etapa 3 do roteiro). Garantem que cada compra e venda seja registrada
/// no histórico, que o histórico preserve a ordem cronológica das operações
/// e que o patrimônio total consolidado em BRL reflita a cotação atual.
/// </summary>
public class ExtratoTests
{
    private const decimal CotacaoBRL = 350000m;

    // ---------- Histórico de operações ----------

    [Fact]
    public void Operacoes_CarteiraSemOperacoes_DeveRetornarHistoricoVazio()
    {
        var extrato = new Extrato();

        Assert.Empty(extrato.Operacoes);
    }

    [Fact]
    public void Registrar_CompraEVenda_DeveRegistrarAsDuasTransacoesNoExtrato()
    {
        var carteira = new Carteira(10000m, 0m);
        var servico = new ServicoTransacoes(carteira, CotacaoBRL);
        var extrato = new Extrato();

        extrato.Registrar(servico.ComprarBitcoin(0.01m));
        extrato.Registrar(servico.VenderBitcoin(0.004m));

        Assert.Equal(2, extrato.Operacoes.Count);
        Assert.Equal(TipoTransacao.Compra, extrato.Operacoes[0].Tipo);
        Assert.Equal(0.01m, extrato.Operacoes[0].QuantidadeBTC);
        Assert.Equal(TipoTransacao.Venda, extrato.Operacoes[1].Tipo);
        Assert.Equal(0.004m, extrato.Operacoes[1].QuantidadeBTC);
    }

    [Fact]
    public void Registrar_TransacoesEmSequencia_DeveManterOrdemCronologicaDoHistorico()
    {
        var extrato = new Extrato();

        extrato.Registrar(CriarTransacao(TipoTransacao.Compra, 0.01m, new DateTime(2026, 11, 1, 10, 0, 0)));
        extrato.Registrar(CriarTransacao(TipoTransacao.Venda, 0.003m, new DateTime(2026, 11, 2, 11, 30, 0)));
        extrato.Registrar(CriarTransacao(TipoTransacao.Compra, 0.002m, new DateTime(2026, 11, 3, 9, 15, 0)));

        Assert.Equal(3, extrato.Operacoes.Count);
        Assert.Equal(
            new[] { TipoTransacao.Compra, TipoTransacao.Venda, TipoTransacao.Compra },
            extrato.Operacoes.Select(transacao => transacao.Tipo));
        Assert.Equal(
            new[]
            {
                new DateTime(2026, 11, 1, 10, 0, 0),
                new DateTime(2026, 11, 2, 11, 30, 0),
                new DateTime(2026, 11, 3, 9, 15, 0)
            },
            extrato.Operacoes.Select(transacao => transacao.DataHora));
    }

    [Fact]
    public void Registrar_ComTransacaoNula_DeveLancarExcecao()
    {
        var extrato = new Extrato();

        Assert.Throws<ArgumentNullException>(() => extrato.Registrar(null!));
    }

    [Fact]
    public void Operacoes_DeveRetornarColecaoSomenteLeitura()
    {
        var extrato = new Extrato();
        extrato.Registrar(CriarTransacao(TipoTransacao.Compra, 0.01m, new DateTime(2026, 11, 1, 10, 0, 0)));

        var colecaoMutavel = Assert.IsAssignableFrom<IList<Transacao>>(extrato.Operacoes);

        Assert.Throws<NotSupportedException>(() =>
            colecaoMutavel.Add(CriarTransacao(TipoTransacao.Venda, 0.002m, new DateTime(2026, 11, 4, 12, 0, 0))));
        Assert.Single(extrato.Operacoes);
    }

    // ---------- Patrimônio total consolidado ----------

    [Fact]
    public void CalcularPatrimonioTotalBRL_ComSaldosEmBRLeBTC_DeveSomarSaldoBRLComBTCAvaliadoPelaCotacao()
    {
        var carteira = new Carteira(10000m, 0.01m);
        var extrato = new Extrato();

        // 10.000 + (0,01 × 350.000) = 10.000 + 3.500 = 13.500
        Assert.Equal(13500m, extrato.CalcularPatrimonioTotalBRL(carteira, CotacaoBRL));
    }

    [Fact]
    public void CalcularPatrimonioTotalBRL_SemSaldoBTC_DeveRetornarSomenteOSaldoEmBRL()
    {
        var carteira = new Carteira(2500m, 0m);
        var extrato = new Extrato();

        Assert.Equal(2500m, extrato.CalcularPatrimonioTotalBRL(carteira, CotacaoBRL));
    }

    [Fact]
    public void CalcularPatrimonioTotalBRL_AposAtualizarCotacao_DeveConsolidarComACotacaoAtual()
    {
        var carteira = new Carteira(10000m, 0m);
        var servico = new ServicoTransacoes(carteira, CotacaoBRL);
        var extrato = new Extrato();

        extrato.Registrar(servico.ComprarBitcoin(0.01m));
        extrato.Registrar(servico.VenderBitcoin(0.004m));

        // Saldos após as operações: 7.900 BRL e 0,006 BTC.
        Assert.Equal(7900m, carteira.SaldoBRL);
        Assert.Equal(0.006m, carteira.SaldoBTC);

        // 7.900 + (0,006 × 350.000) = 7.900 + 2.100 = 10.000
        Assert.Equal(10000m, extrato.CalcularPatrimonioTotalBRL(carteira, servico.CotacaoAtualBRL));

        servico.AtualizarCotacao(400000m);

        // 7.900 + (0,006 × 400.000) = 7.900 + 2.400 = 10.300
        Assert.Equal(10300m, extrato.CalcularPatrimonioTotalBRL(carteira, servico.CotacaoAtualBRL));
    }

    [Fact]
    public void CalcularPatrimonioTotalBRL_ComCarteiraNula_DeveLancarExcecao()
    {
        var extrato = new Extrato();

        Assert.Throws<ArgumentNullException>(() => extrato.CalcularPatrimonioTotalBRL(null!, CotacaoBRL));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void CalcularPatrimonioTotalBRL_ComCotacaoNaoPositiva_DeveLancarExcecao(int cotacaoInvalida)
    {
        var carteira = new Carteira(10000m, 0m);
        var extrato = new Extrato();

        Assert.Throws<ArgumentException>(() => extrato.CalcularPatrimonioTotalBRL(carteira, (decimal)cotacaoInvalida));
    }

    private static Transacao CriarTransacao(TipoTransacao tipo, decimal quantidadeBTC, DateTime dataHora)
        => new(tipo, quantidadeBTC, CotacaoBRL, quantidadeBTC * CotacaoBRL, dataHora);
}
