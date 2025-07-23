using System.Reflection;
using CommunityToolkit.Maui.Behaviors;
using CommunityToolkit.Maui.Markup;
using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Helpers;
using DCCPanelController.Models.DataModel.Entities;
using DCCPanelController.Models.DataModel.Entities.Actions;
using DCCPanelController.Models.DataModel.Entities.Interfaces;
using DCCPanelController.Models.DataModel.Helpers;
using DCCPanelController.View.Base;
using DCCPanelController.View.Converters;
using DCCPanelController.View.Properties.TileProperties.Attributes;
using DCCPanelController.View.Properties.TileProperties.EditableControls;

namespace DCCPanelController.View.Properties.TileProperties;

// Represents a property that exists across multiple entities
public record CommonProperty(
    PropertyInfo Property,
    IEditableProperty Metadata,
    object? CommonValue,        // null means "mixed values"
    bool HasMixedValues);

public static class ValueComparer {
    private static readonly HashSet<Type> MultipleEntityExcludedAttributeTypes = 
    [
        typeof(EditableID),
        typeof(EditableButtonActions),
        typeof(EditableTurnoutActions)
    ];

    public static bool AreEqual(object? value1, object? value2) {
        if (ReferenceEquals(value1, value2)) return true;
        if (value1 == null || value2 == null) return false;
        if (value1.GetType() != value2.GetType()) return false;

        return value1 switch {
            int intValue       => intValue.IsEqualTo((int)value2),
            float floatVal     => floatVal.IsEqualTo((float)value2),
            double doubleVal   => doubleVal.IsEqualTo((double)value2),
            string stringValue => stringValue.Equals(value2),
            _                  => value1.Equals(value2)
        };
    }

    public static bool IsExcludedForMultipleEntities(IEditableProperty metadata) {
        var attributeType = metadata.GetType();
        return MultipleEntityExcludedAttributeTypes.Contains(attributeType);
    }
}

public partial class DynamicPropertyPageViewModel : BaseViewModel, IPropertiesViewModel {
    private readonly StackBase _container = new VerticalStackLayout();
    private List<CommonProperty> _commonProperties = [];
    private readonly Dictionary<string, object?> _originalValues = new();

    [ObservableProperty] private Entity _proxyEntity;
    [ObservableProperty] private List<Entity> _entities;
    [ObservableProperty] private string _title;

    public DynamicPropertyPageViewModel(Entity entity) : this([entity]) { }
    public DynamicPropertyPageViewModel(List<Entity> entities) {
        if (entities is null || entities.Count == 0)
            throw new ArgumentException("There must be at least 1 Entity for Properties.");

        Entities = entities;
        ProxyEntity = entities[0].Clone();
        Title = "Property Editor";

        if (entities[0] is IActionEntity actionEntity) {
            actionEntity.CloneActionsInto((IActionEntity)ProxyEntity);
        }
    }

    public async Task ApplyChangesAsync() {
        await ApplyChangesToAllEntities();
    }

    public Microsoft.Maui.Controls.View CreatePropertiesView() {
        Title = Entities.Count == 1 ? $"{Entities[0].EntityName}" : $"Multiple Tiles ({Entities.Count})";

        if (Entities.Count == 0) {
            AddNoEntitiesMessage();
        } else {
            BuildCommonPropertyUI();
        }

        return _container;
    }

    private void BuildCommonPropertyUI() {
        _commonProperties = FindCommonProperties();

        if (_commonProperties.Count == 0) {
            AddNoCommonPropertiesMessage();
            return;
        }

        BuildUI();
    }

    private List<CommonProperty> FindCommonProperties() {
        return Entities.Count == 1 ? FindSingleEntityProperties() : FindMultiEntityCommonProperties();
    }

    private List<CommonProperty> FindSingleEntityProperties() {
        var properties = EditableExtractor.GetEditableProperties(Entities[0]);
        var result = new List<CommonProperty>();

        foreach (var (property, metadata) in properties) {
            var value = GetPropertyValue(property, Entities[0]);
            _originalValues[property.Name] = value;
            result.Add(new CommonProperty(property, metadata, value, false));
        }

        return SortProperties(result);
    }

    private List<CommonProperty> FindMultiEntityCommonProperties() {
        var firstEntityProperties = EditableExtractor.GetEditableProperties(Entities[0]);
        var result = new List<CommonProperty>();

        foreach (var (property, metadata) in firstEntityProperties) {
            if (ValueComparer.IsExcludedForMultipleEntities(metadata)) continue;
            var commonPropertyResult = AnalyzePropertyAcrossEntities(property, metadata);
            if (commonPropertyResult != null) {
                result.Add(commonPropertyResult);
            }
        }

        return SortProperties(result);
    }

    private CommonProperty? AnalyzePropertyAcrossEntities(PropertyInfo property, IEditableProperty metadata) {
        var values = new List<object?>();

        // Collect all values for this property across all entities
        foreach (var entity in Entities) {
            if (!HasProperty(entity, property))
                return null; // Property doesn't exist in all entities

            var value = GetPropertyValue(property, entity);
            values.Add(value);
        }

        // Determine if all values are the same
        var firstValue = values[0];
        var allSame = values.All(v => ValueComparer.AreEqual(firstValue, v));
        var commonValue = allSame ? firstValue : null;
        metadata.HasMixedValues = !allSame;
        _originalValues[property.Name] = commonValue;

        return new CommonProperty(property, metadata, commonValue, !allSame);
    }

    private static bool HasProperty(Entity entity, PropertyInfo property) {
        var entityProperties = EditableExtractor.GetEditableProperties(entity);
        return entityProperties.Any(p => p.Property.Name == property.Name &&
                                         p.Property.PropertyType == property.PropertyType);
    }

    private static object? GetPropertyValue(PropertyInfo property, Entity entity) {
        try {
            var targetProperty = entity.GetType().GetProperty(property.Name, property.PropertyType);
            return targetProperty?.GetValue(entity);
        } catch (Exception ex) {
            Console.WriteLine($"Error getting property {property.Name}: {ex.Message}");
            return null;
        }
    }

    private static List<CommonProperty> SortProperties(List<CommonProperty> properties) {
        return properties
              .OrderBy(p => p.Metadata.Order)
              .ThenBy(p => p.Metadata.Group)
              .ToList();
    }

    private void BuildUI() {
        (IView? Group, IList<IView>? Container) groupContainer = (null, null);
        var lastGroup = "*";

        foreach (var commonProperty in _commonProperties) {
            // Create the group if needed
            if (commonProperty.Metadata.Group != lastGroup) {
                groupContainer = CreateExpanderGroup(commonProperty.Metadata.Group, lastGroup == "*");
                _container.Children.Add(groupContainer.Group);
                lastGroup = commonProperty.Metadata.Group;
            }

            // Create UI element
            if (groupContainer.Container != null) {
                var cell = commonProperty.Metadata.CreateView(ProxyEntity, commonProperty.Property);
                if (cell != null) {
                    SetInitialValue(commonProperty);
                    groupContainer.Container.Add(cell);
                }
            }
        }
    }

    private void SetInitialValue(CommonProperty commonProperty) {
        if (commonProperty.CommonValue != null && commonProperty.Property.CanWrite) {
            try {
                commonProperty.Property.SetValue(ProxyEntity, commonProperty.CommonValue);
                commonProperty.Metadata.Value = commonProperty.CommonValue;
                commonProperty.Metadata.IsModified = false;
            } catch (Exception ex) {
                Console.WriteLine($"Error setting initial value for {commonProperty.Property.Name}: {ex.Message}");
            }
        }
    }

    public async Task ApplyChangesToAllEntities() {
        foreach (var commonProperty in _commonProperties) {
            await ApplyPropertyChange(commonProperty);
        }
    }

    private async Task ApplyPropertyChange(CommonProperty commonProperty) {
        var propertyName = commonProperty.Property.Name;
        var newValue = GetPropertyValue(commonProperty.Property, ProxyEntity);
        var originalValue = _originalValues.GetValueOrDefault(propertyName);

        // Check if the value actually changed
        var hasChanged = commonProperty.Metadata.IsModified;  //!ValueComparer.AreEqual(newValue, originalValue);
        
        // Always update ButtonActions and TurnoutActions (as per original logic)
        var forceUpdate = Entities.Count > 1 && ValueComparer.IsExcludedForMultipleEntities(commonProperty.Metadata);

        if (hasChanged || forceUpdate) {
            Console.WriteLine($"Updating property '{propertyName}': '{originalValue}' -> '{newValue}'");

            foreach (var entity in Entities) {
                await UpdateEntityProperty(entity, propertyName, newValue);
            }
        }
    }

    private async Task UpdateEntityProperty(Entity entity, string propertyName, object? newValue) {
        try {
            var entityProperties = EditableExtractor.GetEditableProperties(entity);
            var targetProperty = entityProperties.FirstOrDefault(p => p.Property.Name == propertyName).Property;

            if (targetProperty?.CanWrite == true) {
                var valueToSet = CloneValueIfNeeded(newValue, targetProperty.PropertyType);
                targetProperty.SetValue(entity, valueToSet);
            }
        } catch (Exception ex) {
            Console.WriteLine($"Error updating {entity.EntityName}.{propertyName}: {ex.Message}");
        }
    }

    private static object? CloneValueIfNeeded(object? value, Type propertyType) {
        if (value == null) return null;
        if (propertyType.IsValueType || propertyType == typeof(string)) return value;

        return value switch {
            ICloneable cloneable            => cloneable.Clone(),
            List<string> list               => new List<string>(list),
            Dictionary<string, object> dict => new Dictionary<string, object>(dict),
            _                               => value
        };
    }

    // UI Creation Methods (keeping existing implementations)
    private static (IView?, IList<IView>?) CreateExpanderGroup(string groupKey, bool isFirst) {
        if (string.IsNullOrWhiteSpace(groupKey)) return CreateGroup(groupKey);

        var tableExpander = new Expander();
        var expanderHeading = new StackLayout();
        var expanderTitle = new HorizontalStackLayout();
        expanderTitle.Children.Add(GroupChevrons(tableExpander));
        expanderTitle.Children.Add(GroupHeading(groupKey));
        expanderHeading.Children.Add(expanderTitle);
        expanderHeading.Children.Add(GroupDivider());
        tableExpander.Margin = new Thickness(0, isFirst ? 10 : 20, 10, 10);
        tableExpander.Header = expanderHeading;
        tableExpander.IsExpanded = true;

        var stackLayout = new StackLayout {
            Margin = new Thickness(0, 10, 1, 0),
            BackgroundColor = Colors.White
        };
        tableExpander.Content = stackLayout;
        return (tableExpander, stackLayout.Children);
    }

    private static Image GroupChevrons(Expander expander) {
        var chevron = new Image {
            Source = "chevron_circle_down.png",
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
            Rotation = 0
        };
        chevron.Bind(VisualElement.RotationProperty, nameof(expander.IsExpanded),
                     converter: new ExpandRotationConverter(), source: expander);
        return chevron;
    }

    private static (IView?, IList<IView>?) CreateGroup(string groupKey) {
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
        return string.IsNullOrEmpty(groupKey) ? "Properties" : $"{groupKey}";
    }

    private static BoxView GroupDivider(Color? color = null) {
        color ??= StyleColor.Get("Primary");
        return new BoxView {
            BackgroundColor = color,
            HeightRequest = 1,
            HorizontalOptions = LayoutOptions.Fill
        };
    }

    private void AddNoCommonPropertiesMessage() => AddNoPropertiesMessage("There are no common properties between the selected track elements.");

    private void AddNoEntitiesMessage() => AddNoPropertiesMessage("There are no available properties.");

    private void AddNoPropertiesMessage(string message) {
        var labelTitle = new Label {
            FontSize = 18,
            Margin = new Thickness(10, 20, 10, 20),
            FontAttributes = FontAttributes.Bold,
            Text = "No Common Properties",
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center
        };
        var labelDescription = new Label {
            FontSize = 12,
            Text = message,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center
        };
        _container?.Children?.Add(labelTitle);
        _container?.Children?.Add(labelDescription);
    }
}