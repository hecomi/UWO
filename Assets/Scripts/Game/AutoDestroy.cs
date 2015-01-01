using UnityEngine;
using System.Collections;

public class AutoDestroy : MonoBehaviour 
{
	public float deadTime = 5f;

	IEnumerator Start() 
	{
		yield return new WaitForSeconds(deadTime);
		DestroyImmediate(gameObject);
	}
}
