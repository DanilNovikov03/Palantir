import {
    createMultiPolygonGeoJsonFromLeafletCoordinates
} from "./map-geometry.js";

export async function fetchOperation(operationId) {
    const url = `/api/operation/${encodeURIComponent(operationId)}`
    const response = await fetch(url);
    return await response.json();
}

export async function fetchControlZonesByWarAndDate(warId, date) {
    const url = `/api/control-zones/by-war-date?warId=${encodeURIComponent(warId)}&date=${encodeURIComponent(date)}`;

    const response = await fetch(url);

    if (!response.ok) {
        const errorText = await response.text();
        throw new Error(`Ошибка загрузки зон контроля: ${response.status}. ${errorText}`);
    }

    return await response.json();
}

export async function saveControlZoneToBackend(coordinates) {
    const request = {
        warId: 1,
        warSideId: 2,
        dateControl: "1943-07-05",
        precisionControl: "Exact",
        geom: createMultiPolygonGeoJsonFromLeafletCoordinates(coordinates)
    };

    const response = await fetch("/api/control-zones", {
        method: "POST",
        headers: {
            "Content-Type": "application/json"
        },
        body: JSON.stringify(request)
    });

    if (!response.ok) {
        const errorText = await response.text();
        throw new Error(`Ошибка сохранения зоны контроля: ${response.status}. ${errorText}`);
    }

    return await response.json().catch(() => null);
}