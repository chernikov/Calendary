using System.Net;
using Calendary.Domain.Abstractions;
using Calendary.Infrastructure.Options;
using Calendary.Infrastructure.Services;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Calendary.Infrastructure.Tests;

public class SmsClubServiceTests
{
    private class ThrowingHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct) =>
            throw new InvalidOperationException("No HTTP call should have been made.");
    }

    private class RecordingHandler : HttpMessageHandler
    {
        public bool Called;

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
        {
            Called = true;
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{}", System.Text.Encoding.UTF8, "application/json"),
            });
        }
    }

    private class FakeEnvironment(string name) : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = name;
        public string ApplicationName { get; set; } = "Calendary.Api";
        public string ContentRootPath { get; set; } = ".";
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }

    private class FakeAppSettingsService(bool realIntegrationsOnStaging) : IAppSettingsService
    {
        public Task<Domain.Enums.ImageGenerationProvider> GetImageGenerationProviderAsync(CancellationToken ct = default) =>
            throw new NotSupportedException();
        public Task SetImageGenerationProviderAsync(Domain.Enums.ImageGenerationProvider provider, CancellationToken ct = default) =>
            throw new NotSupportedException();
        public Task<decimal> GetBasePriceAsync(CancellationToken ct = default) => throw new NotSupportedException();
        public Task SetBasePriceAsync(decimal basePrice, CancellationToken ct = default) => throw new NotSupportedException();
        public Task<bool> GetRealIntegrationsOnStagingAsync(CancellationToken ct = default) => Task.FromResult(realIntegrationsOnStaging);
        public Task SetRealIntegrationsOnStagingAsync(bool enabled, CancellationToken ct = default) => throw new NotSupportedException();
    }

    private static SmsClubService CreateService(
        HttpMessageHandler handler, string apiKey, string environmentName, bool realIntegrationsOnStaging) =>
        new(
            new HttpClient(handler),
            Microsoft.Extensions.Options.Options.Create(new SmsClubOptions { ApiKey = apiKey }),
            new FakeAppSettingsService(realIntegrationsOnStaging),
            new FakeEnvironment(environmentName),
            NullLogger<SmsClubService>.Instance);

    [Fact]
    public async Task Returns_the_fixed_code_when_no_api_key_is_configured()
    {
        var service = CreateService(new ThrowingHandler(), apiKey: "", environmentName: "Production", realIntegrationsOnStaging: false);

        var code = await service.SendVerificationCodeAsync("+380671234567");

        Assert.Equal("0000", code);
    }

    [Fact]
    public async Task Returns_the_fixed_code_on_staging_even_when_configured_unless_the_toggle_is_on()
    {
        var service = CreateService(new ThrowingHandler(), apiKey: "real-key", environmentName: "Staging", realIntegrationsOnStaging: false);

        var code = await service.SendVerificationCodeAsync("+380671234567");

        Assert.Equal("0000", code);
    }

    [Fact]
    public async Task Sends_a_real_sms_on_production_when_configured()
    {
        var handler = new RecordingHandler();
        var service = CreateService(handler, apiKey: "real-key", environmentName: "Production", realIntegrationsOnStaging: false);

        var code = await service.SendVerificationCodeAsync("+380671234567");

        Assert.True(handler.Called);
        Assert.NotEqual("0000", code);
    }

    [Fact]
    public async Task Sends_a_real_sms_on_staging_when_the_toggle_is_on()
    {
        var handler = new RecordingHandler();
        var service = CreateService(handler, apiKey: "real-key", environmentName: "Staging", realIntegrationsOnStaging: true);

        var code = await service.SendVerificationCodeAsync("+380671234567");

        Assert.True(handler.Called);
        Assert.NotEqual("0000", code);
    }
}
