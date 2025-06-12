
using System.Reflection;
using DCCPanelController.Helpers;

namespace DCCPanelController.View.Properties.TileProperties.EditableControls;

public class EditableEnum(string label, string description = "", int order = 0, string? group = null)
    : EditableProperty(label, description, order, group), IEditableProperty {
    
    public IView? CreateView(object owner, PropertyInfo info) {
        try {
            // Get the enum type from the property
            var enumType = info.PropertyType;
            
            // Handle nullable enums
            if (enumType.IsGenericType && enumType.GetGenericTypeDefinition() == typeof(Nullable<>)) {
                enumType = Nullable.GetUnderlyingType(enumType) ?? enumType;
            }
            
            if (!enumType.IsEnum) {
                throw new InvalidOperationException($"Property '{info.Name}' is not an enum type.");
            }
            
            // Get all enum values
            var enumValues = Enum.GetValues(enumType);
            
            return CreateRadioGroupForEnum(Label, enumValues, enumType, owner, info);
        } catch (Exception e) {
            Console.WriteLine($"Unable to create enum control for {info.Name}: {e.Message}");
            return null;
        }
    }
    
    private IView? CreateRadioGroupForEnum(string name, Array enumValues, Type enumType, object owner, PropertyInfo info) {
        if (owner == null) throw new ArgumentNullException(nameof(owner), "Binding source cannot be null.");
        if (string.IsNullOrWhiteSpace(info.Name)) throw new ArgumentException("Field name cannot be null or whitespace.", nameof(info.Name));

        var initialValue = Value;
        var radioGroup = new StackLayout {
            HeightRequest = 30,
            Orientation = StackOrientation.Horizontal,
            HorizontalOptions = LayoutOptions.Start,
            VerticalOptions = LayoutOptions.Center,
            Margin = new Thickness(-5, 0, 0, 0)
        };
        
        foreach (var enumValue in enumValues) {
            var radioButton = new RadioButton {
                HeightRequest = 30,
                BorderWidth = 0,
                FontSize = 10,
                Value = enumValue,
                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.Center,
                Content = enumValue.ToString()
            };
            
            radioButton.CheckedChanged += (sender, args) => {
                if (sender is RadioButton button && args.Value) {
                    var buttonVal = button.Value;
                    PropertyHelper.SetEnumPropertyValue(owner, info.Name, enumValue);
                    SetModified(buttonVal?.ToString() != initialValue?.ToString());
                }
            };
            
            // Set initial checked state
            var currentValue = info.GetValue(owner);
            radioButton.IsChecked = enumValue.Equals(currentValue);
            
            radioGroup.Children.Add(radioButton);
        }

        return CreateGroupCell(radioGroup);
    }
}




// using System.Reflection;
// using DCCPanelController.Helpers;
// using DCCPanelController.Models.DataModel.Entities;
//
// namespace DCCPanelController.View.Properties.TileProperties.EditableControls;
//
// public class EditableButtonSize(string label, string description = "", int order = 0, string? group = null)
//     : EditableEnum(label, description, order, group), IEditableProperty {
//     public IView? CreateView(object owner, PropertyInfo info) {
//         try {
//             var items = new[] { ButtonSizeEnum.Normal, ButtonSizeEnum.Large };
//             return CreateRadioGroupForEnums("Button Size", items, owner, info);
//         } catch (Exception e) {
//             Console.WriteLine($"Unable to create a Button Size Enum:  {e.Message}");
//             return null;
//         }
//     }
// }
//
// public class EditableAlignment(string label, string description = "", int order = 0, string? group = null)
//     : EditableEnum(label, description, order, group), IEditableProperty {
//     public IView? CreateView(object owner, PropertyInfo info) {
//         try {
//             var items = new[] { TextAlignment.Start, TextAlignment.Center, TextAlignment.End };
//             return CreateRadioGroupForEnums("Alignment", items, owner, info);
//         } catch (Exception e) {
//             Console.WriteLine($"Unable to create a Alignment Enum:  {e.Message}");
//             return null;
//         }
//     }
// }
//
// public class EditableAspectRatio(string label, string description = "", int order = 0, string? group = null)
//     : EditableEnum(label, description, order, group), IEditableProperty {
//     public IView? CreateView(object owner, PropertyInfo info) {
//         try {
//             var items = new[] { Aspect.Center, Aspect.AspectFit, Aspect.AspectFill };
//             return CreateRadioGroupForEnums("Aspect Ratio", items, owner, info);
//         } catch (Exception e) {
//             Console.WriteLine($"Unable to create a Aspect Ratio Enum:  {e.Message}");
//             return null;
//         }
//     }
// }
//
// public class EditableTrackType(string label, string description = "", int order = 0, string? group = null)
//     : EditableEnum(label, description, order, group), IEditableProperty {
//     public IView? CreateView(object owner, PropertyInfo info) {
//         try {
//             var items = new[] { TrackTypeEnum.MainLine, TrackTypeEnum.BranchLine };
//             return CreateRadioGroupForEnums("Track Type", items, owner, info);
//         } catch (Exception e) {
//             Console.WriteLine($"Unable to create a Track Type Enum:  {e.Message}");
//             return null;
//         }
//     }
// }
//
// public class EditableSwitchStyle(string label, string description = "", int order = 0, string? group = null)
//     : EditableEnum(label, description, order, group), IEditableProperty {
//     public IView? CreateView(object owner, PropertyInfo info) {
//         try {
//             var items = new[] { TrackTypeEnum.MainLine, TrackTypeEnum.BranchLine };
//             return CreateRadioGroupForEnums("Track Type", items, owner, info);
//         } catch (Exception e) {
//             Console.WriteLine($"Unable to create a Track Type Enum:  {e.Message}");
//             return null;
//         }
//     }
// }
//
// public class EditableTrackAttribute(string label, string description = "", int order = 0, string? group = null)
//     : EditableEnum(label, description, order, group), IEditableProperty {
//     public IView? CreateView(object owner, PropertyInfo info) {
//         try {
//             var items = new[] { TrackAttributeEnum.Normal, TrackAttributeEnum.Dashed };
//             return CreateRadioGroupForEnums("Track Attribute", items, owner, info);
//         } catch (Exception e) {
//             Console.WriteLine($"Unable to create a Track Attribute Enum:  {e.Message}");
//             return null;
//         }
//     }
// }
//
// public class EditableTrackTerminator(string label, string description = "", int order = 0, string? group = null)
//     : EditableEnum(label, description, order, group), IEditableProperty {
//     public IView? CreateView(object owner, PropertyInfo info) {
//         try {
//             var items = new[] { TrackTerminatorEnum.Arrow, TrackTerminatorEnum.Lines };
//             return CreateRadioGroupForEnums("Terminator", items, owner, info);
//         } catch (Exception e) {
//             Console.WriteLine($"Unable to create a Track Terminator Enum:  {e.Message}");
//             return null;
//         }
//     }
// }
//
// public abstract class EditableEnum(string label, string description = "", int order = 0, string? group = null)
//     : EditableProperty(label, description, order, group) {
//     protected IView? CreateRadioGroupForEnums<T>(string name, T[] items, object owner, PropertyInfo info) where T : struct, Enum {
//         if (owner == null) throw new ArgumentNullException(nameof(owner), "Binding source cannot be null.");
//         if (string.IsNullOrWhiteSpace(info.Name)) throw new ArgumentException("Field name cannot be null or whitespace.", nameof(info.Name));
//
//         var initialValue = Value; 
//         var radioGroup = new StackLayout {
//             HeightRequest = 30,
//             Orientation = StackOrientation.Horizontal,
//             HorizontalOptions = LayoutOptions.Start,
//             VerticalOptions = LayoutOptions.Center,
//             Margin = new Thickness(-5, 0, 0, 0)
//         };
//         
//         foreach (var value in items) {
//             var radioButton = new RadioButton {
//                 HeightRequest = 30,
//                 BorderWidth = 0,
//                 FontSize = 10,
//                 Value = value,
//                 HorizontalOptions = LayoutOptions.Start,
//                 VerticalOptions = LayoutOptions.Center,
//                 Content = value.ToString() // Display the value
//             };
//             radioButton.CheckedChanged += (sender, args) => {
//                 if (sender is RadioButton button) {
//                     if (args.Value) {
//                         var buttonVal = button.Value;
//                         PropertyHelper.SetEnumPropertyValue(owner, info.Name, value);
//                         SetModified(buttonVal?.ToString() != initialValue?.ToString());
//                     }
//                 }
//             };
//             radioButton.IsChecked = value.Equals(PropertyHelper.GetEnumPropertyValue<T>(owner, info.Name));
//             radioGroup.Children.Add(radioButton);
//         }
//
//         return CreateGroupCell(radioGroup);
//     }
// }