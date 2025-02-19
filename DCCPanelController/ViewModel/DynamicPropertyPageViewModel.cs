using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Helpers;
using DCCPanelController.Helpers.Converters;
using DCCPanelController.Helpers.EditableProperties;
using DCCPanelController.Model;
using DCCPanelController.Model.Tracks.Interfaces;
using DCCPanelController.Services;
using DCCPanelController.View.Actions;
using DCCPanelController.View.Components;
using Switch = Microsoft.Maui.Controls.Switch;

namespace DCCPanelController.ViewModel;

public partial class DynamicPropertyPageViewModel : BaseViewModel {
    private readonly NavigationService _navigationService = MauiProgram.ServiceHelper.GetService<NavigationService>();
    [ObservableProperty] private string _propertyName;
    [ObservableProperty] private ITrackPiece _trackPiece;

    public DynamicPropertyPageViewModel(ITrackPiece trackPiece, string? propertyName, VerticalStackLayout tableView) {
        TrackPiece = trackPiece;
        PropertyName = propertyName ?? $"{trackPiece.Name ?? "Track"}";
        BuildProperties(tableView, trackPiece);
        PropertyChanged += OnPropertyChanged;
    }

    private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e) {
        Console.WriteLine($"PropertyChanged: {sender} - {e.PropertyName}");
    }

    private void BuildProperties(VerticalStackLayout tableView, object obj) {
        var propertiesByGroup = EditablePropertyCollector.GetEditableProperties(obj);
        tableView.Children.Clear();
        var isFirst = true;
        foreach (var tableGroup in propertiesByGroup.Select(group => CreateGroup(group.Key, group.Value, isFirst))) {
            tableView.Children.Add(tableGroup);
            isFirst = false;
        }
    }

    private IView CreateGroup(string groupKey, List<EditablePropertyDetails> groupValue, bool isFirst) {
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

    private IView GroupHeading(string groupKey) {
        var heading = new Label {
            Text = groupKey,
            TextColor = StyleColor.Get("Primary"),
            FontSize = 18,
            Margin = new Thickness(0, 0, 0, 0)
        };

        return heading;
    }

    private IView GroupDivider() {
        var divider = new BoxView {
            BackgroundColor = StyleColor.Get("Primary"),
            HeightRequest = 2,
            HorizontalOptions = LayoutOptions.Fill,
            Margin = new Thickness(0, 0, 10, 15)
        };

        return divider;
    }

    private IView GroupCell(EditablePropertyDetails value) {
        var groupCell = new HorizontalStackLayout {
            Margin = new Thickness(0, 5, 0, 5)
        };

        if (!string.IsNullOrWhiteSpace(value.Attribute.Name)) {
            var label = new Label {
                Text = value.Attribute.Name,
                TextColor = Colors.Black,
                FontSize = 15,
                LineBreakMode = LineBreakMode.MiddleTruncation,
                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.Center,
                Margin = new Thickness(0, 5, 0, 5),
                WidthRequest = 150
            };
            groupCell.Children.Add(label);
        }

        var cell = CreateDataEntry(value);
        groupCell.Children.Add(cell);
        return groupCell;
    }

    private IView CreateDataEntry(EditablePropertyDetails value) {
        try {
            return value.Attribute switch {
                EditableActionsPropertyAttribute     => CreateActions(value),
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
                EditableImagePropertyAttribute       => CreateImage(value),
                _                                    => CreateUndefined(value)
            };
        } catch (Exception ex) {
            Debug.WriteLine($"Unable to create entry for {value.Attribute.Name} - {ex.Message}");
            return CreateUndefined(value);
        }
    }

    private IView CreateImage(EditablePropertyDetails value) {
        var stack = new StackLayout {
            Orientation = StackOrientation.Vertical,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
            Margin = new Thickness(0, 0, 0, 0)
        };

        var imageString = PropertyHelper.GetPropertyValue<string>(value.Owner, value.Info.Name) ?? string.Empty;
        var image = new Image {
            Source = string.IsNullOrEmpty(imageString) ? null : ImageSource.FromStream(() => new MemoryStream(Convert.FromBase64String(imageString))) ?? null,
            WidthRequest = 200,
            HeightRequest = 200
        };

        var horizontal = new StackLayout {
            Orientation = StackOrientation.Horizontal,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
            Margin = new Thickness(0, 0, 0, 0)
        };

        var fileButton = new Button {
            Text = "Select Image",
            BackgroundColor = StyleColor.Get("Primary"),
            TextColor = Colors.White,
            FontSize = 15,
            Margin = new Thickness(10, 10, 10, 10)
        };

        fileButton.Clicked += async (s, e) => await SelectImageAsync(image, value);

        var photoButton = new Button {
            Text = "Select Photo",
            BackgroundColor = StyleColor.Get("Primary"),
            TextColor = Colors.White,
            FontSize = 15,
            Margin = new Thickness(10, 10, 10, 10)
        };

        photoButton.Clicked += async (s, e) => await SelectPhotoAsync(image, value);
        horizontal.Children.Add(fileButton);
        horizontal.Children.Add(photoButton);
        stack.Children.Add(image);
        stack.Children.Add(horizontal);
        return stack;
    }

    private async Task SelectPhotoAsync(Image image, EditablePropertyDetails value) {
        try {
            // Open file-picker to select an image
            var result = await MediaPicker.Default.PickPhotoAsync(new MediaPickerOptions {
                Title = "Please select an image"
            });

            if (result == null) {
                // Handle when the user cancels the file picker
                await _navigationService.DisplayOkAlertAsync("Cancelled", "No file was selected.");
                return;
            }

            // Read the selected file's content as a byte array
            var stream = await result.OpenReadAsync();
            using var memoryStream = new MemoryStream();
            await stream.CopyToAsync(memoryStream);
            var fileBytes = memoryStream.ToArray();

            // Convert to Base64 string
            var base64String = Convert.ToBase64String(fileBytes);
            image.Source = ImageSource.FromStream(() => new MemoryStream(Convert.FromBase64String(base64String)));
            PropertyHelper.SetPropertyValue<string>(value.Owner, value.Info.Name, base64String);
        } catch (Exception ex) {
            // Handle any errors
            await _navigationService.DisplayOkAlertAsync("Error", $"Unable to load Image: {ex.Message}");
        }
    }

    private async Task SelectImageAsync(Image image, EditablePropertyDetails value) {
        try {
            // Open file-picker to select an image
            var result = await FilePicker.Default.PickAsync(new PickOptions {
                PickerTitle = "Please select an image",
                FileTypes = FilePickerFileType.Images // Restrict to image files only
            });

            if (result == null) {
                // Handle when the user cancels the file picker
                await _navigationService.DisplayOkAlertAsync("Cancelled", "No file was selected.");
                return;
            }

            // Read the selected file's content as a byte array
            var stream = await result.OpenReadAsync();
            using var memoryStream = new MemoryStream();
            await stream.CopyToAsync(memoryStream);
            var fileBytes = memoryStream.ToArray();

            // Convert to Base64 string
            var base64String = Convert.ToBase64String(fileBytes);
            image.Source = ImageSource.FromStream(() => new MemoryStream(Convert.FromBase64String(base64String)));
            PropertyHelper.SetPropertyValue<string>(value.Owner, value.Info.Name, base64String);
        } catch (Exception ex) {
            // Handle any errors
            await _navigationService.DisplayOkAlertAsync("Error", $"Unable to load Image: {ex.Message}");
        }
    }

    private IView CreateActions(EditablePropertyDetails value) {
        if (value.Attribute is EditableActionsPropertyAttribute attr) {
            var prop = value.Info.GetValue(value.Owner);
            if (value.Type == typeof(TurnoutActions) && value.Info.GetValue(value.Owner) is TurnoutActions turnoutActions) {
                Console.WriteLine("Turnout Actions");
                return new TurnoutActionsView(turnoutActions, attr.IsTurnoutContext) {
                    HorizontalOptions = LayoutOptions.Fill,
                    VerticalOptions = LayoutOptions.Fill
                };
            }

            if (value.Type == typeof(ButtonActions) && value.Info.GetValue(value.Owner) is ButtonActions buttonActions) {
                Console.WriteLine("Button Actions");
                foreach (var buttonAction in buttonActions.Actions) {
                    Console.WriteLine($"---Action: {buttonAction.Id} Active={buttonAction.WhenActiveOrClosed} Inactive={buttonAction.WhenInactiveOrThrown}");
                }
                
                return new ButtonActionsList(buttonActions, TrackPiece) {
                     HorizontalOptions = LayoutOptions.Fill,
                     VerticalOptions = LayoutOptions.Fill
                };
            }
        }

        return CreateUndefined(value);
    }

    private IView CreateTurnout(EditablePropertyDetails value) {
        return CreateUndefined(value, "Turnout");

        //var cell = new Switch { BindingContext = value.Owner };
        //cell.SetBinding(Switch.IsToggledProperty, new Binding(value.Info.Name) { Source = value.Owner, Mode = BindingMode.TwoWay });
        //return cell;    
    }

    private IView CreateTrackType(EditablePropertyDetails value) {
        var attr = value.Attribute as EditableTrackTypePropertyAttribute;
        return CreateRadioGroupForEnums("Track Type", attr?.TrackTypes ?? [], value.Owner, value.Info.Name);
    }

    private IView CreateTrackImage(EditablePropertyDetails value) {
        var attr = value.Attribute as EditableTrackImagePropertyAttribute;
        return CreateRadioGroupForEnums("Track Style", attr?.TrackTypes ?? [], value.Owner, value.Info.Name);
    }

    private Entry CreateString(EditablePropertyDetails value) {
        var cell = new Entry {
            Placeholder = value.Attribute.Description,
            Keyboard = Keyboard.Text,
            WidthRequest = 300,
            BindingContext = value.Owner
        };

        cell.SetBinding(Entry.TextProperty, new Binding(value.Info.Name) { Source = value.Owner, Mode = BindingMode.TwoWay });
        return cell;
    }

    private HorizontalStackLayout CreateInt(EditablePropertyDetails value) {
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
            Minimum = attr?.MinValue ?? 0,  // Define the stepper min value if needed
            Maximum = attr?.MaxValue ?? 99, // Define the stepper max value if needed
            Increment = 1,                  // Increment/decrement step
            HorizontalOptions = LayoutOptions.End
        };

        // To initialize the Stepper with the current value
        if (int.TryParse(datacell.Text, out var initialStepperValue)) {
            updown.Value = initialStepperValue;
        }

        // Sync Stepper and Entry when Stepper value changes
        updown.ValueChanged += (s, e) => { datacell.Text = e?.NewValue.ToString(CultureInfo.InvariantCulture) ?? "0"; };

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

    private Label CreateInfo(EditablePropertyDetails value) {
        var cell = new Label { BindingContext = value.Owner, Text = value.Attribute.Description };
        return cell;
    }

    private IView CreateEnum(EditablePropertyDetails value) {
        if (value.Type == typeof(TextAlignment)) {
            var items = new[] { TextAlignment.Start, TextAlignment.Center, TextAlignment.End };
            return CreateRadioGroupForEnums("Alignment", items, value.Owner, value.Info.Name);
        }

        return CreateUndefined(value, "Enum Value Option");
    }

    private DatePicker CreateDate(EditablePropertyDetails value) {
        var cell = new DatePicker { BindingContext = value.Owner, Format = "D" };
        cell.SetBinding(DatePicker.DateProperty, new Binding(value.Info.Name) { Source = value.Owner, Mode = BindingMode.TwoWay });
        return cell;
    }

    private ColorPickerButton CreateColor(EditablePropertyDetails value) {
        var cell = new ColorPickerButton { WidthRequest = 100, HeightRequest = 30 };
        cell.SetBinding(ColorPickerButton.SelectedColorProperty, new Binding(value.Info.Name) { Source = value.Owner, Mode = BindingMode.TwoWay });
        return cell;
    }

    private Switch CreateBool(EditablePropertyDetails value) {
        var cell = new Switch { BindingContext = value.Owner, OnColor = StyleColor.Get("Primary"), ThumbColor = Colors.White };
        cell.SetBinding(Switch.IsToggledProperty, new Binding(value.Info.Name) { Source = value.Owner, Mode = BindingMode.TwoWay });
        return cell;
    }

    private Label CreateUndefined(EditablePropertyDetails value, string text = "Not yet defined") {
        return new Label {
            Text = text,
            HorizontalOptions = LayoutOptions.Start,
            VerticalOptions = LayoutOptions.Center
        };
    }

    public StackLayout CreateRadioGroupForEnums<T>(string name, T[] items, object source, string fieldName) where T : struct, Enum {
        if (source == null) throw new ArgumentNullException(nameof(source), "Binding source cannot be null.");
        if (string.IsNullOrWhiteSpace(fieldName)) throw new ArgumentException("Field name cannot be null or whitespace.", nameof(fieldName));

        var radioGroup = new StackLayout {
            HeightRequest = 30,
            Orientation = StackOrientation.Horizontal,
            HorizontalOptions = LayoutOptions.Start,
            VerticalOptions = LayoutOptions.Center,
            Margin = new Thickness(-5, 0, 0, 0)
        };

        foreach (var value in items) {
            var radioButton = new RadioButton {
                HeightRequest = 30,
                Content = value.ToString() // Display the value
            };

            radioButton.CheckedChanged += (sender, args) => { PropertyHelper.SetEnumPropertyValue(source, fieldName, value); };
            radioButton.IsChecked = value.Equals(PropertyHelper.GetEnumPropertyValue<T>(source, fieldName));
            radioGroup.Children.Add(radioButton);
        }

        return radioGroup;
    }

    public HorizontalStackLayout CreateFreakyRadioGroupForEnums<T>(object source, string fieldName) where T : struct, Enum {
        if (source == null) throw new ArgumentNullException(nameof(source), "Binding source cannot be null.");
        if (string.IsNullOrWhiteSpace(fieldName)) throw new ArgumentException("Field name cannot be null or whitespace.", nameof(fieldName));

        // Create horizontal StackLayout for the radio group
        var radioGroup = new HorizontalStackLayout();

        // Loop through each value in the enum
        foreach (var value in Enum.GetValues<T>()) {
            var radioButton = new RadioButton {
                HeightRequest = 30,
                MinimumHeightRequest = 30,
                Content = value.ToString() // Display the value
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