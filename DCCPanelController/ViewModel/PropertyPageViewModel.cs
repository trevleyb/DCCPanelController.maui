using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Helpers.Attributes;
using DCCPanelController.Tracks.Base;

namespace DCCPanelController.ViewModel;

public partial class PropertyPageViewModel : BaseViewModel {
    [ObservableProperty] private string _propertyName;

    public PropertyPageViewModel(string propertyName, Object obj, TableView tableView) {
        PropertyName = propertyName;
        BuildProperties(obj, tableView);
    }

    private static void BuildProperties(Object obj, TableView tableView) {
        var propertiesByGroup = EditablePropertyCollector.GetEditableProperties(obj);
        foreach (var group in propertiesByGroup) {
            var tableSection = CreateSection(group.Key);
            foreach (var tableCell in group.Value.Select(CreateCell).OfType<Cell>()) {
                tableSection.Add(tableCell);
            }
            tableView.Root.Add(tableSection);
        }
    }

    private static TableSection CreateSection(string sectionName) {
        var tableSection = new TableSection(sectionName);
        return tableSection;
    }

    private static Cell? CreateCell(EditablePropertyDetails property) {
        switch (property.Type) {
        
        // Deal with String-based data entry fields
        // ---------------------------------------------------------------------------------------
        case { } t when t == typeof(string):
            return new EntryCell {
                Text = property.Info.GetValue(property.Owner)?.ToString() ?? string.Empty,
                Placeholder = property.Attribute.Name ?? string.Empty,
                Label = property.Attribute.Description,
                Keyboard = Keyboard.Text,
                BindingContext = property.Owner
            };
        
        // Deal with Integer-based Data Entry fields
        // ---------------------------------------------------------------------------------------
        case { } t when t == typeof(int):
            return new EntryCell {
                Text = property.Info.GetValue(property.Owner)?.ToString() ?? string.Empty,
                Placeholder = property.Attribute.Name ?? "0",
                Label = property.Attribute.Description,
                Keyboard = Keyboard.Numeric,
                BindingContext = property.Owner
            };
        
        // Deal with Switches (on/off)
        // ---------------------------------------------------------------------------------------
        case { } t when t == typeof(bool):
            return new SwitchCell(){
                Text = property.Attribute.Description ?? string.Empty,
                BindingContext = property.Owner
            };
        default:
            return null;
        }    
    }
}