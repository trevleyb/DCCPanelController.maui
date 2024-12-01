using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DCCPanelController.Model;

namespace DCCPanelController.View.Components  {
    public partial class TurnoutStateControl : ContentView, INotifyPropertyChanged {

        public TurnoutStateControl() {
            InitializeComponent();
            // Use SetBinding instead of directly setting BindingContext
            //SetBinding(BindingContextProperty, new Binding(".", BindingMode.TwoWay) {Source = this});
        }

        //public event PropertyChangedEventHandler PropertyChanged;

        //protected new virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) {
        //    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        //}
        
        public static readonly BindableProperty TurnoutProperty =
            BindableProperty.Create(nameof(Turnout), typeof(Turnout), 
                                    typeof(TurnoutStateControl), null, 
                                    BindingMode.TwoWay, propertyChanged: OnTurnoutChanged);
        
        public static readonly BindableProperty StateProperty =
            BindableProperty.Create(nameof(State), typeof(TurnoutStateEnum), 
                                    typeof(TurnoutStateControl), null, 
                                    BindingMode.TwoWay, propertyChanged: OnStateChanged);

        public static readonly BindableProperty CanToggleStateProperty =
            BindableProperty.Create(nameof(CanToggleState), typeof(bool), 
                                    typeof(TurnoutStateControl), true);

        public static readonly BindableProperty ToggleTurnoutStateCommandProperty =
            BindableProperty.Create(nameof(ToggleTurnoutStateCommand), typeof(ICommand), 
                                    typeof(TurnoutStateControl), null, BindingMode.TwoWay, propertyChanged: ToggleTurnoutStateCommandChanged);

        private static void ToggleTurnoutStateCommandChanged(BindableObject bindable, object oldvalue, object newvalue) {
            Console.WriteLine("ToggleTurnoutStateChanged");
        }

        private static void OnStateChanged(BindableObject bindable, object oldValue, object newValue) { }

        private static void OnTurnoutChanged(BindableObject bindable, object oldValue, object newValue) { }
        
        public Turnout Turnout {
            get => (Turnout)GetValue(TurnoutProperty);
            set => SetValue(TurnoutProperty, value);
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
            OnPropertyChanged(nameof(Turnout));
            if (ToggleTurnoutStateCommand.CanExecute(null)) {
                ToggleTurnoutStateCommand.Execute(null);
            }
        }
    }
}