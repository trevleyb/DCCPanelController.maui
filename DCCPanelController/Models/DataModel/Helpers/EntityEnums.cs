// ReSharper disable once CheckNamespace

namespace DCCPanelController.Models.DataModel.Entities;

public enum ButtonStateEnum { Unknown, On, Off }

public enum TurnoutStateEnum { Unknown, Closed, Thrown }

public enum RouteStateEnum { Unknown, Active, Inactive }

public enum ButtonSizeEnum { Normal, Large }

public enum TrackTypeEnum { MainLine, BranchLine }

public enum TrackTerminatorEnum { Normal, Arrow, Lines }

public enum TrackAttributeEnum { Normal, Dashed }

public static class EnumHelpers {
    public static TurnoutStateEnum ConvertTurnout(string state) {
        return state switch {
            "Closed" => TurnoutStateEnum.Closed,
            "Thrown" => TurnoutStateEnum.Thrown,
            _        => TurnoutStateEnum.Unknown
        };
    }

    public static string ConvertTurnout(TurnoutStateEnum state) {
        return state switch {
            TurnoutStateEnum.Closed => "Closed",
            TurnoutStateEnum.Thrown => "Thrown",
            _                       => "Unknown"
        };
    }

    public static RouteStateEnum ConvertRoute(string state) {
        return state switch {
            "Active"   => RouteStateEnum.Active,
            "Inactive" => RouteStateEnum.Inactive,
            _          => RouteStateEnum.Unknown
        };
    }

    public static string ConvertRoute(RouteStateEnum state) {
        return state switch {
            RouteStateEnum.Active   => "Active",
            RouteStateEnum.Inactive => "Inactive",
            _                       => "Unknown"
        };
    }
}