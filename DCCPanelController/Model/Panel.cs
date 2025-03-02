using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Model.Tracks;
using DCCPanelController.Model.Tracks.Interfaces;
using DCCPanelController.Services;
using DCCPanelController.Tracks.Helpers;

namespace DCCPanelController.Model;

/// <summary>
///     Represents a Panel or Schematic that we can display on the app to control
/// </summary>
public partial class Panel : ObservableObject {
    [ObservableProperty] private string _name = string.Empty;
    [ObservableProperty] private string _description = string.Empty;
    [ObservableProperty] private int _sortOrder;
    [ObservableProperty] [NotifyPropertyChangedFor(nameof(PanelRatio))] private int _cols = 24;
    [ObservableProperty] [NotifyPropertyChangedFor(nameof(PanelRatio))] private int _rows = 18;

    private ObservableCollection<ITrack> _tracks = [];

    [JsonConstructor]
    private Panel() {
        ResetToDefaults();
    }

    public Panel(Panels panels) : this() {
        SetPanels(panels);
    }

    [JsonIgnore] public Panels Panels { get; private set; } = [];
    [JsonIgnore] public string PanelRatio => CalculateRatio(Cols, Rows);
    [JsonIgnore] public List<ITrack> SelectedTracks => _tracks.Where(t => t.IsSelected).ToList() ?? [];
    [JsonIgnore] public List<ITrackTurnout> AllNamedTurnouts => Panels?.SelectMany(p => p.NamedTurnouts).ToList() ?? [];
    [JsonIgnore] public List<ITrackButton> AllNamedButtons => Panels?.SelectMany(p => p.NamedButtons).ToList() ?? [];
    [JsonIgnore] public List<ITrackTurnout> NamedTurnouts => Tracks.OfType<ITrackTurnout>().Where(trk => !string.IsNullOrWhiteSpace(trk.ID)).ToList() ?? [];
    [JsonIgnore] public List<ITrackButton> NamedButtons => Tracks.OfType<ITrackButton>().Where(trk => !string.IsNullOrWhiteSpace(trk.ID)).ToList() ?? [];

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

    public ITrackTurnout? GetTurnout(string id) {
        return AllNamedTurnouts.FirstOrDefault(t => t.ID == id);
    }

    public ITrackButton? GetButton(string id) {
        return AllNamedButtons.FirstOrDefault(t => t.ID == id);
    }

    public void SetPanels(Panels panels) {
        Panels = panels;
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

    public string NextTurnoutID() {
        return GetNextName(AllNamedTurnouts, t => t.ID, "TRN");
    }

    public string NextButtonID() {
        return GetNextName(AllNamedButtons, t => t.ID, "BTN");
    }

    public static string GetNextName<T>(IEnumerable<T> allNamedItems, Func<T, string> idSelector, string defaultPrefix = "UKN") {
        // Step 1: Extract IDs using the provided selector and sort them for pattern detection.
        var ids = allNamedItems
                 .Select(idSelector)
                 .OrderBy(id => id)
                 .ToList();

        if (!ids.Any()) return $"{defaultPrefix}1"; // Default name if the list is empty.

        // Step 2: Look for numerical patterns (Prefix + Number)
        var numericalPattern = ids
                              .Select(id => {
                                   var match = Regex.Match(id, @"^(.*?)(\d+)$"); // Match a prefix and a number (e.g., "Button123").
                                   if (match.Success) {
                                       return (Prefix: match.Groups[1].Value, Number: int.Parse(match.Groups[2].Value));
                                   }

                                   return (Prefix: id, Number: 0); // No numerical suffix, treat as 0.
                               })
                              .OrderBy(item => item.Number)
                              .ToList();

        if (numericalPattern.Count > 0) {
            var latestItem = numericalPattern.Last(); // Get the last item in the sequence.
            var nextNumber = latestItem.Number + 1;   // Increment the number for the new item.
            return $"{latestItem.Prefix}{nextNumber}";
        }

        // Step 3: Look for alphabetical patterns (e.g., ButtonA, ButtonB).
        var alphabeticalPattern = ids
                                 .Where(id => Regex.IsMatch(id, @"^[a-zA-Z]+$")) // Match IDs containing only letters.
                                 .OrderBy(id => id)
                                 .ToList();

        if (alphabeticalPattern.Count > 0) {
            var lastID = alphabeticalPattern.Last();          // Get the last ID in the alphabetical sequence.
            var nextID = IncrementAlphabeticalString(lastID); // Increment alphabetically.
            return nextID;
        }

        // Step 4: Fallback if no discernible pattern is found (e.g., "UnnamedItem1").
        return $"{defaultPrefix}{ids.Count + 1}";
    }

// Helper Method: Increment alphabetical strings (e.g., "A" -> "B", "Z" -> "AA").
    private static string IncrementAlphabeticalString(string input) {
        var chars = input.ToCharArray();
        for (var i = chars.Length - 1; i >= 0; i--) {
            if (chars[i] < 'Z') {
                chars[i]++;
                return new string(chars);
            }

            chars[i] = 'A';
            if (i == 0) {
                // If we're at the beginning and need to carry over (e.g., "Z" -> "AA").
                return 'A' + new string(chars);
            }
        }

        return new string(chars);
    }
}