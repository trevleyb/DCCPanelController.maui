using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Net.Mime;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Helpers;
using DCCPanelController.Helpers.Converters;
using DCCPanelController.Helpers.EditableProperties;
using DCCPanelController.Model.Tracks;
using DCCPanelController.Model.Tracks.Interfaces;
using DCCPanelController.Tracks.StyleManager;
using DCCPanelController.View.Components;
using Maui.FreakyControls;
using Switch = Microsoft.Maui.Controls.Switch;

namespace DCCPanelController.ViewModel;

public partial class DynamicPropertyPageViewModel : BaseViewModel {
    [ObservableProperty] private string _propertyName;

    public DynamicPropertyPageViewModel(ITrackPiece trackPiece, string? propertyName, StackLayout tableView) {
        PropertyName = propertyName ?? $"{trackPiece.Name ?? "Track"}";
        BuildProperties(tableView, trackPiece);
        PropertyChanged += OnPropertyChanged;
    }

    private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e) {
        Console.WriteLine($"PropertyChanged: {sender} - {e.PropertyName}");
    }

    private static void BuildProperties(StackLayout tableView, object obj) {
        var propertiesByGroup = EditablePropertyCollector.GetEditableProperties(obj);
        tableView.Children.Clear();
        bool isFirst = true;
        foreach (var group in propertiesByGroup) {
            var tableGroup = CreateGroup(group.Key, group.Value, isFirst);
            tableView.Children.Add(tableGroup);
            isFirst = false;
        }
    }

    private static IView CreateGroup(string groupKey, List<EditablePropertyDetails> groupValue, bool isFirst) {
        var tableGroup = new StackLayout {
            Margin = new Thickness(0, isFirst ? 0 : 10, 0, 0)
        };
        if (!string.IsNullOrWhiteSpace(groupKey)) {
            tableGroup.Add(GroupHeading(groupKey));
            tableGroup.Add(GroupDivider());
        }
        foreach (var value in groupValue) {
            tableGroup.Add(GroupCell(value));
        }
        return tableGroup;
    }

    private static IView GroupHeading(string groupKey) {
        var heading = new Label() {
            Text = groupKey,
            TextColor = StyleColor.Get("Primary"),
            FontSize = 18,
            Margin = new Thickness(0, 0, 0, 0)
        };
        return heading;
    }

    private static IView GroupDivider() {
        var divider = new BoxView() {
            BackgroundColor = StyleColor.Get("Primary"),
            HeightRequest = 2,
            HorizontalOptions = LayoutOptions.Fill,
            Margin = new Thickness(0, 0, 10, 15)
        };
        return divider;
    }

    private static IView GroupCell(EditablePropertyDetails value) {
        var groupCell = new HorizontalStackLayout() {
            Margin = new Thickness(0, 5, 0, 5),
        };
        var label = new Label() {
            Text = value.Attribute.Name,
            TextColor = Colors.Black,
            FontSize = 15,
            LineBreakMode = LineBreakMode.MiddleTruncation,
            HorizontalOptions = LayoutOptions.Start,
            VerticalOptions = LayoutOptions.Center,
            Margin = new Thickness(0, 5, 0, 5),
            WidthRequest = 150
        };
        var cell = CreateDataEntry(value);
        groupCell.Children.Add(label);
        groupCell.Children.Add(cell);
        return groupCell;
    }

    private static IView CreateDataEntry(EditablePropertyDetails value) {
        try {
            return value.Attribute switch {
                EditableBoolPropertyAttribute        => CreateBool(value),
                EditableColorPropertyAttribute       => CreateColor(value),
                EditableDatePropertyAttribute        => CreateDate(value),
                EditableEnumPropertyAttribute        => CreateEnum(value),
                EditableInformationPropertyAttribute => CreateInfo(value),
                EditableIntPropertyAttribute         => CreateInt(value),
                EditableStringPropertyAttribute      => CreateString(value),
                EditableTrackImagePropertyAttribute  => CreateTrackImage(value),
                EditableTrackTypePropertyAttribute   => CreateTrackType(value),
                EditableTurnoutPropertyAttribute     => CreateTurnout(value),
                _                                    => CreateUndefined(value),
            };
        } catch (Exception ex) {
            Debug.WriteLine($"Unable to create entry for {value.Attribute.Name} - {ex.Message}");
            return CreateUndefined(value);
        }
    }

    private static IView CreateTurnout(EditablePropertyDetails value) {
        return CreateUndefined(value,"Turnout");
        //var cell = new Switch { BindingContext = value.Owner };
        //cell.SetBinding(Switch.IsToggledProperty, new Binding(value.Info.Name) { Source = value.Owner, Mode = BindingMode.TwoWay });
        //return cell;    
    }

    private static IView CreateTrackType(EditablePropertyDetails value) {
        var attr = value.Attribute as EditableTrackTypePropertyAttribute;
        return CreateRadioGroupForEnums<TrackStyleTypeEnum>("Track Type", attr?.TrackTypes ?? [], value.Owner, value.Info.Name);
    }

    private static IView CreateTrackImage(EditablePropertyDetails value) {
        var attr = value.Attribute as EditableTrackImagePropertyAttribute;
        return CreateRadioGroupForEnums<TrackStyleImageEnum>("Track Style", attr?.TrackTypes ?? [], value.Owner, value.Info.Name);
    }

    private static Entry CreateString(EditablePropertyDetails value) {
        var cell = new Entry {
            Placeholder = value.Attribute.Description,
            Keyboard = Keyboard.Text,
            WidthRequest = 300,
            BindingContext = value.Owner
        };
        cell.SetBinding(Entry.TextProperty, new Binding(value.Info.Name) { Source = value.Owner, Mode = BindingMode.TwoWay });
        return cell;
    }

    private static HorizontalStackLayout CreateInt(EditablePropertyDetails value) {
        var cell = new HorizontalStackLayout();
        var datacell = new Entry {
            BindingContext = value.Owner,
            WidthRequest = 75,
            Placeholder = value.Attribute.Description,
            Keyboard = Keyboard.Numeric,
            Margin = new Thickness(0, 0, 10, 0),
            Text = value.Info.GetValue(value.Owner)?.ToString() ?? "0"
        };
        datacell.SetBinding(Entry.TextProperty, new Binding(value.Info.Name) { Source = value.Owner, Mode = BindingMode.TwoWay });

        var attr = value.Attribute as EditableIntPropertyAttribute;
        var updown = new Stepper {
            Minimum = attr?.MinValue ?? 0, // Define the stepper min value if needed
            Maximum = attr?.MaxValue ?? 99, // Define the stepper max value if needed
            Increment = 1,          // Increment/decrement step
            HorizontalOptions = LayoutOptions.End
        };
        
        // To initialize the Stepper with the current value
        if (int.TryParse(datacell.Text, out var initialStepperValue)) {
            updown.Value = initialStepperValue;
        }

        // Sync Stepper and Entry when Stepper value changes
        updown.ValueChanged += (s, e) => {
            datacell.Text = e?.NewValue.ToString(CultureInfo.InvariantCulture) ?? "0";
        };

        // Sync Entry and Stepper when Entry value is manually changed
        datacell.TextChanged += (s, e) => {
            if (int.TryParse(e.NewTextValue, out var parsedValue)) {
                updown.Value = parsedValue;
            }
        };

        // Add Entry and Stepper to the horizontal layout
        cell.Children.Add(datacell);
        cell.Children.Add(updown);
        return cell;    
    }

    private static Label CreateInfo(EditablePropertyDetails value) {
        var cell = new Label { BindingContext = value.Owner, Text = value.Attribute.Description };
        return cell;    
    }

    private static IView CreateEnum(EditablePropertyDetails value) {
        if (value.Type == typeof(TextAlignment)) {
            var items = new TextAlignment[] { TextAlignment.Start, TextAlignment.Center, TextAlignment.End };    
            return CreateRadioGroupForEnums<TextAlignment>("Alignment", items, value.Owner, value.Info.Name);
        }

        return CreateUndefined(value,"Enum Value Option");
    }

    private static DatePicker CreateDate(EditablePropertyDetails value) {
        var cell = new DatePicker() { BindingContext = value.Owner, Format = "D"};
        cell.SetBinding(DatePicker.DateProperty, new Binding(value.Info.Name) { Source = value.Owner, Mode = BindingMode.TwoWay });
        return cell;    
    }

    private static ColorGridDropdown CreateColor(EditablePropertyDetails value) {
        var cell = new ColorGridDropdown() { WidthRequest = 100, HeightRequest = 30 };
        cell.SetBinding(ColorGridDropdown.SelectedColorProperty, new Binding(value.Info.Name) { Source = value.Owner, Mode = BindingMode.TwoWay });
        return cell;    
    }

    private static Switch CreateBool(EditablePropertyDetails value) {
        var cell = new Switch { BindingContext = value.Owner, OnColor = StyleColor.Get("Primary"), ThumbColor = Colors.White};
        cell.SetBinding(Switch.IsToggledProperty, new Binding(value.Info.Name) { Source = value.Owner, Mode = BindingMode.TwoWay });
        return cell;    
    }

    private static Label CreateUndefined(EditablePropertyDetails value, string text="Not yet defined") {
        return new Label() { 
            Text = text,
            HorizontalOptions = LayoutOptions.Start,
            VerticalOptions = LayoutOptions.Center,
        };
    }

    public static IView CreateDropDownForEnums<T>(string name, object source, string fieldName) where T : struct, Enum {
        var items = Enum.GetValues<T>().Select(x => x.ToString()).ToList();
        var picker = new FreakyPicker() {
            Title = name,
            FontAttributes = FontAttributes.Bold,
            FontSize = 15,
            ItemsSource = items,
            TextColor = Colors.Black,
            TitleColor= Colors.LightGray
        };
        picker.SelectedIndexChanged += (s, e) => {
            PropertyHelper.SetEnumPropertyValue(source, fieldName,items[picker.SelectedIndex]);
        };
        picker.SelectedIndex = PropertyHelper.GetEnumValueIndex(source, fieldName);
        return picker;
    }
    
    public static StackLayout CreateRadioGroupForEnums<T>(string name, T[] items, object source, string fieldName) where T : struct, Enum { 
        if (source == null) throw new ArgumentNullException(nameof(source), "Binding source cannot be null.");
        if (string.IsNullOrWhiteSpace(fieldName)) throw new ArgumentException("Field name cannot be null or whitespace.", nameof(fieldName));

        var radioGroup = new StackLayout() {
            HeightRequest = 30,
            Orientation = StackOrientation.Horizontal,
            HorizontalOptions = LayoutOptions.Start,
            VerticalOptions = LayoutOptions.Center,
            Margin = new Thickness(-5, 0, 0, 0),
        };
        
        foreach (var value in items) {
            var radioButton = new RadioButton {
                HeightRequest = 30,
                Content = value.ToString(), // Display the value
            };
            radioButton.CheckedChanged += (sender, args) => { PropertyHelper.SetEnumPropertyValue(source, fieldName, value); };
            radioButton.IsChecked = value.Equals(PropertyHelper.GetEnumPropertyValue<T>(source, fieldName));
            radioGroup.Children.Add(radioButton);
        }
        
        return radioGroup;
    }
    
    public static HorizontalStackLayout CreateFreakyRadioGroupForEnums<T>(object source, string fieldName) where T : struct, Enum { 
        if (source == null) throw new ArgumentNullException(nameof(source), "Binding source cannot be null.");
        if (string.IsNullOrWhiteSpace(fieldName)) throw new ArgumentException("Field name cannot be null or whitespace.", nameof(fieldName));

        
        // <freakyControls:FreakyRadioGroup
//     Orientation="Vertical"
// SelectedIndex="{Binding CheckedRadioButton}"
// SelectedRadioButtonChanged="FreakyRadioGroup_SelectedRadioButtonChanged"
// SelectedRadioButtonChangedCommand="{Binding SelectedIndexCommand}"
// Spacing="10">
//     <HorizontalStackLayout Spacing="10">

        
        // Create horizontal StackLayout for the radio group
        var radioGroup = new HorizontalStackLayout();

        // Loop through each value in the enum
        foreach (var value in Enum.GetValues<T>()) {
            var radioButton = new RadioButton {
                HeightRequest = 30,
                MinimumHeightRequest = 30,
                Content = value.ToString(), // Display the value
            };

            // Bind the IsChecked property to the specified field, using a converter
            radioButton.SetBinding(
                RadioButton.IsCheckedProperty,
                new Binding(fieldName, 
                            BindingMode.TwoWay, 
                            new EnumToIndexConverter<T>()) { Source = source });

            // Add the radio button to the stack
            radioGroup.Children.Add(radioButton);
        }
        return radioGroup;
    }

    
    
}

// <freakyControls:FreakyRadioGroup
//     Orientation="Vertical"
// SelectedIndex="{Binding CheckedRadioButton}"
// SelectedRadioButtonChanged="FreakyRadioGroup_SelectedRadioButtonChanged"
// SelectedRadioButtonChangedCommand="{Binding SelectedIndexCommand}"
// Spacing="10">
//
//     <HorizontalStackLayout Spacing="10">
//
//     <freakyControls:FreakyRadioButton
//     CheckColor="White"
// FillColor="{StaticResource Primary}"
// OutlineColor="{StaticResource Tertiary}" />
//
//     <Label Text="Color Scheme" />
//     </HorizontalStackLayout>