using FrClassifier.Entities;

namespace FrClassifier.DTOs;

public sealed record IFRS18CategoryDto(string Code, string Description);

public static class IFRS18CategoryContract
{
    public static IReadOnlyList<IFRS18CategoryDto> All { get; } =
    [
        new("operating", "Income and expenses in the operating category."),
        new("investing", "Income and expenses in the investing category."),
        new("financing", "Income and expenses in the financing category."),
        new("income_taxes", "Income tax expense or income."),
        new("discontinued_operations", "Income and expenses from discontinued operations.")
    ];

    public static string ToCode(IFRS18Category category) => category switch
    {
        IFRS18Category.Operating => "operating",
        IFRS18Category.Investing => "investing",
        IFRS18Category.Financing => "financing",
        IFRS18Category.IncomeTaxes => "income_taxes",
        IFRS18Category.DiscontinuedOperations => "discontinued_operations",
        _ => throw new ArgumentOutOfRangeException(nameof(category))
    };

    public static bool TryParse(string? code, out IFRS18Category category)
    {
        category = code?.Trim().ToLowerInvariant() switch
        {
            "operating" => IFRS18Category.Operating,
            "investing" => IFRS18Category.Investing,
            "financing" => IFRS18Category.Financing,
            "income_taxes" => IFRS18Category.IncomeTaxes,
            "discontinued_operations" => IFRS18Category.DiscontinuedOperations,
            _ => (IFRS18Category)(-1)
        };
        return Enum.IsDefined(category);
    }
}