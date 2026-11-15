using Godot;
using System;

public partial class EarthMainMenuRotater : Node
{
	[Export] public MeshInstance3D Earth;
	public float RotationSpeed = 0.01f; // Speed of rotation in radians per second

	public override void _Process(double delta)
	{
		if (Earth != null)
		{
			// Rotate the mesh around its own local Y-axis
			Earth.RotateObjectLocal(Vector3.Up, RotationSpeed * (float)delta);
		}
	}
}
