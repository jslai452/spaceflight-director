using Godot;
using System;
using System.Threading.Tasks;

public partial class SplashScreen : Control
{
	private string _mainScenePath = "res://scenes/main_menu_scene.tscn";
	// Called when the node enters the scene tree for the first time.
	public override async void _Ready()
	{
		
		await Task.Delay(2500);
		
		Tween fadeOut = CreateTween();
		fadeOut.TweenProperty(this, "modulate:a", 0.0f, 0.5f);
		await ToSignal(fadeOut, Tween.SignalName.Finished);
		
		SceneLoader.LoadScene(GetTree(), _mainScenePath);
	}
}
