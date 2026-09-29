using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using ClosedXML.Excel;
using FrClassifier.Entities;

namespace FrClassifier.Services;

public sealed class AcdocaXlsxImporter : IDocumentParser
{
    private static readonly string[] RequiredColumns =
    [
        "RLDNR", "RBUKRS", "GJAHR", "BELNR", "DOCLN", "RACCT", "BUDAT", "WSL", "RWCUR"
    ];

    public bool CanParse(string fileName, string contentType) =>
        Path.GetExtension(fileName).Equals(".xlsx", StringComparison.OrdinalIgnoreCase);

    public Task<IReadOnlyList<Account>> ParseAsync(
        Stream content,
        Guid documentId,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        using var workbook = new XLWorkbook(content);
        var accounts = new List<Account>();

        foreach (var worksheet in workbook.Worksheets)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var usedRange = worksheet.RangeUsed();
            if (usedRange is null)
            {
                continue;
            }

            var header = FindHeader(worksheet, usedRange.LastRow().RowNumber());
            if (header is null)
            {
                continue;
            }
            var headerValue = header.Value;

            for (var rowNumber = headerValue.RowNumber + 1;
                 rowNumber <= usedRange.LastRow().RowNumber();
                 rowNumber++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var row = worksheet.Row(rowNumber);
                if (RequiredColumns.All(column => string.IsNullOrWhiteSpace(GetCell(row, headerValue.Columns, column))))
                {
                    continue;
                }

                accounts.Add(ParseAccount(
                    row,
                    headerValue.Columns,
                    documentId,
                    worksheet.Name,
                    rowNumber));
            }
        }

        if (accounts.Count == 0)
        {
            throw new InvalidDataException(
                "No ACDOCA rows were found. Expected columns: " + string.Join(", ", RequiredColumns));
        }

        return Task.FromResult<IReadOnlyList<Account>>(accounts);
    }

    private static (int RowNumber, Dictionary<string, int> Columns)? FindHeader(
        IXLWorksheet worksheet,
        int lastRowNumber)
    {
        var searchThroughRow = Math.Min(lastRowNumber, 50);
        for (var rowNumber = 1; rowNumber <= searchThroughRow; rowNumber++)
        {
            var columns = worksheet.Row(rowNumber)
                .CellsUsed()
                .Where(cell => !string.IsNullOrWhiteSpace(cell.GetString()))
                .ToDictionary(
                    cell => cell.GetString().Trim().ToUpperInvariant(),
                    cell => cell.Address.ColumnNumber,
                    StringComparer.Ordinal);

            if (RequiredColumns.All(columns.ContainsKey))
            {
                return (rowNumber, columns);
            }
        }

        return null;
    }

    private static Account ParseAccount(
        IXLRow row,
        IReadOnlyDictionary<string, int> columns,
        Guid documentId,
        string worksheetName,
        int rowNumber)
    {
        var transactionCurrency = RequiredText(row, columns, "RWCUR", rowNumber);
        if (transactionCurrency.Length > 5)
        {
            throw new InvalidDataException($"RWCUR exceeds five characters on row {rowNumber}.");
        }

        var hashInput = string.Join('\u001f', row.CellsUsed().Select(cell => cell.GetFormattedString()));
        var ledger = RequiredText(row, columns, "RLDNR", rowNumber);
        var entityCode = RequiredText(row, columns, "RBUKRS", rowNumber);
        var fiscalYear = ParseInt(RequiredText(row, columns, "GJAHR", rowNumber), "GJAHR", rowNumber);
        var documentNumber = RequiredText(row, columns, "BELNR", rowNumber);
        var lineNumber = RequiredText(row, columns, "DOCLN", rowNumber);
        var profitCenter = OptionalText(row, columns, "PRCTR");
        var costCenter = OptionalText(row, columns, "RCNTR");
        var segment = OptionalText(row, columns, "SEGMENT");

        return new Account
        {
            Id = Guid.NewGuid(),
            DocumentId = documentId,
            EntityCode = entityCode,
            FiscalYear = fiscalYear,
            AccountCode = RequiredText(row, columns, "RACCT", rowNumber),
            AccountName = OptionalText(row, columns, "TXT50"),
            Description = OptionalText(row, columns, "SGTXT"),
            PostingDate = ParseDate(RequiredText(row, columns, "BUDAT", rowNumber), "BUDAT", rowNumber),
            DocumentDate = ParseOptionalDate(row, columns, "BLDAT", rowNumber),
            Amount = ParseDecimal(
                RequiredText(row, columns, "WSL", rowNumber), "WSL", rowNumber),
            CurrencyCode = transactionCurrency,
            ReportingAmount = ParseOptionalDecimal(row, columns, "HSL", rowNumber),
            ReportingCurrencyCode = OptionalText(row, columns, "RHCUR"),
            SourceReference = $"{ledger}/{entityCode}/{fiscalYear}/{documentNumber}/{lineNumber}",
            SourceLocation = worksheetName,
            SourceRowNumber = rowNumber,
            SourceRowHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(hashInput)))
                .ToLowerInvariant(),
            DimensionsJson = JsonSerializer.Serialize(new { ledger, profitCenter, costCenter, segment })
        };
    }

    private static string RequiredText(
        IXLRow row,
        IReadOnlyDictionary<string, int> columns,
        string name,
        int rowNumber)
    {
        var value = GetCell(row, columns, name);
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidDataException($"Required ACDOCA column {name} is empty on row {rowNumber}.");
        }

        return value;
    }

    private static string? OptionalText(
        IXLRow row,
        IReadOnlyDictionary<string, int> columns,
        string name)
    {
        var value = GetCell(row, columns, name);
        return string.IsNullOrWhiteSpace(value) ? null : value;
    }

    private static string GetCell(IXLRow row, IReadOnlyDictionary<string, int> columns, string name) =>
        columns.TryGetValue(name, out var columnNumber)
            ? row.Cell(columnNumber).GetFormattedString().Trim()
            : string.Empty;

    private static int ParseInt(string value, string column, int rowNumber) =>
        int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result)
            ? result
            : throw new InvalidDataException($"Invalid {column} value on row {rowNumber}.");

    private static decimal ParseDecimal(string value, string column, int rowNumber)
    {
        if (decimal.TryParse(value, NumberStyles.Number | NumberStyles.AllowParentheses,
                CultureInfo.InvariantCulture, out var result)
            || decimal.TryParse(value, NumberStyles.Number | NumberStyles.AllowParentheses,
                CultureInfo.GetCultureInfo("pt-BR"), out result))
        {
            return result;
        }

        throw new InvalidDataException($"Invalid {column} amount on row {rowNumber}.");
    }

    private static DateOnly ParseDate(string value, string column, int rowNumber)
    {
        if (DateOnly.TryParseExact(value, "yyyyMMdd", CultureInfo.InvariantCulture,
                DateTimeStyles.None, out var compactDate)
            || DateOnly.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out compactDate))
        {
            return compactDate;
        }

        throw new InvalidDataException($"Invalid {column} date on row {rowNumber}.");
    }

    private static DateOnly? ParseOptionalDate(
        IXLRow row,
        IReadOnlyDictionary<string, int> columns,
        string name,
        int rowNumber)
    {
        var value = OptionalText(row, columns, name);
        return value is null ? null : ParseDate(value, name, rowNumber);
    }

    private static decimal? ParseOptionalDecimal(
        IXLRow row,
        IReadOnlyDictionary<string, int> columns,
        string name,
        int rowNumber)
    {
        var value = OptionalText(row, columns, name);
        return value is null ? null : ParseDecimal(value, name, rowNumber);
    }
}