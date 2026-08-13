using Godot;

namespace ScooterStunt.Player;

public partial class CameraRig : Node3D
{
	[Export] public NodePath TargetPath;
	[Export] public float FollowSpeed = 6.0f;
	[Export] public float RotationFollowSpeed = 4.0f;
	[Export] public float HeightOffset = 1.0f;

	private Node3D _target;

	public override void _Ready()
	{
		if (TargetPath != null && !TargetPath.IsEmpty)
		{
			_target = GetNode<Node3D>(TargetPath);
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		if (_target == null)
		{
			return;
		}

		var dt = (float)delta;
		var desiredPosition = _target.GlobalPosition + Vector3.Up * HeightOffset;
		GlobalPosition = GlobalPosition.Lerp(desiredPosition, FollowSpeed * dt);

		var targetYaw = _target.GlobalRotation.Y;
		var currentYaw = GlobalRotation.Y;
		var newYaw = Mathf.LerpAngle(currentYaw, targetYaw, RotationFollowSpeed * dt);
		var rot = GlobalRotation;
		rot.Y = newYaw;
		GlobalRotation = rot;
	}
}
