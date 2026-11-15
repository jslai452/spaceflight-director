using Godot;

public partial class StageDecoupler : Node3D
{
	[Export] public Node3D connectedStage;
	[Export] public Node3D planetCenter;
	[Export] public float planetMass;
	[Export] public float minDistance;
	[Export] public float gravitationalConstant;
	[Export] public float decouplePower = 10f;
	[Export] public AudioStreamPlayer DecoupleAudio;

	public void DecoupleStage()
	{
		GD.Print($"[StageDecoupler] DecoupleStage() called on object: {Name}");

		if (connectedStage != null)
		{
			GD.Print($"[StageDecoupler] '{Name}' is decoupling stage: {connectedStage.Name}");

			// 1. Capture the global transform before modifying hierarchy
			Transform3D originalTransform = connectedStage.GlobalTransform;

			// 2. Remove the connected stage from its current parent
			Node originalParent = connectedStage.GetParent();
			if (originalParent != null)
			{
				originalParent.RemoveChild(connectedStage);
			}

			// 3. Create the RigidBody3D dynamically via CelestialBodyGravity
			CelestialBodyGravity gravityBody = new CelestialBodyGravity();
			gravityBody.Name = "DecoupledStageRigidBody";

			// Set celestial gravity parameters
			gravityBody.PlanetCenter = planetCenter;
			gravityBody.PlanetMass = planetMass;
			gravityBody.GravitationalConstant = gravitationalConstant;
			gravityBody.MinDistance = minDistance;

			// Get rid of standard engine gravity and drag
			gravityBody.GravityScale = 0f;
			gravityBody.LinearDamp = 0f;
			gravityBody.AngularDamp = 0f;
			gravityBody.Freeze = false;
			gravityBody.Sleeping = false;

			// Match original transform and add to the scene root
			gravityBody.GlobalTransform = originalTransform;
			GetTree().Root.AddChild(gravityBody);

			// 4. Attach the visual stage node as a child of the new physics body
			gravityBody.AddChild(connectedStage);
			connectedStage.Transform = Transform3D.Identity; // Reset local transform relative to the body

			// 5. Apply decoupling force using Impulse
			Vector3 thrustDirection = GlobalTransform.Basis.Y;
			Vector3 impulseVector = thrustDirection * decouplePower;

			gravityBody.ApplyImpulse(impulseVector);
			if (DecoupleAudio != null){
			DecoupleAudio.Play();
			}

			GD.Print($"[StageDecoupler] Successfully created RigidBody3D and decoupled '{connectedStage.Name}'!");
		}
		else
		{
			GD.PrintErr($"[StageDecoupler] ERROR: Decoupler on '{Name}' has NO 'connectedStage' assigned in the Inspector!");
		}
	}
}
