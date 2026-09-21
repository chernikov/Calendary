using System.Net;
using Calendary.Common;
using Calendary.Domain.Abstractions;
using Calendary.Infrastructure.Options;
using Calendary.Infrastructure.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace Calendary.Infrastructure.Tests;

public class NovaPoshtaServiceTests
{
    // Replays a fixed queue of responses in call order — CreateShipmentAsync's happy path always
    // calls Counterparty/save then InternetDocument/save, in that order.
    private class QueuedHandler(params string[] responses) : HttpMessageHandler
    {
        private int _index;

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
        {
            var body = _index < responses.Length ? responses[_index] : responses[^1];
            _index++;
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(body, System.Text.Encoding.UTF8, "application/json"),
            });
        }
    }

    private class ThrowingHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct) =>
            throw new InvalidOperationException("No HTTP call should have been made.");
    }

    private static NovaPoshtaService CreateService(HttpMessageHandler handler, NovaPoshtaOptions options) =>
        new(new HttpClient(handler), Microsoft.Extensions.Options.Options.Create(options), NullLogger<NovaPoshtaService>.Instance);

    [Fact]
    public async Task Falls_back_to_a_fake_tracking_number_when_no_sender_is_configured()
    {
        var service = CreateService(new ThrowingHandler(), new NovaPoshtaOptions());
        var recipient = new NovaPoshtaShipmentRecipient("Андрій", "Черніков", "+380671234567", Guid.NewGuid(), Guid.NewGuid());

        var result = await service.CreateShipmentAsync(recipient);

        Assert.StartsWith("2040", result.TrackingNumber);
    }

    [Fact]
    public async Task Creates_a_real_shipment_via_Counterparty_save_then_InternetDocument_save()
    {
        // Real response shapes captured against the live API while researching #432.
        const string counterpartySaveResponse = """
            {"success":true,"data":[{"Ref":"92875ced-bf9d-11e6-8ba8-005056881c6b","CounterpartyType":"PrivatePerson",
            "ContactPerson":{"success":true,"data":[{"Ref":"01a0c339-6183-7327-aa74-56bf12efe28e"}],"errors":[]}}],"errors":[]}
            """;
        const string internetDocumentSaveResponse = """
            {"success":true,"data":[{"Ref":"01a0c339-a85c-7e09-8b62-9b98c5597e2b","CostOnSite":30,
            "EstimatedDeliveryDate":"22.09.2026","IntDocNumber":"20451541156738"}],"errors":[]}
            """;
        var options = new NovaPoshtaOptions
        {
            ApiKey = "test-key",
            SenderCounterpartyRef = "9282801b-bf9d-11e6-8ba8-005056881c6b",
            SenderContactRef = "771c9179-1c05-11ea-9937-005056881c6b",
            SenderCityRef = "db5c8904-391c-11dd-90d9-001a92567626",
            SenderWarehouseRef = "01ae2643-e1c2-11e3-8c4a-0050568002cf",
            SendersPhone = "380956035421",
        };
        var service = CreateService(new QueuedHandler(counterpartySaveResponse, internetDocumentSaveResponse), options);
        var recipient = new NovaPoshtaShipmentRecipient(
            "Андрій", "Черніков", "+380671234567",
            Guid.Parse("db5c8904-391c-11dd-90d9-001a92567626"), Guid.Parse("01ae2643-e1c2-11e3-8c4a-0050568002cf"));

        var result = await service.CreateShipmentAsync(recipient);

        Assert.Equal("20451541156738", result.TrackingNumber);
        Assert.Equal(30m, result.CostOnSite);
        Assert.Equal(new DateOnly(2026, 9, 22), result.EstimatedDeliveryDate);
    }

    [Fact]
    public async Task Throws_a_502_when_Counterparty_save_fails()
    {
        const string failure = """{"success":false,"data":[],"errors":["FirstName has invalid characters"]}""";
        var options = new NovaPoshtaOptions { ApiKey = "test-key", SenderCounterpartyRef = "9282801b-bf9d-11e6-8ba8-005056881c6b" };
        var service = CreateService(new QueuedHandler(failure), options);
        var recipient = new NovaPoshtaShipmentRecipient("Андрій", "Черніков", "+380671234567", Guid.NewGuid(), Guid.NewGuid());

        var ex = await Assert.ThrowsAsync<AppOperationException>(() => service.CreateShipmentAsync(recipient));
        Assert.Equal(502, ex.StatusCode);
    }

    [Fact]
    public async Task Throws_a_502_when_InternetDocument_save_fails()
    {
        const string counterpartySaveResponse = """
            {"success":true,"data":[{"Ref":"92875ced-bf9d-11e6-8ba8-005056881c6b",
            "ContactPerson":{"success":true,"data":[{"Ref":"01a0c339-6183-7327-aa74-56bf12efe28e"}],"errors":[]}}],"errors":[]}
            """;
        const string documentFailure = """{"success":false,"data":[],"errors":["SenderAddress not found"]}""";
        var options = new NovaPoshtaOptions { ApiKey = "test-key", SenderCounterpartyRef = "9282801b-bf9d-11e6-8ba8-005056881c6b" };
        var service = CreateService(new QueuedHandler(counterpartySaveResponse, documentFailure), options);
        var recipient = new NovaPoshtaShipmentRecipient("Андрій", "Черніков", "+380671234567", Guid.NewGuid(), Guid.NewGuid());

        var ex = await Assert.ThrowsAsync<AppOperationException>(() => service.CreateShipmentAsync(recipient));
        Assert.Equal(502, ex.StatusCode);
    }
}
