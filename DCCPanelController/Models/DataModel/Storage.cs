using System.Collections.ObjectModel;

namespace DCCPanelController.Models.DataModel;

/// <summary>
///     This is used to Store the Data and Settings
/// </summary>
public class Storage {
    public Settings Settings { get; set; } = new();
    public Panels Panels { get; set; } = [];
    public ObservableCollection<Turnout> Turnouts { get; set; } = [];
    public ObservableCollection<Route> Routes { get; set; } = [];
}