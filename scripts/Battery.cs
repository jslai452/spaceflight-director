using Godot;
using System;

public partial class Battery : Node
{
	[Export] public float Electricity;
	[Export] public float MaxElectricity;
	[Export] public float MinElectricity;
	
	public void DecrementElectricity(float newElectricity) {
		Electricity -= newElectricity;
		Electricity = Mathf.Clamp(Electricity, MinElectricity, MaxElectricity);
	}
	
	public float GetElectricity() {
		return Electricity;
	}
}
