using System.ComponentModel;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Helpers.Converters;
using DCCPanelController.Helpers.EditableProperties;
using DCCPanelController.Tracks.StyleManager;
using DCCPanelController.Tracks.TrackPieces.Interfaces;
using DCCPanelController.View.Components;

namespace DCCPanelController.ViewModel;

public partial class DynamicPropertyPageViewModel : BaseViewModel {
    [ObservableProperty] private string _propertyName;

    public DynamicPropertyPageViewModel(ITrackPiece trackPiece, string? propertyName, TableView tableView) {
        PropertyName = propertyName ?? $"{trackPiece.Name ?? "Track"}";
        BuildProperties(tableView, trackPiece);
        PropertyChanged += OnPropertyChanged;
    }

    private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e) {
        Console.WriteLine($"PropertyChanged: {sender} - {e.PropertyName}");
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

        // Deal with Switches (on/off)
        // ---------------------------------------------------------------------------------------
        case EditableBoolPropertyAttribute boolAttr:
            var switchCell = new SwitchCell {
                Text = boolAttr.Name,
                BindingContext = property.Owner
            };

            switchCell.SetBinding(SwitchCell.OnProperty, new Binding(property.Info.Name) { Source = property.Owner, Mode = BindingMode.TwoWay });
            //switchCell.SetBinding(SwitchCell.OnProperty, new Binding(property.Info.Name,BindingMode.TwoWay));
            return switchCell;
        
        // Deal with String-based data entry fields
        // ---------------------------------------------------------------------------------------
        case EditableStringPropertyAttribute strAttr:
            var entryCell = new EntryCell {
                Placeholder = strAttr.Description,
                Label = strAttr.Name,
                Keyboard = Keyboard.Text,
                BindingContext = property.Owner
            };

            entryCell.SetBinding(EntryCell.TextProperty, new Binding(property.Info.Name) { Source = property.Owner, Mode = BindingMode.TwoWay });
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

            numCell.SetBinding(EntryCell.TextProperty, new Binding(property.Info.Name) { Source = property.Owner, Mode = BindingMode.TwoWay });
            return numCell;

        // Deal with Turnouts (on/off)
        // ---------------------------------------------------------------------------------------
        case EditableTurnoutPropertyAttribute turnoutAttr:
            Console.WriteLine("EditableTurnoutPropertyAttribute");
            var turnoutActionsView = new TrackTurnoutActionsView {
                BindingContext = property.Owner
            };
            //turnoutActionsView.SetBinding(TrackTurnoutActionsView.ActionsProperty, new Binding(nameof(property.Info.Name)) { Source = property.Owner, Mode = BindingMode.TwoWay });
            var viewCell = new ViewCell { View = turnoutActionsView };
            return viewCell;

        case EditableDatePropertyAttribute dateProp:
            Console.WriteLine("EditableDatePropertyAttribute");
            var datePicker = new DatePicker {
                Format = "D",
                BindingContext = property.Owner
            };

            datePicker.SetBinding(DatePicker.DateProperty, new Binding(property.Info.Name, BindingMode.TwoWay) { Source = property.Owner });
            var datePickerCell = new ViewCell { View = datePicker };
            return datePickerCell;

        case EditableTrackImagePropertyAttribute trackImageProp:
            Console.WriteLine("EditableTrackImagePropertyAttribute");
            return CreateRadioGroupForEnums<TrackStyleImage>(trackImageProp.TrackTypes, property.Owner, property.Info.Name);
//            var typePicker = new Picker { Title = "Track Type"};
//            foreach (var value in trackImageProp.TrackTypes) typePicker.Items.Add(value.ToString());
//            typePicker.SetBinding(Picker.SelectedIndexProperty, new Binding(property.Info.Name, BindingMode.TwoWay, new EnumToIndexConverter<TrackStyleImage>()) { Source = property.Owner });
//            var typePickerCell = new ViewCell { View = typePicker };
//            return typePickerCell;
        
        case EditableTrackTypePropertyAttribute trackTypeProp:
            Console.WriteLine("EditableTrackTypePropertyAttribute");
            return CreateRadioGroupForEnums<TrackStyleType>(trackTypeProp.TrackTypes, property.Owner, property.Info.Name);
//            var stylePicker = new Picker { Title = "Track Style"};
//            foreach (var value in trackTypeProp.TrackTypes) stylePicker.Items.Add(value.ToString());
//            stylePicker.SetBinding(Picker.SelectedIndexProperty, new Binding(property.Info.Name, BindingMode.TwoWay, new EnumToIndexConverter<TrackStyleType>()) { Source = property.Owner });
//            var stylePickerCell = new ViewCell { View = stylePicker};
//            return stylePickerCell;

        default:
            return null;
        }
    }
    
    public static ViewCell CreateRadioGroupForEnums<T>(IEnumerable<T> enumCollection, object source, string fieldName) where T : struct, Enum {
        var radioGroup = new StackLayout { Orientation = StackOrientation.Horizontal, VerticalOptions = LayoutOptions.Center };
        foreach (var value in enumCollection) {
            var radioButton = new RadioButton {
                Content = value.ToString(),
                BindingContext = source,
                VerticalOptions = LayoutOptions.Center,
            };

            // Set up binding for IsChecked property
            radioButton.SetBinding(RadioButton.IsCheckedProperty, new Binding(fieldName, BindingMode.TwoWay, new EnumToBoolConverter<T>(value)) {
                Source = source
            });
            radioGroup.Children.Add(radioButton);
        }
        return new ViewCell { View = radioGroup };;
    }    
}

