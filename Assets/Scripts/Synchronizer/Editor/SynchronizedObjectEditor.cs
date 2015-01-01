using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(SynchronizedObject))]
public class SynchronizedObjectEditor : Editor
{
	public override void OnInspectorGUI()
	{
		SetPrefabPath();
		AddSynchronizerGameObject();
		DrawDefaultInspector();
	}

	void SetPrefabPath()
	{
		var obj = target as SynchronizedObject;
		if (!Application.isPlaying && !obj.isOverridePrefab) {
			var parent = PrefabUtility.GetPrefabParent(obj.gameObject) as GameObject;
			if (parent == null) {
				parent = obj.gameObject;
			}
			var prefabPath = AssetDatabase.GetAssetPath(parent);
			if (prefabPath.IndexOf("Assets/Resources/") != 0) {
				EditorGUILayout.HelpBox("Please move this prefab under \"Assets/Resources\" directory", MessageType.Error);
	            obj.prefabPath = prefabPath;
			} else {
				obj.prefabPath = prefabPath.Substring("Assets/Resources/".Length).Replace(".prefab", "");
			}
		}
    }

	void AddSynchronizerGameObject()
	{
		if (GameObject.FindObjectOfType<Synchronizer>() == null) {
			var obj = new GameObject("Synchronizer");
			obj.AddComponent<Synchronizer>();
		}
	}
}
