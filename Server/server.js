var ws           = require('ws').Server;
var server       = new ws({ port: 3000 });
var synchronizer = require('./synchronizer');
var fps          = 60;

var getInfo = function(index, clientNum) {
	var command   = 'i';
	var isMaster  = index === 0 ? 'true' : 'false';
	var timestamp = +new Date();
	return [command, isMaster, timestamp, clientNum].join('\t');
};

server.broadcast = function(data) {
	var self = this;
	this.clients.forEach(function(client, index) {
		var info = getInfo(index, self.clients.length);
		var message = info + '\n' + data;
		client.send(message, function(err) {
			if (err) console.error(err);
		});
	});
};

server.sendSavedComponentsTo = function(socket, isImmediatelyOwned) {
	var messages = synchronizer.getSavedComponentsMessages(isImmediatelyOwned);
	console.log(messages);
	socket.send(messages);
};

synchronizer.loadGameObjects(function() {
	var clientNum = 0;
	server.on('connection', function(socket) {
		console.log('connected');

		var isImmediatelyOwned = clientNum === 0;
		server.sendSavedComponentsTo(socket, isImmediatelyOwned);
		clientNum = server.clients.length;

		socket.on('message', function(message) {
			console.log(message.toString());
			synchronizer.add(message);
		});

		socket.on('close', function() {
			console.log('disconnected...');
			clientNum = server.clients.length;
		});

		setInterval(function() {
			if (synchronizer.hasMessages()) {
				server.broadcast(synchronizer.getMessages());
			}
		}, 1000 / fps);
	});
});

setInterval(function() {
	synchronizer.saveGameObjects();
}, 1000 * 60);

process.on('uncaughtException', function(e) {
	console.error("Unexpected Exception:", e.stack);
});
