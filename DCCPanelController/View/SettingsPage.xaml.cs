using System.ComponentModel;
using CommunityToolkit.Maui.Storage;

namespace DCCPanelController.View;

public partial class SettingsPage : ContentPage, INotifyPropertyChanged {
    private readonly SettingsViewModel? _viewModel;
    private Grid? _lastGridSelected;

    public SettingsPage(SettingsViewModel viewModel) {
        InitializeComponent();
        _viewModel = viewModel; //MauiProgram.ServiceHelper.GetService<SettingsViewModel>();
        if (_viewModel?.CurrentSettings?.Settings is { } current) {
            if (current.Type == "jmri") {
                JmriServerButton.IsChecked = true;
                WiThrottleServerButton.IsChecked = false;
            }
            if (current.Type == "withrottle") {
                JmriServerButton.IsChecked = false;
                WiThrottleServerButton.IsChecked = true;
            }
        }
        BindingContext = _viewModel;
    }

    protected override void OnDisappearing() {
        base.OnDisappearing();
        _viewModel?.SaveSettings();
    }

    private void OnLabelTapped(object sender, EventArgs args) {
        if (_lastGridSelected is not null) {
            foreach (var item in _lastGridSelected.Children) {
                if (item is Label oldLabel) {
                    oldLabel.TextColor = Colors.Black;
                }
            }
        }

        if (sender is Grid grid) {
            foreach (var item in grid.Children) {
                if (item is Label newLabel) {
                    newLabel.TextColor = Colors.Green;
                }
            }

            _lastGridSelected = grid;
        }
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

    private async void JmriServerButton_OnCheckedChanged(object? sender, CheckedChangedEventArgs e) {
        _viewModel?.SetNewConnectionMethod("jmri");
    }

    private void WiThrottleServerButton_OnCheckedChanged(object? sender, CheckedChangedEventArgs e) {
        _viewModel?.SetNewConnectionMethod("withrottle");
    }

    private void Protocol_OnTextChanged(object? sender, TextChangedEventArgs e) {
        if (_viewModel is not null) _viewModel.Url = $"{_viewModel.Protocol}://{_viewModel.IpAddress}:{_viewModel.Port}";
    }
    private void Address_OnTextChanged(object? sender, TextChangedEventArgs e) {
        if (_viewModel is not null) _viewModel.Url = $"{_viewModel.Protocol}://{_viewModel.IpAddress}:{_viewModel.Port}";
    }
    private void Port_OnTextChanged(object? sender, TextChangedEventArgs e) {
        if (_viewModel is not null) _viewModel.Url = $"{_viewModel.Protocol}://{_viewModel.IpAddress}:{_viewModel.Port}";
    }
    private void Url_OnTextChanged(object? sender, TextChangedEventArgs e) {
        if (_viewModel is not null) {
            try {
                var uri = new Uri(_viewModel.Url);
                _viewModel.Protocol = uri.Scheme;
                _viewModel.IpAddress = uri.Host;
                _viewModel.Port = uri.Port;
            } catch { /* ignored */ }
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