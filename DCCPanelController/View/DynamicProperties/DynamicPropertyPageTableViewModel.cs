using System.Diagnostics;
using System.Reflection;
using CommunityToolkit.Maui.Behaviors;
using CommunityToolkit.Maui.Markup;
using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Helpers;
using DCCPanelController.Helpers.Converters;
using DCCPanelController.Models.DataModel.Entities;
using DCCPanelController.Models.DataModel.Helpers;
using DCCPanelController.Models.ViewModel.Interfaces;

namespace DCCPanelController.View.DynamicProperties;

public partial class DynamicPropertyPageTableViewModel : BaseViewModel {
    [ObservableProperty] private string _propertyName;

    public DynamicPropertyPageTableViewModel(ITile tile, string? propertyName, StackBase propertyContainer) {
        PropertyName = propertyName ?? (string.IsNullOrEmpty(tile.Entity.Name) ? "Track" : $"{tile.Entity.Name}");
        BuildProperties(propertyContainer, tile.Entity);
    }

    /// <summary>
    ///     This is the main method that iterates over all the properties in the given ITrack and builds up a dynamic
    ///     collection of the editable properties that the track contains.
    ///     It uses attributes attached to the ITrack properties, and each of the properties knows how to create an
    ///     IView which allows the editing or viewing of that given property.
    /// </summary>
    private void BuildProperties(StackBase view, Entity entity) {
        var properties = EditableExtractor.GetEditableProperties(entity);
        var tableView = new TableView();
        var tableRoot = new TableRoot();

        var isFirst = true;
        var lastGroup = "*";
        TableSection? lastSection = null;
        foreach (var property in properties) {
            // If we have changed our group value, or this is the first property we are processing
            // then we need to create a Group header and expander for the group and get a reference to 
            // where we will add the child elements. 
            // ----------------------------------------------------------------------------------------
            if (property.Metadata.Group != lastGroup) {
                lastGroup = property.Metadata.Group;
                lastSection = new TableSection();
                if (!string.IsNullOrWhiteSpace(property.Metadata.Group)) lastSection.Title = property.Metadata.Group;
                tableRoot.Add(lastSection);
                isFirst = false;
            }

            // Assuming we have a valid container (aka. Children collection) then using the EditableType, 
            // create the views that will manage this property and add it to the children collection. 
            // ---------------------------------------------------------------------------------------------
            IEditableProperty? editableComponent = property.Metadata.Type switch {
                EditableType.Id              => new EditableID(),
                EditableType.Color           => new EditableColor(),
                EditableType.Image           => new EditableImage(),
                EditableType.Double          => new EditableDouble(),
                EditableType.Integer         => new EditableInt(),
                EditableType.Opacity         => new EditableOpacity(),
                EditableType.String          => new EditableString(),
                EditableType.Switch          => new EditableBool(),
                EditableType.ButtonActions   => new EditableButtonActions(),
                EditableType.TurnoutActions  => new EditableTurnoutActions(),
                EditableType.Alignment       => new EditableAlignment(),
                EditableType.AspectRatio     => new EditableAspectRatio(),
                EditableType.TrackType       => new EditableTrackType(),
                EditableType.TrackAttributes => new EditableTrackAttribute(),
                EditableType.TrackTerminator => new EditableTrackTerminator(),
                _                            => new EditableUndefined() // Default to undefined
            };
            var cell = editableComponent.CreateCell(property.Entity, property.Property, property.Metadata);
            try {
                if (cell is ViewCell { View: not null }) {
                    lastSection?.Add(cell);
                } else {
                    Console.WriteLine($"Unable to add cell to table for '{property.Property.Name}'");
                }
            } catch (Exception ex) {
                Console.WriteLine($"Unable to add cell to table for '{property.Property.Name}' : {ex.Message}");
            }
        }
        tableView.Root = tableRoot;
        tableView.Intent = TableIntent.Settings;
        view.Children.Add(tableView);
    }
}
