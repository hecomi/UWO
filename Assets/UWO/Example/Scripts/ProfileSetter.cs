using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UWO;

public class ProfileSetter : SynchronizedComponent
{
	public Text idUi;
	public TwitterIcon twitterIcon;
	public Text messageUi;
	public Text scoreUi;

	#region FROM_WEB
	void SetId(string id)
	{
		idUi.text = id;
	}

	void SetIconUrl(string iconUrl)
	{
		twitterIcon.iconUrl = iconUrl;
	}

	void SetMessage(string message)
	{
		messageUi.text = message;
	}
	#endregion

	protected override void OnSend()
	{
		var values = new MultiValue();
		Debug.Log (values.AsString());
		values.Push(idUi.text);
		values.Push(twitterIcon.iconUrl);
		values.Push(messageUi.text);
		values.Push(scoreUi.text);
		Send(values);
	}

	protected override void OnReceive(MultiValue values)
	{
		SetId(values.PopValue());
		SetIconUrl(values.PopValue());
		SetMessage(values.PopValue());
		scoreUi.text = values.PopValue();
	}

	[ContextMenu("Test")]
	void Test()
	{
		SetId("hogehoge");
		SetIconUrl("https://pbs.twimg.com/profile_images/550200862023634946/6oxByv6N.png");
		SetMessage("kon nichi wa");
	}
}
