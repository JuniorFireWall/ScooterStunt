using Godot;
using ScooterStunt.Progression;
using ScooterStunt.Tricks;

namespace ScooterStunt.Missions;

public partial class MissionManager : Node
{
	[Signal] public delegate void MissionProgressChangedEventHandler(int index, int currentValue, int targetValue);
	[Signal] public delegate void MissionCompletedEventHandler(int index, string missionName, int rewardXp);

	[Export] public Godot.Collections.Array<MissionDefinition> Missions = new();

	private int[] _progress;
	private bool[] _completed;
	private int _tricksLandedCount;
	private ScoreManager _scoreManager;
	private PlayerProgression _progression;

	public override void _Ready()
	{
		_progress = new int[Missions.Count];
		_completed = new bool[Missions.Count];
		_progression = GetNode<PlayerProgression>("/root/Progression");

		for (var i = 0; i < Missions.Count; i++)
		{
			if (_progression.IsMissionCompleted(Missions[i].MissionName))
			{
				_completed[i] = true;
				_progress[i] = Missions[i].TargetValue;
				// Deferred so UI nodes (e.g. MissionPanel) finish their own _Ready
				// and connect to this signal before it fires.
				CallDeferred(MethodName.EmitAlreadyCompleted, i);
			}
		}

		_scoreManager = GetNode<ScoreManager>("/root/Score");
		_scoreManager.ScoreChanged += OnScoreChanged;
		_scoreManager.ComboChanged += OnComboChanged;
		_scoreManager.TrickLanded += OnTrickLanded;
	}

	private void OnScoreChanged(int totalScore)
	{
		for (var i = 0; i < Missions.Count; i++)
		{
			if (Missions[i].Type == MissionType.ReachScore)
			{
				UpdateProgress(i, totalScore);
			}
		}
	}

	private void OnComboChanged(int comboCount)
	{
		for (var i = 0; i < Missions.Count; i++)
		{
			if (Missions[i].Type == MissionType.ReachCombo)
			{
				UpdateProgress(i, Mathf.Max(_progress[i], comboCount));
			}
		}
	}

	private void OnTrickLanded(int fullSpins, int pointsAwarded)
	{
		_tricksLandedCount += 1;
		for (var i = 0; i < Missions.Count; i++)
		{
			if (Missions[i].Type == MissionType.LandTricksCount)
			{
				UpdateProgress(i, _tricksLandedCount);
			}
		}
	}

	private void UpdateProgress(int index, int value)
	{
		if (_completed[index])
		{
			return;
		}

		_progress[index] = value;
		EmitSignal(SignalName.MissionProgressChanged, index, value, Missions[index].TargetValue);

		if (value >= Missions[index].TargetValue)
		{
			_completed[index] = true;
			var mission = Missions[index];
			_progression.AddXp(mission.RewardXp);
			_progression.MarkMissionCompleted(mission.MissionName);
			EmitSignal(SignalName.MissionCompleted, index, mission.MissionName, mission.RewardXp);
		}
	}

	private void EmitAlreadyCompleted(int index)
	{
		EmitSignal(SignalName.MissionCompleted, index, Missions[index].MissionName, 0);
	}
}
