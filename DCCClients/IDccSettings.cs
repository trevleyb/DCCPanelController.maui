using System.Security.Cryptography.X509Certificates;

namespace DCCClients.Interfaces;

public interface IDccSettings {
    string Name { get; }
    string Type { get; }
}