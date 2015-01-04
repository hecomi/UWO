using UnityEngine;
using System.Collections;

public class CameraEffectManager : MonoBehaviour 
{
	void Update()
	{
		if (Input.GetKeyDown(KeyCode.O)) {
			var ao = GetComponent<UnitySampleAssets.ImageEffects.ScreenSpaceAmbientOcclusion>();
			ao.enabled = !ao.enabled;
		}
	}
}
