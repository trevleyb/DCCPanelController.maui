using System.Diagnostics;
using CommunityToolkit.Maui.Behaviors;
using CommunityToolkit.Maui.Markup;
using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Helpers;
using DCCPanelController.Helpers.Converters;
using DCCPanelController.Model.Tracks.Interfaces;
using DCCPanelController.View.EditProperties.Base;

namespace DCCPanelController.ViewModel;

public enum ActionsContext {
    Button,
    Turnout
}

public partial class DynamicPropertyPageViewModel : BaseViewModel {

    //private readonly NavigationService _navigationService = MauiProgram.ServiceHelper.GetService<NavigationService>();
    [ObservableProperty] private string _propertyName;

    public DynamicPropertyPageViewModel(ITrack track, string? propertyName, StackBase propertyContainer) {
        PropertyName = propertyName ?? (string.IsNullOrEmpty(track.Name) ? "Track" : $"{track.Name}");
        BuildProperties(propertyContainer, track);
    }

    /// <summary>
    ///     This is the main method that iterates over all the properties in the given ITrack and builds up a dynamic
    ///     collection of the editable properties that the track contains.
    ///     It uses attributes attached to the ITrack properties, and each of the properties knows how to create an
    ///     IView which allows the editing or viewing of that given property.
    /// </summary>
    /// <param name="tableView">A Stack (Normally VerticalStackLayout) that the properties are added to</param>
    /// <param name="track">The track piece that we will build properties for</param>
    private void BuildProperties(StackBase tableView, ITrack track) {
        var propertiesByGroup = EditableCollector.GetEditableProperties(track);
        tableView.Children.Clear();
        var isFirst = true;
        foreach (var tableGroup in propertiesByGroup.Select(group => CreateExpanderGroup(group.Key, group.Value, isFirst))) {
            tableView.Children.Add(tableGroup);
            isFirst = false;
        }
    }

    private static IView CreateExpanderGroup(string groupKey, List<EditableDetails> groupValue, bool isFirst) {
        if (string.IsNullOrWhiteSpace(groupKey)) return CreateGroup(groupKey, groupValue, isFirst);

        var tableExpander = new Expander {
            Margin = new Thickness(0, isFirst ? 0 : 10, 0, 0)
        };

        var expanderHeading = new StackLayout();
        var expanderTitle = new HorizontalStackLayout();
        expanderTitle.Children.Add(GroupChevrons(tableExpander));
        expanderTitle.Children.Add(GroupHeading(groupKey));
        expanderHeading.Children.Add(expanderTitle);
        expanderHeading.Children.Add(GroupDivider());
        tableExpander.Header = expanderHeading;
        tableExpander.IsExpanded = true;

        var tableGroup = new StackLayout {
            Margin = new Thickness(0)
        };

        foreach (var value in groupValue) {
            tableGroup.Add(GroupCell(value));
        }

        tableExpander.Content = tableGroup;
        return tableExpander;
    }

    private static Image GroupChevrons(Expander expander) {
        var chevron = new Image {
            Source = "chevron_circle_down.png", // Default chevron for expanded state
            Behaviors = {
                new IconTintColorBehavior {
                    TintColor = (Color?)Application.Current?.Resources["Primary"] ?? Colors.Black
                }
            },
            HeightRequest = 16,
            WidthRequest = 16,
            VerticalOptions = LayoutOptions.Center,
            HorizontalOptions = LayoutOptions.Start,
            Margin = new Thickness(0, 0, 5, 0),
            Rotation = 0 // Expanded state rotation (facing down)
        };

        chevron.Bind(VisualElement.RotationProperty, nameof(expander.IsExpanded), converter: new ExpandRotationConverter(), source: expander);
        return chevron;
    }

    private static StackLayout CreateGroup(string groupKey, List<EditableDetails> groupValue, bool isFirst) {
        var tableGroup = new StackLayout {
            Margin = new Thickness(0, isFirst ? 0 : 10, 0, 0)
        };

        if (!string.IsNullOrWhiteSpace(groupKey)) {
            tableGroup.Add(GroupHeading(groupKey));
            tableGroup.Add(GroupDivider());
        }

        foreach (var value in groupValue) tableGroup.Add(GroupCell(value));
        return tableGroup;
    }

    private static Label GroupHeading(string groupKey) {
        return new Label {
            Text = groupKey,
            TextColor = StyleColor.Get("Primary"),
            FontSize = 18,
            Margin = new Thickness(0, 0, 0, 0)
        };
    }

    private static BoxView GroupDivider() {
        return new BoxView {
            BackgroundColor = StyleColor.Get("Primary"),
            HeightRequest = 1,
            HorizontalOptions = LayoutOptions.Fill,
            Margin = new Thickness(0, 0, 10, 15)
        };
    }

    private static HorizontalStackLayout GroupCell(EditableDetails value) {
        var groupCell = new HorizontalStackLayout {
            Margin = new Thickness(0, 5, 0, 5)
        };

        if (!string.IsNullOrWhiteSpace(value.EditableAttribute.Name)) {
            var label = new Label {
                Text = value.EditableAttribute.Name,
                TextColor = Colors.Black,
                FontSize = 12,
                LineBreakMode = LineBreakMode.MiddleTruncation,
                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.Center,
                Margin = new Thickness(0, 5, 0, 5),
                WidthRequest = 120
            };

            groupCell.Children.Add(label);
        }

        var cell = CreateDataEntry(value);
        if (cell is not null) groupCell.Children.Add(cell);
        return groupCell;
    }

    private static IView? CreateDataEntry(EditableDetails value) {
        if (value.EditableAttribute is IEditableAttribute editProperty) {
            return editProperty.CreateView(value);
        }

        Debug.WriteLine($"Unable to create entry for {value.EditableAttribute.Name}");
        return null;
    }
}