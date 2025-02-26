using System.Globalization;
using DCCPanelController.Model;
using Entry = Microsoft.Maui.Controls.Entry;
#if IOS
using UIKit;
using UIModalPresentationStyle = UIKit.UIModalPresentationStyle;
#endif

namespace DCCPanelController.View.PropertPages;

public partial class PanelPropertyPage : ContentPage, IPropertyPage {

    public PanelPropertyPage(Panel panel) {
        InitializeComponent();
        BindingContext = panel;
        RowsStepper.Minimum = GetMaxRows(panel);
        ColsStepper.Minimum = GetMaxCols(panel);
    }

    public event EventHandler? CloseRequested;

    protected override void OnAppearing() {
        base.OnAppearing();

#if IOS
        // Access the native view controller
        var window = App.Current.Windows[0].Page;
        if (window == null) throw new InvalidOperationException("MainPage is not set.");

        //var window = Microsoft.Maui.Controls.Application.Current?.Windows.FirstOrDefault();
        if (window?.Handler?.PlatformView is UIWindow viewController) {
            var rootController = viewController.RootViewController;
            if (rootController?.PresentedViewController != null) {
                var modalController = rootController.PresentedViewController;

                // Set the modal presentation style
                modalController.ModalPresentationStyle = UIModalPresentationStyle.PageSheet;

                // Configure the sheet for iOS 15+
                if (UIDevice.CurrentDevice.CheckSystemVersion(15, 0)) {
                    var sheetController = modalController.SheetPresentationController;
                    if (sheetController != null) {
                        sheetController.Detents = [
                            UISheetPresentationControllerDetent.CreateMediumDetent(),
                            UISheetPresentationControllerDetent.CreateLargeDetent()
                        ];

                        sheetController.PrefersGrabberVisible = true;
                    }
                }
            }
        }
#endif
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

    private void ResetColorsClicked(object? sender, EventArgs e) {
        if (BindingContext is Panel panel) {
            panel.Defaults.ResetToDefaults();
        }
    }

    private static int GetMaxRows(Panel panel) {
        return int.Max(panel.Tracks.Select(track => track.Y).Prepend(0).Max(), 12);
    }

    private static int GetMaxCols(Panel panel) {
        return int.Max(panel.Tracks.Select(track => track.X).Prepend(0).Max(), 12);
    }
}