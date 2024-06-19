using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DCCWiThrottleClient.Client;

public class Turnout(string systemName, string userName, StateEnum stateEnum) : INotifyPropertyChanged {
    public string    _name      = systemName;
    public string    _userName  = userName;
    public StateEnum _stateEnum = stateEnum;

    public string Name         { get => _name;      set => SetField(ref _name, value); }
    public string UserName     { get => _userName;  set => SetField(ref _userName, value); }
    public StateEnum StateEnum { get => _stateEnum; set => SetField(ref _stateEnum, value, "State"); }                                       
    
    public string State =>
        StateEnum switch {
            StateEnum.Unknown => "Unknown",
            StateEnum.Closed  => "Closed",
            StateEnum.Thrown  => "Thrown",
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

public enum StateEnum {
    Unknown = '1',
    Closed  = '2',
    Thrown  = '4'
}
