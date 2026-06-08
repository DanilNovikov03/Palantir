export function convertGeoJsonMultiPolygonToLeafletCoordinates(geom) {
    if (!geom || geom.type !== "MultiPolygon") {
        return [];
    }

    // Берём первый polygon и первое кольцо polygon.
    const firstPolygon = geom.coordinates?.[0];
    const firstRing = firstPolygon?.[0];

    if (!firstRing) {
        return [];
    }

    return firstRing.map(point => {
        const lng = point[0];
        const lat = point[1];

        return [lat, lng];
    });
}

export function createMultiPolygonGeoJsonFromLeafletCoordinates(leafletCoordinates) {
    const ring = leafletCoordinates.map(point => {
        const lat = point[0];
        const lng = point[1];

        return [lng, lat];
    });

    const firstPoint = ring[0];
    const lastPoint = ring[ring.length - 1];

    const isClosed =
        firstPoint[0] === lastPoint[0] &&
        firstPoint[1] === lastPoint[1];

    if (!isClosed) {
        ring.push([...firstPoint]);
    }

    return {
        type: "MultiPolygon",
        coordinates: [
            [
                ring
            ]
        ]
    };
}