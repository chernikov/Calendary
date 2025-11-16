using Calendary.Core.Providers;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;

namespace Calendary.Core.Services;

public interface IImageOptimizer
{
    /// <summary>
    /// Оптимізує зображення для використання в PDF
    /// </summary>
    /// <param name="imagePath">Шлях до вхідного зображення</param>
    /// <param name="maxWidth">Максимальна ширина (за замовчуванням 2480px для A4 @ 300 DPI)</param>
    /// <param name="quality">Якість JPEG (1-100, за замовчуванням 85)</param>
    /// <returns>Шлях до оптимізованого зображення</returns>
    Task<string> OptimizeForPdfAsync(string imagePath, int maxWidth = 2480, int quality = 85);

    /// <summary>
    /// Очищає кеш оптимізованих зображень
    /// </summary>
    void ClearCache();

    /// <summary>
    /// Видаляє конкретне оптимізоване зображення з кешу
    /// </summary>
    /// <param name="imagePath">Шлях до оригінального зображення</param>
    void RemoveFromCache(string imagePath);
}

public class ImageOptimizer(IPathProvider pathProvider) : IImageOptimizer
{
    private const string OptimizedCacheFolder = "uploads/optimized";
    private const int DefaultMaxWidth = 2480; // A4 @ 300 DPI
    private const int DefaultQuality = 85;

    public async Task<string> OptimizeForPdfAsync(string imagePath, int maxWidth = DefaultMaxWidth, int quality = DefaultQuality)
    {
        // Перевірка параметрів
        if (string.IsNullOrWhiteSpace(imagePath))
            throw new ArgumentException("Image path cannot be null or empty", nameof(imagePath));

        if (maxWidth <= 0)
            throw new ArgumentException("Max width must be positive", nameof(maxWidth));

        if (quality < 1 || quality > 100)
            throw new ArgumentException("Quality must be between 1 and 100", nameof(quality));

        var fullPath = pathProvider.MapPath(imagePath);

        if (!File.Exists(fullPath))
            throw new FileNotFoundException($"Image file not found: {fullPath}");

        // Генеруємо шлях для оптимізованого зображення
        var optimizedPath = GetOptimizedPath(imagePath, maxWidth, quality);
        var fullOptimizedPath = pathProvider.MapPath(optimizedPath);

        // Якщо оптимізоване зображення вже існує і новіше за оригінал, повертаємо його
        if (File.Exists(fullOptimizedPath))
        {
            var originalModifiedTime = File.GetLastWriteTimeUtc(fullPath);
            var optimizedModifiedTime = File.GetLastWriteTimeUtc(fullOptimizedPath);

            if (optimizedModifiedTime >= originalModifiedTime)
            {
                return optimizedPath;
            }
        }

        // Створюємо директорію для оптимізованих зображень якщо не існує
        var optimizedDirectory = Path.GetDirectoryName(fullOptimizedPath);
        if (!string.IsNullOrEmpty(optimizedDirectory) && !Directory.Exists(optimizedDirectory))
        {
            Directory.CreateDirectory(optimizedDirectory);
        }

        // Завантажуємо та оптимізуємо зображення
        using (var image = await Image.LoadAsync(fullPath))
        {
            var originalWidth = image.Width;
            var originalHeight = image.Height;

            // Якщо зображення більше за maxWidth, зменшуємо його
            if (originalWidth > maxWidth)
            {
                var ratio = (double)maxWidth / originalWidth;
                var newHeight = (int)(originalHeight * ratio);

                image.Mutate(x => x.Resize(maxWidth, newHeight));
            }

            // Налаштування для JPEG стиснення
            var encoder = new JpegEncoder
            {
                Quality = quality
            };

            // Зберігаємо оптимізоване зображення
            await image.SaveAsync(fullOptimizedPath, encoder);
        }

        // Логування розміру файлів
        var originalSize = new FileInfo(fullPath).Length;
        var optimizedSize = new FileInfo(fullOptimizedPath).Length;
        var compressionRatio = (1 - (double)optimizedSize / originalSize) * 100;

        Console.WriteLine($"Image optimized: {Path.GetFileName(imagePath)}");
        Console.WriteLine($"  Original size: {FormatBytes(originalSize)}");
        Console.WriteLine($"  Optimized size: {FormatBytes(optimizedSize)}");
        Console.WriteLine($"  Compression: {compressionRatio:F1}%");

        return optimizedPath;
    }

    public void ClearCache()
    {
        var cacheDirectory = pathProvider.MapPath(OptimizedCacheFolder);

        if (Directory.Exists(cacheDirectory))
        {
            try
            {
                Directory.Delete(cacheDirectory, true);
                Directory.CreateDirectory(cacheDirectory);
                Console.WriteLine($"Image cache cleared: {cacheDirectory}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error clearing cache: {ex.Message}");
            }
        }
    }

    public void RemoveFromCache(string imagePath)
    {
        if (string.IsNullOrWhiteSpace(imagePath))
            return;

        // Шукаємо всі варіанти оптимізованих версій цього зображення
        var pattern = GetCacheFilePattern(imagePath);
        var cacheDirectory = pathProvider.MapPath(OptimizedCacheFolder);

        if (!Directory.Exists(cacheDirectory))
            return;

        var files = Directory.GetFiles(cacheDirectory, pattern);
        foreach (var file in files)
        {
            try
            {
                File.Delete(file);
                Console.WriteLine($"Removed from cache: {Path.GetFileName(file)}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error removing file from cache: {ex.Message}");
            }
        }
    }

    private string GetOptimizedPath(string imagePath, int maxWidth, int quality)
    {
        var fileName = Path.GetFileNameWithoutExtension(imagePath);
        var extension = Path.GetExtension(imagePath);

        // Генеруємо ім'я файлу з параметрами оптимізації
        var optimizedFileName = $"{fileName}_opt_{maxWidth}_{quality}.jpg"; // Завжди зберігаємо як JPEG

        return Path.Combine(OptimizedCacheFolder, optimizedFileName);
    }

    private string GetCacheFilePattern(string imagePath)
    {
        var fileName = Path.GetFileNameWithoutExtension(imagePath);
        return $"{fileName}_opt_*.jpg";
    }

    private static string FormatBytes(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB" };
        double len = bytes;
        int order = 0;

        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len = len / 1024;
        }

        return $"{len:0.##} {sizes[order]}";
    }
}
