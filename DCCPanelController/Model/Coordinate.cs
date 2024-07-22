using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;

namespace DCCPanelController.Model;

public partial class Coordinate : ObservableObject {

    public Coordinate() : this(0, 0, 1, 1, false) { }
    public Coordinate(int col, int row, int width, int height, bool? isValid = true) {
        Col = col;
        Row = row;
        Width = width;
        Height = height;
        IsValid = isValid ?? true;
    }

    [ObservableProperty] private int _col;
    [ObservableProperty] private int _row;
    [ObservableProperty] private int _width;
    [ObservableProperty] private int _height;

    [JsonIgnore]
    public bool IsValid { get; set; } = true;
    
    [JsonIgnore]
    public static Coordinate Unreferenced => new Coordinate(-1, -1, 1, 1, false);
    
    public override string ToString() => $"{Col},{Row}:{Width}x{Height}";
    
}