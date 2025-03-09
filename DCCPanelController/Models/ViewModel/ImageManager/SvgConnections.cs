namespace DCCPanelController.Models.ViewModel.ImageManager;

public static class SvgConnections {
    public const int CompassPoints = 8;
    public static ConnectionType[] Connections(string directions, int rotation = 0) {
        var connectionTypes = new ConnectionType[CompassPoints];
        for (var i = 0; i < Math.Min(directions.Length, 8); ++i) {
            connectionTypes[i] = char.ToLower(directions[i]) switch {
                't' => ConnectionType.Terminator,
                's' => ConnectionType.Straight,
                'd' => ConnectionType.Diverging,
                'c' => ConnectionType.Connector,
                'x' => ConnectionType.Closed,
                _   => ConnectionType.None
            };
        }
        return RotateConnections(connectionTypes, rotation);
    }

    public static  ConnectionType[] NoConnections => Enumerable.Repeat(ConnectionType.None, CompassPoints).ToArray();
    private static ConnectionType[] RotateConnections(ConnectionType[] connections, SvgDirection direction) => RotateConnections(connections, (int)direction);
    private static ConnectionType[] RotateConnections(ConnectionType[] connections, int rotation = 0) {
        // Fix the rotation. If it is < 0 then inverse it (-90 = +270) and then work out what index 
        // the position would be. So a rotation of -90 is +270 which would be position 6. 
        // N(0)=0, NE(45)=1, E(90)=2, SE(135)=3, S(180)=4, SW(225)=5, W(270)=6, NW(315)=7, N(360)=0
        // ----------------------------------------------------------------------------------------
        if (rotation < 0) rotation = 360 + rotation;
        var rotationIndex = rotation / (360 / CompassPoints);
        if (rotationIndex >= CompassPoints) rotationIndex = 0;

        var rotatedConnections = new ConnectionType[CompassPoints];
        for (var i = 0; i < CompassPoints; i++) {
            rotatedConnections[(i + rotationIndex) % CompassPoints] = connections[i];
        }
        return rotatedConnections;
    }
}

public enum ConnectionType {
    None,
    Terminator,
    Straight,
    Closed,
    Diverging,
    Connector
}