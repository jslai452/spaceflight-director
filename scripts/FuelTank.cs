using Godot;
using System;

public partial class FuelTank : Node3D
{
	[ExportGroup("Tank Mass Settings")]
	[Export] public float DryMass = 500f;
	[Export] public float FuelMass = 2000f;
	public float MaxFuelMass { get; private set; }

	// A C# property that calculates total mass on the fly
	public float TotalMass => DryMass + FuelMass;
	
	public override void _Ready() {
		MaxFuelMass = FuelMass;
	}

	public float ConsumeFuel(float requestedAmount)
	{
		if (FuelMass <= 0f) return 0f;

		// Mathf.Min works exactly the same way in Godot
		float burned = Mathf.Min(FuelMass, requestedAmount);
		FuelMass -= burned;
		GD.Print($"New fuel mass is {FuelMass} kg and burned fuel is {burned}");
		return burned;
	}
}
