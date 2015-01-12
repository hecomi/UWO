using UnityEngine;
using System.Collections;

public class ScreenShot : MonoBehaviour
{
	public KeyCode key = KeyCode.P;

	void Update()
	{
		if (Input.GetKeyDown(key) && !GlobalState.isInputting) {
			StartCoroutine(Capture());
		}
	}

	IEnumerator Capture()
	{
		yield return new WaitForEndOfFrame();

		Texture2D tex = new Texture2D(Screen.width, Screen.height);
		tex.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
		tex.Apply();
		Application.ExternalCall("screenshot", System.Convert.ToBase64String(tex.EncodeToPNG()));
	}
}