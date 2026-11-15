using Godot;
using System;

using Godot;
using System;
using System.Collections.Generic;

public partial class LiquidFuelFinder : Node
{
	[ExportCategory("References")]
	[Export] public RigidBody3D targetRocketRb;
	[Export] public Label LiquidFuelLabel;

	// Public properties so other scripts (like engines or UI) can easily read total fuel stats
	public float TotalFuelMass { get; private set; } = 0f;
	public float TotalCombinedMass { get; private set; } = 0f;
	public float TotalMaxFuelMass { get; private set; } = 0f;

	public override void _Ready()
	{
		// If targetRocketRb isn't assigned in the inspector, automatically look up the parent tree
		if (targetRocketRb == null)
		{
			Node parent = GetParent();
			while (parent != null)
			{
				if (parent is RigidBody3D rb)
				{
					targetRocketRb = rb;
					break;
				}
				parent = parent.GetParent();
			}
		}
	}

	public override void _Process(double delta)
	{
		if (targetRocketRb == null) return;

		// Reset totals every frame before recalculating
		TotalFuelMass = 0f;
		TotalCombinedMass = 0f;
		TotalMaxFuelMass = 0f;

		// Recursively find all FuelTanks attached anywhere under the rocket RigidBody3D
		List<FuelTank> allTanks = GetComponentsInChildren<FuelTank>(targetRocketRb);

		foreach (FuelTank tank in allTanks)
		{
			TotalFuelMass += tank.FuelMass;       // Sum up current fuel[cite: 3]
			TotalCombinedMass += tank.TotalMass;  // Sum up dry mass + fuel mass[cite: 3]
			TotalMaxFuelMass += tank.MaxFuelMass;
		}
		if (LiquidFuelLabel != null) {
			LiquidFuelLabel.Text = $"Liquid Fuel: {TotalFuelMass} kg / {TotalMaxFuelMass} kg";
		}

		// Optional debug print to verify live tracking
		// GD.Print($"[LiquidFuelFinder] Active Tanks: {allTanks.Count} | Total Fuel: {TotalFuelMass:F1} kg");
	}

	// Helper method to recursively search all child nodes deep within the hierarchy
	private List<T> GetComponentsInChildren<T>(Node root) where T : Node
	{
		List<T> results = new List<T>();
		if (root is T match)
		{
			results.Add(match);
		}
		
		// ✅ Fixed: Removed the redundant ternary and invalid Array instantiation
		foreach (Node child in root.GetChildren(true))
		{
			results.AddRange(GetComponentsInChildren<T>(child));
		}
		return results;
	}
}
