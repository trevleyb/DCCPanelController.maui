using System.Collections;
using System.Collections.Specialized;
using DCCPanelController.Services;
using DCCPanelController.Services.ProfileService;
using Microsoft.Extensions.Logging;

namespace DCCPanelController.View;

public partial class DccClientTestView : ContentPage {
    private ILogger<DccClientTestView> _logger;
    public DccClientTestView(ILogger<DccClientTestView> logger, ProfileService prf, ConnectionService svc) {
        _logger = logger;
        InitializeComponent();
        BindingContext = new DccClientTestViewModel(prf,svc);
        
        this.Appearing += (s, e) => {
            if (BindingContext is DccClientTestViewModel vm) {
                vm.ServerMessages.CollectionChanged += OnMessagesChanged;
                vm.ConnectStateChanged += VmOnConnectStateChanged;
            }
        };
        this.Disappearing += (s, e) => {
            if (BindingContext is DccClientTestViewModel vm) {
                vm.ServerMessages.CollectionChanged -= OnMessagesChanged;
                vm.ConnectStateChanged -= VmOnConnectStateChanged;
            }
        };    
    }

    private void VmOnConnectStateChanged(object? sender, bool e) {
        MyExpander.IsExpanded = !e;
        Commands.IsExpanded = e;
        if (BindingContext is DccClientTestViewModel vm && e == true) {
            if (vm.Turnout is null && vm.Profile.Turnouts.Count > 0) vm.Turnout = vm.Profile.Turnouts[0];
            if (vm.Block is null && vm.Profile.Blocks.Count > 0) vm.Block = vm.Profile.Blocks[0];
            if (vm.Sensor is null && vm.Profile.Sensors.Count > 0) vm.Sensor = vm.Profile.Sensors[0];
            if (vm.Signal is null && vm.Profile.Signals.Count > 0) vm.Signal = vm.Profile.Signals[0];
            if (vm.Light is null && vm.Profile.Lights.Count > 0) vm.Light = vm.Profile.Lights[0];
            if (vm.Route is null && vm.Profile.Routes.Count > 0) vm.Route = vm.Profile.Routes[0];
        }
    }

    private void OnMessagesChanged(object? sender, NotifyCollectionChangedEventArgs e) {
        if (e.Action != NotifyCollectionChangedAction.Add) return;
        if (BindingContext is DccClientTestViewModel { AutoScroll: false }) return;
        
        MainThread.BeginInvokeOnMainThread(async void () => {
            try {
                await Task.Yield();
                if (MessagesCollectionView.ItemsSource is IList { Count: > 0 } list)
                    MessagesCollectionView.ScrollTo(list.Count - 1, position: ScrollToPosition.End, animate: true);
            } catch (Exception ex) {
                _logger.LogError($"Exception scrolling UI: {ex.Message}");
            }
        });
    }

    private void MessagesCollectionView_OnScrolled(object? sender, ItemsViewScrolledEventArgs e) {
        if (BindingContext is DccClientTestViewModel vm) {
            var items = (MessagesCollectionView.ItemsSource as IList)?.Count ?? 0;
            var position = e.LastVisibleItemIndex;
            
            // Added the adjustment of 3 due to timing issues as the collection can quickly change
            // while data is being added in the background. So if we are within the last 3 items, consider
            // that we are scrolled to the end. 
            // ------------------------------------------------------------------------------------------------------------
            var scrollState = (position >= items - 3);
            if (vm.AutoScroll != scrollState) vm.AutoScroll = scrollState;
        }
    }

    private void Button_OnClicked(object? sender, EventArgs e) {
        if (BindingContext is DccClientTestViewModel { } vm) vm.AutoScroll = !vm.AutoScroll;
    }
}