using UnityEngine;
using System.Collections;
using UWO;

public class ChangeBlockColorAndSync : SynchronizedComponent
{
	public Color[] colors;
	private int index_ = 0;

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
		GetComponent<Renderer>().material.color = colors[index_];
	}

	protected override void OnSend()
	{
		Send(index_);
	}

	protected override void OnReceive(int index)
	{
		ChangeColor(index);
	}
}
