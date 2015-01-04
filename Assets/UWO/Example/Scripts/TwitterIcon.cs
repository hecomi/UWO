using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class TwitterIcon : MonoBehaviour 
{
	private string iconUrl_;
	public string iconUrl
	{
		get { return iconUrl_; }
		set 
		{
			if (iconUrl_ != value && !string.IsNullOrEmpty(value)) {
				iconUrl_ = value;
				StartCoroutine(LoadImage(iconUrl));
			}
		}
	}

	public void SetIconUrlFromId(string id)
	{
		iconUrl = "http://hecom.in:12003/" + id;
	}

	IEnumerator LoadImage(string iconUrl)
	{
		var www = new WWW(iconUrl);
		yield return www;
		GetComponent<RawImage>().texture = www.texture;
	}
}
