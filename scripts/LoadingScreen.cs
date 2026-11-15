using Godot;

public partial class LoadingScreen : Control
{
	[Export] public string TargetScenePath = "res://scenes/nancy_roman_space_telescope_mission.tscn";

	[Export] public ProgressBar ProgressBar;
	[Export] public Label StatusLabel;
	[Export] public Label TipLabel;

	private string[] _spaceTips = new string[]
	{
		"Tip: Staging too early in thick atmosphere wastes delta-v.",
		"Tip: Specific impulse (Isp) measures fuel efficiency, not total power.",
		"Tip: Turn off main engines during long orbital coast phases to conserve propellant.",
        "Tip: Vacuum engines lose thrust efficiency inside dense lower atmospheres."
	};

	public override void _Ready()
{
	// Retrieve the target path set by SceneLoader
	string targetPath = SceneLoader.NextScenePath;

	if (string.IsNullOrEmpty(targetPath))
	{
		GD.PrintErr("No target scene path provided!");
		return;
	}
	// Display a random tip on load
		var random = new RandomNumberGenerator();
		random.Randomize();
		TipLabel.Text = _spaceTips[random.RandiRange(0, _spaceTips.Length - 1)];

	// Request threaded loading for the target scene
	ResourceLoader.LoadThreadedRequest(targetPath);
}

public override void _Process(double delta)
{
	string targetPath = SceneLoader.NextScenePath;
	
	var progress = new Godot.Collections.Array();
	ResourceLoader.ThreadLoadStatus status = ResourceLoader.LoadThreadedGetStatus(targetPath, progress);

	if (status == ResourceLoader.ThreadLoadStatus.InProgress)
	{
		float loadValue = (float)progress[0];
		ProgressBar.Value = loadValue * 100f;
		StatusLabel.Text = "Loading...";
	}
	else if (status == ResourceLoader.ThreadLoadStatus.Loaded)
	{
		PackedScene loadedScene = (PackedScene)ResourceLoader.LoadThreadedGet(targetPath);
		StatusLabel.Text = "Ready!";
		GetTree().ChangeSceneToPacked(loadedScene);
	}
}
}
