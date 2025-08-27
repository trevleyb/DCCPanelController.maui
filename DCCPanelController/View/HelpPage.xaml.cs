using DCCPanelController.Services;
using DCCPanelController.View.Helpers;

namespace DCCPanelController.View;

// App/Pages/HelpPage.xaml.cs
[QueryProperty(nameof(TopicId), "topicId")]
public partial class HelpPage : ContentPage {
    public string? TopicId { get; set; }
    public string? CurrentId { get; set; }
    public string? CurrentAnchor { get; set; }

    public bool ForwardButtonEnabled => _forward.Count > 0;
    public bool BackButtonEnabled => _back.Count > 0;

    // History
    readonly Stack<HelpHistoryEntry> _back = new();
    readonly Stack<HelpHistoryEntry> _forward = new();

    public HelpPage() {
        BindingContext = this;
        InitializeComponent();
        Viewer.HelpLinkRequested += OnHelpLinkRequested;
    }

    protected override async void OnAppearing() {
        base.OnAppearing();
        await HelpService.Current.InitializeAsync();
        _back.Clear();
        _forward.Clear();
        await NavigateToAsync(
            new HelpHistoryEntry(string.IsNullOrWhiteSpace(TopicId) ? HelpService.DefaultTopicId : TopicId!),
            pushHistory: false);
    }

// Link clicks inside the viewer
    async void OnHelpLinkRequested(object? sender, HelpLinkRequestedEventArgs e) => await NavigateToAsync(new HelpHistoryEntry(e.TopicId, e.Anchor), pushHistory: true);

    private async Task LoadTopicAsync(string id, string? anchor = null, string? referrerId = null) {
        var doc = await HelpService.Current.LoadTopicAsync(id, referrerId, anchor);
        Viewer.Document = doc;
        CurrentId = id;
    }

// Core navigator: loads topic, updates current, manages buttons
    async Task NavigateToAsync(HelpHistoryEntry entry, bool pushHistory) {
        if (pushHistory && CurrentId is not null) {
            if (CurrentId == entry.Id && CurrentAnchor == entry.Anchor) return;
            _back.Push(new HelpHistoryEntry(CurrentId, CurrentAnchor));
            _forward.Clear(); // drop forward history on new branch
        }

        var doc = await HelpService.Current.LoadTopicAsync(entry.Id, referrerId: CurrentId, anchor: entry.Anchor);
        Viewer.Document = doc;

        CurrentId = entry.Id;
        CurrentAnchor = entry.Anchor;
        UpdateNavButtons();
    }

    // Toolbar handlers
    async void OnHome(object? sender, EventArgs e) {
        await GoHomeAsync(true);
    }

    async void OnBack(object? sender, EventArgs e) {
        if (_back.Count == 0) return;
        _forward.Push(new HelpHistoryEntry(CurrentId!, CurrentAnchor));
        var prev = _back.Pop();
        await NavigateToAsync(prev, pushHistory: false);
    }

    async void OnForward(object? sender, EventArgs e) {
        if (_forward.Count == 0) return;
        _back.Push(new HelpHistoryEntry(CurrentId!, CurrentAnchor));
        var next = _forward.Pop();
        await NavigateToAsync(next, pushHistory: false);
    }

    void UpdateNavButtons() {
        OnPropertyChanged(nameof(ForwardButtonEnabled));
        OnPropertyChanged(nameof(BackButtonEnabled));
    }

    // (Optional) intercept platform back (Android hardware / macOS cmd+[ if you wire it)
    protected override bool OnBackButtonPressed() {
        if (_back.Count > 0) {
            OnBack(this, EventArgs.Empty);
            return true; // we handled it
        }
        return base.OnBackButtonPressed();
    }

    // (Optional) public helper so HelpService.NavigateAsync can reuse this page without Shell push
    public Task LoadTopicPublicAsync(string id) => NavigateToAsync(new HelpHistoryEntry(id), pushHistory: true);

    // Optional public helper if other pages want to send you "Home"
    public Task GoHomeAsync(bool hardReset = false) {
        if (hardReset) {
            _back.Clear();
            _forward.Clear();
            return NavigateToAsync(new HelpHistoryEntry(HelpService.DefaultTopicId), pushHistory: false);
        }
        return NavigateToAsync(new HelpHistoryEntry(HelpService.DefaultTopicId), pushHistory: true);
    }
}

public sealed record HelpHistoryEntry(string Id, string? Anchor = null);