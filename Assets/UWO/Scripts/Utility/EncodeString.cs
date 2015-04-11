using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace UWO
{

public static class EncodeString
{
	public static readonly string CommandDelimiterEscapedString = "<%COM%>";
	public static readonly string MessageDelimiterEscapedString = "<%MSG%>";

	public static string ToEncodedString(this string value)
	{
		var encodedStr = value.Replace(CommandDelimiterEscapedString, ""); 
		encodedStr = value.Replace(MessageDelimiterEscapedString, ""); 
		encodedStr = value.Replace(
			System.Convert.ToString(Synchronizer.MessageDelimiterChar), 
			MessageDelimiterEscapedString); 
		encodedStr.Replace(
			System.Convert.ToString(Synchronizer.CommandDelimiterChar), 
			CommandDelimiterEscapedString); 
		return encodedStr;
	}

	public static string ToDecodedString(this string value)
	{
		var decodedStr = value.Replace(
			MessageDelimiterEscapedString,
			System.Convert.ToString(Synchronizer.MessageDelimiterChar)); 
		decodedStr.Replace(
			CommandDelimiterEscapedString,
			System.Convert.ToString(Synchronizer.CommandDelimiterChar)); 
		return decodedStr;
	}
}

}