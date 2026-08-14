using Godot;
using ScooterStunt.Player;
using ScooterStunt.Progression;

namespace ScooterStunt.Tricks;

public partial class TrickController : Node
{
	[Export] public NodePath ScooterPath;
	[Export] public NodePath VisualMeshPath;
	[Export] public float SpinSpeedDegreesPerSecond = 420.0f;
	[Export] public float BaseCleanLandingToleranceDegrees = 35.0f;
	[Export] public int PointsPerFullSpin = 100;
	[Export] public UpgradeDefinition LandingToleranceUpgrade;

	private ScooterController _scooter;
	private Node3D _visualMesh;
	private ScoreManager _scoreManager;
	private PlayerProgression _progression;
	private float _airSpinDegrees;
	private float _effectiveToleranceDegrees;

	public override void _Ready()
	{
		_scooter = GetNode<ScooterController>(ScooterPath);
		_visualMesh = GetNode<Node3D>(VisualMeshPath);
		_scoreManager = GetNode<ScoreManager>("/root/Score");
		_progression = GetNode<PlayerProgression>("/root/Progression");

		_progression.UpgradePurchased += (_, _) => RecalculateUpgrades();
		RecalculateUpgrades();
	}

	private void RecalculateUpgrades()
	{
		var level = LandingToleranceUpgrade != null ? _progression.GetUpgradeLevel(LandingToleranceUpgrade.UpgradeId) : 0;
		_effectiveToleranceDegrees = BaseCleanLandingToleranceDegrees + (LandingToleranceUpgrade != null ? level * LandingToleranceUpgrade.ValuePerLevel : 0.0f);
	}

	public override void _PhysicsProcess(double delta)
	{
		var dt = (float)delta;

		if (_scooter.IsAirborne)
		{
			var spinInput = Input.GetAxis("scooter_steer_left", "scooter_steer_right");
			if (Mathf.Abs(spinInput) > 0.01f)
			{
				var spinDelta = spinInput * SpinSpeedDegreesPerSecond * dt;
				_airSpinDegrees += spinDelta;
				_visualMesh.RotateY(Mathf.DegToRad(spinDelta));
			}
		}

		if (_scooter.JustLanded)
		{
			EvaluateLanding();
		}
	}

	private void EvaluateLanding()
	{
		var absSpin = Mathf.Abs(_airSpinDegrees);
		var fullSpins = Mathf.RoundToInt(absSpin / 360.0f);
		var remainder = absSpin - fullSpins * 360.0f;

		if (fullSpins > 0 && Mathf.Abs(remainder) <= _effectiveToleranceDegrees)
		{
			_scoreManager.AddTrickScore(fullSpins, PointsPerFullSpin);
		}
		else if (fullSpins > 0)
		{
			_scoreManager.BreakCombo();
		}

		_airSpinDegrees = 0.0f;
		var rotation = _visualMesh.Rotation;
		rotation.Y = 0.0f;
		_visualMesh.Rotation = rotation;
	}
}
