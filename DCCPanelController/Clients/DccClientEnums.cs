namespace DCCPanelController.Clients;

public enum DccClientType { Jmri, WiThrottle, Simulator }

public enum DccClientState { Connected, Disconnected, Reconnecting, Initialising, Error }

public enum DccClientCapability { Turnouts, Routes, Blocks, Lights, Signals }

public enum DccClientMessageType { Inbound, Outbound, System, Error }

public enum DccClientOperation { System, Route, Turnout, Block, Light, Signal, Sensor }