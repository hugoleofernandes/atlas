using Atlas.Party.Domain.Shared.Exceptions;
using Atlas.SharedKernel.Domain;

namespace Atlas.Party.Domain.Shared;

/// <summary>
/// Brazilian tax identifier — either CPF (persons, 11 digits) or CNPJ (organizations, 14 digits).
/// Stores only digits; validates the check-digit algorithm on construction.
/// </summary>
public sealed class TaxNumber : ValueObject
{
    public string Value { get; }

    public TaxNumberType Type { get; }

    private TaxNumber(string value, TaxNumberType type)
    {
        Value = value;
        Type = type;
    }

    public static TaxNumber Create(string raw)
    {
        var digits = new string(raw.Where(char.IsDigit).ToArray());

        if (digits.Length == 11 && IsValidCpf(digits))
            return new TaxNumber(digits, TaxNumberType.Cpf);

        if (digits.Length == 14 && IsValidCnpj(digits))
            return new TaxNumber(digits, TaxNumberType.Cnpj);

        throw new InvalidTaxNumberException(raw);
    }

    // CPF: Módulo 11 — two check digits
    private static bool IsValidCpf(string d)
    {
        if (d.Distinct().Count() == 1) return false;

        int sum = 0;
        for (int i = 0; i < 9; i++) sum += (d[i] - '0') * (10 - i);
        int r1 = sum % 11;
        int check1 = r1 < 2 ? 0 : 11 - r1;
        if (d[9] - '0' != check1) return false;

        sum = 0;
        for (int i = 0; i < 10; i++) sum += (d[i] - '0') * (11 - i);
        int r2 = sum % 11;
        int check2 = r2 < 2 ? 0 : 11 - r2;
        return d[10] - '0' == check2;
    }

    // CNPJ: Módulo 11 — two check digits
    private static bool IsValidCnpj(string d)
    {
        if (d.Distinct().Count() == 1) return false;

        int[] weights1 = [5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2];
        int sum = 0;
        for (int i = 0; i < 12; i++) sum += (d[i] - '0') * weights1[i];
        int r1 = sum % 11;
        int check1 = r1 < 2 ? 0 : 11 - r1;
        if (d[12] - '0' != check1) return false;

        int[] weights2 = [6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2];
        sum = 0;
        for (int i = 0; i < 13; i++) sum += (d[i] - '0') * weights2[i];
        int r2 = sum % 11;
        int check2 = r2 < 2 ? 0 : 11 - r2;
        return d[13] - '0' == check2;
    }

    public string Formatted() => Type == TaxNumberType.Cpf
        ? $"{Value[..3]}.{Value[3..6]}.{Value[6..9]}-{Value[9..]}"
        : $"{Value[..2]}.{Value[2..5]}.{Value[5..8]}/{Value[8..12]}-{Value[12..]}";

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}

public enum TaxNumberType { Cpf, Cnpj }

