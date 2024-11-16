namespace Calendary.Api.Dtos.Results
{
    public class AdminOrderResult
    {
        public IReadOnlyCollection<AdminOrderDto> Orders { get; set; } = Array.Empty<AdminOrderDto>();

        public int Total { get; set; }

        public int Page { get; set; }

        public int PageSize { get; set; }
    }
}
