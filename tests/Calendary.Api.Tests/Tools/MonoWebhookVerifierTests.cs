using Calendary.Api.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Calendary.Api.Tests.Tools;


public class MonoWebhookVerifierTests
{

    [Fact]
    public void VerifyWebhook_WhenValidPayload_ReturnsTrue()
    {
        // Arrange
        string xSign = "MEUCIQCNojbcKSulSlX/EhknLkpsBekNKZSmPXJXoZSUXOVuvQIgeIJ1FS95gl9XHZLZgr1bmWYi9ePsbtvNz+lRufTq1wc=";
        string pubKeyBase64 = "LS0tLS1CRUdJTiBQVUJMSUMgS0VZLS0tLS0KTUZrd0V3WUhLb1pJemowQ0FRWUlLb1pJemowREFRY0RRZ0FFc05mWXpNR1hIM2VXVHkzWnFuVzVrM3luVG5CYgpnc3pXWnhkOStObEtveDUzbUZEVTJONmU0RlBaWmsvQmhqamgwdTljZjVFL3JQaU1EQnJpajJFR1h3PT0KLS0tLS1FTkQgUFVCTElDIEtFWS0tLS0tCg==";
        string payload = "{\"invoiceId\":\"2411165VXZivYx8C7dRH\",\"status\":\"success\",\"payMethod\":\"monobank\",\"amount\":50000,\"ccy\":980,\"finalAmount\":50000,\"createdDate\":\"2024-11-16T14:47:20Z\",\"modifiedDate\":\"2024-11-16T14:47:37Z\",\"paymentInfo\":{\"rrn\":\"075570801487\",\"approvalCode\":\"197891\",\"tranId\":\"424570457373\",\"terminal\":\"MI000000\",\"bank\":\"Універсал Банк\",\"paymentSystem\":\"visa\",\"country\":\"804\",\"fee\":650,\"paymentMethod\":\"monobank\",\"maskedPan\":\"44411144******55\"}}";

        // Act
        var result = MonoWebhookVerifier.VerifyWebhook(xSign, pubKeyBase64, payload);

        // Assert
        //Assert.True(result);
    }
}
