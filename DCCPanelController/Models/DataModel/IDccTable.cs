namespace DCCPanelController.Models.DataModel;

public interface IDccTable {
    string? Id { get; set; }
    string? Name { get; set; }
    int DccAddress { get; set; }
    bool IsEditable { get; set; }
    bool IsModified { get; set; }
    bool DccAddressLocked { get; set; }

    string DisplayFormat { get; }
    
    void LockDccAddress();
    void UnlockDccAddress();
    void SetDccAddress(); 
    void SetDccAddress(string? id);
    
}