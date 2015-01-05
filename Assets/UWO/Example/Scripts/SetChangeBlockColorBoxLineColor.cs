using UnityEngine;
using System.Collections;

public class SetChangeBlockColorBoxLineColor : MonoBehaviour 
{
	void ChangeColor(Color color)
	{
		for (var i = 0; i < transform.childCount; ++i) {
			var child = transform.GetChild(i);
			child.GetComponent<Renderer>().material.color = color;
		}
	}
}
