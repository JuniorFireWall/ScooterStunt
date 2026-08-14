using Godot;

namespace ScooterStunt.Progression;

public enum UpgradeStat
{
	JumpHeight,
	MaxSpeed,
	LandingTolerance
}

public partial class UpgradeDefinition : Resource
{
	[Export] public string UpgradeId = "";
	[Export] public string DisplayName = "";
	[Export] public string DescriptionText = "";
	[Export] public UpgradeStat Stat = UpgradeStat.JumpHeight;
	[Export] public int MaxLevel = 3;
	[Export] public int CostPerLevel = 1;
	[Export] public float ValuePerLevel = 1.0f;
}
