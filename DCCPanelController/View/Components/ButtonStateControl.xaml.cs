using System.ComponentModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using DCCPanelController.Model;

namespace DCCPanelController.View.Components;

public partial class ButtonStateControl : ContentView, INotifyPropertyChanged {
    public static readonly BindableProperty ButtonProperty = BindableProperty.Create(nameof(Button), typeof(Button), typeof(ButtonStateControl), null, BindingMode.TwoWay, propertyChanged: OnTurnoutChanged);
    public static readonly BindableProperty StateProperty = BindableProperty.Create(nameof(State), typeof(ButtonStateEnum), typeof(ButtonStateControl), null, BindingMode.TwoWay, propertyChanged: OnStateChanged);
    public static readonly BindableProperty CanToggleStateProperty = BindableProperty.Create(nameof(CanToggleState), typeof(bool), typeof(ButtonStateControl), true);
    public static readonly BindableProperty CanSetStateUnknownProperty = BindableProperty.Create(nameof(CanSetStateUnknown), typeof(bool), typeof(ButtonStateControl), false);
    public static readonly BindableProperty StateChangedCommandProperty = BindableProperty.Create(nameof(StateChangedCommand), typeof(ICommand), typeof(ButtonStateControl), null);

    public static readonly BindableProperty ActiveTextProperty = BindableProperty.Create(nameof(ActiveText), typeof(string), typeof(ButtonStateControl),"Active (On)");
    public static readonly BindableProperty InactiveTextProperty = BindableProperty.Create(nameof(InactiveText), typeof(string), typeof(ButtonStateControl),"Inactive (Off)");
    public static readonly BindableProperty UnknownTextProperty = BindableProperty.Create(nameof(UnknownText), typeof(string), typeof(ButtonStateControl),"Unknown");

    public ButtonStateControl() {
        InitializeComponent();
    }

    public Button Button {
        get => (Button)GetValue(ButtonProperty);
        set => SetValue(ButtonProperty, value);
    }

    public ButtonStateEnum State {
        get => (ButtonStateEnum)GetValue(StateProperty);
        set => SetValue(StateProperty, value);
    }

    public bool CanToggleState {
        get => (bool)GetValue(CanToggleStateProperty);
        init => SetValue(CanToggleStateProperty, value);
    }

    public bool CanSetStateUnknown {
        get => (bool)GetValue(CanSetStateUnknownProperty);
        init => SetValue(CanSetStateUnknownProperty, value);
    }

    public ICommand? StateChangedCommand {
        get => (ICommand)GetValue(StateChangedCommandProperty);
        set => SetValue(StateChangedCommandProperty, value);
    }

    public string ActiveText {
        get => (string)GetValue(ActiveTextProperty);
        set => SetValue(ActiveTextProperty, value);
    }
    public string InactiveText {
        get => (string)GetValue(InactiveTextProperty);
        set => SetValue(InactiveTextProperty, value);
    }
    public string UnknownText {
        get => (string)GetValue(UnknownTextProperty);
        set => SetValue(UnknownTextProperty, value);
    }

    private static void OnStateChanged(BindableObject bindable, object oldValue, object newValue) { }
    private static void OnTurnoutChanged(BindableObject bindable, object oldValue, object newValue) { }

    [RelayCommand]
    public async Task ToggleStateAsync() {
        if (CanToggleState) {
            if (CanSetStateUnknown) {
                State = State switch {
                    ButtonStateEnum.Active   => ButtonStateEnum.Inactive,
                    ButtonStateEnum.Inactive => ButtonStateEnum.Unknown,
                    ButtonStateEnum.Unknown  => ButtonStateEnum.Active,
                    _                        => ButtonStateEnum.Unknown
                };
            } else {
                State = State switch {
                    ButtonStateEnum.Active   => ButtonStateEnum.Inactive,
                    ButtonStateEnum.Inactive => ButtonStateEnum.Active,
                    _                        => ButtonStateEnum.Active
                };
            }

            if (StateChangedCommand is {} command) {
                if (command.CanExecute(Button)) command.Execute(Button);
            }
        }
        OnPropertyChanged(nameof(Button));
    }
}