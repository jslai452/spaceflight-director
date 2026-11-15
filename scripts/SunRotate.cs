using Godot;
using System;

public partial class SunRotate : Node3D
{
	// Rotation speed in degrees per second at normal 1x time
	[Export] public float RotationSpeedDegrees = 45.0f;

	// Path to your TimeWarpManager node in the scene tree
	[Export] public NodePath TimeWarpManagerPath;

	private TimeWarpManager _timeWarpManager;

	public override void _Ready()
	{
		// Try to fetch the manager if a path was provided in the inspector
		if (TimeWarpManagerPath != null)
		{
			_timeWarpManager = GetNodeOrNull<TimeWarpManager>(TimeWarpManagerPath);
		}
		
		// Fallback: search globally if the path wasn't manually set
		if (_timeWarpManager == null)
		{
			_timeWarpManager = GetTree().Root.FindChild("TimeWarpManager", true, false) as TimeWarpManager;
		}

		if (_timeWarpManager == null)
		{
			GD.PrintErr("[SunRotate] Could not find TimeWarpManager! Rotating at default 1x speed.");
		}
	}

	public override void _Process(double delta)
	{
		// 1. Determine current warp multiplier (default to 1.0f if manager is missing)
		float currentWarpRate = _timeWarpManager != null ? _timeWarpManager.CurrentWarpRate : 1.0f;

		// 2. Scale your delta by the warp multiplier
		float warpedDelta = (float)delta * currentWarpRate;

		// 3. Apply the rotation using the warped time step
		float rotationAmount = Mathf.DegToRad(RotationSpeedDegrees) * warpedDelta;
		RotateY(rotationAmount);
	}
}
