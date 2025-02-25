using System.Diagnostics;
using DCCPanelController.Model;
using DCCPanelController.Model.Tracks.Interfaces;
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
                if (value.Type == typeof(TurnoutActions) && value.Info.GetValue(value.Owner) is TurnoutActions turnoutActions) {
                    var turnout = value.Owner as ITrackTurnout;
                    var turnoutID = turnout?.TurnoutID ?? "";
                    var availableTurnouts = turnout?.Parent?.AllNamedTurnouts.Where(t => !string.IsNullOrWhiteSpace(t.TurnoutID) && t.TurnoutID != turnoutID).Select(t => t.TurnoutID).ToList<string>() ?? [];                
                    return new TurnoutActionsGrid(turnoutActions, attr.ActionsContext, availableTurnouts) {
                        HorizontalOptions = LayoutOptions.Fill,
                        VerticalOptions = LayoutOptions.Fill
                    };
                }

                if (value.Type == typeof(ButtonActions) && value.Info.GetValue(value.Owner) is ButtonActions buttonActions) {
                    var button = value.Owner as ITrackButton;
                    var buttonID = button?.ButtonID ?? "";
                    var availableButtons = button?.Parent?.AllNamedButtons.Where(b => !string.IsNullOrWhiteSpace(b.ButtonID) && b.ButtonID != buttonID).Select(b => b.ButtonID) .ToList<string>() ?? [];
                    return new ButtonActionsGrid(buttonActions, attr.ActionsContext, availableButtons) {
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