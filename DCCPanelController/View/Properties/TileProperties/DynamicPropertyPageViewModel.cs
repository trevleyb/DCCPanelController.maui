using System.Collections.Concurrent;
using System.Reflection;
using CommunityToolkit.Maui.Behaviors;
using CommunityToolkit.Maui.Markup;
using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Helpers;
using DCCPanelController.Helpers.Converters;
using DCCPanelController.Models.DataModel.Entities;
using DCCPanelController.View.DynamicProperties.Attributes;
using DCCPanelController.View.DynamicProperties.EditableControls;

namespace DCCPanelController.View.Properties.TileProperties;

public partial class DynamicPropertyPageViewModel : Base.BaseViewModel, IPropertiesViewModel {
    private readonly ConcurrentDictionary<string, bool> _modifiedProperties = new();
    [ObservableProperty] private List<Entity> _entities;
    [ObservableProperty] private Entity _entity;
    [ObservableProperty] private Entity? _proxyEntity;

    private StackBase? _stackBase;
    [ObservableProperty] private string _title;

    [Obsolete("Here for backwards compatibility. Remove ASAP")]
    public DynamicPropertyPageViewModel(List<Entity> entities, StackBase? stackBase = null) : this(entities) {
        _stackBase = stackBase;
    }

    public DynamicPropertyPageViewModel(Entity entity) : this([entity]) { }

    public DynamicPropertyPageViewModel(List<Entity> entities) {
        Entities = entities;
        Entity = entities.FirstOrDefault() ?? throw new Exception("No entities found");
        Title = entities.Count == 1 ? $"{Entity.EntityName}" : $"Multiple Tiles ({entities.Count})";
    }

    public Task ApplyChangesAsync() {
        Console.WriteLine($"Applying changes: Panel Name = {Title}");
        return Task.CompletedTask;
    }

    public Microsoft.Maui.Controls.View CreatePropertiesView() {
        _stackBase ??= new VerticalStackLayout();
        if (Entities.Count == 1) {
            BuildPropertiesForSingleEntity(_stackBase, Entities[0]);
        } else {
            BuildPropertiesForMultipleEntities(_stackBase, Entities);
            _modifiedProperties.Clear();
        }
        return _stackBase;
    }

    /// <summary>
    ///     Determines if a property has been explicitly modified by the user
    /// </summary>
    public bool IsPropertyModified(string propertyName) {
        return _modifiedProperties.ContainsKey(propertyName) && _modifiedProperties[propertyName];
    }

    /// <summary>
    ///     Marks a property as modified by the user
    /// </summary>
    public void MarkPropertyModified(string propertyName) {
        _modifiedProperties[propertyName] = true;
    }

    public void ApplyChangesToAllEntities() {
        if (Entities.Count <= 1) return; // No need for special handling with single entity

        foreach (var propertyName in _modifiedProperties.Keys) {
            if (!_modifiedProperties[propertyName]) continue; // Skip properties not actually modified

            // Get the property value from the proxy entity (Entity)
            var properties = EditableExtractor.GetEditableProperties(Entity);
            var property = properties.FirstOrDefault(p => p.Property.Name == propertyName).Property;

            if (property != null) {
                var newValue = property.GetValue(Entity);
                foreach (var targetEntity in Entities) {
                    var targetProperties = EditableExtractor.GetEditableProperties(targetEntity);
                    var targetProperty = targetProperties.FirstOrDefault(p => p.Property.Name == propertyName).Property;
                    if (targetProperty != null) {
                        targetProperty.SetValue(targetEntity, newValue);
                    }
                }
            }
        }
    }

    private void BuildPropertiesForSingleEntity(StackBase tableView, Entity entity) {
        var properties = EditableExtractor.GetEditableProperties(entity);
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
    ///     New behavior for handling multiple entities. Identifies common properties across all entities
    ///     and only displays those. For each common property, it sets the default value based on whether
    ///     all entities have the same value for that property.
    /// </summary>
    private void BuildPropertiesForMultipleEntities(StackBase tableView, List<Entity> entities) {
        if (entities.Count == 0) return;

        // Get properties from the first entity as a starting point
        var firstEntityProperties = EditableExtractor.GetEditableProperties(entities[0]);

        // Find common properties across all entities
        var commonProperties = new List<(PropertyInfo Property, IEditableProperty Metadata)>();

        foreach (var property in firstEntityProperties) {
            var isCommonProperty = true;

            // Check if this property exists in all other entities
            for (var i = 1; i < entities.Count; i++) {
                var entityProperties = EditableExtractor.GetEditableProperties(entities[i]);
                if (entityProperties.All(p => p.Property.Name != property.Property.Name)) {
                    isCommonProperty = false;
                    break;
                }
            }

            if (isCommonProperty) {
                commonProperties.Add(property);
            }
        }

        // Now build the UI for the common properties
        var isFirst = true;
        var lastGroup = "*";
        (IView? Group, IList<IView>? Container) groupContainer = (null, null);

        var addedProperties = false;
        foreach (var property in commonProperties) {
            // Create group/expander if needed
            if (property.Metadata.Group != lastGroup) {
                groupContainer = CreateExpanderGroup(property.Metadata.Group, isFirst);
                tableView.Children.Add(groupContainer.Group);
                lastGroup = property.Metadata.Group;
                isFirst = false;
            }

            if (groupContainer.Container is not null) {
                // Check if all entities have the same value for this property
                var firstValue = property.Property.GetValue(entities[0]);
                for (var i = 1; i < entities.Count; i++) {
                    // Get the corresponding property from the other entity
                    var entityProperties = EditableExtractor.GetEditableProperties(entities[i]);
                    var matchingProperty = entityProperties.FirstOrDefault(p => p.Property.Name == property.Property.Name);

                    if (matchingProperty.Property != null) {
                        var currentValue = matchingProperty.Property.GetValue(entities[i]);

                        // Compare values, handling null cases
                        if (firstValue == null && currentValue != null ||
                            firstValue != null && !firstValue.Equals(currentValue)) {
                            break;
                        }
                    }
                }

                // Initialize the property with a common value if all entities have the same value,
                // otherwise use the first entity's value but mark it as not modified
                var propertyName = property.Property.Name;
                _modifiedProperties[propertyName] = false; // Initially mark as not modified

                // Create the UI element
                var cell = property.Metadata.CreateView(Entity, property.Property, MarkPropertyModified);
                if (cell is not null) {
                    // Add a property changed handler to detect when the user modifies this property
                    if (cell is BindableObject bindable) {
                        // Create a unique binding context for this property
                        var context = new PropertyBindingContext {
                            PropertyName = propertyName,
                            PropertyViewModel = this,
                            OriginalBindingContext = bindable.BindingContext
                        };
                        bindable.BindingContext = context;
                    }
                    groupContainer.Container.Add(cell);
                    addedProperties = true;
                }
            }
        }

        // If we have no common properties, add a placeholder
        // ----------------------------------------------------------------------
        if (addedProperties == false) {
            var labelTitle = new Label { FontSize = 18, Margin = new Thickness(10, 20, 10, 20), FontAttributes = FontAttributes.Bold, Text = "No Common Properties", HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center };
            var labelDescription = new Label { FontSize = 12, Text = "There are no common properties between the selected track elements.", HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center };
            tableView?.Children?.Add(labelTitle);
            tableView?.Children?.Add(labelDescription);
        }
    }

    private Entity CreateProxyEntity(Entity template, PropertyInfo property) {
        // Create a shallow clone of the template entity
        var proxy = template.Clone();

        // Set the specific property to its default value
        if (property.PropertyType.IsValueType) {
            // For value types, use Activator to create a default instance
            property.SetValue(proxy, Activator.CreateInstance(property.PropertyType));
        } else {
            // For reference types, set to null
            property.SetValue(proxy, null);
        }
        return proxy;
    }

    /// <summary>
    ///     Creates an expander group with a specified header and container for child elements,
    ///     allowing for collapsible sections in the UI. If the group key is empty or whitespace,
    ///     a scrollview-based group is created instead.
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
        tableExpander.Margin = new Thickness(0, isFirst ? 10 : 20, 0, 10);
        tableExpander.Header = expanderHeading;
        tableExpander.IsExpanded = true;

        var stackLayout = new StackLayout {
            Margin = new Thickness(-5, 1, 1, 0),
            BackgroundColor = Colors.White
        };
        tableExpander.Content = stackLayout;
        return (tableExpander, stackLayout.Children);
    }

    /// <summary>
    ///     Generates an image component styled as a chevron icon, which visually represents the expanded or collapsed state of
    ///     the provided Expander control.
    ///     The rotation of the chevron adjusts dynamically based on the state of the Expander (expanded or collapsed).
    /// </summary>
    /// <param name="expander">
    ///     The Expander control whose collapsed or expanded state dictates the rotation and visual representation of the
    ///     chevron.
    /// </param>
    /// <returns>
    ///     An Image representing the chevron icon for the Expander, with its appearance and rotation dynamically bound to the
    ///     Expander's state.
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
    ///     Creates a UI group container and associated child item collection, optionally including a heading and divider if
    ///     the group key is non-empty.
    /// </summary>
    /// <param name="groupKey">The key identifying the group. If non-empty, a group heading and divider are added.</param>
    /// <param name="isFirst">Indicates whether this is the first group in the layout, used to adjust the top margin spacing.</param>
    /// <returns>
    ///     A tuple containing the UI group container as the first element and the collection of child items within the
    ///     group as the second element.
    /// </returns>
    private static (IView?, IList<IView>?) CreateGroup(string groupKey, bool isFirst) {
        var scrollGroup = new ScrollView();
        var tableGroup = new StackLayout {
            Margin = new Thickness(0, 10, 0, 0)
        };

        if (!string.IsNullOrWhiteSpace(groupKey)) {
            tableGroup.Add(GroupHeading(groupKey));
            tableGroup.Add(GroupDivider());
        }
        scrollGroup.Content = tableGroup;
        return (scrollGroup, tableGroup.Children);
    }

    /// <summary>
    ///     Creates a heading label for a group in the UI. This label displays the provided group key as its text
    ///     and applies predefined styles such as text color and font size.
    /// </summary>
    /// <param name="groupKey">The text to display as the group heading. Typically represents the group's name or category.</param>
    /// <returns>A <see cref="Label" /> instance configured with the specified group key as its text and styled accordingly.</returns>
    private static Label GroupHeading(string groupKey) {
        return new Label {
            Text = FormatLabel(groupKey),
            TextColor = StyleColor.Get("Primary"),
            FontSize = 18,
            Margin = new Thickness(0, 0, 0, 0)
        };
    }

    private static string FormatLabel(string groupKey) {
        if (groupKey.EndsWith("s")) return groupKey;
        return string.IsNullOrEmpty(groupKey) ? "Properties" : $"{groupKey} properties";
    }

    /// <summary>
    ///     Creates a visual divider element to separate groups or sections within the property views.
    /// </summary>
    /// <returns>A BoxView element styled as a horizontal divider with specific dimensions and margins.</returns>
    private static BoxView GroupDivider(Color? color = null) {
        color ??= StyleColor.Get("Primary");
        return new BoxView {
            BackgroundColor = color,
            HeightRequest = 1,
            HorizontalOptions = LayoutOptions.Fill
        };
    }

    // Helper class to maintain binding context for each property
    private class PropertyBindingContext {
        public string PropertyName { get; set; } = "";
        public DynamicPropertyPageViewModel? PropertyViewModel { get; set; }
        public object OriginalBindingContext { get; set; } = "";
    }
}