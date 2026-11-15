using Godot;

public partial class CelestialBodyGravity : RigidBody3D
{
	[ExportCategory("Planet Reference")]
	[Export] public Node3D PlanetCenter { get; set; }
	[Export] public float PlanetMass { get; set; } = 147003.3f;
	[Export] public float GravitationalConstant { get; set; } = 6674f;
	[Export] public float PlanetRadius {get; set;} = 60000f;
	[Export] public float AtmosphereHeight {get; set;} = 70000f;
	[Export] public FlightHUD flightHud;

	[ExportCategory("Gravity Settings")]
	[Export] public float MinDistance { get; set; } = 260000f;

	public float Mu => GravitationalConstant * PlanetMass;

	public override void _Ready()
	{
		GravityScale = 0f;
		LinearDampMode = DampMode.Replace;
	LinearDamp = 0.0f;
		//GD.Print($"[CelestialBodyGravity] Initialized on {Name}. Calculated Mu = {Mu}");
	}
	

	public override void _PhysicsProcess(double delta)
	{
		if (PlanetCenter == null || Freeze)
		{
			// GD.Print("[CelestialBodyGravity] Skipped: PlanetCenter is null or body is frozen.");
			return;
		}

		Vector3 displacement = PlanetCenter.GlobalPosition - GlobalPosition;
		float distance = Mathf.Max(displacement.Length(), MinDistance);
		Vector3 direction = displacement.Normalized();

		float accelMag = Mu / (distance * distance);
		
		// Debug print to monitor live gravity calculation values
		//GD.Print($"[CelestialBodyGravity] Distance: {distance:F2}m | Acceleration: {accelMag:F4} m/s²");
		flightHud?.SetAltitude(distance - PlanetRadius);
			// 1. Get the planet's speed (matching your CircularizeOrbit logic)
	Vector3 planetVel = Vector3.Zero;
	if (PlanetCenter is RigidBody3D planetRb)
	{
		planetVel = planetRb.LinearVelocity;
	}

	// 2. Calculate the speeds
	float orbitalSpeed = LinearVelocity.Length();
	float surfaceSpeed = (LinearVelocity - planetVel).Length();

	// 3. Send the speed to your HUD (choose either orbitalSpeed or surfaceSpeed)
	// assuming your HUD script has a SetSpeed method:
	flightHud?.SetSpeed(surfaceSpeed); 

	// -------------------------------
	float currentAltitude = distance - PlanetRadius;

	ApplyCentralForce(direction * accelMag * Mass);
	}
	
	public bool IsOutsideAtmosphere(Vector3 craftGlobalPosition)
	{
		if (PlanetCenter == null) {return true;}
		
		float distanceToCenter = (craftGlobalPosition - PlanetCenter.GlobalPosition).Length();
		float maxAtmosphereRadius = PlanetRadius + AtmosphereHeight;
		return distanceToCenter > maxAtmosphereRadius;
	}
	public void CircularizeOrbit()
{
	if (PlanetCenter == null)
	{
		//GD.Print("[CelestialBodyGravity] Cannot circularize: PlanetCenter is not assigned.");
		return;
	}

	// 1. Get vector from planet center to the craft
	Vector3 displacement = GlobalPosition - PlanetCenter.GlobalPosition;
	float rMag = displacement.Length();

	if (rMag <= 0f) return;

	// 2. Calculate circular orbital speed (v = sqrt(Mu / r))
	float orbitalSpeed = Mathf.Sqrt(Mu / rMag);

	// 3. Find a stable tangential direction (perpendicular to radial and an up axis)
	Vector3 radialDir = displacement.Normalized();
	Vector3 tangentDir = radialDir.Cross(Vector3.Up);

	// Fallback if the craft is directly aligned with the world up-axis
	if (tangentDir.LengthSquared() < 0.0001f)
	{
		tangentDir = radialDir.Cross(Vector3.Forward);
	}
	tangentDir = tangentDir.Normalized();

	// 4. Account for planet velocity if the planet is moving
	Vector3 planetVel = Vector3.Zero;
	if (PlanetCenter is RigidBody3D planetRb)
	{
		planetVel = planetRb.LinearVelocity;
	}

	// 5. Apply the velocity
	LinearVelocity = planetVel + (tangentDir * orbitalSpeed);
	
	//GD.Print($"[CelestialBodyGravity] Craft circularized! Distance: {rMag:F2}m | Orbital Speed: {orbitalSpeed:F2} m/s");
}
}
