// ReSharper disable once CheckNamespace

namespace DCCPanelController.Models.DataModel.Entities;

public enum SwitchStyleEnum { Switch, Light, Button }

public enum ButtonStateEnum { Unknown, On, Off }

public enum TurnoutStateEnum { Unknown, Closed, Thrown }

public enum RouteStateEnum { Unknown, Active, Inactive }

public enum ButtonSizeEnum { Normal, Large }

public enum TrackTypeEnum { MainLine, BranchLine }

public enum TrackTerminatorEnum { Arrow, Lines }

public enum TrackAttributeEnum { Normal, Dashed }

public enum TurnoutStyleEnum { Standard, NoBranch }

public enum TextAlignmentHorizontalEnum {Left, Right, Center, Justify}

public enum TextAlignmentVerticalEnum {Top, Bottom, Center}

public static class EnumHelpers {

    public static HorizontalAlignment ConvertHorizontalAlignment(TextAlignmentHorizontalEnum alignment) {
        return alignment switch {
            TextAlignmentHorizontalEnum.Left    => HorizontalAlignment.Left,
            TextAlignmentHorizontalEnum.Right   => HorizontalAlignment.Right,
            TextAlignmentHorizontalEnum.Center  => HorizontalAlignment.Center,
            TextAlignmentHorizontalEnum.Justify => HorizontalAlignment.Justified,
            _                                     => HorizontalAlignment.Left
        };
    }

    public static VerticalAlignment ConvertVerticalAlignment(TextAlignmentVerticalEnum alignment) {
        return alignment switch {
            TextAlignmentVerticalEnum.Top    => VerticalAlignment.Top,
            TextAlignmentVerticalEnum.Bottom => VerticalAlignment.Bottom,
            TextAlignmentVerticalEnum.Center => VerticalAlignment.Center,
            _                                  => VerticalAlignment.Center
        };
    }

    public static TextAlignment ConvertHorizontalAlignmentToText(TextAlignmentHorizontalEnum alignment) {
        return alignment switch {
            TextAlignmentHorizontalEnum.Left    => TextAlignment.Start,
            TextAlignmentHorizontalEnum.Right   => TextAlignment.End,
            TextAlignmentHorizontalEnum.Center  => TextAlignment.Center,
            TextAlignmentHorizontalEnum.Justify => TextAlignment.Justify,
            _                                     => TextAlignment.Start
        };
    }

    public static TextAlignment ConvertVerticalAlignmentToText(TextAlignmentVerticalEnum alignment) {
        return alignment switch {
            TextAlignmentVerticalEnum.Top    => TextAlignment.Start,
            TextAlignmentVerticalEnum.Bottom => TextAlignment.End,
            TextAlignmentVerticalEnum.Center => TextAlignment.Center,
            _                                  => TextAlignment.Center
        };
    }

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