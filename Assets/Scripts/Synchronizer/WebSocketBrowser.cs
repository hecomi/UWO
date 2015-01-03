using UnityEngine;
using System.Collections.Generic;

namespace UWO
{

public class WebSocketBrowser : MonoBehaviour 
{
	private Queue<string> queue_ = new Queue<string>();
	public string Error = null;

	public void Close()
	{
	}

	public string RecvString()
	{
		return queue_.Count > 0 ? queue_.Dequeue() : null;
	}

	public void Send(byte[] buffer)
	{
	}

	public void SendString(string data)
	{
		Application.ExternalCall("ws.send", data);
	}

	void PushWebSocketData(string data)
	{
		queue_.Enqueue(data);
	}
}

}