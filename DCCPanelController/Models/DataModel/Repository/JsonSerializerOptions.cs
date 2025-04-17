using System.Text.Json;
using DCCPanelController.Models.DataModel.Entities;

namespace DCCPanelController.Models.DataModel.Repository;

public static class JsonOptions {
    public static readonly JsonSerializerOptions? Options = new() {
        WriteIndented = true,
        Converters = {
            new JsonSettingsTypeConverter(),
            new JsonTrackTypeConverter(),
            new MauiColorJsonConverter(),
            new JsonEnumToStringConverter<Aspect>(),
            new JsonEnumToStringConverter<FontWeight>(),
            new JsonEnumToStringConverter<TextAlignment>(),
            new JsonEnumToStringConverter<ButtonStateEnum>(),
            new JsonEnumToStringConverter<TurnoutStateEnum>(),
            new JsonEnumToStringConverter<RouteStateEnum>(),
            new JsonEnumToStringConverter<TrackTypeEnum>(),
            new JsonEnumToStringConverter<TrackAttributeEnum>(),
            new JsonEnumToStringConverter<TrackTerminatorEnum>()
        }
    };
}