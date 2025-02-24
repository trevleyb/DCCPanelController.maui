using System.Diagnostics;
using DCCPanelController.Model;
using DCCPanelController.View.Actions;
using DCCPanelController.View.EditProperties.Base;
using DCCPanelController.ViewModel;

namespace DCCPanelController.View.EditProperties.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class AttributesActionsAttribute : Base.Attributes, IEditableAttribute {
    public ActionsContext ActionsContext { get; set; }

    public IView? CreateView(EditableDetails value) {
        if (value.Attribute is AttributesActionsAttribute attr) {
            try {
            var prop = value.Info.GetValue(value.Owner);
        
            if (value.Type == typeof(TurnoutActions) && value.Info.GetValue(value.Owner) is TurnoutActions turnoutActions) {
                return new TurnoutActionsGrid(turnoutActions, attr.ActionsContext) {
                    HorizontalOptions = LayoutOptions.Fill,
                    VerticalOptions = LayoutOptions.Fill
                };
            }

            if (value.Type == typeof(ButtonActions) && value.Info.GetValue(value.Owner) is ButtonActions buttonActions) {
                return new ButtonActionsGrid(buttonActions, attr.ActionsContext) {
                    HorizontalOptions = LayoutOptions.Fill,
                    VerticalOptions = LayoutOptions.Fill
                };
            }
            } catch (Exception e) {
                Debug.WriteLine($"Unable to create a Action {e.Message}");
                return null;
            }
        }
        Debug.WriteLine("Creating an Action but no valid Action attributes were found.");
        return null;
    }
}