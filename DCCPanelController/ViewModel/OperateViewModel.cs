using System.Collections.ObjectModel;
using DCCPanelController.Model;
using DCCPanelController.Services;
using InvalidOperationException = System.InvalidOperationException;

namespace DCCPanelController.ViewModel;

public class OperateViewModel : BaseViewModel  {

    private readonly SettingsService? _settingsService;
    private readonly ConnectionService? _connectionService;
    public ObservableCollection<Panel> Panels { get; set; } = [];

    public OperateViewModel() {
        _settingsService = App.ServiceProvider?.GetRequiredService<SettingsService>() ?? null;
        _connectionService = App.ServiceProvider?.GetRequiredService<ConnectionService>() ?? null;
        if (_connectionService is null) throw new InvalidOperationException("Unable to get the Connections Service");
        if (_settingsService is null) throw new InvalidOperationException("Unable to get the Settings Service");
        Panels = _settingsService.Panels;
    }
}