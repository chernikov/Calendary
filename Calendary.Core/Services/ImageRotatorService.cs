using iText.IO.Image;
using System.Drawing.Imaging;
using System.Drawing;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Metadata.Profiles.Exif;

using ImagePdf = iText.Layout.Element.Image;
using ImageSh = SixLabors.ImageSharp.Image;

namespace Calendary.Core.Services;

public interface IImageRotatorService
{
    ImagePdf LoadCorrectedImage(string imagePath);
}

public class ImageRotatorService : IImageRotatorService
{
    public ImagePdf LoadCorrectedImage(string imagePath)
    {
        // Завантажуємо зображення за допомогою ImageSharp
        using (var image = ImageSh.Load(imagePath))
        {
            RotateImageIfNeeded(image); // Коригуємо орієнтацію за EXIF-даними

            // Зберігаємо зображення у пам'ять
            using (var memoryStream = new MemoryStream())
            {
                image.SaveAsJpeg(memoryStream); // Зберігаємо як JPEG
                var imageData = ImageDataFactory.Create(memoryStream.ToArray());
                return new ImagePdf(imageData);
            }
        }
    }

    private void RotateImageIfNeeded(ImageSh image)
    {
        // Отримуємо значення орієнтації з EXIF-даних

        var exifProfile = image.Metadata.ExifProfile;
        if (exifProfile is null)
        {
            return;
        }
        if (exifProfile.TryGetValue(ExifTag.Orientation, out var exifOrientation))
        {
            switch (exifOrientation.Value)
            {
                case 3:
                    image.Mutate(x => x.Rotate(180));
                    break;
                case 6:
                    image.Mutate(x => x.Rotate(90));
                    break;
                case 8:
                    image.Mutate(x => x.Rotate(270));
                    break;
            }
        }
    }
}
