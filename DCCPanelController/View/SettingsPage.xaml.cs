using System.ComponentModel;
using CommunityToolkit.Maui.Storage;

namespace DCCPanelController.View;

public partial class SettingsPage : ContentPage, INotifyPropertyChanged {
    private readonly SettingsViewModel? _viewModel;

    public SettingsPage(SettingsViewModel viewModel) {
        InitializeComponent();
        _viewModel = viewModel;
        if (_viewModel?.ConnectionSettings is { } current) {
            _viewModel.IsJmriServer = current.Type == "jmri";
            _viewModel.IsWiThrottle = current.Type == "withrottle";
        }
        Task.Run(() => _viewModel!.RefreshServersAsync());
        BindingContext = _viewModel;
    }

    protected override async void OnDisappearing() {
        base.OnDisappearing();
        if (_viewModel is {} vm) await vm.SaveSettings();
    }

    private void OnEntryFocused(object sender, FocusEventArgs e) {
        if (sender is Entry entry) {
            entry.CursorPosition = 0;
            entry.SelectionLength = entry.Text?.Length ?? 0;
        }
    }

    private void About_OnClicked(object? sender, EventArgs e) {
        Navigation.PushAsync(new AboutPage());
    }

    private void Instructions_OnClicked(object? sender, EventArgs e) {
        Navigation.PushAsync(new InstructionsPage());
    }

    private async void Upload_OnClicked(object? sender, EventArgs e) {
        try {
            // Prompt the user to choose where to save the file
            var fileName = await PromptUserForConfigFile();

            if (!string.IsNullOrEmpty(fileName)) {
                //if (_viewModel is { SettingsService: not null } settings) {
                //    var loadedJson = await LoadJsonFromFile(fileName);
                //    settings.SettingsService.UploadNewSettings(loadedJson);
                //    await DisplayAlert("Success", "File Loaded.", "OK");
                //} else {
                //    throw new Exception("File could not be loaded.");
                // }
            }
        } catch (Exception ex) {
            await DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
        }
    }

    private async void Download_OnClicked(object? sender, EventArgs e) {
        try {
            // Prompt the user to choose where to save the file
            var filePath = await PromptUserForSaveLocation();

            if (!string.IsNullOrEmpty(filePath)) {
                if (_viewModel is { Settings: not null } settings) {
                    var saveFile = Path.Combine(filePath, "dccpanel.settings");

                    //if (settings.SettingsService is { } settingsService) {
                    //    await SaveJsonToFile(saveFile, settingsService.ToJsonString());
                    //    await DisplayAlert("Success", "File Downloaded.", "OK");
                    //    Console.WriteLine(saveFile);
                    //}
                }
            }
        } catch (Exception ex) {
            await DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
        }
    }

    private static async Task<string> PromptUserForConfigFile() {
        var result = await FilePicker.PickAsync(new PickOptions { PickerTitle = "Select the Config file to upload" });

        if (result is not null) {
            return result.FullPath;
        }

        return string.Empty;
    }

    private static async Task<string> PromptUserForSaveLocation() {
        var result = await FolderPicker.PickAsync(new CancellationToken());
        result.EnsureSuccess();
        return result.Folder.Path ?? string.Empty;
    }

    // Method to save the JSON file to the specified location
    private async Task SaveJsonToFile(string filePath, string jsonData) {
        await using var streamWriter = new StreamWriter(filePath, false);
        await streamWriter.WriteAsync(jsonData);
    }

    // Method to save the JSON file to the specified location
    private async Task<string> LoadJsonFromFile(string filePath) {
        using var reader = new StreamReader(filePath);
        return await reader.ReadToEndAsync();
    }

    private void CheckChanged_Jmri(object? sender, CheckedChangedEventArgs e) {
        if (sender is RadioButton { IsChecked: true}) SetServerSelected("jmri");
    }

    private void CheckChanged_WiThrottle(object? sender, CheckedChangedEventArgs e) {
        if (sender is RadioButton { IsChecked: true }) SetServerSelected("withrottle");
    }

    private async void SetServerSelected(string serverType) {
        if (_viewModel is { IsNotBusy: true } && !string.IsNullOrEmpty(serverType)) {
            _viewModel.SetNewConnectionMethod(serverType);
            await _viewModel.RefreshServersAsync();
        }
    }
}

public class EntryValidationBehavior : Behavior<Entry> {
    protected override void OnAttachedTo(Entry entry) {
        entry.TextChanged += OnEntryTextChanged;
        base.OnAttachedTo(entry);
    }

    protected override void OnDetachingFrom(Entry entry) {
        entry.TextChanged -= OnEntryTextChanged;
        base.OnDetachingFrom(entry);
    }

    private void OnEntryTextChanged(object? sender, TextChangedEventArgs args) {
        if (sender is Entry entry) {
            if (!string.IsNullOrEmpty(args.NewTextValue)) {
                if (int.TryParse(args.NewTextValue, out var result)) {
                    entry.TextColor = result is >= 0 and <= 255 ? Colors.Black : Colors.Red;
                }
            }
        }
    }

}