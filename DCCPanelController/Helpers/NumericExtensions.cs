namespace DCCPanelController.Helpers;

public static class NumericExtensions {
    public static bool IsEqualTo(this double value, double other, double tolerance = 1e-9) => Math.Abs(value - other) < tolerance;

    public static bool IsEqualTo(this float value, float other, float tolerance = 1e-6f) => Math.Abs(value - other) < tolerance;

    public static bool IsEqualTo(this int value, int other, float tolerance = 0) => value == other;
}