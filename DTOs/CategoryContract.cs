using FrClassifier.Entities;

namespace FrClassifier.DTOs;

public static class CategoryContract
{
    public static IReadOnlyList<CategoryDto> All { get; } =
    [
        new("operating", "Income and expenses in the operating category."),
        new("investing", "Income and expenses in the investing category."),
        new("financing", "Income and expenses in the financing category."),
        new("income_taxes", "Income tax expense or income."),
        new("discontinued_operations", "Income and expenses from discontinued operations.")
    ];

    public static string ToCode(Category category) => category switch
    {
        Category.Operating => "operating",
        Category.Investing => "investing",
        Category.Financing => "financing",
        Category.IncomeTaxes => "income_taxes",
        Category.DiscontinuedOperations => "discontinued_operations",
        _ => throw new ArgumentOutOfRangeException(nameof(category))
    };

    public static bool TryParse(string? code, out Category category)
    {
        category = code?.Trim().ToLowerInvariant() switch
        {
            "operating" => Category.Operating,
            "investing" => Category.Investing,
            "financing" => Category.Financing,
            "income_taxes" => Category.IncomeTaxes,
            "discontinued_operations" => Category.DiscontinuedOperations,
            _ => (Category)(-1)
        };
        return Enum.IsDefined(category);
    }
}