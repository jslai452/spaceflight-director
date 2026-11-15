using Godot;
using System;

[GlobalClass]
public partial class Objective : Resource
{
	[Export] public string ObjectiveName = "Be in a Low Earth Orbit";
	[Export] public string ObjectiveFunction = "check_low_earth_orbit";
}
