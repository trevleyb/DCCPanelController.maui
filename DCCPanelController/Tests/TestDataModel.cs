using System.Diagnostics;
using DCCPanelController.Models.DataModel;
using DCCPanelController.Models.DataModel.Entities;
using DCCPanelController.Models.DataModel.Helpers;
using DCCPanelController.Models.DataModel.Interfaces;
using DCCPanelController.Models.DataModel.Repository;

namespace DCCPanelController.Tests;

public class TestDataModel {
    public void LoadAndSaveDataModel() {
        var storage = new Storage();
        BuildTestDataModel(storage);
        ValidateTestDataModel(storage);
        JsonRepository.Save(storage);
        var loadedStorage = JsonRepository.Load();
        ValidateTestDataModel(loadedStorage);
    }

    private void BuildTestDataModel(Storage storage) {
        var original = CreatePanel(storage.Panels);
        var clone = storage.Panels.AddFrom(original);
        Debug.Assert(clone.Id != original.Id, "clone.Id != original.Id");
        Debug.Assert(clone.GetHashCode() != original.GetHashCode(), "clone.GetHashCode() != original.GetHashCode()");
    }

    private void ValidateTestDataModel(Storage storage) {
        var panelIDs = new List<string>();
        var entityIDs = new List<string>();
        foreach (var panel in storage.Panels) {
            Debug.Assert(panel.Id != null,"panel.Id != null");
            Debug.Assert(!panelIDs.Contains(panel.Id), $"panelIDs.Contains(panel.Id)");
            Debug.Assert(panel.Panels == storage.Panels, "panel.Panels == storage.Panels");
            panelIDs.Add(panel.Id);

            foreach (var entity in panel.Entities) {
                Debug.Assert(entity.Parent == panel, "entity.Parent == panel");
                if (entity is IEntityID entityID) {
                    Debug.Assert(entityID.Id != null, "entityID.Id != null");
                    Debug.Assert(!entityIDs.Contains(entityID.Id), $"entityIDs.Contains(entityID.Id)");
                    entityIDs.Add(entityID.Id);
                }
            }
        }            
    }

    private Panel CreatePanel(Panels panels) {
        var panel = panels.AddNew();
        CreateEntity<ButtonEntity>(panel);
        CreateEntity<CompassEntity>(panel);
        CreateEntity<CornerEntity>(panel);
        CreateEntity<CornerContinuationEntity>(panel);
        CreateEntity<CrossingEntity>(panel);
        CreateEntity<ImageEntity>(panel);
        CreateEntity<CircleLabelEntity>(panel);
        CreateEntity<LeftTurnoutEntity>(panel);
        CreateEntity<PointsEntity>(panel);
        CreateEntity<RightTurnoutEntity>(panel);
        CreateEntity<StraightEntity>(panel);
        CreateEntity<StraightContinuationEntity>(panel);
        CreateEntity<TerminatorEntity>(panel);
        CreateEntity<TextEntity>(panel);
        CreateEntity<CircleEntity>(panel);
        CreateEntity<RectangleEntity>(panel);
        CreateEntity<LineEntity>(panel);
        return panel;
    }

    private void CreateEntity<T>(Panel panel) where T : Entity {

        var entity = panel.AddNew<T>();
        entity.Height = 1;
        entity.Width = 1;
        entity.X = new Random().Next(0, panel.Cols);
        entity.Y = new Random().Next(0, panel.Rows);
        entity.Rotation = 0;
        entity.IsEnabled = true;
        entity.TrackType = TrackTypeEnum.Normal;
        
        switch (entity) {
        case ButtonEntity buttonEntity:
            buttonEntity.ButtonState = ButtonStateEnum.Off;
            break;

        case CircleLabelEntity circleLabelEntity:
            circleLabelEntity.Label = "Test";
            break;
        }

        var cloned = panel.AddFrom(entity);
    }
}