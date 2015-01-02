using UnityEngine;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class PrefabResourceAttribute : PropertyAttribute
{
	public PrefabResourceAttribute() {}
}

#if UNITY_EDITOR
public static class PrefabResourceWatcher
{
	public static List<string> PathList = new List<string>();
	
	public static void Update()
	{
		if (Application.isPlaying) return;

		var resourcesAbsolutePath = System.IO.Path.Combine(Application.dataPath, "Resources");
		var prefabAbsolutePaths = System.IO.Directory.GetFiles(
			resourcesAbsolutePath, "*.prefab", System.IO.SearchOption.AllDirectories);

		PathList.Clear();
		foreach (var prefabAbsolutePath in prefabAbsolutePaths) {
			var localPath = prefabAbsolutePath.Substring(resourcesAbsolutePath.Length + 1).Replace(".prefab", "");
			PathList.Add(localPath);
		}
	}

	public static int PathIndexOf(string path)
	{
		return PathList.IndexOf(path);
	}
}

[CustomPropertyDrawer(typeof(PrefabResourceAttribute))]
public class PrefabResourceDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
		PrefabResourceWatcher.Update();
        if (property.propertyType == SerializedPropertyType.String) {
			var index = PrefabResourceWatcher.PathIndexOf(property.stringValue);
			var selectedIndex = EditorGUI.Popup(position, label.text, index, PrefabResourceWatcher.PathList.ToArray());
			if (index != selectedIndex) {
				property.stringValue = PrefabResourceWatcher.PathList[selectedIndex];
			}
        }
    }

}
#endif