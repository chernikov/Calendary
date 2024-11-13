using Calendary.Core.Senders.Models;
using System.Text.Json;

namespace Calendary.Core.Tests.Senders;

public class SmsClubResponseTests
{
    [Fact]
    public void DeserializeToValidObject_ResponseFromSmsClub()
    {
        // Arrange
        var str = "{\"success_request\":{\"info\":{\"1183911866\":\"380956035421\"}}}";


        // Act
        var responseData = JsonSerializer.Deserialize<SmsClubResponse>(str);


        // Assert
        Assert.NotNull(responseData);
        Assert.NotNull(responseData.SuccessRequest);

    }
}
