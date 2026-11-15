using Godot;
using System;
using Godot.Collections;

public partial class MissionObjectives : Node
{
	[Export] public Array<Objective> Objectives = new Array<Objective>();
}
