using UnityEngine;
using System.Collections;
using UWO;

public class CreateAndDestroyBlockEffect : MonoBehaviour 
{
	[ResourcePathAsPopup("prefab")]
	public string createEffect;
	[ResourcePathAsPopup("prefab")]
	public string destroyEffect;

	void Start() 
	{
		Synchronizer.Instantiate(createEffect, transform.position, transform.rotation);
	}

	void OnDestroy()
	{
		Synchronizer.Instantiate(destroyEffect, transform.position, transform.rotation);
	}
}
