using UnityEngine;
using System.Collections;

namespace UWO
{

public static class PrimitiveParser
{
	public static readonly char   DelimiterChar = ',';
	public static readonly char[] Delimiter     = {DelimiterChar};

	public static int AsInt(this string value)
	{
		return int.Parse(value);
	}

	public static uint AsUint(this string value)
	{
		return uint.Parse(value);
	}

	public static long AsLong(this string value)
	{
		return long.Parse(value);
	}

	public static ulong AsUlong(this string value)
	{
		return ulong.Parse(value);
	}

	public static float AsFloat(this string value)
	{
		return float.Parse(value);
	}

	public static bool AsBool(this string value)
	{
		return bool.Parse(value);
	}

	public static Vector2 AsVector2(this string value)
	{
		var x = value.Split(Delimiter);
		return new Vector2(x[0].AsFloat(), x[1].AsFloat());
	}

	public static Vector3 AsVector3(this string value)
	{
		var x = value.Split(Delimiter);
		return new Vector3(x[0].AsFloat(), x[1].AsFloat(), x[2].AsFloat());
	}

	public static Quaternion AsQuaternion(this string value)
	{
		var x = value.Split(Delimiter);
		return new Quaternion(x[0].AsFloat(), x[1].AsFloat(), x[2].AsFloat(), x[3].AsFloat());
	}

	public static string AsString(this int value)
	{
		return value.ToString();
	}

	public static string AsString(this uint value)
	{
		return value.ToString();
	}

	public static string AsString(this long value)
	{
		return value.ToString();
	}

	public static string AsString(this ulong value)
	{
		return value.ToString();
	}

	public static string AsString(this float value)
	{
		return value.ToString();
	}

	public static string AsString(this bool value)
	{
		return value.ToString();
	}

	public static string AsString(this Vector2 value)
	{
		return value.x.ToString() + DelimiterChar + value.y.ToString();
	}

	public static string AsString(this Vector3 value)
	{
		return value.x.ToString() + DelimiterChar + 
		       value.y.ToString() + DelimiterChar + 
		       value.z.ToString();
	}

	public static string AsString(this Quaternion value)
	{
		return value.x.ToString() + DelimiterChar +
		       value.y.ToString() + DelimiterChar + 
		       value.z.ToString() + DelimiterChar + 
		       value.w.ToString();
	}
}

}