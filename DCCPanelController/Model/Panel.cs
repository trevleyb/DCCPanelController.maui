using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Helpers;
using DCCPanelController.Model.Tracks.Base;
using DCCPanelController.Model.Tracks.Interfaces;
using DCCPanelController.Tracks.Helpers;

namespace DCCPanelController.Model;

/// <summary>
///     Represents a Panel or Schematic that we can display on the app to control
/// </summary>
public partial class Panel : ObservableObject, ICloneable {
    private ObservableCollection<ITrackPiece> _tracks = [];

    [ObservableProperty] private string _name = string.Empty;
    [ObservableProperty] private string _description = string.Empty;
    [ObservableProperty] private int _sortOrder;
    [ObservableProperty] private PanelDefaults _defaults = new();
    
    [NotifyPropertyChangedFor(nameof(PanelRatio))] [ObservableProperty] private int _cols = 24;
    [NotifyPropertyChangedFor(nameof(PanelRatio))] [ObservableProperty] private int _rows = 18;

    [JsonIgnore] public string PanelRatio => CalculateRatio(Cols, Rows);
    [JsonIgnore] public bool HasSelectedTracks => _tracks.Any(t => t.IsSelected);
    [JsonIgnore] public List<ITrackPiece> SelectedTracks => _tracks.Where(t => t.IsSelected).ToList() ?? [];
    
    public Panel() {
        _tracks = [];
    }

    public Panel(string name, int cols, int rows) : this() {
        Name = name;        
        Cols = cols;
        Rows = rows;
    }

    // When we add an item to the collection, make sure the Parent
    // property is set so we know where this item lives. We need this
    // to be able to get the references to things like colors. 
    // -------------------------------------------------------------------------------
    private void TracksOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) {
       Console.WriteLine($"Collection Changed: {e.Action} {e.NewItems?.Count} {e.OldItems?.Count} {e.NewStartingIndex} {e.OldStartingIndex}");
       if (e.NewItems != null) {
           SetParent(e.NewItems as IList<ITrackPiece>, this);
       } 
    }

    public ITrackPiece AddTrack(ITrackPiece track) {
        track.Parent = this;
        Tracks.Add(track);
        return track;
    }
    
    private static void SetParent(IList<ITrackPiece>? tracks, Panel parent) {
        Console.WriteLine($"Setting Parent: {tracks?.Count}");
        if (tracks is null) return;
        foreach (var track in tracks) track.Parent = parent;
    }

    public ObservableCollection<ITrackPiece> Tracks {
        get => _tracks;
        set {
            if (_tracks != value) {
                try {
                    _tracks = value;
                } catch (Exception ex) {
                    Console.WriteLine("Failed to set Tracks: " + ex.Message);
                }
                _tracks.CollectionChanged += TracksOnCollectionChanged;
                SetParent(_tracks, this);
                OnPropertyChanged(nameof(Tracks));
            }
        }
    }
    
    /// <summary>
    ///     Create a deep copy of the Panel object.
    /// </summary>
    /// <returns>A new instance of the Panel object with the same property values as the original.</returns>
    public object Clone() {
        return ObjectCloner.Clone(this) ?? throw new ArgumentException("Cannot clone the Panel.");
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
