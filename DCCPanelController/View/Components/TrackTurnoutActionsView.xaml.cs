using System.Collections.ObjectModel;
using TrackTurnoutAction = DCCPanelController.Tracks.TrackPieces.Base.TrackTurnoutAction;

namespace DCCPanelController.View.Components;

public partial class TrackTurnoutActionsView : ContentView {
    public TrackTurnoutActionsView() {
        InitializeComponent();
        Actions = new ObservableCollection<TrackTurnoutAction>();

        // Sample for UI Commands
        var addActionCommand = new Command(() => Actions.Add(new TrackTurnoutAction()));
        var removeActionCommand = new Command<TrackTurnoutAction>(action => Actions.Remove(action));

        // Assume Your Layout Here for Actions
        var stackLayout = new StackLayout();

        foreach (var action in Actions) {
            var entry = new Entry {
                Placeholder = "Enter ID",
                Text = action.Id // Two-way binding preferred
                // Connect this to a picker or set proper options based on Turnouts
            };

            entry.SetBinding(Entry.TextProperty, nameof(action.Id), BindingMode.TwoWay);

            var deleteButton = new Button {
                Text = "Remove",
                Command = removeActionCommand,
                CommandParameter = action
            };

            stackLayout.Children.Add(entry);
            stackLayout.Children.Add(deleteButton);
        }

        var addButton = new Button {
            Text = "Add Action",
            Command = addActionCommand
        };

        stackLayout.Children.Add(addButton);

        Content = new ScrollView { Content = stackLayout };
    }

    public ObservableCollection<TrackTurnoutAction> Actions { get; set; }
}