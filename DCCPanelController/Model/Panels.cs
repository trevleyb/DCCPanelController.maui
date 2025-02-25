using System.Collections.ObjectModel;

namespace DCCPanelController.Model.Tracks;

public class Panels : ObservableCollection<Panel> {
    
    public Panel CreatePanel() {
        var panel = new Panel(this);
        Add(panel);
        return panel;
    }
    
    public void ReOrderPanels() {
        if (Count <= 1) return;
        for (var index = 0; index < Count; index++) {
            this[index].SortOrder = index + 1;
        }
    }


}