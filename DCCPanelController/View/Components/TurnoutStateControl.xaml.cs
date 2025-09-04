using System.ComponentModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using DCCPanelController.Models.DataModel;
using DCCPanelController.Models.DataModel.Entities;

namespace DCCPanelController.View.Components;

public partial class TurnoutStateControl : ContentView, INotifyPropertyChanged {
    public static readonly BindableProperty TurnoutProperty = BindableProperty.Create(nameof(Turnout), typeof(Turnout), typeof(TurnoutStateControl), null, BindingMode.TwoWay, propertyChanged: OnTurnoutChanged);
    public static readonly BindableProperty StateProperty = BindableProperty.Create(nameof(State), typeof(TurnoutStateEnum), typeof(TurnoutStateControl), null, BindingMode.TwoWay, propertyChanged: OnStateChanged);
    public static readonly BindableProperty CanToggleStateProperty = BindableProperty.Create(nameof(CanToggleState), typeof(bool), typeof(TurnoutStateControl), true);
    public static readonly BindableProperty CanSetStateUnknownProperty = BindableProperty.Create(nameof(CanSetStateUnknown), typeof(bool), typeof(TurnoutStateControl), false);
    public static readonly BindableProperty StateChangedCommandProperty = BindableProperty.Create(nameof(StateChangedCommand), typeof(ICommand), typeof(TurnoutStateControl));

    public static readonly BindableProperty ClosedTextProperty = BindableProperty.Create(nameof(ClosedText), typeof(string), typeof(TurnoutStateControl), "Closed");
    public static readonly BindableProperty ThrownTextProperty = BindableProperty.Create(nameof(ThrownText), typeof(string), typeof(TurnoutStateControl), "Thrown");
    public static readonly BindableProperty UnknownTextProperty = BindableProperty.Create(nameof(UnknownText), typeof(string), typeof(TurnoutStateControl), "Unknown");

    public TurnoutStateControl() {
        InitializeComponent();
    }

    public Turnout Turnout {
        get => (Turnout)GetValue(TurnoutProperty);
        set => SetValue(TurnoutProperty, value);
    }

    public TurnoutStateEnum State {
        get => (TurnoutStateEnum)GetValue(StateProperty);
        set => SetValue(StateProperty, value);
    }

    public bool CanSetStateUnknown {
        get => (bool)GetValue(CanSetStateUnknownProperty);
        init => SetValue(CanSetStateUnknownProperty, value);
    }

    public bool CanToggleState {
        get => (bool)GetValue(CanToggleStateProperty);
        init => SetValue(CanToggleStateProperty, value);
    }

    public ICommand? StateChangedCommand {
        get => (ICommand)GetValue(StateChangedCommandProperty);
        set => SetValue(StateChangedCommandProperty, value);
    }

    public string ClosedText {
        get => (string)GetValue(ClosedTextProperty);
        set => SetValue(ClosedTextProperty, value);
    }

    public string ThrownText {
        get => (string)GetValue(ThrownTextProperty);
        set => SetValue(ThrownTextProperty, value);
    }

    public string UnknownText {
        get => (string)GetValue(UnknownTextProperty);
        set => SetValue(UnknownTextProperty, value);
    }

    private static void OnStateChanged(BindableObject bindable, object oldValue, object newValue) {
        /* Holding for future use if needed */
    }

    private static void OnTurnoutChanged(BindableObject bindable, object oldValue, object newValue) {
        /* Holding for future use if needed */
    }

    [RelayCommand]
    public async Task ToggleStateAsync() {
        if (CanToggleState) {
            if (CanSetStateUnknown) {
                State = State switch {
                    TurnoutStateEnum.Closed  => TurnoutStateEnum.Thrown,
                    TurnoutStateEnum.Thrown  => TurnoutStateEnum.Unknown,
                    TurnoutStateEnum.Unknown => TurnoutStateEnum.Closed,
                    _                        => TurnoutStateEnum.Closed
                };
            } else {
                State = State switch {
                    TurnoutStateEnum.Closed => TurnoutStateEnum.Thrown,
                    TurnoutStateEnum.Thrown => TurnoutStateEnum.Closed,
                    _                       => TurnoutStateEnum.Closed
                };
            }

            if (StateChangedCommand is { } command) {
                if (command.CanExecute(Turnout)) command.Execute(Turnout);
            }
        }
        OnPropertyChanged(nameof(Turnout));
    }
}