var _ = require('underscore');

var GameObject = function(line) {
	var args = line.split('\t');
	this.id = args[2];
	var componentId = args[1];
	this.components = {};
	this.components[componentId] = line;
};

GameObject.prototype.merge = function(components) {
	this.components = _.extend(this.components, components);
};

module.exports = GameObject;
