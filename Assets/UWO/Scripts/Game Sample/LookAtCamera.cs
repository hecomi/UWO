using UnityEngine;
using System.Collections;

public class LookAtCamera : MonoBehaviour 
{
	public float t = 0.1f;

	void LateUpdate()
	{
		var from = transform.rotation;
		var to   = Quaternion.LookRotation(transform.position - Camera.main.transform.position, Vector3.up);
		transform.rotation = Quaternion.Slerp(from, to, t);
	}
}
