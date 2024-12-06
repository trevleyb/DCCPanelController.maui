namespace DCCPanelController.Model.Tracks;

public static class Compass {
    // Step forward in the Compass
    public static CompassPoints Next(this CompassPoints compassPoints, int count = 1) {
        var values = (CompassPoints[])Enum.GetValues(compassPoints.GetType());
        var index = Array.IndexOf(values, compassPoints) + count;
        if (index >= values.Length) index -= values.Length;
        return values[index];
    }

    // Step backwards in the compass
    public static CompassPoints Prev(this CompassPoints compassPoints, int count = 1) {
        var values = (CompassPoints[])Enum.GetValues(compassPoints.GetType());
        var index = Array.IndexOf(values, compassPoints) - count;
        if (index < 0) index = values.Length + index;
        return values[index];
    }

    public static CompassPoints ToCompass(int rotation) {
        return ConvertFromDegress(rotation);
    }

    public static int ToRotation(this CompassPoints compassPoint) {
        return Degrees(compassPoint);
    }

    public static int Degrees(CompassPoints compassPoint) {
        return compassPoint switch {
            CompassPoints.North     => 0,
            CompassPoints.NorthEast => 45,
            CompassPoints.East      => 90,
            CompassPoints.SouthEast => 135,
            CompassPoints.South     => 180,
            CompassPoints.SouthWest => 225,
            CompassPoints.West      => 270,
            CompassPoints.NorthWest => 315,
            _                       => 0
        };
    }

    // Convert a degress to a compass point
    public static CompassPoints ConvertFromDegress(int degrees) {
        return degrees switch {
            >= 0 and < 45    => CompassPoints.North,
            >= 45 and < 90   => CompassPoints.NorthEast,
            >= 90 and < 135  => CompassPoints.East,
            >= 135 and < 180 => CompassPoints.SouthEast,
            >= 180 and < 225 => CompassPoints.South,
            >= 225 and < 270 => CompassPoints.SouthWest,
            >= 270 and < 315 => CompassPoints.West,
            >= 315 and < 360 => CompassPoints.NorthWest,
            _                => CompassPoints.North
        };
    }
}

public enum CompassPoints {
    North = 0,
    NorthEast = 1,
    East = 2,
    SouthEast = 3,
    South = 4,
    SouthWest = 5,
    West = 6,
    NorthWest = 7
}