using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;

namespace DCCPanelController.Model;

public partial class Coordinate : ObservableObject {

    public Coordinate() {}
    public Coordinate(int col, int row, bool? isValid = true) {
        Col = col;
        Row = row;
        IsValid = isValid ?? true;
    }

    [ObservableProperty] private int _col;
    [ObservableProperty] private int _row;

    [JsonIgnore]
    public bool IsValid { get; set; } = true;

    [JsonIgnore]
    public static Coordinate Unreferenced => new Coordinate(-1, -1, false);
    
    public override string ToString() => $"{Col},{Row}";
    
}