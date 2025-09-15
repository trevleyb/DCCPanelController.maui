namespace DCCPanelController.Models.DataModel.Helpers;

/// <summary>
///     Defines the connection points for an entity with 8-way rotation support
/// </summary>
public class EntityConnections {
    public const int MaxDirections = 8; // All entities support 8 compass directions

    private readonly ConnectionType[] _baseConnections;

    /// <summary>
    ///     Creates entity connections with 8-way rotation capability
    /// </summary>
    public EntityConnections(string connectionPattern, int rotations = 8) => _baseConnections = ParseConnectionPattern(connectionPattern);

    /// <summary>
    ///     Gets the step size for rotation (45° for 8-way rotation)
    /// </summary>
    public int RotationStepSize => 45;

    public string GetRotatedConnectionsStr(int rotation) => ConvertDirectionsToString(GetConnections(rotation));

    /// <summary>
    ///     Gets the connections for the entity at its current rotation
    /// </summary>
    /// <param name="rotation">Current rotation of the entity in degrees</param>
    /// <returns>Array of connection types for each compass direction</returns>
    public ConnectionType[] GetConnections(int rotation) {
        var normalizedRotation = NormalizeRotation(rotation);
        var rotationIndex = CalculateRotationIndex(normalizedRotation);

        //if (rotationIndex == 0) return(ConnectionType[])_baseConnections.Clone();
        var rotated = RotateConnections(_baseConnections, rotationIndex);
        //Console.WriteLine($"Rotated: {normalizedRotation}:{rotationIndex} -> {ConvertDirectionsToString(rotated)}");
        return rotated;
    }

    /// <summary>
    ///     Gets the connection type in a specific direction for the current rotation
    /// </summary>
    /// <param name="direction">Direction index (0=N, 1=NE, 2=E, etc.)</param>
    /// <param name="rotation">Current rotation of the entity in degrees</param>
    /// <returns>Connection type for that direction</returns>
    public ConnectionType GetConnection(int direction, int rotation) {
        var connections = GetConnections(rotation);
        return direction is>= 0 and< MaxDirections ? connections[direction] : ConnectionType.None;
    }

    /// <summary>
    ///     Gets all valid (non-None) connection directions for the current rotation
    /// </summary>
    /// <param name="rotation">Current rotation of the entity in degrees</param>
    /// <returns>List of direction indices that have valid connections</returns>
    public List<int> GetValidDirections(int rotation) {
        var connections = GetConnections(rotation);
        var validDirections = new List<int>();

        for (var i = 0; i < MaxDirections; i++) {
            if (connections[i] != ConnectionType.None) {
                validDirections.Add(i);
            }
        }

        return validDirections;
    }

    /// <summary>
    ///     Checks if the entity can rotate to the specified angle (all entities support 45° increments)
    /// </summary>
    /// <param name="rotation">Desired rotation in degrees</param>
    /// <returns>True if the rotation is valid</returns>
    public bool IsValidRotation(int rotation) {
        var normalizedRotation = NormalizeRotation(rotation);
        return normalizedRotation % 45 == 0;
    }

    private ConnectionType[] ParseConnectionPattern(string pattern) {
        if (string.IsNullOrEmpty(pattern)) {
            pattern = new string('*', MaxDirections);
        }

        // Ensure the pattern is exactly 8 characters
        if (pattern.Length < MaxDirections) {
            pattern = pattern.PadRight(MaxDirections, '*');
        } else if (pattern.Length > MaxDirections) {
            pattern = pattern.Substring(0, MaxDirections);
        }

        var connections = new ConnectionType[MaxDirections];
        for (var i = 0; i < MaxDirections; i++) {
            connections[i] = char.ToLower(pattern[i]) switch {
                't' => ConnectionType.Terminator,
                's' => ConnectionType.Straight,
                'p' => ConnectionType.Crossing,
                'd' => ConnectionType.Diverging,
                'c' => ConnectionType.Connector,
                'x' => ConnectionType.Closed,
                _   => ConnectionType.None,
            };
        }

        return connections;
    }

    private int NormalizeRotation(int rotation) {
        // Normalize rotation to 0-359 range
        while (rotation < 0) rotation += 360;
        return rotation % 360;
    }

    private int CalculateRotationIndex(int normalizedRotation) {
        // Calculate which 45° step this rotation represents
        var step = normalizedRotation / 45;
        return step % MaxDirections;
    }

    private ConnectionType[] RotateConnections(ConnectionType[] connections, int rotationIndex) {
        var rotated = new ConnectionType[MaxDirections];
        for (var i = 0; i < MaxDirections; i++) {
            rotated[(i + rotationIndex) % MaxDirections] = connections[i];
        }
        return rotated;
    }

    public override string ToString() => ConvertDirectionsToString(_baseConnections);

    public static (int dx, int dy) GetDirectionOffset(int direction) => direction switch {
        0 => (0, -1),  // N
        1 => (1, -1),  // NE
        2 => (1, 0),   // E
        3 => (1, 1),   // SE
        4 => (0, 1),   // S
        5 => (-1, 1),  // SW
        6 => (-1, 0),  // W
        7 => (-1, -1), // NW
        _ => (0, 0),
    };

    public string ConvertDirectionsToString(ConnectionType[] connections) {
        var result = string.Empty;
        for (var i = 0; i < MaxDirections; i++) {
            result += connections[i] switch {
                ConnectionType.Terminator => "T",
                ConnectionType.Straight   => "S",
                ConnectionType.Crossing   => "P",
                ConnectionType.Diverging  => "D",
                ConnectionType.Connector  => "C",
                ConnectionType.Closed     => "X",
                _                         => "*",
            };
        }
        return result;
    }

    // Static factory methods for common track types (based on E-W horizontal base orientation)
    public static class TrackPatterns {
        // Straight tracks - base orientation E-W
        public static EntityConnections StraightTrack => new("**S***S*");
        public static EntityConnections TerminatorTrack => new("**T***S*");
        public static EntityConnections CornerTrack => new("*S****S*");
        public static EntityConnections LeftTurnoutTrack => new("*DX***S*");
        public static EntityConnections RightTurnoutTrack => new("**XD**S*");
        public static EntityConnections LeftAngleTurnoutTrack => new("*DX***S*");
        public static EntityConnections RightAngleTurnoutTrack => new("SD**X***");
        public static EntityConnections CrossingTrack => new("S*P*S*P*");
        public static EntityConnections AngleCrossingTrack1 => new("**SP**SP");
        public static EntityConnections AngleCrossingTrack2 => new("SP**SP**");
    }
}

public enum ConnectionType {
    None,
    Terminator,
    Straight,
    Crossing,
    Closed,
    Diverging,
    Connector,
}