using System.Text.Json;
using DCCPanelController.Clients;
using DCCPanelController.Models.DataModel.Entities;

namespace DCCPanelController.Models.DataModel.Repository;

public static class JsonOptions {
    public static readonly JsonSerializerOptions? Options = new() {
        WriteIndented = true,
        Converters = {
            new JsonSettingsTypeConverter(),
            new JsonTrackTypeConverter(),
            new MauiColorJsonConverter(),
            new JsonEnumToStringConverter<DccClientType>(),
            new JsonEnumToStringConverter<Aspect>(),
            new JsonEnumToStringConverter<FontWeight>(),
            new JsonEnumToStringConverter<TextAlignment>(),
            new JsonEnumToStringConverter<TextAlignmentHorizontalEnum>(),
            new JsonEnumToStringConverter<TextAlignmentVerticalEnum>(),
            new JsonEnumToStringConverter<SwitchStyleEnum>(),
            new JsonEnumToStringConverter<ButtonStateEnum>(),
            new JsonEnumToStringConverter<TurnoutStateEnum>(),
            new JsonEnumToStringConverter<RouteStateEnum>(),
            new JsonEnumToStringConverter<TrackTypeEnum>(),
            new JsonEnumToStringConverter<TrackAttributeEnum>(),
            new JsonEnumToStringConverter<TrackTerminatorEnum>(),
            new ExpressionFilterConverter<Turnout>(t => t.IsEditable || t.IsModified),
            new ExpressionFilterConverter<Route>  (t => t.IsEditable || t.IsModified),
            new ExpressionFilterConverter<Sensor> (t => t.IsEditable || t.IsModified),
            new ExpressionFilterConverter<Signal> (t => t.IsEditable || t.IsModified),
            new ExpressionFilterConverter<Block>  (t => t.IsEditable || t.IsModified),
            new ExpressionFilterConverter<Light>  (t => t.IsEditable || t.IsModified),
        }
    };
}
