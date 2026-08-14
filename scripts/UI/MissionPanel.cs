using Godot;
using ScooterStunt.Missions;

namespace ScooterStunt.UI;

public partial class MissionPanel : CanvasLayer
{
	[Export] public NodePath MissionManagerPath;

	private MissionManager _missionManager;
	private VBoxContainer _list;
	private Label[] _labels;

	public override void _Ready()
	{
		_missionManager = GetNode<MissionManager>(MissionManagerPath);
		_list = GetNode<VBoxContainer>("Panel/VBox");

		_labels = new Label[_missionManager.Missions.Count];
		for (var i = 0; i < _missionManager.Missions.Count; i++)
		{
			var mission = _missionManager.Missions[i];
			var label = new Label
			{
				Text = FormatLine(mission.MissionName, 0, mission.TargetValue)
			};
			_list.AddChild(label);
			_labels[i] = label;
		}

		_missionManager.MissionProgressChanged += OnProgressChanged;
		_missionManager.MissionCompleted += OnCompleted;
	}

	private void OnProgressChanged(int index, int currentValue, int targetValue)
	{
		var name = _missionManager.Missions[index].MissionName;
		_labels[index].Text = FormatLine(name, currentValue, targetValue);
	}

	private void OnCompleted(int index, string missionName, int rewardXp)
	{
		_labels[index].Text = $"✓ {missionName} (abgeschlossen, +{rewardXp} XP)";
	}

	private static string FormatLine(string name, int current, int target)
	{
		return $"{name}: {Mathf.Min(current, target)}/{target}";
	}
}
