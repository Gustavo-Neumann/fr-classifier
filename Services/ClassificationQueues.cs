namespace FrClassifier.Services;

public static class ClassificationQueues
{
    public const string Request = "financial.classification.requests";
    public const string Result = "financial.classification.results";
    public const string ResultDeadLetterExchange = "financial.classification.dead";
    public const string ResultDeadLetterQueue = "financial.classification.results.dead";
}