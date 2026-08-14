using Godot;
using ScooterStunt.Tricks;

namespace ScooterStunt.UI;

public partial class Hud : CanvasLayer
{
	private Label _scoreLabel;
	private Label _comboLabel;
	private ScoreManager _scoreManager;

	public override void _Ready()
	{
		_scoreLabel = GetNode<Label>("ScoreLabel");
		_comboLabel = GetNode<Label>("ComboLabel");
		_scoreManager = GetNode<ScoreManager>("/root/Score");

		_scoreManager.ScoreChanged += OnScoreChanged;
		_scoreManager.ComboChanged += OnComboChanged;

		OnScoreChanged(_scoreManager.TotalScore);
		OnComboChanged(_scoreManager.ComboCount);
	}

	private void OnScoreChanged(int totalScore)
	{
		_scoreLabel.Text = $"Score: {totalScore}";
	}

	private void OnComboChanged(int comboCount)
	{
		_comboLabel.Text = comboCount > 0 ? $"Combo x{comboCount}" : "";
	}
}
