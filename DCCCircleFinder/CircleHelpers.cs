using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using Image = SixLabors.ImageSharp.Image;

namespace DCCCircleFinder;

public static class CircleHelpers {
    
    public static string JpgToBase64(Image image) => ImageToBase64(image, new JpegEncoder()); 
    public static string PngToBase64(Image image) => ImageToBase64(image, new PngEncoder()); 
    public static string GifToBase64(Image image) => ImageToBase64(image, new GifEncoder()); 
    public static string ImageToBase64(Image image, IImageEncoder encoder) {
        using var ms = new MemoryStream();
        image.Save(ms, encoder);
        var imageBytes   = ms.ToArray();
        var base64String = Convert.ToBase64String(imageBytes);
        return base64String;
    }

    public static string ImageToBase64(Image image) {
        using var ms = new MemoryStream();
        image.Save(ms, new PngEncoder());
        var imageBytes   = ms.ToArray();
        var base64String = Convert.ToBase64String(imageBytes);
        return base64String;
    }

    public static Image ImageFromBase64(string base64String) {
        var imageBytes = Convert.FromBase64String(base64String);
        var ms         = new MemoryStream(imageBytes, 0, imageBytes.Length);
        ms.Write(imageBytes, 0, imageBytes.Length);
        var image = Image.Load(ms);
        return image;
    }
}