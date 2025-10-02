using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Models.DataModel.Entities;
using DCCPanelController.View.Helpers;

namespace DCCPanelController.Models.DataModel;

public partial class Profile : ObservableObject {
    [ObservableProperty] private string _filename;
    [ObservableProperty] private string _profileName;

    [ObservableProperty] private Panels                          _panels;
    [ObservableProperty] private Settings                        _settings;
    [ObservableProperty] private UiObservableCollection<Light>   _lights;
    [ObservableProperty] private ObservableCollection<Block>     _blocks;
    [ObservableProperty] private UiObservableCollection<Route>   _routes;
    [ObservableProperty] private UiObservableCollection<Sensor>  _sensors;
    [ObservableProperty] private UiObservableCollection<Signal>  _signals;
    [ObservableProperty] private UiObservableCollection<Turnout> _turnouts;

    [ObservableProperty] private DateTime           _fastClock;
    [ObservableProperty] private PowerStateEnum     _powerState;
    [ObservableProperty] private FastClockStateEnum _fastClockState;

    public Profile(string profileName, string? fileName = null) {
        _filename = string.IsNullOrWhiteSpace(fileName) ? Guid.NewGuid().ToString() : fileName;
        _profileName = profileName;
        Panels = new Panels { Profile = this };
        Settings = new Settings();
        Blocks = [];
        Turnouts = [];
        Routes = [];
        Signals = [];
        Sensors = [];
        Lights = [];
        FastClock = DateTime.Now;
        FastClockState = FastClockStateEnum.Unknown;
        PowerState = PowerStateEnum.Unknown;
    }

    public string Version { get; set; } = "1.0.1"; // Ensure we increment this version number when making changes to the schema

    public Turnout? Turnout(string id) => Turnouts.FirstOrDefault(t => t.Id == id);
    public Route? Route(string id) => Routes.FirstOrDefault(r => r.Id == id);
    public Signal? Signal(string id) => Signals.FirstOrDefault(s => s.Id == id);
    public Block? Block(string id) => Blocks.FirstOrDefault(s => s.Id == id);
    public Sensor? Sensor(string id) => Sensors.FirstOrDefault(s => s.Id == id);
    public Light? Light(string id) => Lights.FirstOrDefault(s => s.Id == id);

    /// <summary>
    ///     This method ensures that each panel in the collection of panels is properly initialized with the reference to the
    ///     parent
    ///     collection and performs necessary validation or adjustments by invoking their respective parent-checking logic.
    /// </summary>
    public void FixLoadedPanels() {
        Panels.Profile = this;
        foreach (var panel in Panels) {
            panel.Panels = Panels;
            panel.CheckEntityParents();
        }
    }

    // Add this generic helper method to your Profile class
    private static void RefreshCollection<T>(ObservableCollection<T> collection, Func<T, bool> removePredicate) {
        var itemsToRemove = collection.Where(removePredicate).ToList();
        foreach (var item in itemsToRemove) {
            collection.Remove(item);
        }
    }

    public void RefreshAll() {
        RefreshTurnouts();
        RefreshRoutes();
        RefreshBlocks();
        RefreshLights();
        RefreshSignals();
        RefreshSensors();
    }

    public void RefreshTurnouts() => RefreshCollection(Turnouts, t => t is { IsEditable: false, IsModified: false });
    public void RefreshRoutes() => RefreshCollection(Routes, t => t is { IsEditable    : false, IsModified: false });
    public void RefreshBlocks() => RefreshCollection(Blocks, t => t is { IsEditable    : false, IsModified: false });
    public void RefreshSignals() => RefreshCollection(Signals, t => t is { IsEditable  : false, IsModified: false });
    public void RefreshSensors() => RefreshCollection(Sensors, t => t is { IsEditable  : false, IsModified: false });
    public void RefreshLights() => RefreshCollection(Lights, t => t is { IsEditable    : false, IsModified: false });
}