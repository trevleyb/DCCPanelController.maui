using System.Diagnostics;
using System.Reflection;
using DCCPanelController.Models.DataModel.Entities;
using DCCPanelController.Models.DataModel.Helpers;
using DCCPanelController.Models.DataModel.Interfaces;

namespace DCCPanelController.View.DynamicProperties;

public class EditableID : EditableProperty, IEditableProperty {
    private IEntityID? _entity;
    public IView? CreateView(object owner, PropertyInfo info, EditableAttribute attribute) {
        try {
            _entity = owner as IEntityID;
            var cell = new Entry {
                Margin = new Thickness(5, 5, 5, 5),
                Placeholder = "Unique ID",
                Keyboard = Keyboard.Text,
                WidthRequest = 100,
                HeightRequest = 25,
                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.Center,
                BindingContext = owner
            };
            cell.TextChanged += CellOnTextChanged;
            cell.SetBinding(Entry.TextProperty, new Binding(info.Name) { Source = owner, Mode = BindingMode.TwoWay });
            return CreateGroupCell(cell, owner, info, attribute);
        } catch (Exception e) {
            Debug.WriteLine($"Unable to create a String:  {e.Message}");
            return null;
        }
    }

    public Cell? CreateCell(object owner, PropertyInfo info, EditableAttribute attribute) {
        return new ViewCell() { View = CreateView(owner, info, attribute) as Microsoft.Maui.Controls.View };
    }
    
    private void CellOnTextChanged(object? sender, TextChangedEventArgs e) {
        var isValid = true;
        var value = e.NewTextValue;
        if (_entity is ButtonEntity { Parent: not null } button) {
            var buttons = button.Parent?.GetAllEntitiesWithID<ButtonEntity>();
            var conflictingButtons = buttons?.Where(b => b.Id == value).ToArray() ?? null;
            isValid = conflictingButtons == null || (conflictingButtons.Length == 1 && conflictingButtons[0].Equals(button));
        }
        if (_entity is TurnoutEntity { Parent: not null } turnout) {
            var turnouts = turnout.Parent?.GetAllEntitiesWithID<TurnoutEntity>();
            var conflictingTurnouts = turnouts?.Where(b => b.Id == value).ToArray() ?? null;
            isValid = conflictingTurnouts == null || (conflictingTurnouts.Length == 1 && conflictingTurnouts[0].Equals(turnout));
        }
        if (sender is Entry entry) entry.TextColor = isValid ? Colors.Black : Colors.Red;
    }
}