using Godot;

public partial class MusicManager : AudioStreamPlayer
{
	[Export] public Godot.Collections.Array<AudioStream> SpacePlaylist { get; set; }

	[ExportGroup("Atmosphere Settings")]
	[Export] public Node3D Craft { get; set; }
	[Export] public CelestialBodyGravity PlanetGravity { get; set; }

	private int _currentIndex = -1;
	private RandomNumberGenerator _rng = new RandomNumberGenerator();
	private bool _isInAtmosphere = false;

	public override void _Ready()
	{
		_rng.Randomize();
		Finished += OnTrackFinished;

		// Perform an initial check right when the scene loads
		CheckAtmosphere();

		// If we start in space, begin playing music immediately
		if (!_isInAtmosphere && SpacePlaylist != null && SpacePlaylist.Count > 0)
		{
			PlayRandomTrack();
		}
	}

	public override void _Process(double delta)
	{
		// Continuously check atmosphere state every frame
		CheckAtmosphere();
	}

	private void CheckAtmosphere()
	{
		if (Craft == null || PlanetGravity == null) return;

		// Use your existing function from CelestialBodyGravity
		bool isOutside = PlanetGravity.IsOutsideAtmosphere(Craft.GlobalPosition);
		bool isInAtmosphere = !isOutside;

		// Only trigger state changes if crossing the boundary
		if (isInAtmosphere != _isInAtmosphere)
		{
			_isInAtmosphere = isInAtmosphere;

			if (_isInAtmosphere)
			{
				// Entered atmosphere: Cut the music for silence/wind
				Stop();
				GD.Print("[Audio] Entered Atmosphere - Music Stopped");
			}
			else
			{
				// Exited atmosphere into space: Resume space music
				GD.Print("[Audio] Exited Atmosphere - Resuming Space Music");
				PlayRandomTrack();
			}
		}
	}

	private void OnTrackFinished()
	{
		// Only play the next random track if we are still out in space
		if (!_isInAtmosphere)
		{
			PlayRandomTrack();
		}
	}

	private void PlayRandomTrack()
	{
		if (SpacePlaylist == null || SpacePlaylist.Count == 0) return;

		if (SpacePlaylist.Count == 1)
		{
			Stream = SpacePlaylist[0];
			Play();
			return;
		}

		int nextIndex;
		do
		{
			nextIndex = _rng.RandiRange(0, SpacePlaylist.Count - 1);
		} 
		while (nextIndex == _currentIndex);

		_currentIndex = nextIndex;
		Stream = SpacePlaylist[_currentIndex];
		Play();
	}
}
