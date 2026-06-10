namespace WordsTrainer.Contracts.Auth
{
    public sealed class AuthMessageResponse
    {
        public string Code { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }
}
