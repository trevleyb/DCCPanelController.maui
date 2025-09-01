using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Clients;

namespace DCCPanelController.View.Helpers;

public class CapabilityIcon {
    public required ImageSource IconSource { get; set; }
    public string? Name => Enum.GetName(Capability);
    public DccClientCapability Capability { get; set; }
    public bool IsAvailable { get; set; }
    public bool IsNotAvailable => !IsAvailable;
}

public partial class Capabilities : ObservableObject {
    [ObservableProperty] private ObservableCollection<CapabilityIcon> capabilityIcons;

    public Capabilities() : this([]) { }
    public Capabilities(List<DccClientCapability> availableCapabilities) {
        CapabilityIcons = [];

        // Convert array to HashSet for O(1) lookup performance
        var availableCapabilitiesSet = new HashSet<DccClientCapability>(availableCapabilities);
        foreach (var capability in Enum.GetValues<DccClientCapability>()) {
            var isEnabled = availableCapabilitiesSet.Contains(capability);
            var baseIcon = GetIconForCapability(capability, isEnabled);
            
            CapabilityIcons.Add(new CapabilityIcon {
                IconSource = baseIcon,
                Capability = capability,
                IsAvailable = isEnabled
            });
        }
    }

    private ImageSource GetIconForCapability(DccClientCapability capability, bool isEnabled) {
        return capability switch {
            DccClientCapability.Turnouts => isEnabled ? ImageSource.FromFile("turnout_thrown.png") : ImageSource.FromFile("turnout_unknown.png"),
            DccClientCapability.Routes   => isEnabled ? ImageSource.FromFile("route_active.png") : ImageSource.FromFile("route_unknown.png"),
            DccClientCapability.Sensors  => isEnabled ? ImageSource.FromFile("sensor_active.png") :ImageSource.FromFile("sensor_unknown.png"),
            DccClientCapability.Blocks   => isEnabled ? ImageSource.FromFile("block_free.png") :ImageSource.FromFile("block_unknown.png"),
            DccClientCapability.Signals  => isEnabled ? ImageSource.FromFile("signal_active.png") :ImageSource.FromFile("signal_unknown.png"),
            DccClientCapability.Lights   => isEnabled ? ImageSource.FromFile("light_active.png") :ImageSource.FromFile("light_unknown.png"),
            _                            => throw new ApplicationException("Missing DCCClientCapabilitiesEnum mapping for " + capability + " in Capabilities.GetIconForCapability(")
        };
    }
}