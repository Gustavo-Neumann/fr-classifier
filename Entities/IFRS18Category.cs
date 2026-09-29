namespace FrClassifier.Entities;

public enum IFRS18Category
{
    Operating,
    Investing,
    Financing,
    IncomeTaxes,
    DiscontinuedOperations
}

public enum ClassificationStatus
{
    Pending,
    Queued,
    Classified,
    Failed
}

public enum DocumentImportStatus
{
    Processing,
    Imported,
    Failed
}