namespace MiniBitcoin.Core;

/// <summary>
/// Entidade que representa a carteira do usuário, com saldos em moeda
/// fiduciária (BRL) e em Bitcoin (frações de BTC).
/// Garante que nenhum saldo fique negativo em nenhuma operação.
/// </summary>
public class Carteira
{
    /// <summary>Saldo atual em reais (BRL).</summary>
    public decimal SaldoBRL { get; private set; }

    /// <summary>Saldo atual em Bitcoin (frações de BTC).</summary>
    public decimal SaldoBTC { get; private set; }

    public Carteira(decimal saldoInicialBRL = 0m, decimal saldoInicialBTC = 0m)
    {
        if (saldoInicialBRL < 0m)
        {
            throw new ArgumentException("O saldo inicial em BRL não pode ser negativo.", nameof(saldoInicialBRL));
        }

        if (saldoInicialBTC < 0m)
        {
            throw new ArgumentException("O saldo inicial em BTC não pode ser negativo.", nameof(saldoInicialBTC));
        }

        SaldoBRL = saldoInicialBRL;
        SaldoBTC = saldoInicialBTC;
    }

    /// <summary>Adiciona um valor positivo ao saldo em BRL.</summary>
    public void CreditarBRL(decimal valor)
    {
        ValidarValorPositivo(valor, "creditar em BRL");

        SaldoBRL += valor;
    }

    /// <summary>Debita um valor positivo do saldo em BRL, se houver fundos disponíveis.</summary>
    public void DebitarBRL(decimal valor)
    {
        ValidarValorPositivo(valor, "debitar em BRL");

        if (valor > SaldoBRL)
        {
            throw new InvalidOperationException("Saldo em BRL insuficiente para a operação.");
        }

        SaldoBRL -= valor;
    }

    /// <summary>Adiciona uma quantidade positiva de BTC ao saldo.</summary>
    public void CreditarBTC(decimal quantidade)
    {
        ValidarValorPositivo(quantidade, "creditar em BTC");

        SaldoBTC += quantidade;
    }

    /// <summary>Debita uma quantidade positiva de BTC, se houver saldo disponível.</summary>
    public void DebitarBTC(decimal quantidade)
    {
        ValidarValorPositivo(quantidade, "debitar em BTC");

        if (quantidade > SaldoBTC)
        {
            throw new InvalidOperationException("Saldo em BTC insuficiente para a operação.");
        }

        SaldoBTC -= quantidade;
    }

    private static void ValidarValorPositivo(decimal valor, string operacao)
    {
        if (valor <= 0m)
        {
            throw new ArgumentException($"O valor para {operacao} deve ser maior que zero.");
        }
    }
}
