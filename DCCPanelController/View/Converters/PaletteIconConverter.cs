using System.Globalization;

namespace DCCPanelController.View.Converters;

public class PaletteIconConverter : IValueConverter {
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
        if (value is PanelEditorViewModel.PaletteStateEnum state) {
            return state switch {
                PanelEditorViewModel.PaletteStateEnum.SideVisible   => "right_panel_close_filled_active.png",
                PanelEditorViewModel.PaletteStateEnum.BottomVisible => "bottom_panel_close_filled_active.png",
                PanelEditorViewModel.PaletteStateEnum.SideHidden    => "right_panel_open_filled_active.png",
                PanelEditorViewModel.PaletteStateEnum.BottomHidden  => "bottom_panel_open_filled_active.png",
                _                                                   => "layout_active.png",
            };
        } 
        return"layout_active.png";
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotSupportedException();
}
