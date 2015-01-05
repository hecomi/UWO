using UnityEngine;
using System.Collections;

public class CameraEffectManager : MonoBehaviour 
{
	void Update()
	{
		if (Input.GetKeyDown(KeyCode.O) && !GlobalState.isInputting) {
			var ao = GetComponent<UnitySampleAssets.ImageEffects.ScreenSpaceAmbientOcclusion>();
			ao.enabled = !ao.enabled;
			var bloom = GetComponent<UnitySampleAssets.ImageEffects.BloomOptimized>();
			bloom.enabled = !ao.enabled;
			var contrast = GetComponent<UnitySampleAssets.ImageEffects.ContrastStretch>();
			contrast.enabled = !contrast.enabled;
			var sunshafts = GetComponent<UnitySampleAssets.ImageEffects.SunShafts>();
			sunshafts.enabled = !sunshafts.enabled;
		}
	}
}
