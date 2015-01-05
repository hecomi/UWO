using UnityEngine;
using System.Collections;

public class AddScoreBonus : MonoBehaviour 
{
	public GameObject effect;
	public float revivalDuration = 5f;
	public bool isAlive = true;

	IEnumerator Revival()
	{
		yield return new WaitForSeconds(revivalDuration);
		isAlive = true;
		effect.SetActive(true);
	}

	void OnTriggerEnter(Collider other)
	{
		if (other.tag == "Player" && isAlive) {
			var score = Random.Range(1000, 10000);
			Score.Add(score);
			Notification.Show("You got " + score + " points!");
			isAlive = false;
			effect.SetActive(false);
			SoundManager.Play("HotSpot", transform.position);
			StartCoroutine(Revival());
		}
	}
}
