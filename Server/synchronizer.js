var GameObject = require('./gameObject');
var fs = require('fs');
var savedDataPath = './saved-data.txt';

var Synchronizer = {
	_messages: [],
	_saveComponentGameObjects: {},
	loadGameObjects: function(callback) {
		var self = this;
		fs.readFile(savedDataPath, function(err, body) {
			body.toString().split('\n').forEach(function(line) {
				self._saveComponent(line);
			});
			if (typeof callback === 'function') callback();
		});
	},
	saveGameObjects: function() {
		var messages = this.getSavedComponentsMessages();
		fs.writeFile(savedDataPath, messages, function(err) {
			if (err) console.error(err);
			console.log('saved!');
		});
	},
	hasMessages: function() {
		return this._messages.length > 0;
	},
	add: function(message) {
		var message = message.toString();
		this._messages.push(message);
		this._parse(message);
	},
	clear: function() {
		this._messages = [];
	},
	getMessages: function() {
		var messages = this._messages.join('\n');
		this.clear();
		return messages;
	},
	getSavedComponentsMessages: function(isImmediatelyOwned) {
		var messages = '';
		for (var id in this._saveComponentGameObjects) {
			var gameObject = this._saveComponentGameObjects[id];
			var components = gameObject.components;
			for (var id in components) {
				var updateComponentMessage = components[id];
				if (isImmediatelyOwned) {
					updateComponentMessage = 'o' + updateComponentMessage.slice(1);
				}
				messages += updateComponentMessage + '\n';
			}
		}
		return messages;
	},
	_parse: function(message) {
		var self = this;
		message.split('\n').forEach(function(line) {
			var command = line[0];
			switch (command) {
				case 's': self._saveComponent(line);   break;
				case 'd': self._deleteComponent(line); break;
			}
		});
	},
	_saveComponent: function(line) {
		var gameObject = new GameObject(line);
		var id = gameObject.id;
		if (id in this._saveComponentGameObjects) {
			this._saveComponentGameObjects[id].merge(gameObject.components);
		} else {
			this._saveComponentGameObjects[id] = gameObject;
		}
	},
	_deleteComponent: function(line) {
		var args = line.split('\t');
		var gameObjectId = args[1];
		delete this._saveComponentGameObjects[gameObjectId];
	}
};

module.exports = Synchronizer;
