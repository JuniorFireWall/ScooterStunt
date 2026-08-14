using Godot;

namespace ScooterStunt.Missions;

public enum MissionType
{
	ReachScore,
	LandTricksCount,
	ReachCombo
}

public partial class MissionDefinition : Resource
{
	[Export] public string MissionName = "";
	[Export] public string DescriptionText = "";
	[Export] public MissionType Type = MissionType.ReachScore;
	[Export] public int TargetValue = 1;
	[Export] public int RewardXp = 50;
}
