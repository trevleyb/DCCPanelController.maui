using System.Diagnostics;
using System.Reflection;
using DCCPanelController.Models.DataModel.Entities;
using DCCPanelController.Models.DataModel.Helpers;
using DCCPanelController.Models.DataModel.Interfaces;

namespace DCCPanelController.View.DynamicProperties;

public class EditableID : IEditableProperty {
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
                VerticalOptions = LayoutOptions.Center,
                BindingContext = owner
            };
            cell.TextChanged += CellOnTextChanged;
            cell.SetBinding(Entry.TextProperty, new Binding(info.Name) { Source = owner, Mode = BindingMode.TwoWay });
            return cell;
        } catch (Exception e) {
            Debug.WriteLine($"Unable to create a String:  {e.Message}");
            return null;
        }
    }

    private void CellOnTextChanged(object? sender, TextChangedEventArgs e) {
        var isValid = true;
        if (_entity is ButtonEntity { Parent: not null } button) {
            if (button.Parent?.GetAllEntitiesWithID<ButtonEntity>().Count(b => b.Id == button.Id) > 1) isValid = false;
        }
        if (_entity is TurnoutEntity { Parent: not null } turnout) {
            if (turnout.Parent?.GetAllEntitiesWithID<TurnoutEntity>().Count(b => b.Id == turnout.Id) > 1) isValid = false;
        }
        if (sender is Entry entry) entry.TextColor = isValid ? Colors.Black : Colors.Red;
    }
}