using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCCommon.Client;

namespace DCCPanelController.Helpers;

public class CapabilityIcon {
    public required ImageSource IconSource { get; set; }
    public string? Name => Enum.GetName(Capability);
    public DccClientCapabilitiesEnum Capability { get; set; }
    public bool IsAvailable { get; set; }
    public bool IsNotAvailable => !IsAvailable;
}

public partial class Capabilities : ObservableObject {
    [ObservableProperty] private ObservableCollection<CapabilityIcon> capabilityIcons;

    public Capabilities() : this([]) { }
    public Capabilities(List<DccClientCapabilitiesEnum> availableCapabilities) {
        CapabilityIcons = [];

        // Convert array to HashSet for O(1) lookup performance
        var availableCapabilitiesSet = new HashSet<DccClientCapabilitiesEnum>(availableCapabilities);
        foreach (var capability in Enum.GetValues<DccClientCapabilitiesEnum>()) {
            var isEnabled = availableCapabilitiesSet.Contains(capability);
            var baseIcon = GetIconForCapability(capability, isEnabled);
            
            CapabilityIcons.Add(new CapabilityIcon {
                IconSource = baseIcon,
                Capability = capability,
                IsAvailable = isEnabled
            });
        }
    }

    private ImageSource GetIconForCapability(DccClientCapabilitiesEnum capability, bool isEnabled) {
        return capability switch {
            DccClientCapabilitiesEnum.Turnouts => isEnabled ? ImageSource.FromFile("turnout_thrown.png") : ImageSource.FromFile("turnout_unknown.png"),
            DccClientCapabilitiesEnum.Routes   => isEnabled ? ImageSource.FromFile("route_active.png") : ImageSource.FromFile("route_unknown.png"),
            DccClientCapabilitiesEnum.Sensors  => isEnabled ? ImageSource.FromFile("sensor_active.png") :ImageSource.FromFile("sensor_unknown.png"),
            DccClientCapabilitiesEnum.Blocks   => isEnabled ? ImageSource.FromFile("block_free.png") :ImageSource.FromFile("block_unknown.png"),
            DccClientCapabilitiesEnum.Signals  => isEnabled ? ImageSource.FromFile("signal_active.png") :ImageSource.FromFile("signal_unknown.png"),
            DccClientCapabilitiesEnum.Lights   => isEnabled ? ImageSource.FromFile("light_active.png") :ImageSource.FromFile("light_unknown.png"),
            _ => throw new ApplicationException("Missing DCCClientCapabilitiesEnum mapping for " + capability + " in Capabilities.GetIconForCapability(")
        };
    }
}