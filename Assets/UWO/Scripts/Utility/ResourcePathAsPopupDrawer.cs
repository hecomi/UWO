using UnityEngine;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class ResourcePathAsPopupAttribute : PropertyAttribute
{
	public string resourceType;

	public ResourcePathAsPopupAttribute(string type) 
	{
		resourceType = type;
	}
}

namespace UWO
{

#if UNITY_EDITOR
public static class ResourcePathWatcher
{
	public static Dictionary<string, List<string>> ResourceLists = new Dictionary<string, List<string>>();
	
	public static void Update(string resourceType)
	{
		var resourcesAbsolutePath = System.IO.Path.Combine(Application.dataPath, "Resources");
		var prefabAbsolutePaths = System.IO.Directory.GetFiles(
			resourcesAbsolutePath, "*." + resourceType, System.IO.SearchOption.AllDirectories);

		ResourceLists[resourceType] = new List<string>();
		foreach (var absolutePath in prefabAbsolutePaths) {
			var localPath = absolutePath.Substring(resourcesAbsolutePath.Length + 1).Replace("." + resourceType, "");
			ResourceLists[resourceType].Add(localPath);
		}
	}

	public static int PathIndexOf(string resourceType, string path)
	{
		return ResourceLists[resourceType].IndexOf(path);
	}

	public static string[] GetPathList(string resourceType)
	{
		return ResourceLists[resourceType].ToArray();
	}

	public static string GetPath(string resourceType, int index)
	{
		return GetPathList(resourceType)[index];
	}
}

[CustomPropertyDrawer(typeof(ResourcePathAsPopupAttribute))]
public class ResourcePathAsPopupDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
		var resourcePathAsPopupAttribute = (ResourcePathAsPopupAttribute)attribute;
		var resourceType = resourcePathAsPopupAttribute.resourceType;

		ResourcePathWatcher.Update(resourceType);

        if (property.propertyType == SerializedPropertyType.String) {
			var index = ResourcePathWatcher.PathIndexOf(resourceType, property.stringValue);
			var selectedIndex = EditorGUI.Popup(
				position, label.text, index, ResourcePathWatcher.GetPathList(resourceType));
			if (index != selectedIndex) {
				property.stringValue = ResourcePathWatcher.GetPath(resourceType, selectedIndex);
			}
        }
    }

}
#endif

}