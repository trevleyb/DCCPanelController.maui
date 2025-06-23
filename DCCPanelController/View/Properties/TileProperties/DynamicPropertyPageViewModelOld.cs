using System.Collections.Concurrent;
using System.Reflection;
using CommunityToolkit.Maui.Behaviors;
using CommunityToolkit.Maui.Markup;
using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Helpers;
using DCCPanelController.Helpers.Converters;
using DCCPanelController.Models.DataModel.Entities;
using DCCPanelController.Models.DataModel.Entities.Actions;
using DCCPanelController.Models.DataModel.Entities.Interfaces;
using DCCPanelController.Models.DataModel.Helpers;
using DCCPanelController.View.Base;
using DCCPanelController.View.Properties.TileProperties.Attributes;
using DCCPanelController.View.Properties.TileProperties.EditableControls;

namespace DCCPanelController.View.Properties.TileProperties;

public partial class DynamicPropertyPageViewModelOld : BaseViewModel, IPropertiesViewModel {
    private readonly StackBase _container = new VerticalStackLayout();
    private List<(PropertyInfo Property, IEditableProperty Metadata)> _properties = [];
    [ObservableProperty] private Entity _proxyEntity;
    [ObservableProperty] private List<Entity> _entities;
    [ObservableProperty] private string _title;

    public DynamicPropertyPageViewModelOld(Entity entity) : this([entity]) { }

    public DynamicPropertyPageViewModelOld(List<Entity> entities) {
        if (entities is null || entities.Count == 0) throw new ArgumentException("There must be at least 1 Entity for Properties.");
        ProxyEntity = entities[0].Clone();
        if (entities[0] is IActionEntity actionEntity) {
            actionEntity.CloneActionsInto((IActionEntity)ProxyEntity);
        }
        Title = "Property Editor";
        Entities = entities;
    }

    public async Task ApplyChangesAsync() {
        Console.WriteLine($"Applying changes: Panel Name = {Title}");
        await ApplyChangesToAllEntities();
    }

    public Microsoft.Maui.Controls.View CreatePropertiesView() {
        Title = Entities.Count == 1 ? $"{Entities[0].EntityName}" : $"Multiple Tiles ({Entities.Count})";
        if (Entities.Count == 0) {
            AddNoEntitiesMessage();
        } else {
            BuildCommonPropertyUI(Entities);
        }
        return _container;
    }

    private void FindCommonProperties(List<Entity> entities) {
        var commonProperties = new List<(PropertyInfo Property, IEditableProperty Metadata)>();
        var firstEntityProperties = EditableExtractor.GetEditableProperties(entities[0]);

        foreach (var property in firstEntityProperties) {
            var isCommonProperty = true;
            var commonValue = property.Property.GetValue(entities[0]);

            // Check if this property exists in all other entities
            // ---------------------------------------------------
            if (entities.Count > 1) {
                // If this property is a ButtonActions or TurnoutActions then they 
                // cannot be considered common, so mark them as not being a common property
                // --------------------------------------------------------------------------
                if (property.Property.PropertyType == typeof(ButtonActions) ||
                    property.Property.PropertyType == typeof(TurnoutActions) ) {
                    isCommonProperty = false;
                } else {
                    for (var i = 1; i < entities.Count; i++) {
                        var entityProperties = EditableExtractor.GetEditableProperties(entities[i]);
                        if (entityProperties.All(p => p.Property.Name != property.Property?.Name)) {
                            isCommonProperty = false;
                            break;
                        }

                        // Check if we have a common value for matching items
                        // --------------------------------------------------
                        if (commonValue is not null) {
                            try {
                                var getFromEntity = entities[i];
                                var propertyGetter = property.Property;
                                var compareValue = GetPropertyValueSafely(propertyGetter, getFromEntity);

                                if (compareValue == null && propertyGetter?.PropertyType.IsValueType == true) {
                                    commonValue = null;
                                    continue;
                                }

                                if (compareValue is not null) {
                                    var isSame = commonValue switch {
                                        int intValue                        => intValue.IsEqualTo((int)compareValue),
                                        float floatVal                      => floatVal.IsEqualTo((float)compareValue),
                                        double doubleVal                    => doubleVal.IsEqualTo((double)compareValue),
                                        string stringValue                  => stringValue?.Equals(compareValue) ?? false,
                                        _ when commonValue.GetType().IsEnum => HandleEnumComparison(commonValue, compareValue),
                                        _                                   => commonValue?.Equals(compareValue) ?? false
                                    };
                                    if (!isSame) commonValue = null;
                                }
                            } catch (Exception ex) {
                                Console.WriteLine($"Error comparing values: {ex.Message}");
                                commonValue = null;
                            }
                        }
                    }
                }
            }

            // If there is only a single property, OR we found a matching property,
            // then add this property to our common property collection.
            // If there is only a single entity, all its properties should be added.
            //
            // Mark all as not modified but mark ButtonActions and TurnoutActions 
            // as modified as currently we don't know if they are modified or not. 
            // ----------------------------------------------------------------------
            if (isCommonProperty) {
                property.Metadata.Value = commonValue;
                property.Metadata.IsModified = false;
                commonProperties.Add(property);
            }
        }

        // Finally, sort the properties and store them so they can be used
        // by the other parts of the system
        // --------------------------------------------------------------
        var sorted = commonProperties
                    .OrderBy(item => item.Metadata.Order)
                    .ThenBy(item => item.Metadata.Group)
                    .ToList();

        Console.WriteLine("===== COMMON PROPERTIES =====");
        Console.WriteLine($" ==> {sorted.Count} common properties.");
        foreach (var prop in sorted) {
            Console.WriteLine($"{prop.Metadata.Label} : {prop.Property.PropertyType}");
        }

        _properties = sorted;
    }

    private static bool HandleEnumComparison(object commonValue, object compareValue) {
        return commonValue.GetType() == compareValue.GetType() &&
               commonValue.Equals(compareValue);
    }

    private static object? GetPropertyValueSafely(PropertyInfo? property, object? target) {
        if (property == null || target == null) return null;

        try {
            // Check if the property can be read
            if (!property.CanRead) {
                Console.WriteLine($"Property {property.Name} is not readable");
                return null;
            }

            // Check if property requires index parameters
            if (property.GetIndexParameters().Length > 0) {
                Console.WriteLine($"Property {property.Name} requires index parameters");
                return null;
            }

            var targetType = target.GetType();
            var targetProperty = targetType.GetProperty(property.Name, property.PropertyType);

            if (targetProperty == null) {
                Console.WriteLine($"Property {property.Name} not found on type {targetType.Name}");
                return null;
            }

            // Use the target's property instead of the original property
            // This ensures we're accessing the correct property on the correct type
            return targetProperty.GetValue(target);
        } catch (Exception ex) {
            Console.WriteLine($"Error getting property {property.Name}: {ex.Message}");
            return null;
        }
    }

    private void BuildCommonPropertyUI(List<Entity> entities) {
        FindCommonProperties(entities);
        if (_properties is not { Count: > 0 }) {
            AddNoCommonPropertiesMessage();
            return;
        }

        // Now build the UI for the common properties
        // ------------------------------------------------------------------
        (IView? Group, IList<IView>? Container) groupContainer = (null, null);

        var lastGroup = "*";
        foreach (var property in _properties) {
            // Create a group container if the group has changed
            // -------------------------------------------------------------------------------------
            if (property.Metadata.Group != lastGroup) {
                groupContainer = CreateExpanderGroup(property.Metadata.Group, lastGroup == "*");
                _container.Children.Add(groupContainer.Group);
                lastGroup = property.Metadata.Group;
            }

            // Create the UI element
            // -------------------------------------------------------------------------------------
            if (groupContainer.Container is not null) {
                var cell = property.Metadata.CreateView(ProxyEntity, property.Property);
                if (cell is not null) {
                    if (property.Metadata.Value is not null && property.Property.CanWrite) {
                        property.Property.SetValue(ProxyEntity, property.Metadata.Value);
                    }
                    groupContainer.Container.Add(cell);
                }
            }
        }
    }

    public async Task ApplyChangesToAllEntities() {
        Console.WriteLine("===== APPLYING CHANGES PROPERTIES =====");
        foreach (var modifiedProperty in _properties) {
            var propertyName = modifiedProperty.Property.Name;
            var properties = EditableExtractor.GetEditableProperties(ProxyEntity);
            var property = properties.FirstOrDefault(p => p.Property.Name == propertyName).Property;

            Console.WriteLine($"{propertyName} : modified");

            if (property != null) {
                var newValue = property.GetValue(ProxyEntity);
                var oldValue = modifiedProperty.Metadata.Value;

                // Lets make sure they are of the same type
                //if (newValue?.GetType() != oldValue?.GetType()) {
                //    Console.WriteLine($"Values are not the same type? {newValue?.GetType()} != {oldValue?.GetType()}");
                //    continue;
                //}

                // have the values actually changed? 
                var hasChanged = false;
                if (newValue == null || oldValue == null) {
                    hasChanged = newValue == null && oldValue != null || newValue != null && oldValue == null;
                } else {
                    var isSame = newValue switch {
                        int intValue                     => intValue.IsEqualTo((int)oldValue),
                        float floatVal                   => floatVal.IsEqualTo((float)oldValue),
                        double doubleVal                 => doubleVal.IsEqualTo((double)oldValue),
                        string stringValue               => stringValue?.Equals(oldValue) ?? false,
                        _ when newValue.GetType().IsEnum => HandleEnumComparison(newValue, oldValue),
                        _                                => newValue?.Equals(oldValue) ?? false
                    };
                    hasChanged = !isSame;
                }
                if (hasChanged || (property.PropertyType == typeof(ButtonActions) || property.PropertyType == typeof(TurnoutActions))) {
                    Console.WriteLine($"Values have changed so we are modifying the data: '{newValue ?? "null"}' == '{oldValue ?? "null"}'");
                    foreach (var targetEntity in Entities) {
                        var targetProperties = EditableExtractor.GetEditableProperties(targetEntity);
                        var targetProperty = targetProperties.FirstOrDefault(p => p.Property.Name == propertyName).Property;

                        if (targetProperty != null) {
                            Console.WriteLine($"updating {targetProperty.Name} : {targetProperty.PropertyType}");
                            var valueToSet = newValue;

                            // Only clone if it's a reference type (excluding string)
                            if (newValue != null &&
                                !targetProperty.PropertyType.IsValueType &&
                                targetProperty.PropertyType != typeof(string)) {
                                valueToSet = newValue switch {
                                    ICloneable cloneable            => cloneable.Clone(),
                                    List<string> list               => new List<string>(list),
                                    Dictionary<string, object> dict => new Dictionary<string, object>(dict),
                                    _                               => newValue
                                };
                            }
                            targetProperty.SetValue(targetEntity, valueToSet);
                        }
                    }
                }
            }
        }
    }

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
        var labelTitle = new Label { FontSize = 18, Margin = new Thickness(10, 20, 10, 20), FontAttributes = FontAttributes.Bold, Text = "No Common Properties", HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center };
        var labelDescription = new Label { FontSize = 12, Text = message, HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center };
        _container?.Children?.Add(labelTitle);
        _container?.Children?.Add(labelDescription);
    }
}