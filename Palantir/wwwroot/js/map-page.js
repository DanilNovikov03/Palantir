import { 
    escapeHtml, 
    formatDate, 
    getQueryParam, 
    isValidCoordinate 
} from "./map-utils.js";

import {
    convertGeoJsonMultiPolygonToLeafletCoordinates
} from "./map-geometry.js";

import {
    fetchOperation,
    fetchControlZonesByWarAndDate,
    saveControlZoneToBackend
} from "./map-api.js";

import { createTemporaryMapData } from "./map-demo-data.js";

const DEFAULT_CENTER = [51.75, 36.19];
const DEFAULT_ZOOM = 7;

const pageState = {
    operationId: getQueryParam("operationId"),
    operation: null,
    featureLayers: [],
    currentMapData: null,

    controlZoneDrawing: {
        isDrawing: false,
        coordinates: [],
        pointMarkers: [],
        draftLine: null,
        draftPolygon: null,
        nextTempZoneId: 1000
    }
};

const elements = {
    backToOperationBtn: document.getElementById("backToOperationBtn"),
    fitMapBtn: document.getElementById("fitMapBtn"),
    operationSubtitle: document.getElementById("operationSubtitle"),
    operationInfo: document.getElementById("operationInfo"),

    operationInfoId: document.getElementById("operationInfoId"),
    operationInfoTitle: document.getElementById("operationInfoTitle"),
    operationInfoStartDate: document.getElementById("operationInfoStartDate"),
    operationInfoEndDate: document.getElementById("operationInfoEndDate"),
    operationInfoSummary: document.getElementById("operationInfoSummary"),

    statusMessage: document.getElementById("statusMessage"),
    objectsList: document.getElementById("objectsList"),
    objectListItemTemplate: document.getElementById("objectListItemTemplate"),
    eventsLayerToggle: document.getElementById("eventsLayerToggle"),
    controlZonesLayerToggle: document.getElementById("controlZonesLayerToggle"),
    armiesLayerToggle: document.getElementById("armiesLayerToggle"),

    drawingPanel: null,
    toggleDrawingBtn: null,
    finishDrawingBtn: null,
    clearDraftBtn: null,
    drawingStatus: null,
    coordinatesOutput: null
};

const map = L.map("map", {
    zoomControl: true,
    attributionControl: true
}).setView(DEFAULT_CENTER, DEFAULT_ZOOM);

map.attributionControl.setPrefix(false);

const yandexTilesApiKey = window.PALANTIR_MAP_CONFIG?.YANDEX_TILES_API_KEY;

L.tileLayer(
    `https://tiles.api-maps.yandex.ru/v1/tiles/?x={x}&y={y}&z={z}&lang=ru_RU&l=map&projection=web_mercator&apikey=${yandexTilesApiKey}`,
    {
        maxZoom: 19,
        tileSize: 256,
        attribution: `
            <a href="https://yandex.ru/maps/" target="_blank" rel="noopener">© Яндекс</a>
            <a class="yandex-terms-link" href="https://yandex.ru/legal/maps_api/ru/" target="_blank" rel="noopener">Условия использования</a>
            |
            <a href="https://leafletjs.com/" target="_blank" rel="noopener">Leaflet</a>
        `
    }
).addTo(map);

const mapLayers = {
    events: L.layerGroup().addTo(map),
    controlZones: L.layerGroup().addTo(map),
    armies: L.layerGroup().addTo(map),

    // Отдельный слой для черновика создаваемой зоны.
    // Он не сохраняется в БД и нужен только для рисования.
    controlZoneDraft: L.layerGroup().addTo(map)
};

const eventIcon = L.divIcon({
    className: "",
    html: "<div class='event-marker'>!</div>",
    iconSize: [28, 28],
    iconAnchor: [14, 14],
    popupAnchor: [0, -14]
});

const armyIcon = L.divIcon({
    className: "",
    html: "<div class='army-marker'>A</div>",
    iconSize: [34, 34],
    iconAnchor: [17, 17],
    popupAnchor: [0, -17]
});

init();

async function init() {
    setupNavigation();
    setupLayerToggles();
    setupFitButton();
    setupControlZoneDrawing();

    await loadOperation(pageState.operationId);
    renderTemporaryMapObjects();
}

function setupNavigation() {
    if (!pageState.operationId) {
        elements.backToOperationBtn.href = "/operation.html";
        return;
    }

    elements.backToOperationBtn.href = `/operation.html?operationId=${encodeURIComponent(pageState.operationId)}`;
}

function setupLayerToggles() {
    bindLayerToggle(elements.eventsLayerToggle, mapLayers.events);
    bindLayerToggle(elements.controlZonesLayerToggle, mapLayers.controlZones);
    bindLayerToggle(elements.armiesLayerToggle, mapLayers.armies);
}

function setupFitButton() {
    elements.fitMapBtn.addEventListener("click", fitMapToObjects);
}

function bindLayerToggle(checkbox, layer) {
    if (!checkbox) {
        return;
    }

    checkbox.addEventListener("change", function () {
        if (checkbox.checked) {
            layer.addTo(map);
        } else {
            map.removeLayer(layer);
        }
    });
}

function setupControlZoneDrawing() {
    createControlZoneDrawingPanel();

    elements.toggleDrawingBtn.addEventListener("click", toggleControlZoneDrawingMode);
    elements.finishDrawingBtn.addEventListener("click", finishControlZoneDrawing);
    elements.clearDraftBtn.addEventListener("click", clearControlZoneDraft);

    map.on("click", handleMapClickForControlZoneDrawing);

    updateDrawingPanel();
}

function createControlZoneDrawingPanel() {
    const wrapper = document.querySelector(".map-container-wrapper") ?? document.body;

    if (getComputedStyle(wrapper).position === "static") {
        wrapper.classList.add("map-container-wrapper-relative");
    }

    const template = document.querySelector("#controlZoneDrawingPanelTemplate");
    const panel = template.content.firstElementChild.cloneNode(true);
    wrapper.appendChild(panel);

    // Чтобы карта не реагированал
    L.DomEvent.disableClickPropagation(panel);
    L.DomEvent.disableScrollPropagation(panel);

    elements.drawingPanel = panel;
    elements.toggleDrawingBtn = panel.querySelector("#toggleControlZoneDrawingBtn");
    elements.finishDrawingBtn = panel.querySelector("#finishControlZoneDrawingBtn");
    elements.clearDraftBtn = panel.querySelector("#clearControlZoneDraftBtn");
    elements.drawingStatus = panel.querySelector("#controlZoneDrawingStatus");
    elements.coordinatesOutput = panel.querySelector("#controlZoneCoordinatesOutput");
}

function toggleControlZoneDrawingMode() {
    if (pageState.controlZoneDrawing.isDrawing) {
        stopControlZoneDrawingMode();
    } else {
        startControlZoneDrawingMode();
    }

    updateDrawingPanel();
}

function startControlZoneDrawingMode() {
    pageState.controlZoneDrawing.isDrawing = true;
    map.getContainer().style.cursor = "crosshair";

    setStatus(
        "Режим создания зоны контроля включён. Нажимай по карте, чтобы добавлять точки полигона.",
        "info"
    );
}

function stopControlZoneDrawingMode() {
    pageState.controlZoneDrawing.isDrawing = false;
    map.getContainer().style.cursor = "";

    setStatus(
        "Режим создания зоны контроля выключен. Уже поставленные точки черновика сохранены на фронте.",
        "secondary"
    );
}

function handleMapClickForControlZoneDrawing(event) {
    if (!pageState.controlZoneDrawing.isDrawing) {
        return;
    }

    const lat = Number(event.latlng.lat.toFixed(6));
    const lng = Number(event.latlng.lng.toFixed(6));

    const coordinate = [lat, lng];

    pageState.controlZoneDrawing.coordinates.push(coordinate);

    const pointNumber = pageState.controlZoneDrawing.coordinates.length;

    const marker = L.circleMarker(coordinate, {
        radius: 5,
        color: "#198754",
        fillColor: "#198754",
        fillOpacity: 0.9,
        weight: 2
    }).bindTooltip(`Точка ${pointNumber}: ${lat}, ${lng}`);

    marker.addTo(mapLayers.controlZoneDraft);
    pageState.controlZoneDrawing.pointMarkers.push(marker);

    redrawControlZoneDraft();
    updateDrawingPanel();
}

function redrawControlZoneDraft() {
    const drawing = pageState.controlZoneDrawing;
    const coordinates = drawing.coordinates;

    if (drawing.draftLine) {
        mapLayers.controlZoneDraft.removeLayer(drawing.draftLine);
        drawing.draftLine = null;
    }

    if (drawing.draftPolygon) {
        mapLayers.controlZoneDraft.removeLayer(drawing.draftPolygon);
        drawing.draftPolygon = null;
    }

    if (coordinates.length < 2) {
        return;
    }

    if (coordinates.length === 2) {
        drawing.draftLine = L.polyline(coordinates, {
            color: "#198754",
            weight: 2,
            dashArray: "6 6"
        });

        drawing.draftLine.addTo(mapLayers.controlZoneDraft);
        return;
    }

    drawing.draftPolygon = L.polygon(coordinates, {
        color: "#198754",
        fillColor: "#198754",
        fillOpacity: 0.2,
        weight: 2,
        dashArray: "6 6"
    });

    drawing.draftPolygon.addTo(mapLayers.controlZoneDraft);
}

async function finishControlZoneDrawing() {
    const drawing = pageState.controlZoneDrawing;

    if (drawing.coordinates.length < 3) {
        setStatus("Для зоны контроля нужно минимум 3 точки.", "warning");
        return;
    }

    try {
        setStatus("Сохраняю зону контроля в базу данных...", "info");

        await saveControlZoneToBackend(drawing.coordinates);

        const newZone = {
            id: drawing.nextTempZoneId,
            title: `Новая зона контроля #${drawing.nextTempZoneId}`,
            description: "Зона создана и отправлена на backend.",
            side: "Draft",
            coordinates: drawing.coordinates.map(point => [...point])
        };

        drawing.nextTempZoneId += 1;

        addControlZone(newZone);

        if (pageState.currentMapData) {
            pageState.currentMapData.controlZones.push(newZone);
            renderObjectsList(pageState.currentMapData);
        }

        await renderTemporaryMapObjects();
        clearControlZoneDraft();

        setStatus("Зона контроля сохранена в БД и карта обновлена.", "success");

    } catch (error) {
        console.error(error);
        setStatus("Не удалось сохранить зону контроля в базу данных.", "danger");
    }
}

function mapBackendControlZoneToFrontendZone(zone) {
    return {
        id: zone.id,
        title: `Зона контроля #${zone.id}`,
        description: `Дата: ${formatDate(zone.dateControl)}. Точность: ${zone.precisionControl}`,
        side: String(zone.warSideId),
        warId: zone.warId,
        warSideId: zone.warSideId,
        dateControl: zone.dateControl,
        precisionControl: zone.precisionControl,
        coordinates: convertGeoJsonMultiPolygonToLeafletCoordinates(zone.geom)
    };
}

// Временно
function getCurrentWarId() {
    return pageState.operation?.warId ?? 1;
}

function getCurrentMapDate() {
    return "1943-07-05";
}

function clearControlZoneDraft() {
    const drawing = pageState.controlZoneDrawing;

    drawing.coordinates = [];
    drawing.pointMarkers = [];
    drawing.draftLine = null;
    drawing.draftPolygon = null;

    mapLayers.controlZoneDraft.clearLayers();

    updateDrawingPanel();
}

function updateDrawingPanel() {
    const drawing = pageState.controlZoneDrawing;

    if (!elements.toggleDrawingBtn) {
        return;
    }

    elements.toggleDrawingBtn.textContent = drawing.isDrawing
        ? "Остановить рисование"
        : "Добавить зону контроля";

    elements.toggleDrawingBtn.className = drawing.isDrawing
        ? "btn btn-warning btn-sm"
        : "btn btn-success btn-sm";

    elements.finishDrawingBtn.disabled = drawing.coordinates.length < 3;
    elements.clearDraftBtn.disabled = drawing.coordinates.length === 0;

    elements.drawingStatus.textContent = drawing.isDrawing
        ? `Режим рисования включён. Точек: ${drawing.coordinates.length}.`
        : `Режим рисования выключен. Точек в черновике: ${drawing.coordinates.length}.`;

    elements.coordinatesOutput.textContent = JSON.stringify(drawing.coordinates, null, 2);
}

async function loadOperation(operationId) {
    setStatus("Загружаю данные операции...", "info");

    try {
        const operation = await fetchOperation(operationId);
        pageState.operation = operation;

        renderOperationInfo(operation);
        setMapCenterFromOperation(operation);
        setStatus("Данные операции загружены. На карте пока показаны временные демонстрационные объекты.", "success");
    } catch (error) {
        console.error(error);
        renderOperationInfo(null);
        setStatus("Не удалось загрузить операцию через API. Карта всё равно открыта с тестовыми объектами.", "warning");
    }
}



function renderOperationInfo(operation) {
    if (!operation) {
        const fallbackTitle = pageState.operationId
            ? `Операция #${pageState.operationId}`
            : "Операция не выбрана";

        elements.operationSubtitle.textContent = fallbackTitle;

        elements.operationInfoId.textContent = pageState.operationId ?? "не указан";
        elements.operationInfoTitle.textContent = "не указано";
        elements.operationInfoStartDate.textContent = "не указана";
        elements.operationInfoEndDate.textContent = "не указана";
        elements.operationInfoSummary.textContent = "Данные операции не были получены из API.";

        return;
    }

    const title = operation.title ?? `Операция #${pageState.operationId}`;
    const summary = operation.summary ?? null;
    const startDate = operation.startDate ?? null;
    const endDate = operation.endDate ?? null;

    elements.operationSubtitle.textContent = title;

    elements.operationInfoId.textContent = pageState.operationId ?? operation.id ?? "не указан";
    elements.operationInfoTitle.textContent = title;
    elements.operationInfoStartDate.textContent = formatDate(startDate);
    elements.operationInfoEndDate.textContent = formatDate(endDate);
    elements.operationInfoSummary.textContent = summary;
}

function setMapCenterFromOperation(operation) {
    const lat = operation.centerLat ?? operation.latitude ?? operation.lat;
    const lng = operation.centerLng ?? operation.longitude ?? operation.lng;

    if (isValidCoordinate(lat) && isValidCoordinate(lng)) {
        map.setView([Number(lat), Number(lng)], operation.zoom ?? DEFAULT_ZOOM);
    }
}

async function renderTemporaryMapObjects() {
    clearMapLayers();

    const mapData = createTemporaryMapData();

    const warId = getCurrentWarId();
    const date = getCurrentMapDate();

    const backendZones = await fetchControlZonesByWarAndDate(warId, date);
    const zones = backendZones.map(mapBackendControlZoneToFrontendZone);

    mapData.controlZones = zones;
    pageState.currentMapData = mapData;

    mapData.controlZones.forEach(addControlZone);
    mapData.armies.forEach(addArmyMarker);
    mapData.events.forEach(addEventMarker);

    renderObjectsList(mapData);
    fitMapToObjects();
}

function clearMapLayers() {
    pageState.featureLayers = [];

    mapLayers.events.clearLayers();
    mapLayers.controlZones.clearLayers();
    mapLayers.armies.clearLayers();
    mapLayers.controlZoneDraft.clearLayers();

    pageState.controlZoneDrawing.coordinates = [];
    pageState.controlZoneDrawing.pointMarkers = [];
    pageState.controlZoneDrawing.draftLine = null;
    pageState.controlZoneDrawing.draftPolygon = null;

    updateDrawingPanel();
}

function addEventMarker(event) {
    const marker = L.marker([event.lat, event.lng], { icon: eventIcon })
        .bindPopup(createEventPopupHtml(event));

    marker.addTo(mapLayers.events);
    pageState.featureLayers.push(marker);
}

function createEventPopupHtml(event) {
    return `
    <strong>${escapeHtml(event.title)}</strong><br>
    <span>${escapeHtml(formatDate(event.date))}</span><br>
    <span>${escapeHtml(event.description)}</span>
`;
}

function createArmyPopupHtml(army) {
    return `
    <strong>${escapeHtml(army.title)}</strong><br>
    <span>${escapeHtml(army.description)}</span>
`;
}

function createControlZonePopupHtml(zone) {
    return `
    <strong>${escapeHtml(zone.title)}</strong><br>
    <span>${escapeHtml(zone.description)}</span><br>
    <span>Количество точек: ${zone.coordinates.length}</span>
`;
}

function addArmyMarker(army) {
    const marker = L.marker([army.lat, army.lng], { icon: armyIcon })
        .bindPopup(`
            <strong>${escapeHtml(army.title)}</strong><br>
            <span>${escapeHtml(army.description)}</span>
        `);

    marker.addTo(mapLayers.armies);
    pageState.featureLayers.push(marker);
}

function addControlZone(zone) {
    const style = getControlZoneStyle(zone.side);

    const polygon = L.polygon(zone.coordinates, style)
        .bindPopup(`
            <strong>${escapeHtml(zone.title)}</strong><br>
            <span>${escapeHtml(zone.description)}</span><br>
            <span>Количество точек: ${zone.coordinates.length}</span>
        `);

    polygon.addTo(mapLayers.controlZones);
    pageState.featureLayers.push(polygon);
}

function getControlZoneStyle(side) {
    if (side === "A") {
        return {
            color: "#0d6efd",
            fillColor: "#0d6efd",
            fillOpacity: 0.18,
            weight: 2
        };
    }

    if (side === "B") {
        return {
            color: "#dc3545",
            fillColor: "#dc3545",
            fillOpacity: 0.18,
            weight: 2
        };
    }

    return {
        color: "#198754",
        fillColor: "#198754",
        fillOpacity: 0.18,
        weight: 2
    };
}

function renderObjectsList(demoData) {
    const items = collectMapObjectItems(demoData);

    elements.objectsList.innerHTML = "";

    if (items.length === 0) {
        elements.objectsList.textContent = "Объектов пока нет.";
        elements.objectsList.classList.add("text-secondary");
        return;
    }

    elements.objectsList.classList.remove("text-secondary");

    items.forEach(item => {
        const button = createObjectListButton(item);
        elements.objectsList.appendChild(button);
    });
}

function collectMapObjectItems(mapData) {
    const items = [];

    mapData.events.forEach(event => {
        items.push({
            type: "Событие",
            title: event.title,
            coordinates: [event.lat, event.lng]
        });
    });

    mapData.armies.forEach(army => {
        items.push({
            type: "Соединение",
            title: army.title,
            coordinates: [army.lat, army.lng]
        });
    });

    mapData.controlZones.forEach(zone => {
        items.push({
            type: "Зона контроля",
            title: zone.title,
            coordinates: zone.coordinates[0],
            bounds: zone.coordinates
        });
    });

    return items;
}

function fitMapToObjects() {
    if (pageState.featureLayers.length === 0) {
        map.setView(DEFAULT_CENTER, DEFAULT_ZOOM);
        return;
    }

    const group = L.featureGroup(pageState.featureLayers);
    map.fitBounds(group.getBounds(), {
        padding: [40, 40],
        maxZoom: 10
    });
}

function setStatus(message, type) {
    elements.statusMessage.className = `alert alert-${type} py-2 small`;
    elements.statusMessage.textContent = message;
}    

function createObjectListButton(item) {
    const button = elements.objectListItemTemplate.content.firstElementChild.cloneNode(true);

    button.querySelector("[data-object-title]").textContent = item.title;
    button.querySelector("[data-object-type]").textContent = item.type;

    button.addEventListener("click", function () {
        if (item.bounds && item.bounds.length >= 2) {
            map.fitBounds(L.latLngBounds(item.bounds), {
                padding: [40, 40],
                maxZoom: 10
            });
            return;
        }

        map.setView(item.coordinates, 10);
    });

    return button;
}