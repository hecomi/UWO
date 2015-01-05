using UnityEngine;
using System.Collections;

public class RotateBlock : MonoBehaviour 
{
	public float speed = 1f;

	void Update() 
	{
		transform.Rotate(Vector3.up * speed);
	}
}
