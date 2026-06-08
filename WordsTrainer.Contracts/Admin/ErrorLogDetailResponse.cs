namespace WordsTrainer.Contracts.Admin
{
    public sealed class ErrorLogDetailResponse
    {
        public Guid Id { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public string Level { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string ExceptionType { get; set; } = string.Empty;
        public string? StackTrace { get; set; }
        public string? RequestMethod { get; set; }
        public string? RequestPath { get; set; }
        public string? QueryString { get; set; }
        public string? UserId { get; set; }
        public string? RemoteIp { get; set; }
        public string? UserAgent { get; set; }
        public string? TraceId { get; set; }
    }
}
