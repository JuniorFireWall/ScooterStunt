using Godot;
using ScooterStunt.Player;

namespace ScooterStunt.Tricks;

public partial class TrickController : Node
{
	[Export] public NodePath ScooterPath;
	[Export] public NodePath VisualMeshPath;
	[Export] public float SpinSpeedDegreesPerSecond = 420.0f;
	[Export] public float CleanLandingToleranceDegrees = 35.0f;
	[Export] public int PointsPerFullSpin = 100;

	private ScooterController _scooter;
	private Node3D _visualMesh;
	private ScoreManager _scoreManager;
	private float _airSpinDegrees;

	public override void _Ready()
	{
		_scooter = GetNode<ScooterController>(ScooterPath);
		_visualMesh = GetNode<Node3D>(VisualMeshPath);
		_scoreManager = GetNode<ScoreManager>("/root/Score");
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

		if (fullSpins > 0 && Mathf.Abs(remainder) <= CleanLandingToleranceDegrees)
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
