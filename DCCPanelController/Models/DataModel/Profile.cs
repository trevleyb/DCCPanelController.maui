using System.Collections.ObjectModel;
using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Models.DataModel.Repository;

namespace DCCPanelController.Models.DataModel;

/// <summary>
///     At some point we will support multiple profiles in the system, each stored
///     on the local machine and you can use Setttings to Upload and Download
///     profiles. Need to build a selector to select a profile in the future.
/// </summary>
public partial class Profile : ObservableObject {
    [ObservableProperty] private Panels _panels;
    [ObservableProperty] private string _profileName;
    [ObservableProperty] private ObservableCollection<Route> _routes = [];
    [ObservableProperty] private Settings _settings;
    [ObservableProperty] private ObservableCollection<Turnout> _turnouts = [];
    [ObservableProperty] private ObservableCollection<Signal> _signals = [];

    public Profile(string profileName) {
        _profileName = profileName;
        Panels = new Panels();
        Settings = new Settings();
        Turnouts = new ObservableCollection<Turnout>();
        Routes = new ObservableCollection<Route>();
        Signals = new ObservableCollection<Signal>();
    }

    [JsonIgnore] public ConnectionInfo ActiveConnectionInfo => Settings.ActiveConnection();

    public Turnout? Turnout(string id) {
        return Turnouts.FirstOrDefault(t => t.Id == id);
    }

    public Route? Route(string id) {
        return Routes.FirstOrDefault(r => r.Id == id);
    }
    
    public Signal? Signal(string id) {
        return Signals.FirstOrDefault(s => s.Id == id);
    }

    public static Profile NewOrLoad(string profileName) {
        return JsonRepository.Load(profileName);
    }

    public static Profile Load(string profileName) {
        return JsonRepository.Load(profileName);
    }

    public void Save() {
        JsonRepository.Save(this, ProfileName);
    }

    /// <summary>
    ///     This method ensures that each panel in the collection of panels is properly initialized with the reference to the
    ///     parent
    ///     collection and performs necessary validation or adjustments by invoking their respective parent-checking logic.
    /// </summary>
    public void FixLoadedPanels() {
        foreach (var panel in Panels) {
            panel.Panels = Panels;
            panel.CheckEntityParents();
        }
    }
}
