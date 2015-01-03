using UnityEngine;
using System.Collections;

public class LookAtTarget : MonoBehaviour 
{
	public Transform target;
	public bool isSmooth = false;
	public float t = 0.1f;

	void LateUpdate() 
	{
		var from = transform.rotation;
		var to   = Quaternion.LookRotation(target.position - transform.position, Vector3.up);
		if (isSmooth) {
			transform.rotation = Quaternion.Slerp(from, to, t);
		} else {
			transform.rotation = to;
		}
	}
}
