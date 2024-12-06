using DCCPanelController.Helpers;
using DCCPanelController.Model.Tracks;

namespace DCCPanelController.Tracks.ImageManager;

public class SvgCompass {
    public const int CompassPoints = 8;

    /// <summary>
    ///     Track what connections this particular component supports based on a Rotation of '0'
    /// </summary>
    /// <param name="directions">A string representing the compass Directions. Example, straight is = '**S***S*'</param>
    public SvgCompass(string directions) {
        for (var i = 0; i < Math.Min(directions.Length, 8); ++i) {
            ConnectionsArray[i] = char.ToLower(directions[i]) switch {
                't' => TrackConnectionsEnum.Terminator,
                's' => TrackConnectionsEnum.Straight,
                'd' => TrackConnectionsEnum.Diverging,
                'c' => TrackConnectionsEnum.Connector,
                'x' => TrackConnectionsEnum.Closed,
                _   => TrackConnectionsEnum.None
            };
        }
    }

    public SvgCompass() {
        ConnectionsArray = NoConnections();
    }

    private TrackConnectionsEnum[] ConnectionsArray { get; } = new TrackConnectionsEnum[8];

    public static TrackConnectionsEnum[] NoConnections() {
        return [
            TrackConnectionsEnum.None,
            TrackConnectionsEnum.None,
            TrackConnectionsEnum.None,
            TrackConnectionsEnum.None,
            TrackConnectionsEnum.None,
            TrackConnectionsEnum.None,
            TrackConnectionsEnum.None,
            TrackConnectionsEnum.None
        ];
    }

    public TrackConnectionsEnum[] ConnectionPointsRotated(int rotation = 0) {
        // Fix the rotation. If it is < 0 then inverse it (-90 = +270) and then work out what index 
        // the position would be. So a rotation of -90 is +270 which would be position 6. 
        // N(0)=0, NE(45)=1, E(90)=2, SE(135)=3, S(180)=4, SW(225)=5, W(270)=6, NW(315)=7, N(360)=0
        // ----------------------------------------------------------------------------------------
        if (rotation < 0) rotation = 360 + rotation;
        var rotationIndex = rotation / (360 / CompassPoints);
        if (rotationIndex >= CompassPoints) rotationIndex = 0;

        // Recalculate by rotating the Compass in he correct direction, and return the connection points 
        // for this image.
        // ----------------------------------------------------------------------------------------------
        var result = new TrackConnectionsEnum[CompassPoints];
        for (var i = 0; i < CompassPoints; ++i) {
            var newIndex = (i + rotationIndex) % CompassPoints;
            result[newIndex] = ConnectionsArray[i];
        }

        return result;
    }

    public TrackConnectionsEnum Connection(CompassPoints direction, int rotation = 0) {
        var connections = ConnectionPointsRotated(rotation);
        return connections[(int)direction];
    }
}

public enum TrackConnectionsEnum {
    None = 'N',
    Terminator = 'T',
    Straight = 'S',
    Closed = 'X',
    Diverging = 'D',
    Connector = 'C'
}