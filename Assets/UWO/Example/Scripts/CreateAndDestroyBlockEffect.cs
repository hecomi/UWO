using UnityEngine;
using System.Collections;
using UWO;

public class CreateAndDestroyBlockEffect : MonoBehaviour 
{
	public GameObject createEffect;
	public GameObject destroyEffect;
	private bool isAppRunning_ = true;

	void Start() 
	{
		Instantiate(createEffect, transform.position, transform.rotation);
	}

	void OnDestroy()
	{
		if (isAppRunning_) {
			Instantiate(destroyEffect, transform.position, transform.rotation);
		}
	}

	void OnApplicationQuit()
	{
		isAppRunning_ = false;
	}
}
