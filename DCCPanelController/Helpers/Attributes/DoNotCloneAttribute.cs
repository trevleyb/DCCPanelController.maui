namespace DCCPanelController.Helpers.Attributes;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class DoNotCloneAttribute : Attribute { }