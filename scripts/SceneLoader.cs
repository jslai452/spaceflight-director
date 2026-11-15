using Godot;

public partial class SceneLoader : Node
{
	// Holds the path of the scene we want to load next
	public static string NextScenePath { get; private set; }

	// Call this method from anywhere in your code to switch scenes
	public static void LoadScene(SceneTree tree, string targetScenePath)
	{
		NextScenePath = targetScenePath;
		// Transition to your loading screen first
		tree.ChangeSceneToFile("res://scenes/loading_screen.tscn");
	}
}
