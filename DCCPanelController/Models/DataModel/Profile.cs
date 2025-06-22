using System.Collections.ObjectModel;
using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Models.DataModel.Repository;

namespace DCCPanelController.Models.DataModel;

public partial class Profile : ObservableObject {
    [ObservableProperty] private string _profileName;
    [ObservableProperty] private Panels _panels;
    [ObservableProperty] private Settings _settings;
    [ObservableProperty] private ObservableCollection<Block> _blocks;
    [ObservableProperty] private ObservableCollection<Route> _routes;
    [ObservableProperty] private ObservableCollection<Signal> _signals;
    [ObservableProperty] private ObservableCollection<Turnout> _turnouts;
    [ObservableProperty] private ObservableCollection<Sensor> _sensors;
    [ObservableProperty] private ObservableCollection<Light> _lights;

    public Profile(string profileName) {
        _profileName = profileName;
        Panels = new Panels {
            Profile = this
        };
        Settings = new Settings();
        Blocks = new ObservableCollection<Block>();
        Turnouts = new ObservableCollection<Turnout>();
        Routes = new ObservableCollection<Route>();
        Signals = new ObservableCollection<Signal>();
        Sensors = new ObservableCollection<Sensor>();
        Lights = new ObservableCollection<Light>();
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

    public static Profile NewOrLoad(string profileName) {
        return Task.Run(() => JsonRepository.LoadAsync(profileName)).Result;
    }

    public static async Task<Profile> LoadAsync(string profileName) {
        return await JsonRepository.LoadAsync(profileName);
    }

    public async Task SaveAsync() {
        await JsonRepository.SaveAsync(this, ProfileName);
    }

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
}