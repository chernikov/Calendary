using Calendary.Core.Providers;

namespace Calendary.Api.Providers;

public class WebPathProvider(IWebHostEnvironment webHostEnvironment) : IPathProvider
{
    public string MapPath(string relativePath)
    {
        // Перевіряємо, чи шлях є кореневим (починається з \ або /)
        if (Path.IsPathRooted(relativePath))
        {
            relativePath = relativePath.TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        }

        // Замінюємо всі '/' на коректний роздільник для поточної ОС
        var fixedPath = relativePath.Replace('/', Path.DirectorySeparatorChar);

        // Комбінуємо з кореневим шляхом wwwroot
        var path = Path.Combine(webHostEnvironment.WebRootPath, fixedPath);

        return path;
    }
}
