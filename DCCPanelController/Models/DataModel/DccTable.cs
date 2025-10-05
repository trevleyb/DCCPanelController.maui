using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using CommunityToolkit.Mvvm.ComponentModel;

namespace DCCPanelController.Models.DataModel;

public partial class DccTable : ObservableObject, IDccTable, IEquatable<DccTable>, IComparable<DccTable> {
    [ObservableProperty] private string? _id;
    [ObservableProperty] private string? _name;
    [ObservableProperty] private bool    _isModified;

    [NotifyPropertyChangedFor(nameof(CanEditDccAddress))]
    [ObservableProperty] private int     _dccAddress;
    
    [NotifyPropertyChangedFor(nameof(CanEditDccAddress))]
    [ObservableProperty] private bool    _isEditable;
    
    [NotifyPropertyChangedFor(nameof(CanEditDccAddress))]
    [ObservableProperty] private bool    _dccAddressLocked;

    public bool CanEditDccAddress => IsEditable && !DccAddressLocked;
    
    [JsonIgnore]
    public string DisplayFormat => $"{(Name ?? "Unnamed")} ({(Id ?? "—")})";

    // --- Normalization & change tracking ---
    partial void OnIdChanged(string? oldValue, string? newValue) {
        var norm = NormalizeId(newValue);
        if (!string.Equals(newValue, norm, StringComparison.Ordinal)) Id = norm; // re-set normalized once
        if (!string.Equals(oldValue, Id, StringComparison.Ordinal)) {
            IsModified = true;
            if (!_dccAddressLocked && TryDeriveDccAddress(Id, out var addr)) DccAddress = addr;
        }
    }

    partial void OnNameChanged(string? oldValue, string? newValue) {
        var norm = NormalizeName(newValue);
        if (!string.Equals(newValue, norm, StringComparison.Ordinal)) Name = norm;
        if (!string.Equals(oldValue, Name, StringComparison.Ordinal)) IsModified = true;
    }

    partial void OnDccAddressChanged(int oldValue, int newValue) {
        if (oldValue != newValue) IsModified = true;
    }

    partial void OnIsEditableChanged(bool oldValue, bool newValue) {
        if (oldValue != newValue) IsModified = true;
    }

    // --- Public helpers ---
    public void LockDccAddress() => DccAddressLocked = true;
    public void UnlockDccAddress() => DccAddressLocked = false;
    public void SetDccAddress() => SetDccAddress(Id);
    public void SetDccAddress(string? id) {
        if (!DccAddressLocked && TryDeriveDccAddress(id, out var addr)) DccAddress = addr;
    }

    public static bool TryDeriveDccAddress(string? id, out int address) {
        address = 0;
        if (string.IsNullOrWhiteSpace(id)) return false;
        var m = Digits().Match(id);
        return m.Success && int.TryParse(m.Value, out address);
    }

    // --- Equality & ordering (by Id, then DccAddress, then Name) ---
    public bool Equals(DccTable? other) => other is not null && string.Equals(Id, other.Id, StringComparison.OrdinalIgnoreCase);

    public override bool Equals(object? obj) => obj is DccTable o && Equals(o);

    public override int GetHashCode() => (Id ?? string.Empty).ToUpperInvariant().GetHashCode();

    public int CompareTo(DccTable? other) {
        if (other is null) return 1;
        var byAddr = DccAddress.CompareTo(other.DccAddress);
        if (byAddr != 0) return byAddr;
        var byName = string.Compare(Name, other.Name, StringComparison.OrdinalIgnoreCase);
        if (byName != 0) return byName;
        return string.Compare(Id, other.Id, StringComparison.OrdinalIgnoreCase);
    }

    // --- Internals ---
    private static string? NormalizeId(string? id) => id?.Trim();
    private static string? NormalizeName(string? nm) => nm?.Trim();

    [GeneratedRegex(@"\d+")]
    private static partial Regex Digits();
}