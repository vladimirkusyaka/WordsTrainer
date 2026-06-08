namespace WordsTrainer.Contracts.Admin
{
    public sealed class ErrorLogListResponse
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public List<ErrorLogListItemResponse> Items { get; set; } = [];
    }
}
