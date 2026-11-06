namespace MiniBitcoin.Core;

/// <summary>
/// Extrato (histórico) da carteira: registra cada operação de compra e venda
/// de Bitcoin realizada e calcula o patrimônio total consolidado em BRL
/// com base na cotação atual.
/// </summary>
public class Extrato
{
    private readonly List<Transacao> _transacoes = new();

    /// <summary>Operações registradas no extrato, em ordem cronológica (somente leitura).</summary>
    public IReadOnlyList<Transacao> Operacoes => _transacoes.AsReadOnly();

    /// <summary>Registra no extrato uma transação de compra ou venda de Bitcoin.</summary>
    public void Registrar(Transacao transacao)
    {
        if (transacao is null)
        {
            throw new ArgumentNullException(nameof(transacao));
        }

        _transacoes.Add(transacao);
    }

    /// <summary>
    /// Calcula o patrimônio total consolidado da carteira em BRL:
    /// saldo em reais somado ao saldo em Bitcoin convertido pela cotação atual.
    /// </summary>
    public decimal CalcularPatrimonioTotalBRL(Carteira carteira, decimal cotacaoAtualBRL)
    {
        if (carteira is null)
        {
            throw new ArgumentNullException(nameof(carteira));
        }

        if (cotacaoAtualBRL <= 0m)
        {
            throw new ArgumentException("A cotação do Bitcoin deve ser maior que zero.", nameof(cotacaoAtualBRL));
        }

        return carteira.SaldoBRL + (carteira.SaldoBTC * cotacaoAtualBRL);
    }
}
