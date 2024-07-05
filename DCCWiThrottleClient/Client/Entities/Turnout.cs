using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DCCWiThrottleClient.Client;

public class Turnout(string systemName, string userName, TurnoutStateEnum turnoutStateEnum) : INotifyPropertyChanged, IEntity {
    public string    _name      = systemName;
    public string    _userName  = userName;
    public TurnoutStateEnum _stateEnum = turnoutStateEnum;

    public Turnout() : this("unknown","unknown",TurnoutStateEnum.Unknown) { }

    public string Name         { get => _name;      set => SetField(ref _name, value); }
    public string UserName     { get => _userName;  set => SetField(ref _userName, value); }
    public TurnoutStateEnum StateEnum { get => _stateEnum; set => SetField(ref _stateEnum, value, "State"); }                                       
    
    public string State =>
        StateEnum switch {
            TurnoutStateEnum.Unknown => "Unknown",
            TurnoutStateEnum.Closed  => "Closed",
            TurnoutStateEnum.Thrown  => "Thrown",
            _                 => "Unknown"
        };

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null) {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null) {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}

public enum TurnoutStateEnum {
    Unknown = '1',
    Closed  = '2',
    Thrown  = '4'
}
