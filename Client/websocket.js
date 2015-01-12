(function() {
	window.ws = new WebSocket('ws://127.0.0.1:3000');

	var isInitialized = false;
	var cache = "";

	window.init = function() {
		isInitialized = true;
		SendMessage('Synchronizer', 'PushWebSocketData', cache);
	};

	ws.onmessage = function(event) {
		if (isInitialized) {
			SendMessage('Synchronizer', 'PushWebSocketData', event.data);
		} else {
			cache += event.data + '\n';
		}
	};
})();
