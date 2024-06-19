using System.Diagnostics;

namespace DCCCircleFinder;

[DebuggerDisplay("X={X} Y={Y} Radius={Radius}")]
public class Circle {
    public int X      { get; set; }
    public int Y      { get; set; }
    public int Radius { get; set; }
}