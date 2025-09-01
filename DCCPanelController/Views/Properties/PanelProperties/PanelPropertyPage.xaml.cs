using System.Globalization;
using DCCPanelController.Models.DataModel;

namespace DCCPanelController.Views.Properties.PanelProperties;

public partial class PanelPropertyPage : ContentView {
    public readonly PanelPropertyViewModel ViewModel;

    public PanelPropertyPage(PanelPropertyViewModel viewModel) {
        InitializeComponent();
        ViewModel = viewModel;
        BindingContext = ViewModel;
        RowsStepper.Minimum = GetMaxRows(ViewModel.Panel);
        ColsStepper.Minimum = GetMaxCols(ViewModel.Panel);
    }

    private void PanelPropertyPage_SizeChanged(object sender, EventArgs e) {
        UpdateSpanBasedOnSize(Width);
    }

    private void UpdateSpanBasedOnSize(double currentWidth) {
        if (currentWidth > 0) {
            var newSpan = currentWidth switch {
                < 600 => 1, // iPhone - single column
                < 900 => 2, // iPad portrait or smaller tablets - 2 columns  
                _     => 3  // iPad landscape or desktop - 3 columns
            };
            if (ViewModel.ColorGridSpan != newSpan) {
                ViewModel.ColorGridSpan = newSpan;
            }
        }
    }

    private void Order_OnTextChanged(object? sender, TextChangedEventArgs e) {
        if (sender is Entry field) {
            if (int.TryParse(e.NewTextValue, out var order)) {
                field.Text = order switch {
                    >= 99 => "99",
                    <= 0  => "0",
                    _     => field.Text
                };
            } else {
                field.Text = e.NewTextValue;
            }
        }
    }

    private void Cols_OnTextChanged(object? sender, TextChangedEventArgs e) {
        if (sender is Entry field) {
            if (int.TryParse(e.NewTextValue, out var cols)) {
                if (cols >= ColsStepper.Maximum) field.Text = ColsStepper.Maximum.ToString(CultureInfo.InvariantCulture);
                if (cols <= ColsStepper.Minimum) field.Text = ColsStepper.Minimum.ToString(CultureInfo.InvariantCulture);
            } else {
                field.Text = e.NewTextValue;
            }
        }
    }

    private void Rows_OnTextChanged(object? sender, TextChangedEventArgs e) {
        if (sender is Entry field) {
            if (int.TryParse(e.NewTextValue, out var rows)) {
                if (rows >= RowsStepper.Maximum) field.Text = RowsStepper.Maximum.ToString(CultureInfo.InvariantCulture);
                if (rows <= RowsStepper.Minimum) field.Text = RowsStepper.Minimum.ToString(CultureInfo.InvariantCulture);
            } else {
                field.Text = e.NewTextValue;
            }
        }
    }

    private static int GetMaxRows(Panel panel) {
        return int.Max(panel.Entities.Select(track => track.Row).Prepend(0).Max(), 12);
    }

    private static int GetMaxCols(Panel panel) {
        return int.Max(panel.Entities.Select(track => track.Col).Prepend(0).Max(), 12);
    }
}