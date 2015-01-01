using UnityEngine;
using System.Collections;

public class AutoDestructParticle : MonoBehaviour 
{
	IEnumerator Start()
	{
		var duration = GetComponent<ParticleSystem>().duration;
		var lifeTime = GetComponent<ParticleSystem>().startLifetime;
		yield return new WaitForSeconds(lifeTime + duration);
		DestroyImmediate(gameObject);
	}
}
