using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(SynchronizedComponent), true)]
public class SynchronizedComponentEditor : Editor
{
	public override void OnInspectorGUI()
	{
		AddSynchronizerGameObject();
		DrawDefaultInspector();
	}

	void AddSynchronizerGameObject()
	{
		if (GameObject.FindObjectOfType<Synchronizer>() == null) {
			var obj = new GameObject("Synchronizer");
			obj.AddComponent<Synchronizer>();
		}
	}
}
