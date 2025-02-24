using System.Collections.ObjectModel;
using System.Text.Json.Serialization;
using DCCPanelController.Model.Tracks.Interfaces;

namespace DCCPanelController.Model;

/// <summary>
///     This is used to Store the Data and Settings
/// </summary>
public class Storage {
    public Settings Settings { get; set; } = new();
    public ObservableCollection<Turnout> Turnouts { get; set; } = [];
    public ObservableCollection<Route> Routes { get; set; } = [];
    public ObservableCollection<Panel> Panels { get; set; } = [];

    [JsonIgnore] public List<ITrackTurnout> TurnoutsInUse => Panels.SelectMany(panel => panel.TurnoutsInUse).ToList();
    [JsonIgnore] public List<ITrackButton>  ButtonsInUse => Panels.SelectMany(panel => panel.ButtonsInUse).ToList();
    
    public void ReOrderPanels() {
        if (Panels.Count <= 1) {
            foreach (var panel in Services.SampleData.Panels.DemoData()) {
                Panels.Add(panel);
            }
        }

        for (var index = 0; index < Panels.Count; index++) {
            Panels[index].SortOrder = index + 1;
        }
    }
}