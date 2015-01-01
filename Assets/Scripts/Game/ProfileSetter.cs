using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ProfileSetter : SynchronizedComponent
{
	public Text idUi;
	public RawImage imageUi;
	public Text messageUi;
	private string iconUrl_ = "";

	void SetId(string id)
	{
		idUi.text = id;
	}

	void SetIconUrl(string iconUrl)
	{
		if (iconUrl_ != iconUrl && !string.IsNullOrEmpty(iconUrl)) {
			iconUrl_ = iconUrl;
			StartCoroutine(LoadImage(iconUrl));
		}
	}

	void SetMessage(string message)
	{
		messageUi.text = message;
	}

	IEnumerator LoadImage(string iconUrl)
	{
		var www = new WWW(iconUrl);
		yield return www;
		imageUi.texture = www.texture;
	}

	protected override void OnSend()
	{
		var profiles = idUi.text + "," + iconUrl_ + "," + messageUi.text;
		Send(profiles);
	}

	protected override void OnReceive(string value)
	{
		var args = value.Split(new char[] {','});
		SetId(args[0]);
		SetIconUrl(args[1]);
		SetMessage(args[2]);
	}

	[ContextMenu("Test")]
	void Test()
	{
		SetId("hogehoge");
		SetIconUrl("https://pbs.twimg.com/profile_images/550200862023634946/6oxByv6N.png");
		SetMessage("kon nichi wa");
	}
}
