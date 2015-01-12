using UnityEngine;
using System.Collections;

public class AutoToggleRendererByDistance : MonoBehaviour 
{
	public float distanceThreshold = 100f;

	IEnumerator Start()
	{
		var player = GameObject.FindGameObjectWithTag("Player").transform;
		var collider = GetComponent<Renderer>();
		var renderer = GetComponent<Collider>();

		for (;;) {
			var distance = Vector3.Distance(transform.position, player.position);
			renderer.enabled = collider.enabled = distance < distanceThreshold;
			yield return new WaitForSeconds(5f);
		}
	}
}
