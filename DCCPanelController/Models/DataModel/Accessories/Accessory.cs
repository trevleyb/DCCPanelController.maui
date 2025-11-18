using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using CommunityToolkit.Mvvm.ComponentModel;

namespace DCCPanelController.Models.DataModel.Accessories;

/// <summary>
/// Base class for all accessories (turnouts, routes, signals, etc.) that can be
/// addressed over a WiThrottle-style connection.
///
/// Rules:
/// - SystemId is primary when the server knows about it (PTL/PRL).
/// - DccAddress is editable; if not set explicitly, we infer it from trailing digits
///   of SystemId (e.g. "NT432" -> 432).
/// - On each connection, we validate against that server's system IDs.
///   If valid, we send SystemId. If not, and numeric fallback is allowed,
///   we send DccAddress as a pure numeric name.
/// </summary>
public abstract partial class Accessory : ObservableObject, IEquatable<Accessory>, IComparable<Accessory> {
    
    [ObservableProperty] private string?              _systemId;
    [ObservableProperty] private string?              _name;
    [ObservableProperty] private int?                 _dccAddress;
    [ObservableProperty] private bool                 _isValidForCurrentConnection;
    [ObservableProperty] private AccessorySource      _source      = AccessorySource.Unknown;
    [ObservableProperty] private AccessoryBindingMode _bindingMode = AccessoryBindingMode.Unbound;

    [JsonIgnore] public string DisplayFormat => $"{(SystemId ?? "Unnamed")} ({(DccAddress.HasValue ? DccAddress.Value.ToString() : "—")})";

    // Tracks whether DccAddress was explicitly edited vs inferred from SystemId.
    private bool dccAddressHasBeenExplicitlySet;

    partial void OnSystemIdChanged(string? value) {
        // If the user hasn't explicitly edited DccAddress, try to infer it
        // from the new SystemId (e.g., "NT432" -> 432).
        if (!dccAddressHasBeenExplicitlySet) {
            InferDccAddressFromSystemId();
        }
    }

    partial void OnDccAddressChanged(int? value) {
        // Any explicit change via the property marks it as "explicit".
        dccAddressHasBeenExplicitlySet = value.HasValue;
    }

    // -----------------------------
    // Public helpers
    // -----------------------------

    /// <summary>
    /// Returns the identifier that should be sent to the backend for this accessory
    /// on the current connection (e.g. in a PTA command), or null if unbound.
    /// </summary>
    public string? GetCommandIdentifier() {
        return BindingMode switch {
            AccessoryBindingMode.SystemId when!string.IsNullOrWhiteSpace(SystemId) => SystemId!.Trim(),
            AccessoryBindingMode.NumericDccAddress when DccAddress.HasValue => DccAddress.Value.ToString(),
            _ => null
        };
    }

    /// <summary>
    /// Re-evaluates how this accessory should be bound for the current connection,
    /// given the set of known system IDs (from PTL/PRL) and server capabilities.
    /// </summary>
    public void UpdateBindingForConnection(ISet<string> knownSystemIds, AccessoryCapabilities? capabilities = null) {
        capabilities ??= AccessoryCapabilities.Default;

        BindingMode = AccessoryBindingMode.Unbound;
        IsValidForCurrentConnection = false;

        var trimmedSystemId = SystemId?.Trim();

        // 1. Prefer a direct SystemId match from the server's PTL/PRL.
        if (!string.IsNullOrEmpty(trimmedSystemId) &&
            knownSystemIds.Contains(trimmedSystemId)) {
            BindingMode = AccessoryBindingMode.SystemId;
            IsValidForCurrentConnection = true;
            return;
        }

        // 2. Otherwise, attempt numeric fallback if explicitly allowed.
        if (capabilities.SupportsNumericTurnoutName &&
            capabilities.NumericNameIsDccAddress &&
            DccAddress.HasValue) {
            BindingMode = AccessoryBindingMode.NumericDccAddress;
            IsValidForCurrentConnection = true;
            return;
        }

        // 3. Nothing matched; leave unbound.
        BindingMode = AccessoryBindingMode.Unbound;
        IsValidForCurrentConnection = false;
    }

    /// <summary>
    /// Ensures DccAddress is populated if possible using the current SystemId.
    /// Useful after bulk imports when you want to auto-fill addresses from IDs.
    /// </summary>
    public void EnsureDccAddressInitialized() {
        if (!dccAddressHasBeenExplicitlySet && !DccAddress.HasValue) {
            InferDccAddressFromSystemId();
        }
    }

    // -----------------------------
    // Internal helpers
    // -----------------------------

    /// <summary>
    /// Attempts to infer DCC address from trailing digits of SystemId (e.g. "NT432" -> 432).
    /// This implements your JMRI-style assumption: NTnnn / LTnnn => DccAddress nnn.
    /// </summary>
    public void InferDccAddressFromSystemId() => InferDccAddressFrom(SystemId); 
    
    public void InferDccAddressFrom(string? id) {
        if (string.IsNullOrWhiteSpace(id)) return;

        // Find trailing digit run.
        id = id.Trim();
        var index = id.Length - 1;
        while (index >= 0 && char.IsDigit(id[index])) index--;

        var digitsStart = index + 1;
        if (digitsStart >= id.Length) return; // no trailing digits

        var numericPart = id.Substring(digitsStart);
        if (int.TryParse(numericPart, out var address)) {
            // Set the backing field directly so we don't mark it as explicitly set.
            DccAddress = address;
            OnPropertyChanged(nameof(DccAddress));

            // dccAddressHasBeenExplicitlySet remains false so future SystemId changes
            // can re-infer if needed.
        }
    }
    
    // --- Equality & ordering (by Id, then DccAddress, then Name) ---
    public bool Equals(Accessory? other) => other is not null && string.Equals(SystemId, other.SystemId, StringComparison.OrdinalIgnoreCase);
    public override bool Equals(object? obj) => obj is Accessory acc && Equals(acc);

    public override int GetHashCode() => (SystemId ?? string.Empty).ToUpperInvariant().GetHashCode();

    public int CompareTo(Accessory? other) {
        if (other is null) return 1;
        var byAddr = DccAddress == other.DccAddress;
        if (byAddr) return 0;
        var byName = string.Compare(Name, other.Name, StringComparison.OrdinalIgnoreCase);
        return byName != 0 ? byName : string.Compare(SystemId, other.SystemId, StringComparison.OrdinalIgnoreCase);
    }

    // --- Internals ---
    private static string? NormalizeId(string? id) => id?.Trim();
    private static string? NormalizeName(string? nm) => nm?.Trim();

    [GeneratedRegex(@"\d+")]
    private static partial Regex Digits();
}