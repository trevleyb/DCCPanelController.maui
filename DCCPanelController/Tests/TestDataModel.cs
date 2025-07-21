using System.Diagnostics;
using DCCPanelController.Models.DataModel;
using DCCPanelController.Models.DataModel.Entities;
using DCCPanelController.Models.DataModel.Entities.Interfaces;
using DCCPanelController.Models.DataModel.Repository;

namespace DCCPanelController.Tests;

public class TestDataModel {
    public async void LoadAndSaveDataModel() {
        try {
            var profile = new Profile("testProfile");
            BuildTestDataModel(profile);
            ValidateTestDataModel(profile);
            //await profile.SaveAsync();
            var loadedStorage = await JsonRepository.LoadAsync();
            ValidateTestDataModel(loadedStorage);
        } catch (Exception ex) {
            Debug.Fail("Exception thrown:",ex.Message);
        }
    }

    private void BuildTestDataModel(Profile storage) {
        var original = CreatePanel(storage.Panels);
        var clone = storage.Panels.CreatePanelFrom(original);
        storage.Panels.Add(clone);
        Debug.Assert(clone.Id != original.Id);
        Debug.Assert(clone.GetHashCode() != original.GetHashCode());
    }

    private void ValidateTestDataModel(Profile storage) {
        var panelIDs = new List<string>();
        var entityIDs = new List<string>();
        foreach (var panel in storage.Panels) {
            Debug.Assert(panel.Id != null);
            Debug.Assert(!panelIDs.Contains(panel.Id), "panelIDs.Contains(panel.Id)");
            Debug.Assert(panel.Panels == storage.Panels);
            panelIDs.Add(panel.Id);

            foreach (var entity in panel.Entities) {
                Debug.Assert(entity.Parent == panel);
                if (entity is IEntityID entityID) {
                    Debug.Assert(entityID.Id != null);
                    Debug.Assert(!entityIDs.Contains(entityID.Id), "entityIDs.Contains(entityID.Id)");
                    entityIDs.Add(entityID.Id);
                }
            }
        }
    }

    private Panel CreatePanel(Panels panels) {
        var panel = panels.CreatePanel();
        panels.Add(panel);
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
        var entity = panel.CreateEntity<T>();
        entity.Height = 1;
        entity.Width = 1;
        entity.Col = new Random().Next(0, panel.Cols);
        entity.Row = new Random().Next(0, panel.Rows);
        entity.Rotation = 0;
        entity.IsEnabled = true;
        if (entity is TrackEntity trackEntity) {
            trackEntity.TrackType = TrackTypeEnum.MainLine;
            trackEntity.TrackAttribute = TrackAttributeEnum.Normal;
        }

        switch (entity) {
        case ButtonEntity buttonEntity:
            buttonEntity.State = ButtonStateEnum.Off;
            break;

        }
        panel.AddEntity(entity);
        var cloned = panel.CreateEntityFrom(entity);
        panel.AddEntity(cloned);
    }
}