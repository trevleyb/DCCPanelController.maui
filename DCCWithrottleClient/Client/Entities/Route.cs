using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DCCWithrottleClient.Client.Entities;

public class Route(string systemName, string userName, RouteStateEnum stateEnum) : INotifyPropertyChanged, IEntity {
    public string    _name      = systemName;
    public string    _userName  = userName;
    public RouteStateEnum _stateEnum = stateEnum;

    public Route() : this("unknown","unknown",RouteStateEnum.Unknown) {}

    public string Name         { get => _name;      set => SetField(ref _name, value); }
    public string UserName     { get => _userName;  set => SetField(ref _userName, value); }
    public RouteStateEnum StateEnum { get => _stateEnum; set => SetField(ref _stateEnum, value, "State"); }                                       
    
    public string State =>
        StateEnum switch {
            RouteStateEnum.Unknown  => "Unknown",
            RouteStateEnum.Active   => "Active",
            RouteStateEnum.Inactive => "Inactive",
            _                       => "Unknown"
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

public enum RouteStateEnum {
    Unknown = '1',
    Active  = '2',
    Inactive  = '4'
}
