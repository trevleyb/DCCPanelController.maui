using System.Reflection;
using DCCPanelController.Models.DataModel;
using DCCPanelController.View.Components;

namespace DCCPanelController.View.DynamicProperties.EditableControls;

public class EditableBlockAttribute(string label, string description = "", int order = 0, string? group = null, int width = 250, int dropDownWidth=300, int dropDownHeight=200)
    : EditableProperty(label, description, order, group), IEditableProperty {
    public IView? CreateView(object owner, PropertyInfo info, Action<string>? propertyModified = null) {
        try {
            var profile = MauiProgram.ServiceHelper.GetService<Profile>();
            var blocks = profile.Blocks.Select(t => t.Id).ToList();
            
            #if IOS || ANDROID
                var picker = new Picker {
                    WidthRequest = width,
                    ItemsSource = blocks,
                    IsEnabled = blocks.Count > 0,
                    Title = "Select a Occupancy Block",
                    HorizontalOptions = LayoutOptions.Start
                };
                picker.SetBinding(DropDownBoxBase.SelectedItemProperty, new Binding(info.Name) { Source = owner, Mode = BindingMode.TwoWay });
                picker.PropertyChanged += (_, _) => propertyModified?.Invoke(info.Name);
                return CreateGroupCell(picker);
            #else
                var cell = new DropDownListBox() {
                    WidthRequest = width,
                    DropDownWidth = dropDownWidth,
                    DropDownHeight = dropDownHeight, 
                    TextSize = 9,
                    ItemsSource = blocks,
                    IsEnabled = blocks.Count > 0,
                    Placeholder = "Select a Occupancy Block",
                    HorizontalOptions = LayoutOptions.Start
                };
                cell.SetBinding(DropDownBoxBase.SelectedItemProperty, new Binding(info.Name) { Source = owner, Mode = BindingMode.TwoWay });
                cell.PropertyChanged += (_, _) => propertyModified?.Invoke(info.Name);
                return CreateGroupCell(cell);
            #endif

        } catch (Exception e) {
            Console.WriteLine($"Unable to create a Block: {e.Message}");
            return null;
        }
    }
}