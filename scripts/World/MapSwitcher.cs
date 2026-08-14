using Godot;

namespace ScooterStunt.World;

public partial class MapSwitcher : Node3D
{
	[Export] public string NextMapPath = "";

	public override void _UnhandledInput(InputEvent @event)
	{
		if (string.IsNullOrEmpty(NextMapPath))
		{
			return;
		}

		if (@event is InputEventKey { Pressed: true, Keycode: Key.Tab })
		{
			GetTree().ChangeSceneToFile(NextMapPath);
		}
	}
}
