using Godot;
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

	public override void _Ready()
	{
		_progress = new int[Missions.Count];
		_completed = new bool[Missions.Count];

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
			EmitSignal(SignalName.MissionCompleted, index, Missions[index].MissionName, Missions[index].RewardXp);
		}
	}
}
