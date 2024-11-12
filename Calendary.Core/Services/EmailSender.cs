﻿using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Calendary.Core.Services;

public interface IEmailSender
{
    Task<bool> SendEmailAsync(string fromEmail, string fromName, string toEmail, string subject, string plainTextContent, string htmlContent);
}


public class SendGridSender : IEmailSender
{
    private readonly string _apiKey;

    public SendGridSender(string apiKey)
    {
        _apiKey = apiKey;
    }

    public async Task<bool> SendEmailAsync(string fromEmail, string fromName, string toEmail, string subject, string plainTextContent, string htmlContent)
    {
        var client = new SendGridClient(_apiKey);
        var from = new EmailAddress(fromEmail, fromName);
        var to = new EmailAddress(toEmail);
        var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);

        var result = await client.SendEmailAsync(msg);
        return result.IsSuccessStatusCode;
    }
}