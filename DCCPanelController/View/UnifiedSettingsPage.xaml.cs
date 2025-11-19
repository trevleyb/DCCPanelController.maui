// UnifiedSettingsPage.xaml.cs

using Microsoft.Extensions.Logging;

namespace DCCPanelController.View;

public partial class UnifiedSettingsPage : ContentPage {
    public UnifiedSettingsPage(ILogger<UnifiedSettingsPage> logger, UnifiedSettingsViewModel vm) {
        BindingContext = vm;
        InitializeComponent();
    }

    protected override void OnAppearing() {
        base.OnAppearing();
        if (BindingContext is UnifiedSettingsViewModel vm) {
            vm.OnProfileChanged();
        }
    }

    protected override void OnSizeAllocated(double width, double height) {
        base.OnSizeAllocated(width, height);
        if (BindingContext is UnifiedSettingsViewModel vm) {
            vm.IsWide = width >= 900;
        }
    }

    protected override async void OnNavigatedFrom(NavigatedFromEventArgs args) {
        base.OnNavigatedFrom(args);
        if (BindingContext is UnifiedSettingsViewModel { IsDirty: true } vm) await vm.SaveAsync();
    }

    private async void ShowHelpPage(object? sender, EventArgs e) {
        await Navigation.PushAsync(new HelpPage());
    }
}