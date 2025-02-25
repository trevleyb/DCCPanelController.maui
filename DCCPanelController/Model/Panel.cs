using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Text.Json;
using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Model.Tracks;
using DCCPanelController.Model.Tracks.Interfaces;
using DCCPanelController.Services;
using DCCPanelController.Services.SampleData;
using DCCPanelController.Tracks.Helpers;

namespace DCCPanelController.Model;

/// <summary>
///     Represents a Panel or Schematic that we can display on the app to control
/// </summary>
public partial class Panel : ObservableObject {
    [ObservableProperty] [NotifyPropertyChangedFor(nameof(PanelRatio))] private int _cols = 24;
    [ObservableProperty] private PanelDefaults _defaults = new();
    [ObservableProperty] private string _description = string.Empty;
    [ObservableProperty] private string _name = string.Empty;
    [ObservableProperty] [NotifyPropertyChangedFor(nameof(PanelRatio))] private int _rows = 18;
    [ObservableProperty] private int _sortOrder;

    [JsonIgnore] public string PanelRatio => CalculateRatio(Cols, Rows);
    [JsonIgnore] public List<ITrack> SelectedTracks => _tracks.Where(t => t.IsSelected).ToList() ?? [];
    [JsonIgnore] public List<ITrackTurnout> AllNamedTurnouts => Panels?.SelectMany(p => p.NamedTurnouts).ToList() ?? [];
    [JsonIgnore] public List<ITrackButton> AllNamedButtons => Panels?.SelectMany(p => p.NamedButtons).ToList() ?? [];
    [JsonIgnore] public List<ITrackTurnout> NamedTurnouts => Tracks.OfType<ITrackTurnout>().Where(trk => !string.IsNullOrWhiteSpace(trk.TurnoutID)).ToList() ?? [];
    [JsonIgnore] public List<ITrackButton> NamedButtons => Tracks.OfType<ITrackButton>().Where(trk => !string.IsNullOrWhiteSpace(trk.ButtonID)).ToList() ?? [];
    [JsonIgnore] public Panels Panels { get; private set; } = [];

    private ObservableCollection<ITrack> _tracks = [];

    [JsonConstructor]
    private Panel() { }
    public Panel(Panels panels) {
        SetPanels(panels);
    }

    public void SetPanels(Panels panels) {
        Panels = panels;
    }
    
    public string Title { 
        get {
            if (string.IsNullOrEmpty(Name) && string.IsNullOrEmpty(Description)) return "DCC Panel Controller";
            if (!string.IsNullOrEmpty(Name) && string.IsNullOrEmpty(Description)) return Name;
            if (string.IsNullOrEmpty(Name) && !string.IsNullOrEmpty(Description)) return Description;
            return Name + $" - {Description}";
        }
    }


    public string Id { get; init; } = Guid.NewGuid().ToString();

    public ObservableCollection<ITrack> Tracks {
        get => _tracks;
        init {
            try {
                _tracks = value;
            } catch (Exception ex) {
                Console.WriteLine("Failed to set Tracks: " + ex.Message);
            }
            SetParent(_tracks, this);
            OnPropertyChanged();
        }
    }

    // When we add an item to the collection, make sure the Panels
    // property is set so we know where this item lives. We need this
    // to be able to get the references to things like colors. 
    // -------------------------------------------------------------------------------
    private void TracksOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) {
        Console.WriteLine($"Collection Changed: {e.Action} {e.NewItems?.Count} {e.OldItems?.Count} {e.NewStartingIndex} {e.OldStartingIndex}");
        if (e.NewItems != null) {
            SetParent(e.NewItems as IList<ITrack>, this);
        }
    }

    public ITrack AddTrack(ITrack track) {
        track.Parent = this;
        Tracks.Add(track);
        return track;
    }

    public static void SetParent(IList<ITrack>? tracks, Panel parent) {
        if (tracks is null) return;
        foreach (var track in tracks) track.Parent = parent;
    }

    public bool HasChanged(Panel original, Panel modified) {
        var originalJson = JsonSerializer.Serialize(this, SettingsService.PanelSerializerOptions);
        var modifiedJson = JsonSerializer.Serialize(this, SettingsService.PanelSerializerOptions);
        var originalHash = originalJson.GetHashCode();
        var modifiedHash = modifiedJson.GetHashCode();
        return originalJson.GetHashCode() != modifiedJson.GetHashCode();
    }

    private bool[] GetConnectedTracksStatus(IEnumerable<ITrack> trackPieces, ITrack track) {
        return TrackPointsValidator.GetConnectedTracksStatus(trackPieces, track, Cols, Rows);
    }

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

    public Panel Clone() {
        var copy = JsonSerializer.Serialize(this, SettingsService.PanelSerializerOptions);
        var panel = JsonSerializer.Deserialize<Panel>(copy, SettingsService.PanelSerializerOptions);
        return panel ?? throw new Exception("Failed to clone panel");
    }
}