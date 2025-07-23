function initMap(selector) {
	const map = L.map(selector).fitBounds([[30, -20], [60, 30]]);

	L.tileLayer('https://tile.openstreetmap.org/{z}/{x}/{y}.png', {
		maxZoom: 19,
		attribution: '&copy; <a href="http://www.openstreetmap.org/copyright">OpenStreetMap</a>',
		opacity: 0.5,
	}).addTo(map);

	return map;
}

function buildNodePlate(node) {
	return `<a class="node"><span style="background-color: ${node.backColor}; color: ${node.foreColor}">${node.shortName}</span>${node.longName}</a>`;
}

function buildNodeTooltip(node) {
	return buildNodePlate(node);
}

function buildNodePopup(node) {
	return `<table><caption>${buildNodePlate(node)}</caption><tbody><tr><th>Last seen:</th><td>${node.lastSeen}</td></tr><tr><th>Last position update:</th><td>${node.lastPositionUpdate}</td></tr></tbody></table>`;
}

function addNodes(map, nodes) {
	const nodeIcon = L.divIcon({ className: 'map-node' });
	const nodeGroup = L.layerGroup().addTo(map);
	for (const node of nodes) {
		const marker = L.marker(node.coordinates, { icon: nodeIcon })
			.bindPopup(_ => buildNodePopup(node))
			.bindTooltip(_ => buildNodeTooltip(node));
		nodeGroup.addLayer(marker);
	}
}