using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace UWO
{

public class MultiValue
{
	public struct Value
	{
		public static readonly string DelimiterString = "<%VAL%>";
		public static readonly string[] Delimiter = new string[] { DelimiterString };
		public string type;
		public string value;
	}

	public static readonly string DelimiterString = "<%MVAL%>";
	public static readonly string[] Delimiter = new string[] { DelimiterString };
	public List<Value> values = new List<Value>();

	public void Push(int value)
	{
		values.Add(new Value() {
			type  = "int",
			value = value.AsString()
		});
	}

	public void Push(uint value)
	{
		values.Add(new Value() {
			type  = "uint",
			value = value.AsString()
		});
	}

	public void Push(long value)
	{
		values.Add(new Value() {
			type  = "long",
			value = value.AsString()
		});
	}

	public void Push(ulong value)
	{
		values.Add(new Value() {
			type  = "ulong",
			value = value.AsString()
		});
	}

	public void Push(float value)
	{
		values.Add(new Value() {
			type  = "float",
			value = value.AsString()
		});
	}

	public void Push(bool value)
	{
		values.Add(new Value() {
			type  = "bool",
			value = value.AsString()
		});
	}

	public void Push(string value)
	{
		values.Add(new Value() {
			type  = "string",
			value = Encode(value)
		});
	}

	public void Push(Vector2 value)
	{
		values.Add(new Value() {
			type  = "vector2",
			value = value.AsString()
		});
	}

	public void Push(Vector3 value)
	{
		values.Add(new Value() {
			type  = "vector3",
			value = value.AsString()
		});
	}

	public void Push(Quaternion value)
	{
		values.Add(new Value() {
			type  = "quaternion",
			value = value.AsString()
		});
	}

	public Value Pop()
	{
		if (values.Count == 0) return new Value() { type = "invalid", value = "" };
		var value = values[0];
		values.RemoveAt(0);
		return value;
	}

	private string Encode(string value)
	{
		return value.Replace(Value.DelimiterString, "").Replace(DelimiterString, "");
	}
}

}
