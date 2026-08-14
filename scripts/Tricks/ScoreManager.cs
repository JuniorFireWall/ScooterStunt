using Godot;

namespace ScooterStunt.Tricks;

public partial class ScoreManager : Node
{
	[Signal] public delegate void ScoreChangedEventHandler(int totalScore);
	[Signal] public delegate void ComboChangedEventHandler(int comboCount);
	[Signal] public delegate void TrickLandedEventHandler(int fullSpins, int pointsAwarded);

	public int TotalScore { get; private set; }
	public int ComboCount { get; private set; }

	public void AddTrickScore(int fullSpins, int basePointsPerSpin)
	{
		ComboCount += 1;
		var points = fullSpins * basePointsPerSpin * ComboCount;
		TotalScore += points;
		EmitSignal(SignalName.TrickLanded, fullSpins, points);
		EmitSignal(SignalName.ScoreChanged, TotalScore);
		EmitSignal(SignalName.ComboChanged, ComboCount);
	}

	public void BreakCombo()
	{
		if (ComboCount == 0)
		{
			return;
		}

		ComboCount = 0;
		EmitSignal(SignalName.ComboChanged, ComboCount);
	}
}
