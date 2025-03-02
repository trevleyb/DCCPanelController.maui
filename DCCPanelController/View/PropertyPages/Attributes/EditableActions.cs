using System.Diagnostics;
using DCCPanelController.Model;
using DCCPanelController.Model.Tracks.Interfaces;
using DCCPanelController.View.Actions;
using DCCPanelController.View.PropertyPages.Base;

namespace DCCPanelController.View.PropertyPages.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class EditableActionsAttribute : EditableAttribute, IEditableAttribute {
    public ActionsContext ActionsContext { get; set; }

    public IView? CreateView(EditableDetails value) {
        if (value.EditableAttribute is EditableActionsAttribute attr) {
            try {
                if (value.Type == typeof(TurnoutActions) && value.Info.GetValue(value.Owner) is TurnoutActions turnoutActions) {
                    var owner = value.Owner as ITrack;
                    var turnout = owner as ITrackTurnout;
                    var turnoutID = turnout?.ID ?? "";
                    var availableTurnouts = owner?.Parent?.AllNamedTurnouts.Where(t => !string.IsNullOrWhiteSpace(t.ID) && t.ID != turnoutID).Select(t => t.ID).ToList<string>() ?? [];

                    return new TurnoutActionsGrid(turnoutActions, attr.ActionsContext, availableTurnouts) {
                        HorizontalOptions = LayoutOptions.Fill,
                        VerticalOptions = LayoutOptions.Fill
                    };
                }

                if (value.Type == typeof(ButtonActions) && value.Info.GetValue(value.Owner) is ButtonActions buttonActions) {
                    var owner = value.Owner as ITrack;
                    var button = owner as ITrackButton;
                    var buttonID = button?.ID ?? "";
                    var availableButtons = owner?.Parent?.AllNamedButtons.Where(b => !string.IsNullOrWhiteSpace(b.ID) && b.ID != buttonID).Select(b => b.ID).ToList<string>() ?? [];

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