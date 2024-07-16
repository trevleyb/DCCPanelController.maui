using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
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
    
    [ObservableProperty] private ObservableCollection<Track> _tracks = [];

    [JsonIgnore]
    public string PanelRatio => CalculateRatio(Cols, Rows);

    public void Validate() {

        // Make sure that all the Coordinates for the Track Pieces are valid and 
        // if not, make sure they are within the bounds of the Panel. 
        if (Tracks.Any()) {
            foreach (var track in Tracks) {
                if (track.Coordinate.Col <= 0) track.Coordinate.Col = 1;
                if (track.Coordinate.Col >= Cols) track.Coordinate.Col = Cols;
                if (track.Coordinate.Row <= 0) track.Coordinate.Row = 1;
                if (track.Coordinate.Row >= Rows) track.Coordinate.Row = Rows;
            }
        }

        // Validate that none of the tracks overlap any other tracks. If they do, 
        // then we need to remove them or move them (lets try to move them). 
        List<Track> invalidTracks = new();
        foreach (var track in Tracks) {
            while (Tracks.Any(t => t.Coordinate.Col == track.Coordinate.Col && t.Coordinate.Row == track.Coordinate.Row && t != track)) {
                track.Coordinate.Col += 1;
                if (track.Coordinate.Col >= Cols) {
                    track.Coordinate.Col = 1;
                    track.Coordinate.Row += 1;
                    if (track.Coordinate.Row >= Rows) {
                        track.Coordinate.Row = -1;
                        track.Coordinate.Col = -1;
                        invalidTracks.Add(track);
                        break;
                    }
                }
            }
        }
        foreach (var invalidTrack in invalidTracks) Tracks.Remove(invalidTrack);
    }

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

    [JsonIgnore]
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

    public void RaiseChangedEvent() {
        OnPropertyChanged(nameof(Tracks));
        OnPropertyChanged(nameof(Name));
    }
}

[JsonSerializable(typeof(List<Panel>))]
internal sealed partial class PanelStateContext : JsonSerializerContext{ }
