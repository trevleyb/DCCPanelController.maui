using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using CommunityToolkit.Mvvm.ComponentModel;

namespace DCCPanelController.Models.DataModel;

public partial class DccDccTable : ObservableObject, IDccTable {
    [ObservableProperty] private string? _id;
    [ObservableProperty] private string? _name;
    [ObservableProperty] private int     _dccAddress;
    [ObservableProperty] private bool    _isEditable;
    [ObservableProperty] private bool    _isModified;

    [JsonIgnore]
    public string DisplayFormat => $"{Name} ({Id})";

    partial void OnIdChanged(string? oldValue, string? newValue) {
        SetDccAddress(newValue);
    }

    public void SetDccAddress() => SetDccAddress(Id);
    public void SetDccAddress(string? id) {
        if (id is {} && DccAddress == 0) {
            var number = int.Parse(MyRegex().Match(id).Value);
        }
    }

    [GeneratedRegex(@"\d+")]
    private static partial Regex MyRegex();
}