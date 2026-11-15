using Godot;

public partial class SolarPanel : Node3D
{
	[Export] public bool IsDeployed { get; set; } = true;
	[Export] public float MaxGenerationRate { get; set; } = 5.0f; // Electric charge units per second
	[Export] public Node3D SunReference { get; set; } // Reference to the Sun node in your scene
	[Export] public Marker3D PanelFacingDirection { get; set; } // Points out from the face of the panel

	public float GetCurrentGeneration(Vector3 spacecraftPosition)
	{
		if (!IsDeployed || SunReference == null || PanelFacingDirection == null) 
			return 0f;

		// 1. Calculate direction vector from spacecraft to the Sun
		Vector3 sunDir = (SunReference.GlobalPosition - spacecraftPosition).Normalized();
		
		// 2. Get the panel's facing direction vector
		Vector3 panelNormal = -PanelFacingDirection.GlobalTransform.Basis.Z;

		// 3. Use Dot Product to check alignment (1.0 = facing directly at sun, <= 0 = facing away)
		float alignment = panelNormal.Dot(sunDir);

		if (alignment <= 0f)
		{
			return 0f; // Facing away from the sun
		}

		// 4. Check if a planet is blocking the line of sight to the sun (shadow/eclipse check)
		PhysicsDirectSpaceState3D spaceState = GetWorld3D()?.DirectSpaceState;
		if (spaceState != null)
		{
			var query = PhysicsRayQueryParameters3D.Create(spacecraftPosition, SunReference.GlobalPosition);
			// Optional: Set collision mask to only hit planets/asteroids if you have specific layers
			// query.CollisionMask = 0b00000000_00000000_00000000_00000010; 
			
			var result = spaceState.IntersectRay(query);
			if (result.Count > 0)
			{
				return 0f; // Ray hit an occluder (planet/moon), we are in shadow!
			}
		}

		// 5. Calculate final power generation scaled by how directly it faces the sun
		return MaxGenerationRate * alignment;
	}
}
