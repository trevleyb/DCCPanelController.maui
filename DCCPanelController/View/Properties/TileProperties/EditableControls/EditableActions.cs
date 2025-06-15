using System.Reflection;
using DCCPanelController.Models.DataModel.Entities;
using DCCPanelController.Models.DataModel.Entities.Interfaces;
using DCCPanelController.View.Actions;

namespace DCCPanelController.View.Properties.TileProperties.EditableControls;

public class EditableButtonActions(string label, string description = "", int order = 0, string? group = null, ActionsContext context = ActionsContext.Button)
    : EditableProperty(label, description, order, group), IEditableProperty {
    public IView? CreateView(object owner, PropertyInfo info) {
        try {
            var entity = owner as Entity;
            var entityID = (owner as IEntityID)?.Id ?? "";
            var availableButtons = entity?.Parent?.GetAllEntitiesWithID<ButtonEntity>().Where(b => !string.IsNullOrWhiteSpace(b.Id) && b.Id != entityID).Select(b => b.Id).ToList<string>() ?? [];
            if (entity is not null) {
                return new ButtonActionsGrid(((IActionEntity)entity).ButtonPanelActions, context, availableButtons) {
                    HorizontalOptions = LayoutOptions.Fill,
                    VerticalOptions = LayoutOptions.Fill
                };
            }
        } catch (Exception e) {
            Console.WriteLine($"Unable to create a Action {e.Message}");
            return null;
        }
        Console.WriteLine("Creating an Action but no valid Action attributes were found.");
        return null;
    }
}

public class EditableTurnoutActions(string label, string description = "", int order = 0, string? group = null, ActionsContext context = ActionsContext.Button)
    : EditableProperty(label, description, order, group), IEditableProperty {
    public IView? CreateView(object owner, PropertyInfo info) {
        try {
            var entity = owner as Entity;
            var entityID = (owner as IEntityID)?.Id ?? "";

            var t1 = entity?.Parent?.GetAllEntitiesWithID<TurnoutEntity>();
            var t2 = t1?.Where(b => !string.IsNullOrWhiteSpace(b.TurnoutID));
            var t3 = t1?.Where(b => !string.IsNullOrWhiteSpace(b.TurnoutID) && b.TurnoutID != entityID);
            var t4 = t1?.Where(b => !string.IsNullOrWhiteSpace(b.TurnoutID) && b.TurnoutID != entityID).Select(b => b.TurnoutID).ToList<string>() ?? [];
            
            var b1 = entity?.Parent?.GetAllEntitiesWithID<ButtonEntity>();
            
            var availableTurnouts = entity?.Parent?.GetAllEntitiesWithID<TurnoutEntity>().Where(b => !string.IsNullOrWhiteSpace(b.TurnoutID) && b.TurnoutID != entityID).Select(b => b.TurnoutID).ToList<string>() ?? [];
            if (entity is not null) {
                return new TurnoutActionsGrid(((IActionEntity)entity).TurnoutPanelActions, context, availableTurnouts) {
                    HorizontalOptions = LayoutOptions.Fill,
                    VerticalOptions = LayoutOptions.Fill
                };
            }
        } catch (Exception e) {
            Console.WriteLine($"Unable to create a Action {e.Message}");
            return null;
        }

        Console.WriteLine("Creating an Action but no valid Action attributes were found.");
        return null;
    }
}