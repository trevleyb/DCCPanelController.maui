using System.Diagnostics;
using System.Reflection;
using DCCPanelController.Helpers;
using DCCPanelController.Models.DataModel.Entities;
using DCCPanelController.Models.DataModel.Helpers;

namespace DCCPanelController.View.DynamicProperties;

public class EditableAlignment : EditableEnum, IEditableProperty {
    public IView? CreateView(object owner, PropertyInfo info, EditableAttribute attribute) {
        try {
            var items = new[] { TextAlignment.Start, TextAlignment.Center, TextAlignment.End };
            return CreateRadioGroupForEnums("Alignment", items, owner, info.Name);
        } catch (Exception e) {
            Debug.WriteLine($"Unable to create a Alignment Enum:  {e.Message}");
            return null;
        }
    }
}

public class EditableAspectRatio : EditableEnum, IEditableProperty {
    public IView? CreateView(object owner, PropertyInfo info, EditableAttribute attribute) {
        try {
            var items = new[] { Aspect.Center, Aspect.AspectFit, Aspect.AspectFill};
            return CreateRadioGroupForEnums("Aspect Ratio", items, owner, info.Name);
        } catch (Exception e) {
            Debug.WriteLine($"Unable to create a Aspect Ratio Enum:  {e.Message}");
            return null;
        }
    }
}

public class EditableTrackType : EditableEnum, IEditableProperty {
    public IView? CreateView(object owner, PropertyInfo info, EditableAttribute attribute) {
        try {
            var items = new[] { TrackTypeEnum.MainLine, TrackTypeEnum.BranchLine };
            return CreateRadioGroupForEnums("Track Type", items, owner, info.Name);
        } catch (Exception e) {
            Debug.WriteLine($"Unable to create a Track Type Enum:  {e.Message}");
            return null;
        }
    }
}

public class EditableTrackAttribute : EditableEnum, IEditableProperty {
    public IView? CreateView(object owner, PropertyInfo info, EditableAttribute attribute) {
        try {
            var items = new[] { TrackAttributeEnum.Normal, TrackAttributeEnum.Dashed, TrackAttributeEnum.Opaque };
            return CreateRadioGroupForEnums("Track Attribute", items, owner, info.Name);
        } catch (Exception e) {
            Debug.WriteLine($"Unable to create a Track Attribute Enum:  {e.Message}");
            return null;
        }
    }
}

public class EditableTrackTerminator : EditableEnum, IEditableProperty {
    public IView? CreateView(object owner, PropertyInfo info, EditableAttribute attribute) {
        try {
            var items = new[] { TrackTerminatorEnum.Arrow, TrackTerminatorEnum.Lines };
            return CreateRadioGroupForEnums("Terminator", items, owner, info.Name);
        } catch (Exception e) {
            Debug.WriteLine($"Unable to create a Track Terminator Enum:  {e.Message}");
            return null;
        }
    }
}

public abstract class EditableEnum  {
    protected StackLayout CreateRadioGroupForEnums<T>(string name, T[] items, object source, string fieldName) where T : struct, Enum {
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
                BorderWidth = 0,
                FontSize = 12,
                VerticalOptions = LayoutOptions.Center,
                Content = value.ToString() // Display the value
            };

            radioButton.CheckedChanged += (sender, args) => { PropertyHelper.SetEnumPropertyValue(source, fieldName, value); };
            radioButton.IsChecked = value.Equals(PropertyHelper.GetEnumPropertyValue<T>(source, fieldName));
            radioGroup.Children.Add(radioButton);
        }

        return radioGroup;
    }
}