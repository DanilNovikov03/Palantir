import {
    createMultiPolygonGeoJsonFromLeafletCoordinates
} from "./map-geometry.js";

export async function fetchOperation(operationId) {
    const url = `/api/operation/${encodeURIComponent(operationId)}`;

    const response = await fetch(url);

    if (!response.ok) {
        const errorText = await response.text();
        throw new Error(`Ошибка загрузки операции: ${response.status}. ${errorText}`);
    }

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

export async function fetchArmiesByWarAndDate(warId, date) {
    const url = `/api/army/by-war/${encodeURIComponent(warId)}/positions?date=${encodeURIComponent(date)}`;

    const response = await fetch(url);

    if (!response.ok) {
        const errorText = await response.text();
        throw new Error(`Ошибка загрузки армий: ${response.status}. ${errorText}`);
    }

    return await response.json();
}

export async function saveControlZoneToBackend(request) {
    const body = {
        warId: request.warId,
        warSideId: request.warSideId,
        dateControl: request.dateControl,
        precisionControl: request.precisionControl,
        geom: createMultiPolygonGeoJsonFromLeafletCoordinates(request.coordinates)
    };

    const response = await fetch("/api/control-zones", {
        method: "POST",
        headers: {
            "Content-Type": "application/json"
        },
        body: JSON.stringify(body)
    });

    if (!response.ok) {
        const errorText = await response.text();
        throw new Error(`Ошибка сохранения зоны контроля: ${response.status}. ${errorText}`);
    }

    return await response.json().catch(() => null);
}

export async function saveArmyToBackend(request) {
    const body = {
        warSideId: request.warSideId,
        name: request.name,
        typeArmy: request.typeArmy,
        summary: request.summary,
        datePosition: request.datePosition,
        coordinate: createPointGeoJsonFromLeafletCoordinate(request.coordinate),
        positionNote: request.positionNote
    };

    const response = await fetch("/api/army/with-position", {
        method: "POST",
        headers: {
            "Content-Type": "application/json"
        },
        body: JSON.stringify(body)
    });

    if (!response.ok) {
        const errorText = await response.text();
        throw new Error(`Ошибка сохранения армии: ${response.status}. ${errorText}`);
    }

    return await response.json();
}

export async function updateArmyPositionOnBackend(armyId, request) {
    const body = {
        datePosition: request.datePosition,
        coordinate: createPointGeoJsonFromLeafletCoordinate(request.coordinate),
        note: request.note
    };

    const response = await fetch(`/api/army/${encodeURIComponent(armyId)}/position`, {
        method: "PUT",
        headers: {
            "Content-Type": "application/json"
        },
        body: JSON.stringify(body)
    });

    if (!response.ok) {
        const errorText = await response.text();
        throw new Error(`Ошибка перемещения армии: ${response.status}. ${errorText}`);
    }

    return await response.json();
}

export async function updateEventPositionOnBackend(eventId, request) {
    return {
        updatedOnServer: false,
        eventId,
        latitude: request.coordinate[0],
        longitude: request.coordinate[1],
        reason: "API перемещения событий пока не реализован."
    };
}

export async function deleteMapObjectFromBackend(objectType, objectId) {
    const url = getDeleteUrl(objectType, objectId);

    if (!url) {
        return {
            deletedOnServer: false,
            reason: "Для этого типа объекта API удаления пока не реализован."
        };
    }

    const response = await fetch(url, {
        method: "DELETE"
    });

    if (!response.ok) {
        const errorText = await response.text();
        throw new Error(`Ошибка удаления объекта: ${response.status}. ${errorText}`);
    }

    return {
        deletedOnServer: true
    };
}

function createPointGeoJsonFromLeafletCoordinate(coordinate) {
    const lat = coordinate[0];
    const lng = coordinate[1];

    return {
        type: "Point",
        coordinates: [lng, lat]
    };
}

function getDeleteUrl(objectType, objectId) {
    const encodedId = encodeURIComponent(objectId);

    if (objectType === "controlZone") {
        return `/api/control-zones/${encodedId}`;
    }

    if (objectType === "army") {
        return `/api/army/${encodedId}`;
    }

    return null;
}
