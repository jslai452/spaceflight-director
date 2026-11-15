using Godot;
using System;
using System.Collections.Generic;

public partial class FlightController : Node
{
	private float _throttle = 0f;

	[Export] public RocketFuelSystem fuelSystem;
	[Export] public RigidBody3D RocketRb;
	[Export] public MeshInstance3D debugLine;
	[Export] public CelestialBodyGravity rocketGravity;
	[Export] public FlightHUD flightHud;

	private void EvaluateStages()
	{
		if (RocketRb == null) return;

		Node lowestStage = null;
		int lowestStageNumber = int.MaxValue;

		// 1. Find the actual lowest stage numerically
		foreach (Node child in RocketRb.GetChildren())
		{
			if (child.IsInGroup("Stage") && child is Stage stageScript)
			{
				int stageNum = stageScript.ReturnStage();
				if (stageNum < lowestStageNumber)
				{
					lowestStageNumber = stageNum;
					lowestStage = child;
				}
			}
		}

		if (lowestStage == null) return;

		List<StageDecoupler> decouplersToTrigger = new List<StageDecoupler>();

		// 2. Find components inside the lowest stage
		foreach (Node child in lowestStage.GetChildren())
		{
			if (child.IsInGroup("Decoupler") && child is StageDecoupler sd)
			{
				decouplersToTrigger.Add(sd);
			}

			// Activate thrusters inside this stage
			List<Thruster> stageThrusters = GetComponentsInChildren<Thruster>(child);
			foreach (Thruster thruster in stageThrusters)
			{
				thruster.IsOn = true;
				GD.Print($"Thruster activated on: {thruster.Name}");
			}
		}

		// 3. Fire decouplers
		foreach (StageDecoupler decoupler in decouplersToTrigger)
		{
			decoupler.DecoupleStage();
		}

		lowestStage.RemoveFromGroup("Stage");
		lowestStage.AddToGroup("SpentStage");
	}

	private void DrawDebugForce(Vector3 startPos, Vector3 forceVector)
{
	if (debugLine == null) return;

	ImmediateMesh im = debugLine.Mesh as ImmediateMesh;
	if (im == null)
	{
		im = new ImmediateMesh();
		debugLine.Mesh = im;
	}

	im.ClearSurfaces();
	im.SurfaceBegin(Mesh.PrimitiveType.Lines);

	// Scale down the force vector so it fits on screen (e.g., 1 unit per 10,000 Newtons)
	float scaleFactor = 0.0001f; 
	Vector3 endPos = startPos + (forceVector * scaleFactor);

	// Draw line from the thruster's global position in the direction of the force
	im.SurfaceAddVertex(debugLine.ToLocal(startPos));
	im.SurfaceAddVertex(debugLine.ToLocal(endPos));

	im.SurfaceEnd();
}

	private List<T> GetComponentsInChildren<T>(Node root) where T : Node
	{
		List<T> results = new List<T>();
		if (root is T match)
		{
			results.Add(match);
		}
		foreach (Node child in root.GetChildren(includeInternal: true))
		{
			results.AddRange(GetComponentsInChildren<T>(child));
		}
		return results;
	}

	public override void _PhysicsProcess(double delta)
	{

		if (RocketRb == null || fuelSystem == null) return;

		float throttlePercent = _throttle / 100f;
		List<Thruster> allThrusters = GetComponentsInChildren<Thruster>(RocketRb);
		
		bool isBurningFuel = false;

		foreach (Thruster thruster in allThrusters)
		{
			// If engine is off or throttle is zero, ensure effects are stopped
			if (!thruster.IsOn || _throttle <= 0.01f)
			{
				thruster.ForceStopEffects(); // We will add this helper method to Thruster.cs
				continue;
			}

			float generatedThrust = thruster.BurnFuel(throttlePercent, thruster.SpecificImpulse, thruster.MaxConsumptionRate, delta);
			if (generatedThrust <= 0f) continue;

			isBurningFuel = true;

			// 1. Match Unity's childTransform.up direction
			Vector3 thrustDirection = thruster.GlobalTransform.Basis.Y; 
			float appliedForce = generatedThrust * thruster.ThrustMultiplier;
			Vector3 forceVector = thrustDirection * appliedForce;

			// 2. Calculate global position offset
			Vector3 globalOffset = thruster.GlobalPosition - RocketRb.GlobalTransform.Origin;

			// 3. Apply force with offset
			RocketRb.ApplyForce(forceVector, globalOffset);
		}

		if (isBurningFuel)
		{
			fuelSystem.UpdateTotalMassAndCenterOfMass();
		}
	}

	public override void _Process(double delta)
	{
		
		if (Input.IsActionPressed("max_throttle")) {
			_throttle = 100f;
			if (flightHud != null){
			flightHud.SetThrottle(_throttle);
			}
		}
		if (Input.IsActionPressed("cut_throttle")) {
			_throttle = 0f;
			if (flightHud != null) {
			flightHud.SetThrottle(_throttle);
			}
		}

		if (Input.IsActionPressed("increase_throttle"))
		{
			_throttle = Mathf.Clamp(_throttle + 20f * (float)delta, 0f, 100f);
			if (flightHud != null) {
			flightHud.SetThrottle(_throttle);
			}
		}
		if (Input.IsActionPressed("decrease_throttle"))
		{
			_throttle = Mathf.Clamp(_throttle - 20f * (float)delta, 0f, 100f);
			if (flightHud != null) {
			flightHud.SetThrottle(_throttle);
			}
		}
		if (Input.IsActionJustPressed("stage"))        EvaluateStages();
		
		
	}
}
