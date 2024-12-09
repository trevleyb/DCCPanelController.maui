using System.Globalization;
using DCCPanelController.Model;

namespace DCCPanelController.View.PropertPages;

public partial class PanelPropertyPage : ContentPage, IPropertyPage {
    public event EventHandler? CloseRequested;

    public PanelPropertyPage(Panel panel) {
        InitializeComponent();
        BindingContext = panel;
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

    private void ClosePropertyPage(object? sender, EventArgs e) {
        //Navigation.PopModalAsync(true);
        CloseRequested?.Invoke(this, EventArgs.Empty);
    }
}