namespace DCCPanelController.Components.TrackImages;

public class TrackConnections {
    /// <summary>
    /// Track what connections this particular component supports based on a Rotation of '0'
    /// </summary>
    /// <param name="directions">A string representing the compass Directions. Example, straight is = '**S***S*'</param>
    public TrackConnections(string directions) {
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
    
    private TrackConnectionsEnum[] ConnectionsArray { get; } = new TrackConnectionsEnum[8];
    public TrackConnectionsEnum[]  ConnectionPointsRotated(int rotation = 0) {

        var rotationIndex = rotation switch {
            >= 0 and < 90    => 0,
            >= 90 and < 180  => 2,
            >= 180 and < 270 => 4,
            >= 270 and < 360 => 6,
            >= 360            => 0,
            _                 => 0
        };
        
        var result = new TrackConnectionsEnum[8];
        for (var i = 0; i < 8; ++i) {
            var newIndex = (i + rotationIndex) % 8;
            result[newIndex] = ConnectionsArray[i];
        }
        return result;
    }

    public TrackConnectionsEnum Connection(TrackDirectionEnum direction, int rotation = 0) {
        var connections = ConnectionPointsRotated(rotation);
        return connections[(int)direction];
    }
}

public enum TrackConnectionsEnum {
    None        = 'N',
    Terminator  = 'T', 
    Straight    = 'S',
    Closed      = 'X',
    Diverging   = 'D',
    Connector   = 'C'
}

public enum TrackDirectionEnum {
    North       = 0, 
    NorthEast   = 1, 
    East        = 2, 
    SouthEast   = 3, 
    South       = 4, 
    SouthWest   = 5, 
    West        = 6, 
    NorthWest   = 7
}
