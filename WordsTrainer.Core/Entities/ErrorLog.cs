using System;

namespace WordsTrainer.Core.Entities
{
    public class ErrorLog
    {
        public Guid Id { get; set; }
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

        public string Level { get; set; } = "Error";
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
