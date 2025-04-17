using System.Diagnostics;
using System.Reflection;
using DCCPanelController.Models.DataModel.Entities;
using DCCPanelController.Models.DataModel.Helpers;
using DCCPanelController.Models.DataModel.Interfaces;

namespace DCCPanelController.View.DynamicProperties;

public class EditableID(string label, string description = "", int order = 0, string? group = null)
    : EditableProperty(label, description, order, group), IEditableProperty {
    private IEntityID? _entity;
    public IView? CreateView(object owner, PropertyInfo info, Action<string>? propertyModified = null) {
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
            cell.PropertyChanged += (_, _) => propertyModified?.Invoke(info.Name);
            cell.Completed += CellOnCompleted;
            cell.SetBinding(Entry.TextProperty, new Binding(info.Name) { Source = owner, Mode = BindingMode.TwoWay });
            return CreateGroupCell(cell);
        } catch (Exception e) {
            Debug.WriteLine($"Unable to create a String:  {e.Message}");
            return null;
        }
    }

    private void CellOnCompleted(object? sender, EventArgs e) {
        if (_entity is ButtonEntity { Parent: not null } button) {
            if (!IsIDValid(button.Id)) button.Id = button.Parent.GenerateID();
        }
        if (_entity is TurnoutEntity { Parent: not null } turnout) {
            if (!IsIDValid(turnout.Id)) turnout.Id = turnout.Parent.GenerateID();
        }
    }

    private void CellOnTextChanged(object? sender, TextChangedEventArgs e) {
        var isValid = IsIDValid(e.NewTextValue);
        if (sender is Entry entry) entry.TextColor = isValid ? Colors.Black : Colors.Red;
    }

    private bool IsIDValid(string value) {
        var isValid = true;
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
        return isValid;
    }
}