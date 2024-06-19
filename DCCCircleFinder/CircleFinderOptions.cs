using SixLabors.ImageSharp.PixelFormats;

namespace DCCCircleFinder;

public class CircleFinderOptions {
    /// <summary>
    /// What is the minimum Size that we expect a Circle to be.
    /// This must all be the same Inner Color 
    /// </summary>
    public int    MinRadius             { get; set; } = 20;
    
    /// <summary>
    /// What is the Maximum size w ewill search for. Smaller would make
    /// this faster as it needs to search less pixels.
    /// </summary>
    public int    MaxRadius             { get; set; } = 50;
    
    /// <summary>
    /// Once we find a circle, how far away from it would we expect
    /// the next one to be at a minimum? Radius of the circle * this number. 
    /// </summary>
    public int    FoundMoveMultiplier { get; set; } = 2;
    
    /// <summary>
    /// How many points around the boarder to we check to see if it is a circle.
    /// </summary>
    public int    CircleBorderThreshold     { get; set; } = 2;
    
    /// <summary>
    /// How many points of the compass do we check. Default is 90 whcih is
    /// every 4th one around the circle. 
    /// </summary>
    public int    CirclePointsToCheck        { get; set; } = 90;
    
    /// <summary>
    /// We will likely find multiple circles, how far apart do they need to be
    /// to determine that they are in fact the same found circle. 
    /// </summary>
    public int    CommonCircleThreshold   { get; set; } = 15;
    
    /// <summary>
    /// What color (White) should the circle we are searching for be?
    /// Note, if we convert to Black White then there are only 2 colors. 
    /// </summary>
    public Rgba32 CircleColor          { get; set; } = new Rgba32(255, 255, 255); // White color (R=255, G=255, B=255)
    
    /// <summary>
    /// What color (Black) should the border be.  
    /// Note, if we convert to Black White then there are only 2 colors. 
    /// </summary>
    public Rgba32 BorderColor         { get; set; } = new Rgba32(0, 0, 0);       // Black color (R=0, G=0, B=0)

    /// <summary>
    /// Default is true as this faster and more accurate
    /// </summary>
    public bool   ConvertToBlackWhite { get; set; } = true;
    
    /// <summary>
    /// Dump test data to the console.
    /// </summary>
    public bool   Debug               { get; set; } = true;
}