using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class TwitterIcon : MonoBehaviour 
{
	private const float retryTime = 5f;
	public delegate void ChangeEvent(Texture texture);
	public event ChangeEvent onChange = texture => {};

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
		if (www.error != null) {
			Debug.LogWarning(www.error);
			yield return new WaitForSeconds(retryTime);
			StartCoroutine(LoadImage(iconUrl));
		} else {
			GetComponent<RawImage>().texture = www.texture;
			onChange(www.texture);
		}
	}
}
