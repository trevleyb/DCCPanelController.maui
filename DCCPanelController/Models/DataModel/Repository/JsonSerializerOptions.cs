using System.Text.Json;
using DCCPanelController.Clients;
using DCCPanelController.Models.DataModel.Accessories;
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

            new JsonEnumToStringConverter<SwitchStyleEnum>(),
            new JsonEnumToStringConverter<ButtonStateEnum>(),
            new JsonEnumToStringConverter<TurnoutStateEnum>(),
            new JsonEnumToStringConverter<RouteStateEnum>(),
            new JsonEnumToStringConverter<ButtonSizeEnum>(),
            new JsonEnumToStringConverter<TitleBarTextDisplayEnum>(),
            new JsonEnumToStringConverter<TrackTypeEnum>(),
            new JsonEnumToStringConverter<TrackStyleEnum>(),
            new JsonEnumToStringConverter<TrackAttributeEnum>(),
            new JsonEnumToStringConverter<TurnoutStyleEnum>(),

            new JsonEnumToStringConverter<TextAlignment>(),
            new JsonEnumToStringConverter<TextAlignmentHorizontalEnum>(),
            new JsonEnumToStringConverter<TextAlignmentVerticalEnum>(),

            new JsonEnumToStringConverter<AccessorySource>(),
            new JsonEnumToStringConverter<AccessoryBindingMode>(),
            
            // We can filter to only save certain Accessories but currently write all of them
            // ------------------------------------------------------------------------------
            
            new ExpressionFilterConverter<Turnout>(t => true),
            new ExpressionFilterConverter<Route>(t => true),
            new ExpressionFilterConverter<Sensor>(t => true),
            new ExpressionFilterConverter<Signal>(t => true),
            new ExpressionFilterConverter<Block>(t => true),
            new ExpressionFilterConverter<Light>(t => true),
        },
    };
}