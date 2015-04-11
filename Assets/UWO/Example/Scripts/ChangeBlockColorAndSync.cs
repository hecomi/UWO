#pragma warning disable 108
using UnityEngine;
using System.Collections.Generic;
using UWO;

public class ChangeBlockColorAndSync : SynchronizedComponent
{
	public Color[] colors;
	static private ChangeBlockColorAndSync Instance;
	static private Dictionary<int, Material> Materials = new Dictionary<int, Material>();
	private int index_ = 0;

	private Renderer renderer
	{
		get { return GetComponent<Renderer>(); }
	}

	static private Material GetCachedMaterial(int index) 
	{
		if (index < 0 || index > Instance.colors.Length) {
			Debug.LogWarning("Invalid material index " + index);
			return null;
		}
		if (!Materials.ContainsKey(index)) {
			var mat = new Material(Instance.renderer.material);
			mat.color = Instance.colors[index];
			Materials.Add(index, mat);
		}
		return Materials[index];
	}

	protected override void OnInitialize()
	{
		if (Instance == null) {
			Instance = this;
		}
	}

	public void SetPrevColor()
	{
		ChangeColor(index_ - 1);
	}

	public void SetNextColor()
	{
		ChangeColor(index_ + 1);
	}

	public Color GetCurrentColor()
	{
		return colors[index_];
	}

	public Color GetNextColor()
	{
		return colors[(index_ + 1) % colors.Length];
	}

	void ChangeColor(int index)
	{
		index_ = index % colors.Length;
		//renderer.material.color = colors[index_];
		renderer.material = GetCachedMaterial(index);
		Send(index_);
	}

	protected override void OnReceive(int index)
	{
		ChangeColor(index);
	}
}
