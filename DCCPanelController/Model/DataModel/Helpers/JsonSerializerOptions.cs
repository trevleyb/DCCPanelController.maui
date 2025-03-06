using DCCPanelController.Helpers;
using DCCPanelController.Model.DataModel.Tracks;

namespace DCCPanelController.Model.DataModel.Helpers;

public static class JsonOptions {
    public static readonly System.Text.Json.JsonSerializerOptions? Options = new() {
        WriteIndented = true,
        Converters = {
            new JsonTrackTypeConverter(),
            new MauiColorJsonConverter(),
            new JsonEnumToStringConverter<Aspect>(),
            new JsonEnumToStringConverter<FontWeight>(),
            new JsonEnumToStringConverter<TextAlignment>(),
            new JsonEnumToStringConverter<ButtonStateEnum>(),
            new JsonEnumToStringConverter<TurnoutStateEnum>(),
            new JsonEnumToStringConverter<TrackTypeEnum>(),
            new JsonEnumToStringConverter<TerminatorStyleEnum>(),
        }
    };
}