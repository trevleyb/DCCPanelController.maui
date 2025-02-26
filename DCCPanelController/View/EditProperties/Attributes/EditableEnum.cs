using System.Diagnostics;
using DCCPanelController.View.EditProperties.Base;

namespace DCCPanelController.View.EditProperties.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class EditableEnumAttribute : EditableAttribute, IEditableAttribute {
    public IView? CreateView(EditableDetails value) {
        try {
            if (value.Type == typeof(TextAlignment)) {
                var items = new[] { TextAlignment.Start, TextAlignment.Center, TextAlignment.End };
                return CreateRadioGroupForEnums("Alignment", items, value.Owner, value.Info.Name);
            }

            if (value.Type == typeof(FontWeight)) {
                var items = new[] { FontWeight.Regular, FontWeight.Bold, FontWeight.Light };
                return CreateRadioGroupForEnums("Font Weight", items, value.Owner, value.Info.Name);
            }
        } catch (Exception e) {
            Debug.WriteLine($"Unable to create a Alignment Enum:  {e.Message}");
            return null;
        }

        Debug.WriteLine($"Invalid Alignment Enum type:  {value.Type.Name} is not supported");
        return null;
    }
}