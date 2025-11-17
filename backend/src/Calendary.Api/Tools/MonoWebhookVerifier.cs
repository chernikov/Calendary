using System.Security.Cryptography;
using System.Text;

namespace Calendary.Api.Tools;

public static class MonoWebhookVerifier
{
    public static bool VerifyWebhook(string xSign, string pubKeyBase64, string payload)
    {
        try
        {
            // Розшифровуємо публічний ключ з Base64
            byte[] pubKeyBytes = Convert.FromBase64String(pubKeyBase64);

            // Розшифровуємо X-Sign з Base64
            byte[] signatureBytes = Convert.FromBase64String(xSign);

            // Конвертуємо пейлоад в байти
            byte[] payloadBytes = Encoding.UTF8.GetBytes(payload);

            // Створюємо ECDSA-провайдер
            using var ecdsa = ECDsa.Create();
            ecdsa.ImportSubjectPublicKeyInfo(pubKeyBytes, out _);

            // Перевіряємо підпис
            bool isValid = ecdsa.VerifyData(payloadBytes, signatureBytes, HashAlgorithmName.SHA256);

            return isValid;
        }
        catch (Exception ex)
        {
            // Логування помилки (опціонально)
            Console.WriteLine($"Помилка верифікації: {ex.Message}");
            return false;
        }
    }
}
