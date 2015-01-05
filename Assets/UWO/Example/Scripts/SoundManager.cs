using UnityEngine;
using System.Linq;

public class SoundManager : MonoBehaviour 
{
	public static SoundManager Instance;

	[System.Serializable]
	public struct Sound
	{
		public string name;
		public AudioClip audio;
		public float volume;
	}

	public Sound[] sounds;

	void Awake()
	{
		Instance = this;
	}

	public static void Play(string name, Vector3 position)
	{
		var sound = Instance.sounds.FirstOrDefault(s => s.name == name);
		if (!sound.Equals(default(Sound))) {
			AudioSource.PlayClipAtPoint(sound.audio, position, sound.volume);
		} else {
			Debug.LogWarning(name + " is not registered sound");
		}
	}
}
