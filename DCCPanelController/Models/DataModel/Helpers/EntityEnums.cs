// ReSharper disable once CheckNamespace

namespace DCCPanelController.Models.DataModel.Entities;

public enum SwitchStyleEnum { Switch, Light, Button }

public enum ButtonStateEnum { Unknown, On, Off }

public enum TurnoutStateEnum { Unknown, Closed, Thrown }

public enum RouteStateEnum { Unknown, Active, Inactive }

public enum ButtonSizeEnum { Normal, Large }

public enum TrackTypeEnum { MainLine, BranchLine }

public enum TrackStyleEnum { Normal, Rounded, Arrow, Lines, Terminator, Bridge, Platform, Tunnel }

public enum TrackAttributeEnum { Normal, Dashed }

public enum TurnoutStyleEnum { Standard, NoBranch }

public enum TextAlignmentHorizontalEnum { Left, Right, Center, Justify }

public enum TextAlignmentVerticalEnum { Top, Bottom, Center }

public enum TextAttributeEnum { Regular, Bold, Italic, BoldItalic, Light, LightItalic, Medium, MediumItalic, SemiBold, SemiBoldItalic, ExtraBold, ExtraBoldItalic }

public static class EnumHelpers {
    public static string ConvertFontStyle(TextAttributeEnum style) => style switch {
        TextAttributeEnum.Regular         => "OpenSansRegular",
        TextAttributeEnum.Bold            => "OpenSansBold",
        TextAttributeEnum.Italic          => "OpenSansItalic",
        TextAttributeEnum.BoldItalic      => "OpenSansBoldItalic",
        TextAttributeEnum.Light           => "OpenSansLight",
        TextAttributeEnum.LightItalic     => "OpenSansLightItalic",
        TextAttributeEnum.Medium          => "OpenSansMedium",
        TextAttributeEnum.MediumItalic    => "OpenSansMediumItalic",
        TextAttributeEnum.SemiBold        => "OpenSansSemiBold",
        TextAttributeEnum.SemiBoldItalic  => "OpenSansSemiBoldItalic",
        TextAttributeEnum.ExtraBold       => "OpenSansExtraBold",
        TextAttributeEnum.ExtraBoldItalic => "OpenSansExtraBoldItalic",
        _                                 => "OpenSansRegular",
    };

    public static HorizontalAlignment ConvertHorizontalAlignment(TextAlignmentHorizontalEnum alignment) => alignment switch {
        TextAlignmentHorizontalEnum.Left    => HorizontalAlignment.Left,
        TextAlignmentHorizontalEnum.Right   => HorizontalAlignment.Right,
        TextAlignmentHorizontalEnum.Center  => HorizontalAlignment.Center,
        TextAlignmentHorizontalEnum.Justify => HorizontalAlignment.Justified,
        _                                   => HorizontalAlignment.Left,
    };

    public static VerticalAlignment ConvertVerticalAlignment(TextAlignmentVerticalEnum alignment) => alignment switch {
        TextAlignmentVerticalEnum.Top    => VerticalAlignment.Top,
        TextAlignmentVerticalEnum.Bottom => VerticalAlignment.Bottom,
        TextAlignmentVerticalEnum.Center => VerticalAlignment.Center,
        _                                => VerticalAlignment.Center,
    };

    public static TextAlignmentHorizontalEnum ConvertHorizontalAlignment(string alignment) => alignment switch {
        "Left"    => TextAlignmentHorizontalEnum.Left,
        "Right"   => TextAlignmentHorizontalEnum.Right,
        "Center"  => TextAlignmentHorizontalEnum.Center,
        "Justify" => TextAlignmentHorizontalEnum.Justify,
        _         => TextAlignmentHorizontalEnum.Center,
    };

    public static TextAlignmentVerticalEnum ConvertVerticalAlignment(string alignment) => alignment switch {
        "Top"    => TextAlignmentVerticalEnum.Top,
        "Bottom" => TextAlignmentVerticalEnum.Bottom,
        "Center" => TextAlignmentVerticalEnum.Center,
        _        => TextAlignmentVerticalEnum.Center,
    };

    public static TextAlignment ConvertHorizontalAlignmentToText(TextAlignmentHorizontalEnum alignment) => alignment switch {
        TextAlignmentHorizontalEnum.Left    => TextAlignment.Start,
        TextAlignmentHorizontalEnum.Right   => TextAlignment.End,
        TextAlignmentHorizontalEnum.Center  => TextAlignment.Center,
        TextAlignmentHorizontalEnum.Justify => TextAlignment.Justify,
        _                                   => TextAlignment.Start,
    };

    public static TextAlignment ConvertVerticalAlignmentToText(TextAlignmentVerticalEnum alignment) => alignment switch {
        TextAlignmentVerticalEnum.Top    => TextAlignment.Start,
        TextAlignmentVerticalEnum.Bottom => TextAlignment.End,
        TextAlignmentVerticalEnum.Center => TextAlignment.Center,
        _                                => TextAlignment.Center,
    };

    public static TurnoutStateEnum ConvertTurnout(string state) => state switch {
        "Closed" => TurnoutStateEnum.Closed,
        "Thrown" => TurnoutStateEnum.Thrown,
        _        => TurnoutStateEnum.Unknown,
    };

    public static string ConvertTurnout(TurnoutStateEnum state) => state switch {
        TurnoutStateEnum.Closed => "Closed",
        TurnoutStateEnum.Thrown => "Thrown",
        _                       => "Unknown",
    };

    public static RouteStateEnum ConvertRoute(string state) => state switch {
        "Active"   => RouteStateEnum.Active,
        "Inactive" => RouteStateEnum.Inactive,
        _          => RouteStateEnum.Unknown,
    };

    public static string ConvertRoute(RouteStateEnum state) => state switch {
        RouteStateEnum.Active   => "Active",
        RouteStateEnum.Inactive => "Inactive",
        _                       => "Unknown",
    };
}