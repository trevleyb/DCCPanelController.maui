using System.Collections.ObjectModel;
using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Helpers;
using DCCPanelController.Model.Elements;
using DCCPanelController.Model.Elements.Base;
using DCCPanelController.Tracks.Base;

namespace DCCPanelController.Model;

/// <summary>
/// Represents a Panel or Schematic that we can display on the app to control
/// </summary>
public partial class Panel : ObservableValidator, ICloneable {
    
    [ObservableProperty] private string _id = "new";
    [ObservableProperty] private string _name = string.Empty;
    [ObservableProperty] private int _sortOrder = 0;
    
    [NotifyPropertyChangedFor(nameof(PanelRatio))]
    [ObservableProperty] private int _cols = 24;

    [NotifyPropertyChangedFor(nameof(PanelRatio))]
    [ObservableProperty] private int _rows = 18;
    
    [ObservableProperty] private ObservableCollection<ITrackPiece> _tracks = [];

    [JsonIgnore]
    public string PanelRatio => CalculateRatio(Cols, Rows);

    public void Validate() {

        // Make sure that all the Coordinates for the Track Pieces are valid and 
        // if not, make sure they are within the bounds of the Panel. 
        if (Tracks.Any()) {
            foreach (var track in Tracks) {
                if (track.X <= 0)     track.X = 1;
                if (track.X >= Cols)  track.X = Cols;
                if (track.Y <= 0)     track.Y = 1;
                if (track.Y >= Rows)  track.Y = Rows;
            }
        }
    }

    /// <summary>
    /// Create a deep copy of the Panel object.
    /// </summary>
    /// <returns>A new instance of the Panel object with the same property values as the original.</returns>
    public object Clone() {
        try {
            return ObjectCloner.Clone(this) ?? throw new ArgumentException("Cannot clone the Panel.");
        } catch (Exception ex) {
            throw new ArgumentException("Cannot clone the Panel.", ex);
        }
    }

    [JsonIgnore]
    public Panel? Copy => Clone() as Panel;
    
    private static string CalculateRatio(int col, int row) {
        var gcd = Gcd(col, row);
        var x = col / gcd;
        var y = row / gcd;
        return $"{x:0.#}:{y:0.#}";

        double Gcd(double a, double b) {
            while (b != 0) {
                var temp = b;
                b = a % b;
                a = temp;
            }
            return a;
        }
    }
}

[JsonSerializable(typeof(List<Panel>))]
internal sealed partial class PanelStateContext : JsonSerializerContext{ }
