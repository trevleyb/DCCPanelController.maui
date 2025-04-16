using System.Diagnostics;
using System.Reflection;
using CommunityToolkit.Maui.Behaviors;
using CommunityToolkit.Maui.Markup;
using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Helpers;
using DCCPanelController.Helpers.Converters;
using DCCPanelController.Models.DataModel.Entities;
using DCCPanelController.Models.DataModel.Helpers;

namespace DCCPanelController.View.DynamicProperties;

public partial class DynamicPropertyPageViewModel : BaseViewModel {
    [ObservableProperty] private string _propertyName;
    [ObservableProperty] private Entity _entity;
    public DynamicPropertyPageViewModel(Entity entity, string? propertyName, StackBase propertyContainer) {
        Entity = entity;
        PropertyName = propertyName ?? (string.IsNullOrEmpty(entity.EntityName) ? "Track" : $"{entity.EntityName}");
        BuildProperties(propertyContainer, entity);
    }

    /// <summary>
    ///     This is the main method that iterates over all the properties in the given ITrack and builds up a dynamic
    ///     collection of the editable properties that the track contains.
    ///     It uses attributes attached to the ITrack properties, and each of the properties knows how to create an
    ///     IView which allows the editing or viewing of that given property.
    /// </summary>
    private void BuildProperties(StackBase tableView, Entity entity) {
        var properties = EditableExtractor.GetEditableProperties(entity);
        tableView.Children.Clear();
        var isFirst = true;

        var lastGroup = "*";
        (IView? Group, IList<IView>? Container) groupContainer = (null, null);
        foreach (var property in properties) {
            
            // If we have changed our group value, or this is the first property we are processing
            // then we need to create a Group header and expander for the group and get a reference to 
            // where we will add the child elements. 
            // ----------------------------------------------------------------------------------------
            if (property.Metadata.Group != lastGroup) {
                groupContainer = CreateExpanderGroup(property.Metadata.Group, isFirst);
                tableView.Children.Add(groupContainer.Group);
                lastGroup = property.Metadata.Group;
                isFirst = false;
            }

            // Assuming we have a valid container (aka. Children collection) then using the EditableType, 
            // create the views that will manage this property and add it to the children collection. 
            // ---------------------------------------------------------------------------------------------
            if (groupContainer.Container is not null) {
                var cell = property.Metadata.CreateView(entity, property.Property);
                if (cell is not null) groupContainer.Container.Add(cell);
            }
        }
    }

    /// <summary>
    /// Creates an expander group with a specified header and container for child elements,
    /// allowing for collapsible sections in the UI. If the group key is empty or whitespace,
    /// a scrollview-based group is created instead.
    /// </summary>
    /// <param name="groupKey">The unique key of the group, used as its heading label.</param>
    /// <param name="isFirst">Indicates whether this is the first group in the sequence.</param>
    /// <returns>A tuple containing the expander view or scrollview, and a collection of views as its child container.</returns>
    private static (IView?, IList<IView>?) CreateExpanderGroup(string groupKey, bool isFirst) {
        if (string.IsNullOrWhiteSpace(groupKey)) return CreateGroup(groupKey, isFirst);

        var tableExpander = new Expander();
        var expanderHeading = new StackLayout();
        var expanderTitle = new HorizontalStackLayout();
        expanderTitle.Children.Add(GroupChevrons(tableExpander));
        expanderTitle.Children.Add(GroupHeading(groupKey));
        expanderHeading.Children.Add(expanderTitle);
        expanderHeading.Children.Add(GroupDivider());
        tableExpander.Margin = new Thickness(0, isFirst ? 10 : 20, 0, 0);
        tableExpander.Header = expanderHeading;
        tableExpander.IsExpanded = true;

        var stackLayout = new StackLayout {
            Margin = new Thickness(0),
            BackgroundColor = Colors.White,
        };
        tableExpander.Content = stackLayout;
        return (tableExpander, stackLayout.Children);
    }

    /// <summary>
    /// Generates an image component styled as a chevron icon, which visually represents the expanded or collapsed state of the provided Expander control.
    /// The rotation of the chevron adjusts dynamically based on the state of the Expander (expanded or collapsed).
    /// </summary>
    /// <param name="expander">
    /// The Expander control whose collapsed or expanded state dictates the rotation and visual representation of the chevron.
    /// </param>
    /// <returns>
    /// An Image representing the chevron icon for the Expander, with its appearance and rotation dynamically bound to the Expander's state.
    /// </returns>
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

    /// <summary>
    /// Creates a UI group container and associated child item collection, optionally including a heading and divider if the group key is non-empty.
    /// </summary>
    /// <param name="groupKey">The key identifying the group. If non-empty, a group heading and divider are added.</param>
    /// <param name="isFirst">Indicates whether this is the first group in the layout, used to adjust the top margin spacing.</param>
    /// <returns>A tuple containing the UI group container as the first element and the collection of child items within the group as the second element.</returns>
    private static (IView?, IList<IView>?) CreateGroup(string groupKey, bool isFirst) {
        var scrollGroup = new ScrollView();
        var tableGroup = new StackLayout {
            Margin = new Thickness(0, isFirst ? 0 : 10, 0, 0)
        };

        if (!string.IsNullOrWhiteSpace(groupKey)) {
            tableGroup.Add(GroupHeading(groupKey));
            tableGroup.Add(GroupDivider());
        }
        scrollGroup.Content = tableGroup;
        return (scrollGroup, tableGroup.Children);
    }

    /// <summary>
    /// Creates a heading label for a group in the UI. This label displays the provided group key as its text
    /// and applies predefined styles such as text color and font size.
    /// </summary>
    /// <param name="groupKey">The text to display as the group heading. Typically represents the group's name or category.</param>
    /// <returns>A <see cref="Label"/> instance configured with the specified group key as its text and styled accordingly.</returns>
    private static Label GroupHeading(string groupKey) {
        return new Label {
            Text = groupKey,
            TextColor = StyleColor.Get("Primary"),
            FontSize = 18,
            Margin = new Thickness(0, 0, 0, 0)
        };
    }

    /// <summary>
    /// Creates a visual divider element to separate groups or sections within the property views.
    /// </summary>
    /// <returns>A BoxView element styled as a horizontal divider with specific dimensions and margins.</returns>
    private static BoxView GroupDivider(Color? color = null) {
        color ??= StyleColor.Get("Primary");
        return new BoxView {
            BackgroundColor = color,
            HeightRequest = 1,
            HorizontalOptions = LayoutOptions.Fill,
        };
    }
}