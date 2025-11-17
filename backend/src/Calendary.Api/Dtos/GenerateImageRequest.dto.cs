namespace Calendary.Api.Dtos;

public class GenerateImageRequest
{
    public string Prompt { get; set; }
    public string ModelVersion { get; set; }
    public int Width { get; set; } = 1024;
    public int Height { get; set; } = 1024;
    public int Seed { get; set; } = -1;
}
