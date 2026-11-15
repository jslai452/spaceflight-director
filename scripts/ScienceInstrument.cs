using Godot;
using System;

public partial class ScienceInstrument : Node
{
	[Export] public Godot.Collections.Array<string> ExportData { get; set; } = new();

	public Godot.Collections.Array<string> Activate()
	{
		if (ExportData.Count > 0)
		{
			return ExportData;
		}
		
		return new Godot.Collections.Array<string>(); // Fixes missing return path
	}
}
