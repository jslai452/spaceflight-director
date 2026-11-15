using Godot;
using System;
using System.Collections.Generic;

public partial class RocketFuelSystem : Node
{
	[Export] public Node currentRocket;
	
	
	private RigidBody3D _rb;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		CacheRigidbody();
		UpdateTotalMassAndCenterOfMass();
	}
	
	public void UpdateTotalMassAndCenterOfMass() {
		if (currentRocket == null) {
			return;
		}
		if (_rb == null) CacheRigidbody();
		float totalMass = 0f;
		Vector3 weightedPositionSum = Vector3.Zero;
		List<RocketPart> structuralParts = GetComponentsInChildren<RocketPart>(currentRocket);
		foreach (RocketPart part in structuralParts) {
			float m = part.PartMass;
			totalMass += m;
			Vector3 localPos = _rb.ToLocal(part.GlobalPosition);
			weightedPositionSum += localPos * m;
		}
		List<FuelTank> tanks = GetComponentsInChildren<FuelTank>(currentRocket);
				foreach (FuelTank tank in tanks)
		{
			float m = tank.TotalMass;
			totalMass += m;

			Vector3 localPos = _rb.ToLocal(tank.GlobalPosition);
			weightedPositionSum += localPos * m;
		}

		// 3. Apply final mass and dynamic center of mass to the RigidBody3D
		_rb.Mass = Mathf.Max(1f, totalMass); // Ensure mass never drops to 0 or below
		if (totalMass > 0f)
		{
			_rb.CenterOfMass = weightedPositionSum / totalMass;
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
		private void CacheRigidbody()
	{
		if (currentRocket is RigidBody3D rb)
		{
			_rb = rb;
		}
		else if (currentRocket != null)
		{
			// Fallback: Check if the currentRocket contains a RigidBody3D node
			_rb = currentRocket.GetNodeOrNull<RigidBody3D>("RigidBody3D");
		}
	}
		private List<T> GetComponentsInChildren<T>(Node root) where T : Node
	{
		List<T> results = new List<T>();
		if (root is T match) results.Add(match);

		foreach (Node child in root.GetChildren(includeInternal: true))
		{
			results.AddRange(GetComponentsInChildren<T>(child));
		}
		return results;
	}
}
