using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Helpers.EditableProperties;
using DCCPanelController.Tracks.TrackPieces.Interfaces;
using DCCPanelController.View.Components;

namespace DCCPanelController.ViewModel;

public partial class DynamicPropertyPageViewModel : BaseViewModel {
    [ObservableProperty] private string _propertyName;

    public DynamicPropertyPageViewModel(ITrackPiece trackPiece, string? propertyName, TableView tableView) {
        PropertyName = propertyName ?? $"{trackPiece.Name} Properties";
        BuildProperties(tableView, trackPiece);
    }

    private static void BuildProperties(TableView tableView, object obj) {
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
        switch (property.Attribute) {
        // Deal with String-based data entry fields
        // ---------------------------------------------------------------------------------------
        case EditableStrPropertyAttribute strAttr:
            var entryCell = new EntryCell {
                Placeholder = strAttr.Description,
                Label = strAttr.Name,
                Keyboard = Keyboard.Text,
                BindingContext = property.Owner
            };

            entryCell.SetBinding(EntryCell.TextProperty, new Binding(nameof(property.Info.Name)) { Source = property.Owner, Mode = BindingMode.TwoWay });
            return entryCell;

        // Deal with Integer-based Data Entry fields
        // ---------------------------------------------------------------------------------------
        case EditableIntPropertyAttribute intAttr:
            var numCell = new EntryCell {
                Placeholder = intAttr.Name ?? "0",
                Label = intAttr.Description,
                Keyboard = Keyboard.Numeric,
                BindingContext = property.Owner
            };

            numCell.SetBinding(EntryCell.TextProperty, new Binding(nameof(property.Info.Name)) { Source = property.Owner, Mode = BindingMode.TwoWay });
            return numCell;

        // Deal with Switches (on/off)
        // ---------------------------------------------------------------------------------------
        case EditableBoolPropertyAttribute boolAttr:
            var switchCell = new SwitchCell {
                Text = boolAttr.Name ?? string.Empty,
                BindingContext = property.Owner
            };

            switchCell.SetBinding(SwitchCell.OnProperty, new Binding(nameof(property.Info.Name)) { Source = property.Owner, Mode = BindingMode.TwoWay });
            return switchCell;

        // Deal with Switches (on/off)
        // ---------------------------------------------------------------------------------------
        case EditableTurnoutPropertyAttribute turnoutAttr:
            var turnoutActionsView = new TrackTurnoutActionsView {
                BindingContext = property.Owner
            };

            //turnoutActionsView.SetBinding(TrackTurnoutActionsView.ActionsProperty, new Binding(nameof(property.Info.Name)) { Source = property.Owner, Mode = BindingMode.TwoWay });

            var viewCell = new ViewCell { View = turnoutActionsView };
            return viewCell;

        //var switchCell = new SwitchCell(){
        //    Text = boolAttr.Name ?? string.Empty,
        //    BindingContext = property.Owner
        //};
        //switchCell.SetBinding(SwitchCell.OnProperty, new Binding(nameof(property.Info.Name)) { Source = property.Owner, Mode = BindingMode.TwoWay });
        //return switchCell;
        //return null;

        default:
            return null;
        }
    }
}