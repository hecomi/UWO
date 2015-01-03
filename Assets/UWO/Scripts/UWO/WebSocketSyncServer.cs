// #define WEBSOCKET_BROWSER_DEBUG
using UnityEngine;
using System;
using System.Collections;

namespace UWO
{

public class WebSocketSyncServer : MonoBehaviour 
{
	public string websocketServerUrl = "ws://127.0.0.1:3000";
#if (UNITY_EDITOR || !UNITY_WEBGL) && !WEBSOCKET_BROWSER_DEBUG 
    private WebSocket ws_;
#else
    private WebSocketBrowser ws_;
#endif

	private bool isConnected_ = false;
	public bool isConnected
	{
		get { return isConnected_; }
	}

	public delegate void ReceiveEventListener(string data);
	public ReceiveEventListener OnReceive = data => {};


	IEnumerator Start() 
    {
        for (;;) {
#if (UNITY_EDITOR || !UNITY_WEBGL) && !WEBSOCKET_BROWSER_DEBUG 
            ws_ = new WebSocket( new Uri(websocketServerUrl) );
            yield return StartCoroutine( ws_.Connect() );
#else
			if (!ws_) {
				ws_ = GetComponent<WebSocketBrowser>() ?? gameObject.AddComponent<WebSocketBrowser>();
				Application.ExternalEval("init();");
			}
#endif
            isConnected_ = true;

            for (;;) {
                var message = ws_.RecvString();
                while (message != null) {
					if (Application.isPlaying) {
						OnReceive(message);
					}
                    message = ws_.RecvString();
                }
                if (ws_.Error != null) {
                    LogError(ws_.Error);
                    isConnected_ = false;
                    yield return new WaitForSeconds(1);
                    break;
                }
                yield return new WaitForEndOfFrame();
            }
        }
    }

	void OnDestroy()
	{
		ws_.Close();
	}

	public void Send(byte[] data)
	{
		ws_.Send(data);
	}

	public void Send(string data)
	{
		ws_.SendString(data);
	}

	void Log(string message)
	{
		Debug.Log(message);
		Application.ExternalCall("console.log", message);
	}
	
	void LogError(string error)
	{
		Debug.LogError(error);
		Application.ExternalCall("console.error", error);
	}
}

}
