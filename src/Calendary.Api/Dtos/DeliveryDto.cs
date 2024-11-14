using Calendary.Core.Models;

namespace Calendary.Api.Dtos;

public class DeliveryDto
{
    public NovaPostApiResponseItem? City { get; set; }

    public NovaPostApiResponseItem? PostOffice { get; set; }
}
