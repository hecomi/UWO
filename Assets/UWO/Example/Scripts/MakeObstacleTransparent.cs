using UnityEngine;
using System.Collections.Generic;

public class MakeObstacleTransparent : MonoBehaviour 
{
	public Dictionary<Renderer, Material> originalMaterials_ = new Dictionary<Renderer, Material>();
	public Transform target;
	public LayerMask layerMask;
	public Material transparentMaterial;

	void LateUpdate() 
	{
		var from = transform.position;
		var to = target.position;
		var direction = to - from;
		var distance = 4f;

		Reset();
		foreach (var hit in Physics.RaycastAll(from, direction, distance, layerMask)) {
			Set(hit.transform.gameObject, hit.distance / distance);
		}
	}

	void Reset()
	{
		foreach (var x in originalMaterials_) {
			var renderer = x.Key;
			var material = x.Value;
			renderer.material = material; 
		}
		originalMaterials_.Clear();
	}

	void Set(GameObject obj, float distance)
	{
		var renderer = obj.GetComponent<Renderer>();
		var originalMaterial = renderer.material;
		originalMaterials_.Add(renderer, originalMaterial);
		renderer.material = transparentMaterial;
		var color = originalMaterial.color;
		color.a = 0.75f * distance;
		renderer.material.color = color;
		renderer.material.mainTexture = originalMaterial.mainTexture;
	}
}
