using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DCCPanelController.Model;

namespace DCCPanelController.View.PropertPages;

public partial class PanelPropertyPage : ContentView {
    public PanelPropertyPage(Panel panel) {
        InitializeComponent();
        BindingContext = panel;
    }

    private void Order_OnTextChanged(object? sender, TextChangedEventArgs e) {
        if (sender is Entry field) {
            if (int.TryParse(e.NewTextValue, out var order)) {
                if (order >= 99) field.Text = "99";
                if (order <= 0) field.Text = "0";
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
}