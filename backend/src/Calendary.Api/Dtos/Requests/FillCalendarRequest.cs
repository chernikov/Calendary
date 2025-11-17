namespace Calendary.Api.Dtos.Requests
{
    public class FillCalendarRequest
    {
        public int FluxModelId { get; set; }
        public List<ImageDto> Images { get; set; } = [];
    }
}
