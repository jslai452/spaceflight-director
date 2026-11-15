using Godot;

public partial class AtmosphericDrag : Node3D
{
	[Export] public RigidBody3D TargetBody;
	[Export] public Node3D PlanetCenter; // <--- Drag-and-drop your Planet Node3D here!
	[Export] public float DragCoefficient = 0.0005f; 
	[Export] public float PlanetRadius = 260000.0f;     // Planet radius in meters (260 km)
	[Export] public float AtmosphereHeight = 30000.0f;  // Atmosphere height (30 km)

	public override void _Ready()
	{
		if (TargetBody == null)
		{
			TargetBody = GetParent<RigidBody3D>();
		}

		if (PlanetCenter == null)
		{
			//GD.PrintErr("AtmosphericDrag: PlanetCenter node is not assigned in the Inspector! Defaulting to Vector3.Zero.");
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		if (TargetBody == null) return;

		// Fetch the global position of your PlanetCenter node (falls back to Vector3.Zero if unassigned)
		Vector3 centerPosition = PlanetCenter != null ? PlanetCenter.GlobalPosition : Vector3.Zero;

		// 1. Calculate true distance from the Node3D's global position
		float distanceFromCenter = TargetBody.GlobalPosition.DistanceTo(centerPosition);

		// 2. Altitude is distance from center minus the planet radius
		float currentAltitude = distanceFromCenter - PlanetRadius;

		// 3. Check if the rocket is inside the atmosphere layer
		if (currentAltitude <= AtmosphereHeight && currentAltitude >= 0.0f)
		{
			// Make air thinner the higher up you go
			float densityMultiplier = 1.0f - (currentAltitude / AtmosphereHeight);

			// Get current velocity vector
			Vector3 velocity = TargetBody.LinearVelocity;
			float speed = velocity.Length();

			if (speed > 0.0f)
			{
				// Calculate drag
				float dragMagnitude = DragCoefficient * speed * speed * densityMultiplier;
				Vector3 dragForce = -velocity.Normalized() * dragMagnitude;

				// Apply force
				TargetBody.ApplyCentralForce(dragForce);

				// --- DEBUG PRINT ---
				string nodeName = PlanetCenter != null ? PlanetCenter.Name : "World Origin";
				//GD.Print($"[Atmosphere] Target: {nodeName} | Alt: {currentAltitude:F1}m | Speed: {speed:F1}m/s | Force: {dragForce}");
			}
		}
	}
}
