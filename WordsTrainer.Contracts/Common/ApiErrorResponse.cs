namespace WordsTrainer.Contracts.Common;

public sealed class ApiErrorResponse
{
    public string Code { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}
