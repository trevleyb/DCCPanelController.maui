using System.Collections.ObjectModel;
using Syncfusion.Maui.Toolkit.Popup;

namespace DCCPanelController.View.Components;

public record ProfileItem(int Index, string Name);

public partial class ProfileSelector : ContentView {
    public ProfileSelector(string title, IReadOnlyList<string> items, int? preselect = null) {
        BindingContext = this;
        InitializeComponent();
        IsOkEnabled = false;
        Title = title;
        for (var i = 0; i < items.Count; i++) Items.Add(new ProfileItem(i, items[i]));
        ProfilesList.ItemsSource = Items;
    }

    public string Title {
        get;
        set {
            field = value;
            OnPropertyChanged();
        }
    }
    public bool IsOkEnabled { get; set; }
    public ProfileItem? SelectedItem { get; set; }
    public ObservableCollection<ProfileItem> Items { get; set; } = [];

    public event Action<int>? SelectionCompleted;
    public event Action? SelectionCancelled;

    private async void OnSelectButtonClicked(object? sender, EventArgs e) => SelectionCompleted?.Invoke(SelectedItem?.Index ?? -1);

    private async void OnCancelButtonClicked(object? sender, EventArgs e) => SelectionCancelled?.Invoke();

    private void ProfileSelectionChanged(object? sender, SelectionChangedEventArgs e) {
        IsOkEnabled = SelectedItem is { };
        OnPropertyChanged(nameof(IsOkEnabled));
    }

    public static async Task<int?> ShowProfileSelector(string title, IReadOnlyList<string> items, int? preselect = null) {
        var tcs = new TaskCompletionSource<int?>();
        var selector = new ProfileSelector(title, items, preselect);
        var popup = new SfPopup {
            ContentTemplate = new DataTemplate(() => selector),
            ShowHeader = false,
            ShowFooter = false,
            BackgroundColor = Colors.White,
            PopupStyle = new PopupStyle {
                CornerRadius = 10,
                HasShadow = false,
                BlurIntensity = PopupBlurIntensity.Light,
            },
            AutoSizeMode = PopupAutoSizeMode.Both,
            AnimationMode = PopupAnimationMode.Zoom,
            AnimationDuration = 300,
        };

        selector.SelectionCompleted += index => {
            popup.Dismiss();
            tcs.SetResult(index);
        };

        selector.SelectionCancelled += () => {
            popup.Dismiss();
            tcs.SetResult(null);
        };

        // Handle popup closing
        popup.Closed += (_, args) => {
            if (!tcs.Task.IsCompleted) {
                tcs.SetResult(null);
            }
        };

        popup.Show();
        return await tcs.Task;
    }
}