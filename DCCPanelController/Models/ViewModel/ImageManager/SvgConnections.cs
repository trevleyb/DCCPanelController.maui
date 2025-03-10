using Math = System.Math;

namespace DCCPanelController.Models.ViewModel.ImageManager;

public class SvgConnections {
    public const int CompassPoints = 8;
    public ConnectionType[] Connections;

    public SvgConnections(string directions, int rotation = 0) {
        Connections = BuildConnections(directions, rotation);
    }

    public SvgConnections(SvgConnections connections, int rotation = 0) {
        Connections = RotateConnections(connections.Connections, rotation);
    }

    public override string ToString() {
        var result = string.Empty;
        for (var i = 0; i < CompassPoints; ++i) {
            result += Connections[i] switch {
                ConnectionType.Terminator => 'T',
                ConnectionType.Straight   => 'S',
                ConnectionType.Diverging  => 'D',
                ConnectionType.Connector  => 'C',
                ConnectionType.Closed     => 'X',
                _                         => '*'
            };
        }
        return result ?? "UNKNOWN";
    }

    private ConnectionType[] BuildConnections(string directions, int rotation = 0) {
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

    public static SvgConnections NoConnections => new SvgConnections("********");
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