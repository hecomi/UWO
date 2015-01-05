#pragma warning disable 0108

using UnityEngine;
using System.Collections;

public class ToggleLight : MonoBehaviour 
{
	public KeyCode key = KeyCode.L;
	public Light light
	{
		get { return GetComponent<Light>(); }
	}

	void Update() 
	{
		if (Input.GetKeyDown(key) && !GlobalState.isInputting) {
			light.enabled = !light.enabled;
		}
	}
}
