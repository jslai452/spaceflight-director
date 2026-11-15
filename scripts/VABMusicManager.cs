using Godot;

public partial class VABMusicManager : AudioStreamPlayer
{
	[Export] 
	public Godot.Collections.Array<AudioStream> VABSongs;

	private int _currentIndex = -1;
	private RandomNumberGenerator _rng = new RandomNumberGenerator();

	public override void _Ready()
	{
		_rng.Randomize();
		Finished += OnTrackFinished;

		if (VABSongs != null && VABSongs.Count > 0)
		{
			PlayRandomTrack();
		}
	}

	private void OnTrackFinished()
	{
		PlayRandomTrack();
	}

	private void PlayRandomTrack()
	{
		if (VABSongs == null || VABSongs.Count == 0) return;

		// If there's only one song, just play it loop-style
		if (VABSongs.Count == 1)
		{
			Stream = VABSongs[0];
			Play();
			return;
		}

		// Pick a new index that is different from the last played track
		int nextIndex;
		do
		{
			nextIndex = _rng.RandiRange(0, VABSongs.Count - 1);
		} while (nextIndex == _currentIndex);

		_currentIndex = nextIndex;
		Stream = VABSongs[_currentIndex];
		Play();
	}
}
