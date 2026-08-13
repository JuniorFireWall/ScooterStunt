using Godot;

namespace ScooterStunt.Player;

public partial class ScooterController : CharacterBody3D
{
	[Export] public float Acceleration = 16.0f;
	[Export] public float MaxSpeed = 12.0f;
	[Export] public float Friction = 10.0f;
	[Export] public float TurnSpeed = 2.5f;
	[Export] public float JumpVelocity = 10.5f;
	[Export] public float Gravity = 22.0f;

	private float _speed;

	public bool IsAirborne { get; private set; }
	public bool JustLanded { get; private set; }

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
				_speed += throttle * Acceleration * dt;
			}
			else
			{
				_speed = Mathf.MoveToward(_speed, 0.0f, Friction * dt);
			}

			_speed = Mathf.Clamp(_speed, -MaxSpeed * 0.5f, MaxSpeed);

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
				velocity.Y = JumpVelocity;
			}
		}

		IsAirborne = !onFloor;
		JustLanded = wasAirborne && onFloor;

		Velocity = velocity;
		MoveAndSlide();
	}
}
