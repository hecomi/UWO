using UnityEngine;
using System.Collections;

public static class Score
{
	public static int point_ = 0;
	public static int point 
	{
		get { return point_; }
		private set { point_ = value; }
	}

	public static void Add(int val) {
		point += val;
	}

	public static void Sub(int val) {
		point -= val;
	}

	public static void Load()
	{
		if (PlayerPrefs.HasKey("score")) {
			point = PlayerPrefs.GetInt("score");
		}
	}

	public static void Save()
	{
		PlayerPrefs.SetInt("score", point);
	}
}
