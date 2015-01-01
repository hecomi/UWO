using UnityEngine;

public class InstantiateSync : SynchronizedComponent
{
	public GameObject prefab;

	protected override void OnSend()
	{
		Instantiate(prefab, transform.position, transform.rotation);
		Destroy(gameObject);
	}

	protected override void OnReceive(Vector3 value)
	{
		Instantiate(prefab, transform.position, transform.rotation);
	}
}