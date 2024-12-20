using System.ComponentModel;
using System.Diagnostics;
using System.Net.Mime;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Helpers.Converters;
using DCCPanelController.Helpers.EditableProperties;
using DCCPanelController.Model.Tracks;
using DCCPanelController.Model.Tracks.Interfaces;
using DCCPanelController.Tracks.StyleManager;
using DCCPanelController.View.Components;

namespace DCCPanelController.ViewModel;

public partial class DynamicPropertyPageViewModelCopy : BaseViewModel {
    [ObservableProperty] private string _propertyName;

    public DynamicPropertyPageViewModelCopy(ITrackPiece trackPiece, string? propertyName, TableView tableView) {
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

        // Deal with Date-based Data Entry fields
        // ---------------------------------------------------------------------------------------
        case EditableDatePropertyAttribute dateProp:
            Console.WriteLine("EditableDatePropertyAttribute");
            var datePicker = new DatePicker {
                Format = "D",
                BindingContext = property.Owner
            };

            datePicker.SetBinding(DatePicker.DateProperty, new Binding(property.Info.Name, BindingMode.TwoWay) { Source = property.Owner });
            var datePickerCell = new ViewCell { View = datePicker };
            return datePickerCell;

        // Deal with Track Image and Track Type
        // ---------------------------------------------------------------------------------------
        case EditableTrackImagePropertyAttribute trackImageProp:
            Console.WriteLine("EditableTrackImagePropertyAttribute");
            return CreateRadioGroupForEnums<TrackStyleImageEnum>(trackImageProp.TrackTypes, property.Owner, property.Info.Name);
//            var typePicker = new Picker { Title = "Track Type"};
//            foreach (var value in trackImageProp.TrackTypes) typePicker.Items.Add(value.ToString());
//            typePicker.SetBinding(Picker.SelectedIndexProperty, new Binding(property.Info.Name, BindingMode.TwoWay, new EnumToIndexConverter<TrackStyleImage>()) { Source = property.Owner });
//            var typePickerCell = new ViewCell { View = typePicker };
//            return typePickerCell;
        
        // Deal with Track Image and Track Type
        // ---------------------------------------------------------------------------------------
        case EditableTrackTypePropertyAttribute trackTypeProp:
            Console.WriteLine("EditableTrackTypePropertyAttribute");
            return CreateRadioGroupForEnums<TrackStyleTypeEnum>(trackTypeProp.TrackTypes, property.Owner, property.Info.Name);
//            var stylePicker = new Picker { Title = "Track Style"};
//            foreach (var value in trackTypeProp.TrackTypes) stylePicker.Items.Add(value.ToString());
//            stylePicker.SetBinding(Picker.SelectedIndexProperty, new Binding(property.Info.Name, BindingMode.TwoWay, new EnumToIndexConverter<TrackStyleTypeEnum>()) { Source = property.Owner });
//            var stylePickerCell = new ViewCell { View = stylePicker};
//            return stylePickerCell;

        case EditableEnumPropertyAttribute enumAttr:
            if (property.Type == typeof(TextAlignment))
                return CreateRadioGroupForEnums<TextAlignment>(enumAttr.Name, property.Owner, property.Info.Name);
            return null;

        case EditableColorPropertyAttribute colorAttr:
            var colorCell = new HorizontalStackLayout() { HorizontalOptions = LayoutOptions.Start,  HeightRequest = 30, VerticalOptions = LayoutOptions.Center, WidthRequest = 200, Margin=new Thickness(0,5,0,5)};
            var colorCellLabel = new Label { Text = colorAttr.Name, VerticalOptions = LayoutOptions.Center, WidthRequest = 100, HeightRequest = 30};
            var colorCellSelector = new ColorGridDropdown() { BindingContext = property.Owner, WidthRequest = 100, HeightRequest = 30};
            colorCellSelector.SetBinding(ColorGridDropdown.SelectedColorProperty, new Binding(property.Info.Name) { Source = property.Owner, Mode = BindingMode.TwoWay });
            colorCell.Children.Add(colorCellLabel);
            colorCell.Children.Add(colorCellSelector);
            return new ViewCell { View = colorCell };

        default:
            return null;
        }
    }

    public static ViewCell CreateRadioGroupForEnums<T>(string name, object source, string fieldName) where T : struct, Enum { 
        if (source == null) throw new ArgumentNullException(nameof(source), "Binding source cannot be null.");
        if (string.IsNullOrWhiteSpace(fieldName)) throw new ArgumentException("Field name cannot be null or whitespace.", nameof(fieldName));

        // Create horizontal StackLayout for the radio group
        var radioGroup = new StackLayout {
            Margin = new Thickness(5, 0, 0, 0),
            Orientation = StackOrientation.Horizontal,
            VerticalOptions = LayoutOptions.Center
        };

        // Loop through each value in the enum
        foreach (var value in Enum.GetValues(typeof(T))) {
            var radioButton = new RadioButton {
                Content = value.ToString(), // Display the value
                VerticalOptions = LayoutOptions.Center,
                BindingContext = source // Set the source for the binding
            };

            // Bind the IsChecked property to the specified field, using a converter
            radioButton.SetBinding(
                RadioButton.IsCheckedProperty,
                new Binding(fieldName, BindingMode.TwoWay, new EnumToIndexConverter<T>()) {
                    Source = source
                });

            // Add the radio button to the stack
            radioGroup.Children.Add(radioButton);
        }

        // Return a ViewCell containing the radio group
        var group = new HorizontalStackLayout() { HorizontalOptions = LayoutOptions.Start,  HeightRequest = 30, VerticalOptions = LayoutOptions.Center, WidthRequest = 200, Margin=new Thickness(0,5,0,5)};
        var groupLabel = new Label { Text = name, VerticalOptions = LayoutOptions.Center, WidthRequest = 100, HeightRequest = 30};
        group.Children.Add(groupLabel);
        group.Children.Add(radioGroup);
        return new ViewCell { View = group };
    }
    
    public static ViewCell CreateRadioGroupForEnums<T>(IEnumerable<T> enumCollection, object source, string fieldName) where T : struct, Enum {
        var radioGroup = new StackLayout { Margin=new Thickness(5,0,0,0), Orientation = StackOrientation.Horizontal, VerticalOptions = LayoutOptions.Center };
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

