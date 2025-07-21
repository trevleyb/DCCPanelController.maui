using System.Collections.ObjectModel;
using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Models.DataModel.Repository;

namespace DCCPanelController.Models.DataModel;

public partial class Profile : ObservableObject {
    [ObservableProperty] private string _profileName;
    [ObservableProperty] private Panels _panels;
    [ObservableProperty] private Settings _settings;

    [ObservableProperty] [JsonIgnore] private ObservableCollection<Block> _blocks;
    [ObservableProperty] [JsonIgnore] private ObservableCollection<Route> _routes;
    [ObservableProperty] [JsonIgnore] private ObservableCollection<Signal> _signals;
    [ObservableProperty] [JsonIgnore] private ObservableCollection<Sensor> _sensors;
    [ObservableProperty] [JsonIgnore] private ObservableCollection<Light> _lights;
    [ObservableProperty] [JsonIgnore] private ObservableCollection<Turnout> _turnouts;

    public Profile(string profileName) {
        _profileName = profileName;
        Panels = new Panels { Profile = this };
        Settings = new Settings();
        Blocks = [];
        Turnouts = [];
        Routes = [];
        Signals = [];
        Sensors = [];
        Lights = [];
    }

    public Turnout? Turnout(string id) {
        return Turnouts.FirstOrDefault(t => t.Id == id);
    }

    public Route? Route(string id) {
        return Routes.FirstOrDefault(r => r.Id == id);
    }

    public Signal? Signal(string id) {
        return Signals.FirstOrDefault(s => s.Id == id);
    }

    public Block? Block(string id) {
        return Blocks.FirstOrDefault(s => s.Id == id);
    }

    //public static Profile NewOrLoad(string profileName) {
    //    return Task.Run(() => JsonRepository.LoadAsync(profileName)).Result;
    //}

    //public static async Task<Profile> LoadAsync(string profileName) {
    //    return await JsonRepository.LoadAsync(profileName);
    //}

    //public async Task SaveAsync() {
    //    await JsonRepository.SaveAsync(this, ProfileName);
    //}

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
    private static void RefreshCollection<T>(ObservableCollection<T> collection, Func<T, bool> predicate) {
        var itemsToRemove = collection.Where(predicate).ToList();
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

    public void RefreshTurnouts()   => RefreshCollection(Turnouts, t => t.IsEditable == false);
    public void RefreshRoutes()     => RefreshCollection(Routes, t => t.IsEditable == false);
    public void RefreshBlocks()     => RefreshCollection(Blocks, t => t.IsEditable == false);
    public void RefreshSignals()    => RefreshCollection(Signals, t => t.IsEditable == false);
    public void RefreshSensors()    => RefreshCollection(Sensors, t => t.IsEditable == false);
    public void RefreshLights()     => RefreshCollection(Lights, t => t.IsEditable == false);
}