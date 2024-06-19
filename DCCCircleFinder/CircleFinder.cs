using System.Collections.Concurrent;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Image = SixLabors.ImageSharp.Image;

namespace DCCCircleFinder;

public class CircleFinder (CircleFinderOptions options) {
   
    /// <summary>
    /// Find an image by loading it first from Disk and processing the circles. 
    /// </summary>
    /// <param name="fileName">The name (fulkly qualified) image to load.</param>
    /// <returns>A collection of the offsets and radius of found circles.</returns>
    public List<Circle> Find(string fileName) {
        using var image = Image.Load<Rgba32>(fileName);
        return Find(fileName, image);
    }
    
    /// <summary>
    /// Process an image from a Base64 string and find the circles.
    /// </summary>
    /// <param name="imageName">The name of the image</param>
    /// <param name="base64String">The base64 representation</param>
    /// <returns>A collection of the offsets and radius of found circles.</returns>
    public List<Circle> Find(string imageName, string base64String) {
        var image = CircleHelpers.ImageFromBase64(base64String);
        return Find(imageName, image);
    }

    /// <summary>
    /// Process an image and find the circles.
    /// </summary>
    /// <param name="imageName">The name of the image</param>
    /// <param name="original">The image as an image format</param>
    /// <returns>A collection of the offsets and radius of found circles.</returns>
    public List<Circle> Find(string imageName, Image original) {
        var start = DateTime.Now;
        var image = original.CloneAs<Rgba32>();
        if (options.ConvertToBlackWhite) image.Mutate(ctx => ctx.BlackWhite());
        var foundCircles  = FindAndProcessCircles(image);
        var uniqueCircles = FindUniqueCircles(foundCircles, options.CommonCircleThreshold);
        var finish  = DateTime.Now;

        if (options.Debug) WriteDebugInfo(imageName, image, uniqueCircles, finish, start);
        return uniqueCircles;
    }

    /// <summary>
    /// Debugging output. 
    /// </summary>
    private void WriteDebugInfo(string imageName, Image<Rgba32> image, List<Circle> foundCircles, DateTime finish, DateTime start) {
        var timeTaken   = (finish - start).TotalMilliseconds / 1000;
        var displayName = System.IO.Path.GetFileName(imageName);
        Console.WriteLine($"{displayName} : Found {foundCircles.Count} circles in {timeTaken:F3}.");

        // Save or display the modified image
        foreach (var circle in foundCircles) {
            image.Mutate(ctx => ctx.Draw(Color.Red, 5, new EllipsePolygon(circle.X, circle.Y, circle.Radius)));
        }
        var fileName      = System.IO.Path.GetFileName(imageName);
        var extension     = System.IO.Path.GetExtension(imageName);
        var markedUpImage = $"{fileName}_circles{extension}";
        image.Save(markedUpImage); // Save the processed image
    }

    /// <summary>
    /// Search through an image and look for potential circles. This will possibly
    /// find more than we expect, but we collapse them later to find the unique ones.
    ///  
    /// </summary>
    /// <param name="image">The image, as a Rgba32 format, to process</param>
    /// <returns></returns>
    private List<Circle> FindAndProcessCircles(Image<Rgba32> image) {

        var circles             = new Stack<Circle>();
        
        // Loop through each pixel. When we find a pixel that is WHITE, we look UP until we find a black
        // pixel, but only if it is within the confines of the minSize and maxSize. If we find a black pixel
        // then we will look around as if a circle had been drawn and see if there is a circle. 

        // We will also assume that Cirlces are even on the Horizontal and Vertical axis, so we will adjust
        // ourselves by this at the end of each loop to speed up processing. 
        Parallel.For(options.MinRadius, image.Height, y => {
            for (var x = options.MinRadius; x < image.Width; x++) {
            //Parallel.For(options.MinRadius, image.Width, x => {

                // Found a white pixel, then see if it is a Circle.
                // -----------------------------------------------------------------------------
                if (IsPixelOfColor(image, x, y, options.CircleColor)) {
                    var radius = FindCircle(image, x, y);
                    if (radius != 0) {
                        circles.Push(new Circle { X = x, Y = y, Radius = radius });
                        x += radius * options.FoundMoveMultiplier; // move along by the radius to speed up processing
                    }
                }
            }
        });
        return circles.ToList();
    }

    // Find Circle does the following. 
    // It looks UP until it finds a black pixel making sure that all pixels up to minSize are white
    // and that it finds a Black pixel within Min and Max. If it does find a black pixel, then it will
    // look in a circle around the pixel to see if it is a circle. It will then return the radius of 
    // the circle it has found. 
    // ----------------------------------------------------------------------------------------------------
    private int FindCircle(Image<Rgba32> image, int x, int y){

        // Look through the 4 compass points to see if we have found a circle.
        // ------------------------------------------------------------------
        for (var i = 0; i < options.MinRadius; i++) {
            if (!IsPixelOfColor(image, x, y - i, options.CircleColor)) return 0;
            if (!IsPixelOfColor(image, x, y + i, options.CircleColor)) return 0;
            if (!IsPixelOfColor(image, x - i, y, options.CircleColor)) return 0;
            if (!IsPixelOfColor(image, x + i, y, options.CircleColor)) return 0;
        }

        // Continue looking UP until we either get to maxSize or find a black pixel. Any other color
        // is a no and we can abort. But note that we need to take into account the threshold as the edges
        // might be slightly different colors. 
        // ------------------------------------------------------------------------------------------
        var outerRadius = 0;
        //var innerRadius = 0;
        for (var i = options.MinRadius; i < options.MaxRadius; i++) {
            if (y - i < 0 || x - i < 0 || y + i > image.Height || x + i > image.Width) return 0; // Out of bounds
            if ((IsPixelOfColor(image, x, y - i, options.BorderColor)) && 
                (IsPixelOfColor(image, x, y + i, options.BorderColor)) &&
                (IsPixelOfColor(image, x - i, y, options.BorderColor)) && 
                (IsPixelOfColor(image, x + i, y, options.BorderColor))) {
                if (y - (i + options.CircleBorderThreshold) < 0) return 0; // Gone out of bounds so not found.
                outerRadius = i;                                           // + options.CircleBorderThreshold;
                //innerRadius = i - options.CircleBorderThreshold;
                break;
            }
        }
        if (outerRadius == 0) return 0;
        if (x-outerRadius < 0 || x+outerRadius >= image.Width || y-outerRadius < 0 || y+outerRadius >= image.Height) return 0; // Out of bounds
        return SearchCirclePointsNew(image, x, y, outerRadius, options.CirclePointsToCheck);
    }

    private int IsPixelWithinFoundCircle(ConcurrentBag<Circle> circles, int x, int y, int circleThreshold) {
        // Given X,Y coordinate, look in our collection to see if we already 
        // found this circle. If we did, then return the Radius that we found, 
        // otherwise return 0;
        return (from circle in circles where Math.Sqrt(Math.Pow(circle.X - x, 2) + Math.Pow(circle.Y - y, 2)) <= circleThreshold select circle.Radius).FirstOrDefault();
    }

    private bool IsPixelOfColor(Image<Rgba32> image, int x, int y, Rgba32 color) {
        if (x < 0 || x >= image.Width || y < 0 || y >= image.Height) return false;
        var pixel = image[x, y];
        return pixel.R == color.R && pixel.G == color.G && pixel.B == color.B;
    }

    private int SearchCirclePointsNew(Image<Rgba32> image, int cx, int cy, int radius, int numPoints) {
        for (var k = 0; k < numPoints; k++) {
            var theta    = (2 * Math.PI * k) / numPoints;
            var cosTheta = Math.Cos(theta);
            var sinTheta = Math.Sin(theta);
            var x        = (int)(cx + radius * cosTheta);
            var y        = (int)(cy + radius * sinTheta);

            // if the pixel in the circle IS the border color (black) 
            // or the surrounding ones are black within the threshold
            // then we are OK and keep looking. 
            // if it is NOT then we should return false
            // -----------------------------------------------------
            if (!(IsPixelOfColor(image, x, y, options.BorderColor) || 
                   IsPixelOfColor(image, x + options.CircleBorderThreshold, y, options.BorderColor) || 
                   IsPixelOfColor(image, x - options.CircleBorderThreshold, y, options.BorderColor) || 
                   IsPixelOfColor(image, x, y + options.CircleBorderThreshold, options.BorderColor) || 
                   IsPixelOfColor(image, x, y - options.CircleBorderThreshold, options.BorderColor)))  {
                return 0;
            }
        }
        return radius;
    }

    private int SearchCirclePoints(Image<Rgba32> image, int cx, int cy, int outerRadius, int innerRadius, int numPoints) {
        for (var k = 0; k < numPoints; k++) {
            var theta    = (2 * Math.PI * k) / numPoints;
            var cosTheta = Math.Cos(theta);
            var sinTheta = Math.Sin(theta);
            var x        = (int)(cx + outerRadius * cosTheta);
            var y        = (int)(cy + outerRadius * sinTheta);
            var a        = (int)(cx + innerRadius * cosTheta);
            var b        = (int)(cy + innerRadius * sinTheta);

            // if the pixel in the circle IS the border color (black) 
            // or the surrounding ones are black within the threshold
            // the we are OK and keep looking. 
            // if it is NOT then we should return false
            // -----------------------------------------------------
            if (!((IsPixelOfColor(image, x, y, options.BorderColor) || IsPixelOfColor(image, x + options.CircleBorderThreshold, y, options.BorderColor) || IsPixelOfColor(image, x - options.CircleBorderThreshold, y, options.BorderColor) || IsPixelOfColor(image, x, y + options.CircleBorderThreshold, options.BorderColor) || IsPixelOfColor(image, x, y - options.CircleBorderThreshold, options.BorderColor)) && (IsPixelOfColor(image, a, b, options.BorderColor) || IsPixelOfColor(image, a + options.CircleBorderThreshold, b, options.BorderColor) || IsPixelOfColor(image, a - options.CircleBorderThreshold, b, options.BorderColor) || IsPixelOfColor(image, a, b + options.CircleBorderThreshold, options.BorderColor) || IsPixelOfColor(image, a, b - options.CircleBorderThreshold, options.BorderColor)))) {
                return 0;
            }
        }
        return outerRadius;
    }

    
    private List<Circle> FindUniqueCircles(List<Circle> circles, int threshold) {
        var collapsedCircles = new List<Circle>();

        foreach (var circle in circles) {
            var closeCircle = collapsedCircles.FirstOrDefault(c => Math.Sqrt(Math.Pow(c.X - circle.X, 2) + Math.Pow(c.Y - circle.Y, 2)) <= threshold);
            if (closeCircle != null) {
                closeCircle.X      = (closeCircle.X + circle.X) / 2;
                closeCircle.Y      = (closeCircle.Y + circle.Y) / 2;
                closeCircle.Radius = (closeCircle.Radius + circle.Radius) / 2;
            }
            else {
                collapsedCircles.Add(new Circle { X = circle.X, Y = circle.Y, Radius = circle.Radius });
            }
        }
        return collapsedCircles;
    }
}