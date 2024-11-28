using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DCCPanelController.Model;

namespace DCCPanelController.View.Components  {
    public partial class TurnoutStateControl : ContentView {

        public TurnoutStateControl() {
            InitializeComponent();
            BindingContext = this;
        }
        
        public static readonly BindableProperty StateProperty =
            BindableProperty.Create(nameof(State), typeof(TurnoutStateEnum), typeof(TurnoutStateControl), null, BindingMode.TwoWay, propertyChanged: OnStateChanged);

        public static readonly BindableProperty CanToggleStateProperty =
            BindableProperty.Create(nameof(CanToggleState), typeof(bool), typeof(TurnoutStateControl), true);

        public static readonly BindableProperty ToggleTurnoutStateCommandProperty =
            BindableProperty.Create(nameof(ToggleTurnoutStateCommand), typeof(ICommand), typeof(TurnoutStateControl));
        
        private static void OnStateChanged(BindableObject bindable, object oldValue, object newValue) {
            var control = (TurnoutStateControl)bindable;
            control.OnStateChanged((TurnoutStateEnum)oldValue, (TurnoutStateEnum)newValue);
        }
        
        protected virtual void OnStateChanged(TurnoutStateEnum oldValue, TurnoutStateEnum newValue) {
            OnPropertyChanged(nameof(State));
        }
        
        public TurnoutStateEnum State {
            get => (TurnoutStateEnum)GetValue(StateProperty);
            set => SetValue(StateProperty, value);
        }

        public bool CanToggleState {
            get => (bool)GetValue(CanToggleStateProperty);
            init => SetValue(CanToggleStateProperty, value);
        }
        
        public ICommand ToggleTurnoutStateCommand {
            get => (ICommand)GetValue(ToggleTurnoutStateCommandProperty);
            init => SetValue(ToggleTurnoutStateCommandProperty, value);
        }
        
        [RelayCommand]
        public async Task ToggleStateAsync() {
            if (CanToggleState) {
                State = State switch {
                    TurnoutStateEnum.Closed => TurnoutStateEnum.Thrown,
                    TurnoutStateEnum.Thrown => TurnoutStateEnum.Closed,
                    _                       => TurnoutStateEnum.Closed
                };
            }
        }
    }
}