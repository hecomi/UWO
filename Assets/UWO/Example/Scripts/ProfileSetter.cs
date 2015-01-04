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
		var profiles = idUi.text + "," + twitterIcon.iconUrl + "," + messageUi.text + "," + scoreUi.text;
		Send(profiles);
	}

	protected override void OnReceive(string value)
	{
		var args = value.Split(new char[] {','});
		SetId(args[0]);
		SetIconUrl(args[1]);
		SetMessage(args[2]);
		scoreUi.text = args[3];
	}

	[ContextMenu("Test")]
	void Test()
	{
		SetId("hogehoge");
		SetIconUrl("https://pbs.twimg.com/profile_images/550200862023634946/6oxByv6N.png");
		SetMessage("kon nichi wa");
	}
}
