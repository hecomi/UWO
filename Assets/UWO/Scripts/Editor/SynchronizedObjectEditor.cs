using UnityEngine;
using UnityEditor;
using System.Collections;

namespace UWO
{

[CustomEditor(typeof(SynchronizedObject))]
public class SynchronizedObjectEditor : Editor
{
	public override void OnInspectorGUI()
	{
		SetPrefabPath();
		SynchronizerEditorUtility.AddSynchronizerGameObject();

		DrawExtraInspector();
		SynchronizerEditorUtility.DrawHorizontalLine(10f);
		DrawDefaultInspector();
	}

	void DrawExtraInspector()
	{
		var syncObj = target as SynchronizedObject;
		SynchronizerEditorUtility.ReadOnlyTextField("Sync ID", syncObj.id);
		if (syncObj.isOverridePrefab) {
			ResourcePathWatcher.Update("prefab");
			var index = ResourcePathWatcher.PathIndexOf("prefab", syncObj.prefabPath);

			var selectedIndex = EditorGUILayout.Popup("Prefab Path", index, ResourcePathWatcher.GetPathList("prefab"));
			if (index != selectedIndex) {
				syncObj.prefabPath = ResourcePathWatcher.GetPath("prefab", selectedIndex);
			}
		} else {
			SynchronizerEditorUtility.ReadOnlyTextField("Prefab Path", syncObj.prefabPath);
		}
		EditorGUILayout.Toggle("Is Local", syncObj.isLocal);
	}

	void SetPrefabPath()
	{
		var obj = target as SynchronizedObject;
		if (!obj.isOverridePrefab) {
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

}
