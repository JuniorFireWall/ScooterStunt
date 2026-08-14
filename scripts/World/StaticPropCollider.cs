using Godot;

namespace ScooterStunt.World;

// Auto-generates trimesh collision for every mesh in an imported static
// prop (e.g. a skatepark piece from a third-party FBX pack) so we don't
// need to hand-author collision shapes for geometry we didn't build.
public partial class StaticPropCollider : Node3D
{
	public override void _Ready()
	{
		ApplyRecursive(this);
	}

	private static void ApplyRecursive(Node node)
	{
		foreach (var child in node.GetChildren())
		{
			if (child is MeshInstance3D meshInstance)
			{
				meshInstance.CreateTrimeshCollision();
			}

			ApplyRecursive(child);
		}
	}
}
