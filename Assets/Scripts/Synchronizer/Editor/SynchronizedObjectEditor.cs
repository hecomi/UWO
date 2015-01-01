using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(SynchronizedObject))]
public class SynchronizedObjectEditor : Editor
{
	public override void OnInspectorGUI()
	{
		SetPrefabPath();
		SynchronizerEditorUtility.AddSynchronizerGameObject();

		DrawOriginalInspector();
		SynchronizerEditorUtility.DrawHorizontalLine(10f);
		DrawDefaultInspector();
	}

	void DrawOriginalInspector()
	{
		var syncObj = target as SynchronizedObject;
		SynchronizerEditorUtility.ReadOnlyTextField("Sync ID", syncObj.id);
		if (syncObj.isOverridePrefab) {
			var prefabPath = EditorGUILayout.TextField("Prefab Path", syncObj.prefabPath);
			if (prefabPath != syncObj.prefabPath) {
				syncObj.prefabPath = prefabPath;
			}
		} else {
			SynchronizerEditorUtility.ReadOnlyTextField("Prefab Path", syncObj.prefabPath);
		}
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
}
