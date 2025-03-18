using System.Diagnostics;
using System.Reflection;
using DCCPanelController.Models.DataModel;
using DCCPanelController.Models.DataModel.Entities;
using DCCPanelController.Models.DataModel.Helpers;
using DCCPanelController.Models.DataModel.Interfaces;
using DCCPanelController.View.Actions;

namespace DCCPanelController.View.DynamicProperties;

public class EditableButtonActions : EditableProperty, IEditableProperty {
    public IView? CreateView(object owner, PropertyInfo info, EditableAttribute attribute) {
        try {
            var button = owner as IEntityID;
            var buttonID = button?.Id ?? "";
            var entity = owner as ButtonEntity;
            var availableButtons = entity?.Parent?.GetAllEntitiesWithID<ButtonEntity>().Where(b => !string.IsNullOrWhiteSpace(b.Id) && b.Id != buttonID).Select(b => b.Id).ToList<string>() ?? [];
            var contextEnum = attribute.GetOption<ActionsContextEnum>(0);

            if (entity is not null) {
                return new ButtonActionsGrid(entity.ButtonActions, contextEnum, availableButtons) {
                    HorizontalOptions = LayoutOptions.Fill,
                    VerticalOptions = LayoutOptions.Fill
                };
            }
        } catch (Exception e) {
            Debug.WriteLine($"Unable to create a Action {e.Message}");
            return null;
        }
        Debug.WriteLine("Creating an Action but no valid Action attributes were found.");
        return null;
    }

    public Cell? CreateCell(object owner, PropertyInfo info, EditableAttribute attribute) {
        return new ViewCell() { View = CreateView(owner, info, attribute) as Microsoft.Maui.Controls.View };
    }
}

public class EditableTurnoutActions : IEditableProperty {
    public IView? CreateView(object owner, PropertyInfo info, EditableAttribute attribute) {
        try {
            var turnout = owner as IEntityID;
            var turnoutID = turnout?.Id ?? "";
            var entity = owner as TurnoutEntity;
            var availableButtons = entity?.Parent?.GetAllEntitiesWithID<TurnoutEntity>().Where(b => !string.IsNullOrWhiteSpace(b.Id) && b.Id != turnoutID).Select(b => b.Id).ToList<string>() ?? [];
            var contextEnum = attribute.GetOption<ActionsContextEnum>(0);

            if (entity is not null) {
                return new TurnoutActionsGrid(entity.TurnoutActions, contextEnum, availableButtons) {
                    HorizontalOptions = LayoutOptions.Fill,
                    VerticalOptions = LayoutOptions.Fill
                };
            }
        } catch (Exception e) {
            Debug.WriteLine($"Unable to create a Action {e.Message}");
            return null;
        }

        Debug.WriteLine("Creating an Action but no valid Action attributes were found.");
        return null;
    }
    public Cell? CreateCell(object owner, PropertyInfo info, EditableAttribute attribute) {
        return new ViewCell() { View = CreateView(owner, info, attribute) as Microsoft.Maui.Controls.View };
    }
}