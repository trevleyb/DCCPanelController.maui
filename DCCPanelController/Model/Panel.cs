using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Helpers;
using DCCPanelController.Model.Tracks.Interfaces;
using DCCPanelController.Tracks.Helpers;

namespace DCCPanelController.Model;

/// <summary>
///     Represents a Panel or Schematic that we can display on the app to control
/// </summary>
public partial class Panel : ObservableValidator, ICloneable {
    [ObservableProperty] private ObservableCollection<ITrackPiece> _tracks = [];

    [ObservableProperty] private string _name = string.Empty;
    [ObservableProperty] private string _description = string.Empty;
    [ObservableProperty] private int _sortOrder;
    [ObservableProperty] private Color _backgroundColor = Colors.White;

    [NotifyPropertyChangedFor(nameof(PanelRatio))] [ObservableProperty] private int _cols = 24;
    [NotifyPropertyChangedFor(nameof(PanelRatio))] [ObservableProperty] private int _rows = 18;

    [JsonIgnore] public string PanelRatio => CalculateRatio(Cols, Rows);
    [JsonIgnore] public Panel? Copy => Clone() as Panel;
    [JsonIgnore] public bool HasSelectedTracks => Tracks.Any(t => t.IsSelected);
    [JsonIgnore] public List<ITrackPiece> SelectedTracks => this.Tracks.Where(t => t.IsSelected).ToList() ?? [];

    /// <summary>
    ///     Create a deep copy of the Panel object.
    /// </summary>
    /// <returns>A new instance of the Panel object with the same property values as the original.</returns>
    public object Clone() {
        try {
            return ObjectCloner.Clone(this) ?? throw new ArgumentException("Cannot clone the Panel.");
        } catch (Exception ex) {
            throw new ArgumentException("Cannot clone the Panel.", ex);
        }
    }

    private bool[] GetConnectedTracksStatus(IEnumerable<ITrackPiece> trackPieces, ITrackPiece trackPiece) => TrackPointsValidator.GetConnectedTracksStatus(trackPieces, trackPiece, Cols, Rows);

    private static string CalculateRatio(int col, int row) {
        var gcd = Gcd(col, row);
        var x = col / gcd;
        var y = row / gcd;
        return $"{x:0.#}:{y:0.#}";

        static double Gcd(double a, double b) {
            while (b != 0) {
                var temp = b;
                b = a % b;
                a = temp;
            }

            return a;
        }
    }
}
