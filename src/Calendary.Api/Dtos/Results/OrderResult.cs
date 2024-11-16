namespace Calendary.Api.Dtos.Results
{
    public class OrderResult
    {
        public IReadOnlyCollection<OrderDto> Orders { get; set; } = Array.Empty<OrderDto>();

        public int Total { get; set; }

        public int Page { get; set; }

        public int PageSize { get; set; }
    }
}
