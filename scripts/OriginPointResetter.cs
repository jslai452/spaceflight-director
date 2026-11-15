using Godot;
using System;

public partial class OriginPointResetter : Node
{
	[Export(PropertyHint.NodePathValidTypes, "Node3D")]
	public NodePath PlayerPath { get; set; }

	[Export]
	public float Threshold { get; set; } = 5000f;

	private Node3D _playerNode;

	public override void _Ready()
	{
		if (!string.IsNullOrEmpty(PlayerPath))
		{
			_playerNode = GetNodeOrNull<Node3D>(PlayerPath);
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		if (_playerNode == null) return;

		if (_playerNode.GlobalPosition.Length() > Threshold)
		{
			ResetWorldOrigin();
		}
	}

	private void ResetWorldOrigin()
	{
		// Calculate the exact shift offset to bring the player back to (0,0,0)
		Vector3 offset = _playerNode.GlobalPosition;

		// Loop through ONLY the direct children of the Root tree.
		// This safely captures CurrentScene, decoupled stages, and planets 
		// without double-shifting nested child nodes.
		foreach (Node child in GetTree().Root.GetChildren())
		{
			if (child is Node3D node3D)
			{
				if (node3D is RigidBody3D rb)
				{
					// Shift the physics body transform via server
					Transform3D currentTransform = rb.GlobalTransform;
					currentTransform.Origin -= offset;

					PhysicsServer3D.BodySetState(
						rb.GetRid(), 
						PhysicsServer3D.BodyState.Transform, 
						currentTransform
					);
				}
				else
				{
					// Shift standard 3D root nodes (their children move automatically)
					node3D.GlobalPosition -= offset;
				}
			}
		}

		GD.Print($"[Floating Origin] World origin reset! Shifted universe by {offset}. Player is back near (0,0,0)");
	}
}
