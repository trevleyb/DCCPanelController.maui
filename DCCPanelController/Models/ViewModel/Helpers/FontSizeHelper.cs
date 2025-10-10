namespace DCCPanelController.Models.ViewModel.Helpers;

// Map unit -> size (linear scale). u starts at 1.
public static class FontSizeHelper {
    
    private const float FontSizeStep = 5;
    private const float FontSizeMinimum  = 5;

    public static float UnitToSize(int u, int min = (int)FontSizeMinimum, int step = (int)FontSizeStep) => UnitToSize((float)u, (float)min, (float)step);
    public static float UnitToSize(float u, float min = FontSizeMinimum, float step = FontSizeStep) {
        if (step <= 0) step = FontSizeStep;
        if (u < 1) u = 1; 
        return min + (u - 1) * step;
    }

// (Optional) Inverse: size -> unit.
// Choose Floor (never exceed), Ceil (always at least), or Nearest.
    public static int SizeToUnit_Floor(float size, float min = FontSizeMinimum, float step = FontSizeStep) {
        if (step <= 0) step = FontSizeStep;
        var u = 1 + (size - min) / step;
        return Math.Max(1, (int)Math.Floor(u));
    }

    public static int SizeToUnit_Ceil(float size, float min = FontSizeMinimum, float step = FontSizeStep) {
        if (step <= 0) step = FontSizeStep;
        var u = 1 + (size - min) / step;
        return Math.Max(1, (int)Math.Ceiling(u));
    }

    public static int SizeToUnit_Nearest(float size, float min = FontSizeMinimum, float step = FontSizeStep) {
        if (step <= 0) step = FontSizeStep;
        var u = 1 + (size - min) / step;
        return Math.Max(1, (int)Math.Round(u));
    }
}