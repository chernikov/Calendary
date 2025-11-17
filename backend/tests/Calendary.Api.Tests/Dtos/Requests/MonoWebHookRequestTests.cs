using Calendary.Api.Dtos.Requests;
using System.Text.Json;

namespace Calendary.Api.Tests.Dtos.Requests;

public class MonoWebHookRequestTests
{
    [Fact]
    public void Parse_String_Valid_MonoWebHookRequest() 
    {

        var str = "{\"invoiceId\":\"2411156oa2SZnM6Kv3ix\",\"status\":\"success\",\"payMethod\":\"monobank\",\"amount\":50000,\"ccy\":980,\"finalAmount\":50000,\"createdDate\":\"2024-11-15T08:31:11Z\",\"modifiedDate\":\"2024-11-15T08:31:30Z\",\"paymentInfo\":{\"rrn\":\"073798071705\",\"approvalCode\":\"694737\",\"tranId\":\"424727855441\",\"terminal\":\"MI000000\",\"bank\":\"Універсал Банк\",\"paymentSystem\":\"visa\",\"country\":\"804\",\"fee\":650,\"paymentMethod\":\"monobank\",\"maskedPan\":\"44411144******55\"}}";

        var request = JsonSerializer.Deserialize<MonoWebHookRequest>(str);

        Assert.NotNull(request);
        Assert.Equal("2411156oa2SZnM6Kv3ix", request.InvoiceId);
        Assert.Equal("success", request.Status);
        Assert.Equal("monobank", request.PayMethod);
        Assert.Equal(50000, request.Amount);
        Assert.Equal(980, request.Ccy);
        Assert.Equal(50000, request.FinalAmount);
        Assert.Equal(new DateTime(2024, 11, 15, 8, 31, 11), request.CreatedDate);
        Assert.Equal(new DateTime(2024, 11, 15, 8, 31, 30), request.ModifiedDate);
        Assert.NotNull(request.PaymentInfo);
        Assert.Equal("073798071705", request.PaymentInfo.Rrn);
        Assert.Equal("694737", request.PaymentInfo.ApprovalCode);
        Assert.Equal("424727855441", request.PaymentInfo.TranId);
        Assert.Equal("MI000000", request.PaymentInfo.Terminal);
        Assert.Equal("Універсал Банк", request.PaymentInfo.Bank);
        Assert.Equal("visa", request.PaymentInfo.PaymentSystem);
        Assert.Equal("804", request.PaymentInfo.Country);
        Assert.Equal(650, request.PaymentInfo.Fee);
    }
}
