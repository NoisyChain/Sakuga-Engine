using Godot;
using SakugaEngine.Resources;

namespace SakugaEngine
{
	public partial class AudioManager : Node
	{
		[Export] private AudioStreamPlayer Announcer;
		[Export] private AudioStreamPlayer Menu;
		[Export] private AudioStreamPlayer BGM;
		[Export] private SoundsList AnnouncerClips;
		[Export] private SoundsList MenuClips;

		public static AudioManager Instance { get; private set; }
		public override void _Ready()
		{
			Instance = this;
		}

		public void PlayAnnouncerClip(int soundIndex)
        {
			if (AnnouncerClips == null) return;
			if (soundIndex < 0) return;
            Announcer.Stream = AnnouncerClips.Sounds[soundIndex];
            Announcer.Play();
        }

		public void PlayMenuClip(int soundIndex)
        {
			if (MenuClips == null) return;
			if (soundIndex < 0) return;
            Menu.Stream = MenuClips.Sounds[soundIndex];
            Menu.Play();
        }

		public void PlayBGM(AudioStream song)
		{
			if (song == null) return;
			if (BGM.Playing) StopBGM();

			BGM.Stream = song;
            BGM.Play();
		}

		public void StopBGM()
		{
            BGM.Stop();
		}
	}
}
