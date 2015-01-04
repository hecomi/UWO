using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerGUI : MonoBehaviour 
{
	private static PlayerGUI Instance_;
	public static PlayerGUI Instance
	{
		get { return Instance_; }
		private set { Instance_ = value; }
	}

	public Text idUi;
	public TwitterIcon twitterIcon;
	public Text messageUi;
	public Text scoreUi;

	public static string id
	{
		get { return Instance.idUi.text;  }
		set { Instance.idUi.text = value; }
	}
	public static string iconUrl
	{
		get { return Instance.twitterIcon.iconUrl;  }
		set { Instance.twitterIcon.iconUrl = value; }
	}
	public static string message
	{
		get { return Instance.messageUi.text;  }
		set { Instance.messageUi.text = value; }
	}
	public static string score
	{
		get { return Instance.scoreUi.text;  }
		set { Instance.scoreUi.text = value; }
	}

	void Awake()
	{
		Instance = this;
		LoadAll();
		Score.Load();
	}

	IEnumerator Start()
	{
		for (;;) {
			yield return new WaitForSeconds(5f);
			SaveAll();
		}
	}

	void Update()
	{
		score = Score.point.ToString();
	}

	void SaveAll()
	{
		SaveString("id", id);
		SaveString("iconUrl", iconUrl);
		SaveString("message", message);
		Score.Save();
	}

	void LoadAll()
	{
		id      = LoadString("id", id);
		iconUrl = LoadString("iconUrl", iconUrl);
		message = LoadString("message", message);
		Score.Load();
		score   = Score.point.ToString();
	}

	void SaveString(string key, string value)
	{
		PlayerPrefs.SetString(key, value);
	}

	string LoadString(string key, string defaultValue)
	{
		if (PlayerPrefs.HasKey(key)) {
			return PlayerPrefs.GetString(key);
		}
		return defaultValue;
	}
}
