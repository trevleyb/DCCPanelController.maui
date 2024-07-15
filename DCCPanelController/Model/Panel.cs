using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Helpers.Attributes;
using Microsoft.Maui.Graphics.Platform;

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
    
    [ObservableProperty] private ObservableCollection<Track> _tracks = new();

    [JsonIgnore]
    public string PanelRatio => CalculateRatio(Cols, Rows);

    /// <summary>
    /// Create a deep copy of the Panel object.
    /// </summary>
    /// <returns>A new instance of the Panel object with the same property values as the original.</returns>
    public object Clone() {
        var newPanel = new Panel {
            Id = Id,
            Name = Name,
            SortOrder = SortOrder,
            Cols = Cols,
            Rows = Rows
        };    
        foreach (var track in _tracks) newPanel.Tracks.Add(track);
        return newPanel;
    }

    public Panel? Copy => Clone() as Panel;
    
    private static string CalculateRatio(int col, int row) {
        double GCD(double a, double b) {
            while (b != 0) {
                var temp = b;
                b = a % b;
                a = temp;
            }
            return a;
        }

        var gcd = GCD(col, row);
        var x = col / gcd;
        var y = row / gcd;
        return $"{x:0.#}:{y:0.#}";
    }
}

[JsonSerializable(typeof(List<Panel>))]
internal sealed partial class PanelStateContext : JsonSerializerContext{ }
