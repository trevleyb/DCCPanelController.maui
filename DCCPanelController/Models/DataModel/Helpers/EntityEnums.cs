using System.Diagnostics;

// ReSharper disable once CheckNamespace
namespace DCCPanelController.Models.DataModel.Entities;

public enum ButtonStateEnum     { Unknown, On, Off }
public enum TurnoutStateEnum    { Unknown, Closed, Thrown }
public enum RouteStateEnum      { Unknown, Active, Inactive, }
public enum ButtonSizeEnum      { Normal, Large }
public enum TrackTypeEnum       { MainLine, BranchLine }
public enum TrackTerminatorEnum { Normal, Arrow, Lines }
public enum TrackAttributeEnum  { Normal, Dashed }