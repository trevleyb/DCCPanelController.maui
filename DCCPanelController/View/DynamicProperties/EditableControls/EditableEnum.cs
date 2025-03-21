using System.Diagnostics;
using System.Reflection;
using DCCPanelController.Helpers;
using DCCPanelController.Models.DataModel.Entities;
using DCCPanelController.Models.DataModel.Helpers;

namespace DCCPanelController.View.DynamicProperties;

public class EditableButtonSize(string label, string description = "", int order = 0, string? group = null)
    : EditableEnum(label, description, order, group), IEditableProperty {
    public IView? CreateView(object owner, PropertyInfo info) {
        try {
            var items = new[] { ButtonSizeEnum.Normal, ButtonSizeEnum.Large};
            return CreateRadioGroupForEnums("Button Size", items, owner, info);
        } catch (Exception e) {
            Debug.WriteLine($"Unable to create a Button Size Enum:  {e.Message}");
            return null;
        }
    }
}

public class EditableAlignment(string label, string description = "", int order = 0, string? group = null)
    : EditableEnum(label, description, order, group), IEditableProperty {
    public IView? CreateView(object owner, PropertyInfo info) {
        try {
            var items = new[] { TextAlignment.Start, TextAlignment.Center, TextAlignment.End };
            return CreateRadioGroupForEnums("Alignment", items, owner, info);
        } catch (Exception e) {
            Debug.WriteLine($"Unable to create a Alignment Enum:  {e.Message}");
            return null;
        }
    }
}

public class EditableAspectRatio(string label, string description = "", int order = 0, string? group = null)
    : EditableEnum(label, description, order, group), IEditableProperty {
    public IView? CreateView(object owner, PropertyInfo info) {
        try {
            var items = new[] { Aspect.Center, Aspect.AspectFit, Aspect.AspectFill};
            return CreateRadioGroupForEnums("Aspect Ratio", items, owner, info);
        } catch (Exception e) {
            Debug.WriteLine($"Unable to create a Aspect Ratio Enum:  {e.Message}");
            return null;
        }
    }
}

public class EditableTrackType(string label, string description = "", int order = 0, string? group = null)
    : EditableEnum(label, description, order, group), IEditableProperty {
    public IView? CreateView(object owner, PropertyInfo info) {
        try {
            var items = new[] { TrackTypeEnum.MainLine, TrackTypeEnum.BranchLine };
            return CreateRadioGroupForEnums("Track Type", items, owner, info);
        } catch (Exception e) {
            Debug.WriteLine($"Unable to create a Track Type Enum:  {e.Message}");
            return null;
        }
    }
}

public class EditableTrackAttribute(string label, string description = "", int order = 0, string? group = null)
    : EditableEnum(label, description, order, group), IEditableProperty {
    public IView? CreateView(object owner, PropertyInfo info) {
        try {
            var items = new[] { TrackAttributeEnum.Normal, TrackAttributeEnum.Dashed };
            return CreateRadioGroupForEnums("Track Attribute", items, owner, info);
        } catch (Exception e) {
            Debug.WriteLine($"Unable to create a Track Attribute Enum:  {e.Message}");
            return null;
        }
    }
}

public class EditableTrackTerminator(string label, string description = "", int order = 0, string? group = null)
    : EditableEnum(label, description, order, group), IEditableProperty {
    public IView? CreateView(object owner, PropertyInfo info) {
        try {
            var items = new[] { TrackTerminatorEnum.Arrow, TrackTerminatorEnum.Lines };
            return CreateRadioGroupForEnums("Terminator", items, owner, info);
        } catch (Exception e) {
            Debug.WriteLine($"Unable to create a Track Terminator Enum:  {e.Message}");
            return null;
        }
    }
}

public abstract class EditableEnum(string label, string description = "", int order = 0, string? group = null)
    : EditableProperty(label, description, order, group) {
    protected IView? CreateRadioGroupForEnums<T>(string name, T[] items, object owner, PropertyInfo info) where T : struct, Enum {
        if (owner == null) throw new ArgumentNullException(nameof(owner), "Binding source cannot be null.");
        if (string.IsNullOrWhiteSpace(info.Name)) throw new ArgumentException("Field name cannot be null or whitespace.", nameof(info.Name));

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
                BorderWidth = 0,
                FontSize = 10,
                WidthRequest = 120,
                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.Center,
                Content = value.ToString() // Display the value
            };
            radioButton.CheckedChanged += (sender, args) => { PropertyHelper.SetEnumPropertyValue(owner, info.Name, value); };
            radioButton.IsChecked = value.Equals(PropertyHelper.GetEnumPropertyValue<T>(owner, info.Name));
            radioGroup.Children.Add(radioButton);
        }
        return CreateGroupCell(radioGroup, owner, info);
    }
}