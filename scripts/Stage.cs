using Godot;
using System;

public partial class Stage : Node3D
{
	// [Export] makes this variable visible in the Godot Inspector (like Unity's public fields)
	[Export] 
	public int StageNumber = 0;

	// PascalCase method name to match Godot C# standards and your FlightController script
	public int ReturnStage()
	{
		return StageNumber;
	}
}
