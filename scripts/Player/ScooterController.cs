using Godot;
using ScooterStunt.Progression;

namespace ScooterStunt.Player;

public partial class ScooterController : CharacterBody3D
{
	[Export] public float BaseAcceleration = 16.0f;
	[Export] public float BaseMaxSpeed = 12.0f;
	[Export] public float Friction = 10.0f;
	[Export] public float TurnSpeed = 2.5f;
	[Export] public float BaseJumpVelocity = 10.5f;
	[Export] public float Gravity = 22.0f;

	[Export] public UpgradeDefinition JumpHeightUpgrade;
	[Export] public UpgradeDefinition MaxSpeedUpgrade;

	private float _speed;
	private float _effectiveJumpVelocity;
	private float _effectiveMaxSpeed;
	private PlayerProgression _progression;

	public bool IsAirborne { get; private set; }
	public bool JustLanded { get; private set; }

	public override void _Ready()
	{
		_progression = GetNode<PlayerProgression>("/root/Progression");
		_progression.UpgradePurchased += (_, _) => RecalculateUpgrades();
		RecalculateUpgrades();
	}

	private void RecalculateUpgrades()
	{
		var jumpLevel = JumpHeightUpgrade != null ? _progression.GetUpgradeLevel(JumpHeightUpgrade.UpgradeId) : 0;
		var speedLevel = MaxSpeedUpgrade != null ? _progression.GetUpgradeLevel(MaxSpeedUpgrade.UpgradeId) : 0;

		_effectiveJumpVelocity = BaseJumpVelocity + (JumpHeightUpgrade != null ? jumpLevel * JumpHeightUpgrade.ValuePerLevel : 0.0f);
		_effectiveMaxSpeed = BaseMaxSpeed + (MaxSpeedUpgrade != null ? speedLevel * MaxSpeedUpgrade.ValuePerLevel : 0.0f);
	}

	public override void _PhysicsProcess(double delta)
	{
		var dt = (float)delta;
		var velocity = Velocity;
		var wasAirborne = IsAirborne;
		var onFloor = IsOnFloor();

		if (!onFloor)
		{
			velocity.Y -= Gravity * dt;
		}

		var throttle = Input.GetAxis("scooter_brake", "scooter_accelerate");
		var steer = Input.GetAxis("scooter_steer_left", "scooter_steer_right");

		if (onFloor)
		{
			if (Mathf.Abs(throttle) > 0.01f)
			{
				_speed += throttle * BaseAcceleration * dt;
			}
			else
			{
				_speed = Mathf.MoveToward(_speed, 0.0f, Friction * dt);
			}

			_speed = Mathf.Clamp(_speed, -_effectiveMaxSpeed * 0.5f, _effectiveMaxSpeed);

			if (Mathf.Abs(_speed) > 0.1f)
			{
				var turnAmount = -steer * TurnSpeed * dt * Mathf.Sign(_speed);
				RotateY(turnAmount);
			}

			var forward = -GlobalTransform.Basis.Z;
			velocity.X = forward.X * _speed;
			velocity.Z = forward.Z * _speed;

			if (Input.IsActionJustPressed("scooter_jump"))
			{
				velocity.Y = _effectiveJumpVelocity;
			}
		}

		IsAirborne = !onFloor;
		JustLanded = wasAirborne && onFloor;

		Velocity = velocity;
		MoveAndSlide();
	}
}
