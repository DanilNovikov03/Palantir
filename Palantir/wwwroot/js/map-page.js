import {
    getQueryParam,
    formatDate,
    isValidCoordinate,
    escapeHtml
} from "./map-utils.js";

import {
    convertGeoJsonMultiPolygonToLeafletCoordinates
} from "./map-geometry.js";

import {
    fetchOperation,
    fetchOperationSides,
    fetchWarSides,
    fetchControlZonesByWarAndDate,
    fetchArmiesByWarAndDate,
    fetchEventsByWarAndDate,
    saveControlZoneToBackend,
    updateControlZoneGeometryForDate,
    saveArmyToBackend,
    saveEventToBackend,
    updateArmyPositionOnBackend,
    updateEventPositionOnBackend,
    deleteMapObjectFromBackend
} from "./map-api.js";

import {
    createTemporaryMapData
} from "./map-demo-data.js";

import {
    getSideColor
} from "./map-side-colors.js";

const DEFAULT_CENTER = [51.75, 36.19];
const DEFAULT_ZOOM = 7;
const DEFAULT_MAP_DATE = "1943-07-05";
const MAP_ACTION_MODES = {
    CONTROL_ZONE: "control-zone",
    ARMY: "army",
    EVENT: "event",
    MOVE_ARMY: "move-army",
    MOVE_EVENT: "move-event",
    EDIT_CONTROL_ZONE: "edit-control-zone"
};
const MAP_ACTIVE_MODE_CLASS = "map-active-mode";
const MAP_MOVE_MODE_CLASS = "map-move-mode";
const MAP_EDIT_CONTROL_ZONE_CLASS = "map-edit-control-zone";

const DEFAULT_CONTROL_ZONE_PRECISION = "Approximate";

const pageState = {
    operationId: getQueryParam("operationId"),
    warId: Number(getQueryParam("warId")) || null,
    mapDate: normalizeDate(getQueryParam("date")) ?? DEFAULT_MAP_DATE,
    operation: null,
    operationSides: [],
    featureLayers: [],
    currentMapData: null,
    activeMode: null,
    pendingPoint: null,
    movingObject: null,
    nextTempArmyId: 2000,
    nextTempEventId: 3000,

    controlZoneDrawing: {
        isDrawing: false,
        coordinates: [],
        pointMarkers: [],
        draftLine: null,
        draftPolygon: null,
        nextTempZoneId: 1000
    },

    controlZoneEditing: {
        zone: null,
        originalCoordinates: [],
        coordinates: [],
        vertexMarkers: [],
        polygon: null,
        isSaving: false
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

    mapDateInput: document.getElementById("mapDateInput"),
    applyDateBtn: document.getElementById("applyDateBtn"),

    eventsLayerToggle: document.getElementById("eventsLayerToggle"),
    controlZonesLayerToggle: document.getElementById("controlZonesLayerToggle"),
    armiesLayerToggle: document.getElementById("armiesLayerToggle"),

    actionsPanel: null,
    addControlZoneModeBtn: null,
    addArmyModeBtn: null,
    addEventModeBtn: null,
    cancelMapActionBtn: null,
    actionModePanel: null,
    actionHint: null,
    controlZoneModeControls: null,
    controlZoneSideSelect: null,
    finishDrawingBtn: null,
    clearDraftBtn: null,
    editControlZoneModeControls: null,
    editControlZoneWarning: null,
    finishControlZoneEditingBtn: null,
    cancelControlZoneEditingBtn: null,
    mapObjectForm: null,
    mapObjectTitleLabel: null,
    mapObjectTitleInput: null,
    mapObjectTypeGroup: null,
    mapObjectTypeLabel: null,
    mapObjectTypeInput: null,
    mapObjectDescriptionGroup: null,
    mapObjectDescriptionInput: null,
    mapObjectSideSelect: null,
    mapObjectCoordinatePreview: null,
    saveMapObjectBtn: null,
    cancelMapObjectFormBtn: null
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
    controlZoneDraft: L.layerGroup().addTo(map),
    controlZoneEdit: L.layerGroup().addTo(map)
};

init();

async function init() {
    setupNavigation();
    setupLayerToggles();
    setupFitButton();
    setupDateFilter();
    setupControlZoneDrawing();
    setupMapObjectPopupActions();

    await loadOperation(pageState.operationId);
    await loadMapSides();
    await renderTemporaryMapObjects();
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

function setupDateFilter() {
    if (!elements.mapDateInput || !elements.applyDateBtn) {
        return;
    }

    elements.mapDateInput.disabled = false;
    elements.applyDateBtn.disabled = false;
    elements.mapDateInput.value = getCurrentMapDate();

    elements.applyDateBtn.addEventListener("click", applySelectedDate);

    elements.mapDateInput.addEventListener("keydown", function (event) {
        if (event.key === "Enter") {
            applySelectedDate();
        }
    });
}

async function applySelectedDate() {
    const selectedDate = normalizeDate(elements.mapDateInput?.value);

    if (!selectedDate) {
        setStatus("Выбери корректную дату для загрузки зон контроля.", "warning");
        return;
    }

    setCurrentMapDate(selectedDate, true);
    await renderTemporaryMapObjects();
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
    createMapActionsPanel();

    elements.addControlZoneModeBtn.addEventListener("click", () => activateMapMode(MAP_ACTION_MODES.CONTROL_ZONE));
    elements.addArmyModeBtn.addEventListener("click", () => activateMapMode(MAP_ACTION_MODES.ARMY));
    elements.addEventModeBtn.addEventListener("click", () => activateMapMode(MAP_ACTION_MODES.EVENT));
    elements.cancelMapActionBtn.addEventListener("click", cancelActiveMapMode);
    elements.finishDrawingBtn.addEventListener("click", finishControlZoneDrawing);
    elements.clearDraftBtn.addEventListener("click", clearControlZoneDraft);
    elements.finishControlZoneEditingBtn.addEventListener("click", finishControlZoneEditing);
    elements.cancelControlZoneEditingBtn.addEventListener("click", cancelControlZoneEditing);
    elements.mapObjectForm.addEventListener("submit", saveMapObjectDraft);
    elements.cancelMapObjectFormBtn.addEventListener("click", clearPendingPointForm);

    map.on("click", handleMapClickForActiveMode);

    updateMapActionsPanel();
}

function setupMapObjectPopupActions() {
    document.addEventListener("click", async function (event) {
        const editControlZoneButton = event.target.closest("[data-edit-control-zone]");

        if (editControlZoneButton) {
            const controlZoneId = Number(editControlZoneButton.dataset.objectId);

            if (!Number.isInteger(controlZoneId)) {
                setStatus("Не удалось определить зону контроля для редактирования.", "warning");
                return;
            }

            startControlZoneEditing(controlZoneId);
            return;
        }

        const deleteButton = event.target.closest("[data-delete-map-object]");

        if (deleteButton) {
            const objectType = deleteButton.dataset.objectType;
            const objectId = Number(deleteButton.dataset.objectId);

            if (!objectType || !Number.isInteger(objectId)) {
                setStatus("Не удалось определить объект для удаления.", "warning");
                return;
            }

            await deleteMapObject(objectType, objectId);
            return;
        }

        const moveButton = event.target.closest("[data-move-map-object]");

        if (!moveButton) {
            return;
        }

        const objectType = moveButton.dataset.objectType;
        const objectId = Number(moveButton.dataset.objectId);

        if (!objectType || !Number.isInteger(objectId)) {
            setStatus("Не удалось определить объект для перемещения.", "warning");
            return;
        }

        startMoveMapObjectMode(objectType, objectId);
    });
}

async function deleteMapObject(objectType, objectId) {
    const confirmed = window.confirm("Вы действительно хотите удалить этот объект?");

    if (!confirmed) {
        return;
    }

    try {
        const result = await deleteMapObjectFromBackend(objectType, objectId);

        removeMapObjectFromCurrentData(objectType, objectId);
        redrawCurrentMapData();

        const message = result.deletedOnServer
            ? "Объект удалён из БД и убран с карты."
            : "Объект удалён с карты. API удаления для этого типа пока не реализован.";

        setStatus(message, result.deletedOnServer ? "success" : "secondary");
    } catch (error) {
        console.error(error);
        setStatus("Не удалось удалить объект. Проверь API удаления и идентификатор объекта.", "danger");
    }
}

function startMoveMapObjectMode(objectType, objectId) {
    const object = findMapObject(objectType, objectId);

    if (!object) {
        setStatus("Объект для перемещения не найден на карте.", "warning");
        return;
    }

    clearTransientEditingState();
    map.closePopup();
    pageState.movingObject = { objectType, objectId };

    if (objectType === "army") {
        pageState.activeMode = MAP_ACTION_MODES.MOVE_ARMY;
        setStatus("Выберите новую позицию армии на карте.", "info");
    } else if (objectType === "event") {
        pageState.activeMode = MAP_ACTION_MODES.MOVE_EVENT;
        setStatus("Выберите новую позицию события на карте.", "info");
    } else {
        pageState.movingObject = null;
        setStatus("Этот тип объекта нельзя перемещать.", "warning");
        return;
    }

    updateMapInteractionClass();
    map.getContainer().style.cursor = "crosshair";
    updateMapActionsPanel();
}

function findMapObject(objectType, objectId) {
    ensureCurrentMapData();

    if (objectType === "army") {
        return pageState.currentMapData.armies
            .find(army => army.id === objectId) ?? null;
    }

    if (objectType === "event") {
        return pageState.currentMapData.events
            .find(mapEvent => mapEvent.id === objectId) ?? null;
    }

    if (objectType === "controlZone") {
        return pageState.currentMapData.controlZones
            .find(zone => zone.id === objectId) ?? null;
    }

    return null;
}

function removeMapObjectFromCurrentData(objectType, objectId) {
    ensureCurrentMapData();

    if (objectType === "controlZone") {
        pageState.currentMapData.controlZones = pageState.currentMapData.controlZones
            .filter(zone => zone.id !== objectId);
    }

    if (objectType === "army") {
        pageState.currentMapData.armies = pageState.currentMapData.armies
            .filter(army => army.id !== objectId);
    }

    if (objectType === "event") {
        pageState.currentMapData.events = pageState.currentMapData.events
            .filter(mapEvent => mapEvent.id !== objectId);
    }
}

function redrawCurrentMapData() {
    ensureCurrentMapData();

    pageState.featureLayers = [];
    mapLayers.events.clearLayers();
    mapLayers.controlZones.clearLayers();
    mapLayers.armies.clearLayers();

    pageState.currentMapData.controlZones.forEach(addControlZone);
    pageState.currentMapData.armies.forEach(addArmyMarker);
    pageState.currentMapData.events.forEach(addEventMarker);

    renderObjectsList(pageState.currentMapData);
}

function createMapActionsPanel() {
    const wrapper = document.querySelector(".map-container-wrapper") ?? document.body;

    if (getComputedStyle(wrapper).position === "static") {
        wrapper.classList.add("map-container-wrapper-relative");
    }

    const template = document.querySelector("#mapActionsPanelTemplate");
    const panel = template.content.firstElementChild.cloneNode(true);
    wrapper.appendChild(panel);

    // Чтобы карта не реагировала на клики внутри панели.
    L.DomEvent.disableClickPropagation(panel);
    L.DomEvent.disableScrollPropagation(panel);

    elements.actionsPanel = panel;
    elements.addControlZoneModeBtn = panel.querySelector("#addControlZoneModeBtn");
    elements.addArmyModeBtn = panel.querySelector("#addArmyModeBtn");
    elements.addEventModeBtn = panel.querySelector("#addEventModeBtn");
    elements.cancelMapActionBtn = panel.querySelector("#cancelMapActionBtn");
    elements.actionModePanel = panel.querySelector("#mapActionModePanel");
    elements.actionHint = panel.querySelector("#mapActionHint");
    elements.controlZoneModeControls = panel.querySelector("#controlZoneModeControls");
    elements.controlZoneSideSelect = panel.querySelector("#controlZoneSideSelect");
    elements.finishDrawingBtn = panel.querySelector("#finishControlZoneDrawingBtn");
    elements.clearDraftBtn = panel.querySelector("#clearControlZoneDraftBtn");
    elements.editControlZoneModeControls = panel.querySelector("#editControlZoneModeControls");
    elements.editControlZoneWarning = panel.querySelector("#editControlZoneWarning");
    elements.finishControlZoneEditingBtn = panel.querySelector("#finishControlZoneEditingBtn");
    elements.cancelControlZoneEditingBtn = panel.querySelector("#cancelControlZoneEditingBtn");
    elements.mapObjectForm = panel.querySelector("#mapObjectForm");
    elements.mapObjectTitleLabel = panel.querySelector("#mapObjectTitleLabel");
    elements.mapObjectTitleInput = panel.querySelector("#mapObjectTitleInput");
    elements.mapObjectTypeGroup = panel.querySelector("#mapObjectTypeGroup");
    elements.mapObjectTypeLabel = panel.querySelector("#mapObjectTypeLabel");
    elements.mapObjectTypeInput = panel.querySelector("#mapObjectTypeInput");
    elements.mapObjectDescriptionGroup = panel.querySelector("#mapObjectDescriptionGroup");
    elements.mapObjectDescriptionInput = panel.querySelector("#mapObjectDescriptionInput");
    elements.mapObjectSideSelect = panel.querySelector("#mapObjectSideSelect");
    elements.mapObjectCoordinatePreview = panel.querySelector("#mapObjectCoordinatePreview");
    elements.saveMapObjectBtn = panel.querySelector("#saveMapObjectBtn");
    elements.cancelMapObjectFormBtn = panel.querySelector("#cancelMapObjectFormBtn");

    fillSideSelect(elements.controlZoneSideSelect, pageState.operationSides);
    fillSideSelect(elements.mapObjectSideSelect, pageState.operationSides);
}

function fillSideSelect(select, sides) {
    if (!select) {
        return;
    }

    select.innerHTML = "";

    const placeholder = document.createElement("option");
    placeholder.value = "";
    placeholder.textContent = sides.length ? "Выберите сторону" : "Стороны операции не указаны";
    select.appendChild(placeholder);

    sides.forEach(side => {
        const option = document.createElement("option");
        option.value = String(side.warSideId);
        option.textContent = side.title;
        option.dataset.sideId = String(side.sideId);
        option.dataset.colorHex = getSideColor(side);
        select.appendChild(option);
    });

    select.value = "";
    select.disabled = sides.length === 0;
}

function activateMapMode(mode) {
    if (pageState.activeMode === mode) {
        cancelActiveMapMode();
        return;
    }

    if (pageState.operationSides.length === 0) {
        setStatus("Для операции не указаны стороны. Сначала добавьте связи operation_sides.", "warning");
        return;
    }

    clearTransientEditingState();
    map.closePopup();
    pageState.activeMode = mode;
    updateMapInteractionClass();

    if (mode === MAP_ACTION_MODES.CONTROL_ZONE) {
        pageState.controlZoneDrawing.isDrawing = true;
        map.getContainer().style.cursor = "crosshair";
        setStatus("Выберите точки зоны контроля. Минимум нужно 3 точки.", "info");
    }

    if (mode === MAP_ACTION_MODES.ARMY) {
        map.getContainer().style.cursor = "crosshair";
        setStatus("Выберите точку для размещения армии.", "info");
    }

    if (mode === MAP_ACTION_MODES.EVENT) {
        map.getContainer().style.cursor = "crosshair";
        setStatus("Выберите точку для события.", "info");
    }

    updateMapActionsPanel();
}

function cancelActiveMapMode() {
    if (pageState.activeMode === MAP_ACTION_MODES.EDIT_CONTROL_ZONE) {
        cancelControlZoneEditing();
        return;
    }

    clearTransientEditingState();
    pageState.activeMode = null;
    updateMapInteractionClass();
    map.getContainer().style.cursor = "";

    setStatus("Режим добавления объекта отменён.", "secondary");
    updateMapActionsPanel();
}

function clearTransientEditingState() {
    pageState.controlZoneDrawing.isDrawing = false;
    pageState.pendingPoint = null;
    pageState.movingObject = null;
    map.getContainer().classList.remove(MAP_ACTIVE_MODE_CLASS, MAP_MOVE_MODE_CLASS);
    clearControlZoneDraft(false);
    clearControlZoneEditingState();
    setMapDateControlsDisabled(false);
    hideMapObjectForm();
}

function updateMapInteractionClass() {
    const mapContainer = map.getContainer();
    const hasActiveMode = Boolean(pageState.activeMode);
    const isMoveMode =
        pageState.activeMode === MAP_ACTION_MODES.MOVE_ARMY ||
        pageState.activeMode === MAP_ACTION_MODES.MOVE_EVENT;

    mapContainer.classList.toggle(MAP_ACTIVE_MODE_CLASS, hasActiveMode);
    mapContainer.classList.toggle(MAP_MOVE_MODE_CLASS, isMoveMode);
    mapContainer.classList.toggle(
        MAP_EDIT_CONTROL_ZONE_CLASS,
        pageState.activeMode === MAP_ACTION_MODES.EDIT_CONTROL_ZONE
    );
}

function startControlZoneEditing(controlZoneId) {
    const zone = findMapObject("controlZone", controlZoneId);
    const coordinates = getEditableControlZoneCoordinates(zone?.coordinates);

    if (!zone || coordinates.length < 3) {
        setStatus("Зона контроля не содержит достаточно точек для редактирования.", "warning");
        return;
    }

    clearTransientEditingState();
    map.closePopup();

    pageState.activeMode = MAP_ACTION_MODES.EDIT_CONTROL_ZONE;
    pageState.controlZoneEditing.zone = zone;
    pageState.controlZoneEditing.originalCoordinates = cloneControlZoneCoordinates(coordinates);
    pageState.controlZoneEditing.coordinates = cloneControlZoneCoordinates(coordinates);

    renderEditableControlZone();
    setMapDateControlsDisabled(true);
    updateMapInteractionClass();
    updateMapActionsPanel();

    const isNewDate = normalizeDate(zone.dateControl) !== getCurrentMapDate();
    setStatus(
        isNewDate
            ? "Переместите точки зоны. Изменения будут сохранены как новый снимок на выбранную дату."
            : "Переместите точки зоны контроля. Нажмите «Готово», чтобы сохранить изменения.",
        isNewDate ? "warning" : "info"
    );
}

function renderEditableControlZone() {
    const editing = pageState.controlZoneEditing;
    const style = getControlZoneStyle(editing.zone?.side);

    mapLayers.controlZoneEdit.clearLayers();
    editing.vertexMarkers = [];
    editing.polygon = L.polygon(editing.coordinates, {
        ...style,
        fillOpacity: 0.15,
        weight: 3,
        dashArray: "6 5"
    }).addTo(mapLayers.controlZoneEdit);

    editing.coordinates.forEach((coordinate, index) => {
        const marker = L.marker(coordinate, {
            draggable: true,
            keyboard: false,
            autoPan: true,
            icon: L.divIcon({
                className: "control-zone-edit-marker",
                html: '<div class="control-zone-edit-handle"></div>',
                iconSize: [16, 16],
                iconAnchor: [8, 8]
            })
        });

        marker.on("drag", event => {
            const latLng = event.target.getLatLng();
            editing.coordinates[index] = [
                Number(latLng.lat.toFixed(6)),
                Number(latLng.lng.toFixed(6))
            ];
            editing.polygon.setLatLngs(editing.coordinates);
        });

        marker.addTo(mapLayers.controlZoneEdit);
        editing.vertexMarkers.push(marker);
    });
}

async function finishControlZoneEditing() {
    const editing = pageState.controlZoneEditing;

    if (!editing.zone || editing.coordinates.length < 3 || editing.isSaving) {
        setStatus("Для зоны контроля нужно минимум 3 точки.", "warning");
        return;
    }

    try {
        editing.isSaving = true;
        elements.finishControlZoneEditingBtn.disabled = true;
        setStatus("Сохраняю изменения зоны контроля...", "info");

        const response = await updateControlZoneGeometryForDate(
            editing.zone.id,
            getCurrentMapDate(),
            editing.coordinates
        );

        const isNewVersion = Boolean(response?.isNewVersion);
        completeControlZoneEditing();
        await renderTemporaryMapObjects();
        setStatus(
            isNewVersion
                ? "Создана новая зона контроля на выбранную дату."
                : "Зона контроля обновлена.",
            "success"
        );
    } catch (error) {
        console.error(error);
        editing.isSaving = false;
        elements.finishControlZoneEditingBtn.disabled = false;
        setStatus(error.message || "Не удалось сохранить изменения зоны контроля.", "danger");
    }
}

function cancelControlZoneEditing() {
    completeControlZoneEditing();
    setStatus("Редактирование зоны контроля отменено.", "secondary");
}

function completeControlZoneEditing() {
    clearControlZoneEditingState();
    pageState.activeMode = null;
    setMapDateControlsDisabled(false);
    updateMapInteractionClass();
    updateMapActionsPanel();
}

function clearControlZoneEditingState() {
    const editing = pageState.controlZoneEditing;

    mapLayers.controlZoneEdit.clearLayers();
    editing.zone = null;
    editing.originalCoordinates = [];
    editing.coordinates = [];
    editing.vertexMarkers = [];
    editing.polygon = null;
    editing.isSaving = false;

    if (elements.finishControlZoneEditingBtn) {
        elements.finishControlZoneEditingBtn.disabled = false;
    }
}

function setMapDateControlsDisabled(disabled) {
    if (elements.mapDateInput) elements.mapDateInput.disabled = disabled;
    if (elements.applyDateBtn) elements.applyDateBtn.disabled = disabled;
}

function getEditableControlZoneCoordinates(coordinates) {
    const result = cloneControlZoneCoordinates(coordinates ?? []);

    if (result.length > 1 && areSameCoordinates(result[0], result[result.length - 1])) {
        result.pop();
    }

    return result;
}

function cloneControlZoneCoordinates(coordinates) {
    return coordinates.map(point => [Number(point[0]), Number(point[1])]);
}

function areSameCoordinates(first, second) {
    return first?.[0] === second?.[0] && first?.[1] === second?.[1];
}

function handleMapClickForActiveMode(event) {
    if (
        pageState.activeMode === MAP_ACTION_MODES.MOVE_ARMY ||
        pageState.activeMode === MAP_ACTION_MODES.MOVE_EVENT
    ) {
        moveSelectedMapObject(event);
        return;
    }

    if (pageState.activeMode === MAP_ACTION_MODES.CONTROL_ZONE) {
        handleMapClickForControlZoneDrawing(event);
        return;
    }

    if (pageState.activeMode === MAP_ACTION_MODES.ARMY) {
        preparePointObjectForm(event, MAP_ACTION_MODES.ARMY);
        return;
    }

    if (pageState.activeMode === MAP_ACTION_MODES.EVENT) {
        preparePointObjectForm(event, MAP_ACTION_MODES.EVENT);
    }
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
    updateMapActionsPanel();
}

async function moveSelectedMapObject(event) {
    if (!pageState.movingObject) {
        setStatus("Не выбран объект для перемещения.", "warning");
        return;
    }

    const lat = Number(event.latlng.lat.toFixed(6));
    const lng = Number(event.latlng.lng.toFixed(6));
    const { objectType, objectId } = pageState.movingObject;

    if (objectType === "army") {
        await moveArmy(objectId, lat, lng);
        return;
    }

    if (objectType === "event") {
        await moveEvent(objectId, lat, lng);
    }
}

async function moveArmy(armyId, lat, lng) {
    try {
        setStatus("Сохраняю новую позицию армии...", "info");

        const response = await updateArmyPositionOnBackend(armyId, {
            datePosition: getCurrentMapDate(),
            coordinate: [lat, lng],
            note: `Позиция обновлена на карте ${formatDate(getCurrentMapDate())}.`
        });

        const movedArmy = mapBackendArmyToFrontendArmy(response);

        ensureCurrentMapData();
        pageState.currentMapData.armies = pageState.currentMapData.armies
            .map(army => army.id === armyId ? movedArmy : army);

        completeMoveMode("Позиция армии сохранена в БД и обновлена на карте.", "success");
    } catch (error) {
        console.error(error);
        setStatus("Не удалось сохранить новую позицию армии.", "danger");
    }
}

async function moveEvent(eventId, lat, lng) {
    try {
        setStatus("Сохраняю новую позицию события...", "info");

        const response = await updateEventPositionOnBackend(eventId, {
            coordinate: [lat, lng]
        });
        const movedEvent = mapBackendEventToFrontendEvent(response);

        ensureCurrentMapData();
        pageState.currentMapData.events = pageState.currentMapData.events
            .map(mapEvent => mapEvent.id === eventId
                ? movedEvent
                : mapEvent);

        completeMoveMode("Позиция события сохранена в БД и обновлена на карте.", "success");
    } catch (error) {
        console.error(error);
        setStatus("Не удалось переместить событие.", "danger");
    }
}

function completeMoveMode(message, type) {
    pageState.activeMode = null;
    pageState.movingObject = null;
    updateMapInteractionClass();
    map.getContainer().style.cursor = "";
    redrawCurrentMapData();
    updateMapActionsPanel();
    setStatus(message, type);
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

    if (!getSelectedWarSide(elements.controlZoneSideSelect)) {
        setStatus("Выберите сторону для зоны контроля.", "warning");
        return;
    }

    try {
        setStatus("Сохраняю зону контроля в базу данных...", "info");

        const request = createControlZoneSaveRequest(drawing.coordinates);

        await saveControlZoneToBackend(request);

        const newZone = {
            id: drawing.nextTempZoneId,
            title: `Новая зона контроля #${drawing.nextTempZoneId}`,
            description: `Зона создана и отправлена на backend. Дата: ${formatDate(request.dateControl)}.`,
            side: findMapSideByWarSideId(request.warSideId),
            warId: request.warId,
            warSideId: request.warSideId,
            dateControl: request.dateControl,
            precisionControl: request.precisionControl,
            coordinates: request.coordinates.map(point => [...point])
        };

        drawing.nextTempZoneId += 1;

        addControlZone(newZone);

        if (pageState.currentMapData) {
            pageState.currentMapData.controlZones.push(newZone);
            renderObjectsList(pageState.currentMapData);
        }

        await renderTemporaryMapObjects();
        clearControlZoneDraft(false);
        pageState.activeMode = null;
        pageState.controlZoneDrawing.isDrawing = false;
        updateMapInteractionClass();
        map.getContainer().style.cursor = "";
        updateMapActionsPanel();

        setStatus("Зона контроля сохранена в БД и карта обновлена.", "success");
    } catch (error) {
        console.error(error);
        setStatus("Не удалось сохранить зону контроля в базу данных.", "danger");
    }
}

function createControlZoneSaveRequest(coordinates) {
    const selectedSide = getSelectedWarSide(elements.controlZoneSideSelect);

    return {
        warId: getCurrentWarId(),
        warSideId: selectedSide.warSideId,
        dateControl: getCurrentMapDate(),
        precisionControl: DEFAULT_CONTROL_ZONE_PRECISION,
        coordinates: coordinates.map(point => [...point])
    };
}

function mapBackendControlZoneToFrontendZone(zone) {
    return {
        id: zone.id,
        title: `Зона контроля #${zone.id}`,
        description: `Дата: ${formatDate(zone.dateControl)}. Точность: ${zone.precisionControl}`,
        side: findMapSideByWarSideId(zone.warSideId),
        warId: zone.warId,
        warSideId: zone.warSideId,
        dateControl: zone.dateControl,
        precisionControl: zone.precisionControl,
        coordinates: getLeafletCoordinatesFromBackendZone(zone)
    };
}

function mapBackendArmyToFrontendArmy(army) {
    const coordinate = getLeafletCoordinateFromBackendPoint(army.coordinate ?? army.position?.coordinate);
    const side = findMapSideByWarSideId(army.warSideId) ?? {
        warSideId: army.warSideId,
        sideId: null,
        id: army.warSideId,
        title: army.sideTitle,
        colorHex: army.colorHex
    };
    const type = army.typeArmy ?? army.type ?? "";

    return {
        id: army.id,
        positionId: army.positionId,
        title: army.name ?? army.title ?? `Армия #${army.id}`,
        type,
        side,
        warSideId: army.warSideId,
        date: army.datePosition ?? army.position?.date,
        description: `Тип: ${type || "не указан"}. Сторона: ${side.title ?? army.warSideId}. Дата: ${formatDate(army.datePosition ?? army.position?.date)}.`,
        lat: coordinate?.[0],
        lng: coordinate?.[1],
        markerLetter: getArmyMarkerLetter(type)
    };
}

function mapBackendEventToFrontendEvent(mapEvent) {
    const coordinate = getLeafletCoordinateFromBackendEvent(mapEvent);
    const side = findMapSideByWarSideId(mapEvent.warSideId) ?? {
        warSideId: mapEvent.warSideId,
        sideId: null,
        id: mapEvent.warSideId,
        title: mapEvent.sideTitle,
        colorHex: mapEvent.colorHex
    };
    const type = mapEvent.type ?? "";
    const text = mapEvent.text ?? "";

    return {
        id: mapEvent.id,
        title: mapEvent.title ?? `Событие #${mapEvent.id}`,
        type,
        side,
        warId: mapEvent.warId,
        operationId: mapEvent.operationId,
        warSideId: mapEvent.warSideId,
        date: mapEvent.date,
        description: text || `Тип: ${type || "не указан"}. Сторона: ${side.title ?? mapEvent.warSideId ?? "не указана"}.`,
        lat: coordinate?.[0],
        lng: coordinate?.[1],
        markerLetter: "!"
    };
}

function getCurrentWarId() {
    return pageState.operation?.warId ?? pageState.warId ?? 1;
}

function getCurrentOperationId() {
    const operationId = Number(pageState.operationId);

    return Number.isInteger(operationId) && operationId > 0
        ? operationId
        : null;
}

function getCurrentMapDate() {
    return pageState.mapDate ?? DEFAULT_MAP_DATE;
}

function setCurrentMapDate(date, updateUrl = false) {
    const normalizedDate = normalizeDate(date) ?? DEFAULT_MAP_DATE;
    pageState.mapDate = normalizedDate;

    if (elements.mapDateInput) {
        elements.mapDateInput.value = normalizedDate;
    }

    if (updateUrl) {
        const url = new URL(window.location.href);
        url.searchParams.set("date", normalizedDate);
        window.history.replaceState({}, "", url);
    }
}

function normalizeDate(value) {
    if (!value) {
        return null;
    }

    const date = String(value).trim().slice(0, 10);
    return /^\d{4}-\d{2}-\d{2}$/.test(date) ? date : null;
}

function setMapDateFromOperation(operation) {
    if (getQueryParam("date")) {
        setCurrentMapDate(getCurrentMapDate(), false);
        return;
    }

    const operationStartDate = normalizeDate(operation?.startDate);

    if (operationStartDate) {
        setCurrentMapDate(operationStartDate, false);
    }
}

function getLeafletCoordinatesFromBackendZone(zone) {
    const geometry = zone.geom ?? zone.geometry ?? zone.geoJson;

    if (geometry) {
        return convertBackendGeometryToLeafletCoordinates(geometry);
    }

    if (Array.isArray(zone.coordinates)) {
        return normalizeBackendCoordinatesArray(zone.coordinates);
    }

    return [];
}

function getLeafletCoordinateFromBackendPoint(point) {
    const parsedPoint = typeof point === "string"
        ? tryParseJson(point)
        : point;

    if (!parsedPoint) {
        return null;
    }

    const pointObject = parsedPoint.type === "Feature"
        ? parsedPoint.geometry
        : parsedPoint;

    if (pointObject?.type !== "Point" || !Array.isArray(pointObject.coordinates)) {
        return null;
    }

    const lng = pointObject.coordinates[0];
    const lat = pointObject.coordinates[1];

    return [lat, lng];
}

function getLeafletCoordinateFromBackendEvent(mapEvent) {
    if (isValidCoordinate(mapEvent.latitude) && isValidCoordinate(mapEvent.longitude)) {
        return [Number(mapEvent.latitude), Number(mapEvent.longitude)];
    }

    return getLeafletCoordinateFromBackendPoint(mapEvent.coordinate);
}

function convertBackendGeometryToLeafletCoordinates(geometry) {
    const parsedGeometry = typeof geometry === "string"
        ? tryParseJson(geometry)
        : geometry;

    if (!parsedGeometry) {
        return [];
    }

    const geometryObject = parsedGeometry.type === "Feature"
        ? parsedGeometry.geometry
        : parsedGeometry;

    if (geometryObject?.type === "Polygon") {
        return convertGeoJsonPolygonToLeafletCoordinates(geometryObject);
    }

    if (geometryObject?.type === "MultiPolygon") {
        return convertGeoJsonMultiPolygonToLeafletCoordinates(geometryObject);
    }

    return [];
}

function normalizeBackendCoordinatesArray(coordinates) {
    // Вариант 1: backend вернул уже Leaflet-координаты:
    // [[lat, lng], [lat, lng], ...]
    if (isLeafletPointArray(coordinates)) {
        return coordinates;
    }

    // Вариант 2: backend вернул GeoJSON Polygon coordinates:
    // [[[lng, lat], [lng, lat], ...]]
    if (Array.isArray(coordinates[0]) && isGeoJsonPointArray(coordinates[0])) {
        return coordinates[0].map(([lng, lat]) => [lat, lng]);
    }

    // Вариант 3: backend вернул GeoJSON MultiPolygon coordinates:
    // [[[[lng, lat], [lng, lat], ...]]]
    if (
        Array.isArray(coordinates[0]) &&
        Array.isArray(coordinates[0][0]) &&
        isGeoJsonPointArray(coordinates[0][0])
    ) {
        return coordinates[0][0].map(([lng, lat]) => [lat, lng]);
    }

    return [];
}

function isLeafletPointArray(value) {
    return Array.isArray(value)
        && value.length > 0
        && Array.isArray(value[0])
        && typeof value[0][0] === "number"
        && typeof value[0][1] === "number";
}

function isGeoJsonPointArray(value) {
    return Array.isArray(value)
        && value.length > 0
        && Array.isArray(value[0])
        && typeof value[0][0] === "number"
        && typeof value[0][1] === "number";
}

function convertGeoJsonPolygonToLeafletCoordinates(geometry) {
    const exteriorRing = geometry.coordinates?.[0] ?? [];

    return exteriorRing.map(function (position) {
        const lng = position[0];
        const lat = position[1];
        return [lat, lng];
    });
}

function tryParseJson(value) {
    try {
        return JSON.parse(value);
    } catch {
        return null;
    }
}

function isValidControlZone(zone) {
    return Array.isArray(zone.coordinates) && zone.coordinates.length > 0;
}

function isValidArmy(army) {
    return isValidCoordinate(army.lat) && isValidCoordinate(army.lng);
}

function isValidEvent(mapEvent) {
    return isValidCoordinate(mapEvent.lat) && isValidCoordinate(mapEvent.lng);
}

function preparePointObjectForm(event, mode) {
    const lat = Number(event.latlng.lat.toFixed(6));
    const lng = Number(event.latlng.lng.toFixed(6));

    pageState.pendingPoint = { lat, lng, mode };

    elements.mapObjectForm.hidden = false;
    elements.controlZoneModeControls.hidden = true;
    elements.mapObjectForm.reset();
    elements.mapObjectSideSelect.value = "";
    elements.mapObjectCoordinatePreview.textContent = `Координата: ${lat}, ${lng}`;

    if (mode === MAP_ACTION_MODES.ARMY) {
        elements.mapObjectTitleLabel.textContent = "Название армии";
        elements.mapObjectTitleInput.placeholder = "Например: 5-я гвардейская армия";
        elements.mapObjectTypeGroup.hidden = false;
        elements.mapObjectTypeLabel.textContent = "Тип армии";
        elements.mapObjectTypeInput.placeholder = "армия, корпус";
        elements.mapObjectDescriptionGroup.hidden = true;
        elements.saveMapObjectBtn.textContent = "Сохранить армию";
        setStatus("Заполните данные армии. Позиция будет связана с выбранной датой карты.", "info");
    }

    if (mode === MAP_ACTION_MODES.EVENT) {
        elements.mapObjectTitleLabel.textContent = "Название события";
        elements.mapObjectTitleInput.placeholder = "Например: Начало наступления";
        elements.mapObjectTypeGroup.hidden = false;
        elements.mapObjectTypeLabel.textContent = "Тип события";
        elements.mapObjectTypeInput.placeholder = "бой, наступление, перегруппировка";
        elements.mapObjectDescriptionGroup.hidden = false;
        elements.saveMapObjectBtn.textContent = "Сохранить событие";
        setStatus("Заполните данные события и выберите сторону операции.", "info");
    }

    updateMapActionsPanel();
}

function clearControlZoneDraft(updatePanel = true) {
    const drawing = pageState.controlZoneDrawing;

    drawing.coordinates = [];
    drawing.pointMarkers = [];
    drawing.draftLine = null;
    drawing.draftPolygon = null;

    mapLayers.controlZoneDraft.clearLayers();

    if (updatePanel) {
        pageState.activeMode = null;
        pageState.controlZoneDrawing.isDrawing = false;
        updateMapInteractionClass();
        map.getContainer().style.cursor = "";
        setStatus("Рисование зоны контроля отменено.", "secondary");
        updateMapActionsPanel();
    }
}

function updateMapActionsPanel() {
    const drawing = pageState.controlZoneDrawing;

    if (!elements.actionsPanel) {
        return;
    }

    const hasActiveMode = Boolean(pageState.activeMode);
    elements.actionModePanel.hidden = !hasActiveMode;
    elements.cancelMapActionBtn.hidden = !hasActiveMode;
    elements.controlZoneModeControls.hidden = pageState.activeMode !== MAP_ACTION_MODES.CONTROL_ZONE;
    elements.editControlZoneModeControls.hidden = pageState.activeMode !== MAP_ACTION_MODES.EDIT_CONTROL_ZONE;
    elements.finishDrawingBtn.disabled = drawing.coordinates.length < 3;
    elements.clearDraftBtn.disabled = drawing.coordinates.length === 0;

    elements.addControlZoneModeBtn.classList.toggle("is-active", pageState.activeMode === MAP_ACTION_MODES.CONTROL_ZONE);
    elements.addArmyModeBtn.classList.toggle("is-active", pageState.activeMode === MAP_ACTION_MODES.ARMY);
    elements.addEventModeBtn.classList.toggle("is-active", pageState.activeMode === MAP_ACTION_MODES.EVENT);

    if (pageState.activeMode === MAP_ACTION_MODES.EDIT_CONTROL_ZONE) {
        const zoneDate = normalizeDate(pageState.controlZoneEditing.zone?.dateControl);
        const isNewDate = zoneDate !== getCurrentMapDate();

        elements.actionHint.textContent = "Переместите точки зоны контроля. Нажмите «Готово», чтобы сохранить изменения.";
        elements.editControlZoneWarning.hidden = !isNewDate;
        elements.editControlZoneWarning.textContent = isNewDate
            ? "Изменения будут сохранены как новая зона контроля на выбранную дату. Исходная зона останется без изменений."
            : "";
        elements.mapObjectForm.hidden = true;
        return;
    }

    if (pageState.activeMode === MAP_ACTION_MODES.CONTROL_ZONE) {
        elements.actionHint.textContent = `Выберите точки зоны контроля. Точек: ${drawing.coordinates.length}.`;
        elements.mapObjectForm.hidden = true;
        return;
    }

    if (pageState.activeMode === MAP_ACTION_MODES.ARMY) {
        elements.actionHint.textContent = pageState.pendingPoint
            ? "Заполните данные армии."
            : "Выберите точку для размещения армии.";
        return;
    }

    if (pageState.activeMode === MAP_ACTION_MODES.EVENT) {
        elements.actionHint.textContent = pageState.pendingPoint
            ? "Заполните данные события."
            : "Выберите точку для события.";
        return;
    }

    if (pageState.activeMode === MAP_ACTION_MODES.MOVE_ARMY) {
        elements.actionHint.textContent = "Выберите новую позицию армии на карте.";
        elements.mapObjectForm.hidden = true;
        return;
    }

    if (pageState.activeMode === MAP_ACTION_MODES.MOVE_EVENT) {
        elements.actionHint.textContent = "Выберите новую позицию события на карте.";
        elements.mapObjectForm.hidden = true;
    }
}

function hideMapObjectForm() {
    if (!elements.mapObjectForm) {
        return;
    }

    elements.mapObjectForm.hidden = true;
    elements.mapObjectForm.reset();
    elements.mapObjectCoordinatePreview.textContent = "";
}

function clearPendingPointForm() {
    pageState.pendingPoint = null;
    hideMapObjectForm();
    updateMapActionsPanel();

    if (pageState.activeMode === MAP_ACTION_MODES.ARMY) {
        setStatus("Выберите точку для размещения армии.", "info");
    }

    if (pageState.activeMode === MAP_ACTION_MODES.EVENT) {
        setStatus("Выберите точку для события.", "info");
    }
}

async function saveMapObjectDraft(event) {
    event.preventDefault();

    if (!pageState.pendingPoint) {
        setStatus("Сначала выберите точку на карте.", "warning");
        return;
    }

    if (pageState.pendingPoint.mode === MAP_ACTION_MODES.ARMY) {
        await saveArmyDraft();
        return;
    }

    if (pageState.pendingPoint.mode === MAP_ACTION_MODES.EVENT) {
        await saveEventDraft();
    }
}

async function saveArmyDraft() {
    ensureCurrentMapData();

    const selectedSide = getSelectedWarSide(elements.mapObjectSideSelect);
    if (!selectedSide) {
        setStatus("Выберите сторону армии.", "warning");
        return;
    }

    const type = elements.mapObjectTypeInput.value.trim() || "армия";
    const title = elements.mapObjectTitleInput.value.trim() || `Армия #${pageState.nextTempArmyId}`;

    try {
        setStatus("Сохраняю армию и позицию в базу данных...", "info");

        const response = await saveArmyToBackend({
            warSideId: selectedSide.warSideId,
            name: title,
            typeArmy: type,
            summary: null,
            datePosition: getCurrentMapDate(),
            coordinate: [pageState.pendingPoint.lat, pageState.pendingPoint.lng],
            positionNote: `Позиция создана на карте ${formatDate(getCurrentMapDate())}.`
        });

        const army = mapBackendArmyToFrontendArmy(response);

        pageState.currentMapData.armies.push(army);
        addArmyMarker(army);
        renderObjectsList(pageState.currentMapData);
        completePointObjectMode("Армия сохранена в БД и добавлена на карту.");
    } catch (error) {
        console.error(error);
        setStatus("Не удалось сохранить армию в БД.", "danger");
    }
}

async function saveEventDraft() {
    ensureCurrentMapData();

    const selectedSide = getSelectedWarSide(elements.mapObjectSideSelect);
    if (!selectedSide) {
        setStatus("Выберите сторону события.", "warning");
        return;
    }

    const title = elements.mapObjectTitleInput.value.trim() || `Событие #${pageState.nextTempEventId}`;
    const type = elements.mapObjectTypeInput.value.trim();
    const text = elements.mapObjectDescriptionInput.value.trim();

    try {
        setStatus("Сохраняю событие в базу данных...", "info");

        const response = await saveEventToBackend({
            warId: getCurrentWarId(),
            operationId: getCurrentOperationId(),
            warSideId: selectedSide.warSideId,
            title,
            text: text || null,
            type: type || null,
            date: getCurrentMapDate(),
            coordinate: [pageState.pendingPoint.lat, pageState.pendingPoint.lng]
        });

        const mapEvent = mapBackendEventToFrontendEvent(response);

        pageState.currentMapData.events.push(mapEvent);
        addEventMarker(mapEvent);
        renderObjectsList(pageState.currentMapData);
        completePointObjectMode("Событие сохранено в БД и добавлено на карту.");
    } catch (error) {
        console.error(error);
        setStatus("Не удалось сохранить событие в БД.", "danger");
    }
}

function completePointObjectMode(message) {
    pageState.pendingPoint = null;
    pageState.activeMode = null;
    updateMapInteractionClass();
    map.getContainer().style.cursor = "";
    hideMapObjectForm();
    updateMapActionsPanel();
    setStatus(message, "success");
}

function ensureCurrentMapData() {
    if (pageState.currentMapData) {
        return;
    }

    pageState.currentMapData = {
        events: [],
        armies: [],
        controlZones: []
    };
}

function getArmyMarkerLetter(type) {
    const normalizedType = String(type ?? "").trim().toLowerCase();

    if (normalizedType.includes("корпус")) {
        return "К";
    }

    if (normalizedType.includes("арм")) {
        return "А";
    }

    return "В";
}

async function loadOperation(operationId) {
    setStatus("Загружаю данные операции...", "info");

    try {
        const operation = await fetchOperation(operationId);
        pageState.operation = operation;

        renderOperationInfo(operation);
        setMapDateFromOperation(operation);
        setMapCenterFromOperation(operation);

        setStatus("Данные операции загружены. Загружаю объекты карты...", "success");
    } catch (error) {
        console.error(error);
        renderOperationInfo(null);

        setStatus("Не удалось загрузить операцию через API. Карта всё равно открыта.", "warning");
    }
}

async function loadMapSides() {
    try {
        const sides = getCurrentOperationId()
            ? await fetchOperationSides(getCurrentOperationId())
            : await fetchWarSides(getCurrentWarId());

        pageState.operationSides = Array.isArray(sides)
            ? sides
                .map(normalizeMapSide)
                .filter(side => Number.isInteger(side.warSideId) && side.warSideId > 0)
            : [];

        if (pageState.operationSides.length === 0) {
            setStatus("Для выбранной операции стороны не указаны.", "secondary");
        }
    } catch (error) {
        console.error("Не удалось загрузить стороны карты:", error);
        pageState.operationSides = [];
        setStatus("Не удалось загрузить стороны операции. Добавление объектов временно недоступно.", "warning");
    }

    fillSideSelect(elements.controlZoneSideSelect, pageState.operationSides);
    fillSideSelect(elements.mapObjectSideSelect, pageState.operationSides);
}

function normalizeMapSide(side) {
    return {
        warSideId: Number(side.warSideId),
        sideId: Number(side.sideId),
        title: side.title ?? side.name ?? side.sideTitle ?? "Без названия",
        colorHex: side.colorHex ?? null
    };
}

function getSelectedWarSide(select) {
    const warSideId = Number(select?.value);
    return findMapSideByWarSideId(warSideId);
}

function findMapSideByWarSideId(warSideId) {
    const normalizedId = Number(warSideId);
    return pageState.operationSides.find(side => side.warSideId === normalizedId) ?? null;
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
    mapData.events = [];

    try {
        const warId = getCurrentWarId();
        const date = getCurrentMapDate();

        const backendZones = await fetchControlZonesByWarAndDate(warId, date);

        if (!Array.isArray(backendZones)) {
            throw new Error("API зон контроля вернул не массив.");
        }

        const zones = backendZones
            .map(mapBackendControlZoneToFrontendZone)
            .filter(isValidControlZone);

        mapData.controlZones = zones;

        if (zones.length === 0) {
            setStatus(
                `Данные из БД загружены, но на ${formatDate(date)} зон контроля нет. Выбери другую дату или создай новую зону.`,
                "secondary"
            );
        } else {
            const fallbackZones = zones.filter(zone => normalizeDate(zone.dateControl) !== date);
            const fallbackSideIds = new Set(fallbackZones.map(zone => zone.warSideId));
            const fallbackDates = [...new Set(fallbackZones
                .map(zone => normalizeDate(zone.dateControl))
                .filter(Boolean))]
                .sort()
                .map(formatDate);

            setStatus(fallbackZones.length
                ? `Зоны контроля загружены: ${zones.length}. Для сторон с более ранними снимками: ${fallbackSideIds.size}; даты: ${fallbackDates.join(", ")}.`
                : `Зоны контроля из БД загружены: ${zones.length}. Дата: ${formatDate(date)}.`,
            fallbackZones.length ? "info" : "success");
        }
    } catch (error) {
        console.error("Не удалось загрузить зоны контроля из БД:", error);

        mapData.controlZones = [];

        setStatus(
            "Не удалось получить зоны контроля из БД. Проверь API, параметры warId/date и наличие данных в таблице ControlZones.",
            "warning"
        );
    }

    try {
        const warId = getCurrentWarId();
        const date = getCurrentMapDate();

        const backendArmies = await fetchArmiesByWarAndDate(warId, date);

        if (!Array.isArray(backendArmies)) {
            throw new Error("API армий вернул не массив.");
        }

        mapData.armies = backendArmies
            .map(mapBackendArmyToFrontendArmy)
            .filter(isValidArmy);
    } catch (error) {
        console.error("Не удалось загрузить армии из БД:", error);

        mapData.armies = [];

        setStatus(
            "Зоны контроля обработаны, но армии из БД не загрузились. Проверь API армий и параметры warId/date.",
            "warning"
        );
    }

    try {
        const warId = getCurrentWarId();
        const date = getCurrentMapDate();
        const operationId = getCurrentOperationId();

        const backendEvents = await fetchEventsByWarAndDate(warId, date, operationId);

        if (!Array.isArray(backendEvents)) {
            throw new Error("API событий вернул не массив.");
        }

        mapData.events = backendEvents
            .map(mapBackendEventToFrontendEvent)
            .filter(isValidEvent);

        if (mapData.events.length === 0) {
            setStatus(
                `На ${formatDate(date)} события отсутствуют.`,
                "secondary"
            );
        }
    } catch (error) {
        console.error("Не удалось загрузить события из БД:", error);

        mapData.events = [];

        setStatus(
            "Зоны контроля и армии обработаны, но события из БД не загрузились. Проверь API событий и параметры warId/date/operationId.",
            "warning"
        );
    }

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
    mapLayers.controlZoneEdit.clearLayers();

    pageState.controlZoneDrawing.coordinates = [];
    pageState.controlZoneDrawing.pointMarkers = [];
    pageState.controlZoneDrawing.draftLine = null;
    pageState.controlZoneDrawing.draftPolygon = null;

    updateMapActionsPanel();
}

function addEventMarker(event) {
    const marker = L.marker([event.lat, event.lng], {
        icon: createEventIcon(event)
    })
        .bindPopup(createEventPopupHtml(event));

    marker.addTo(mapLayers.events);
    pageState.featureLayers.push(marker);
}

function createEventPopupHtml(event) {
    return `
        <strong>${escapeHtml(event.title)}</strong><br>
        <span>${escapeHtml(formatDate(event.date))}</span><br>
        <span>${escapeHtml(event.description)}</span>
        ${createObjectActionButtonsHtml("event", event.id, true)}
    `;
}

function createArmyPopupHtml(army) {
    return `
        <strong>${escapeHtml(army.title)}</strong><br>
        <span>${escapeHtml(army.description)}</span>
        ${createObjectActionButtonsHtml("army", army.id, true)}
    `;
}

function createControlZonePopupHtml(zone) {
    return `
        <strong>${escapeHtml(zone.title)}</strong><br>
        <span>${escapeHtml(zone.description)}</span><br>
        <span>Количество точек: ${zone.coordinates.length}</span>
        ${createObjectActionButtonsHtml("controlZone", zone.id, false, true)}
    `;
}

function createObjectActionButtonsHtml(objectType, objectId, canMove, canEdit = false) {
    return `
        <div class="d-flex gap-2 mt-2">
            ${canMove ? createMoveButtonHtml(objectType, objectId) : ""}
            ${canEdit ? `
                <button class="btn btn-sm btn-outline-primary"
                        type="button"
                        data-edit-control-zone
                        data-object-id="${escapeHtml(String(objectId))}">
                    Изменить
                </button>` : ""}
            <button class="btn btn-sm btn-outline-danger"
                    type="button"
                    data-delete-map-object
                    data-object-type="${escapeHtml(objectType)}"
                    data-object-id="${escapeHtml(String(objectId))}">
                Удалить
            </button>
        </div>
    `;
}

function createMoveButtonHtml(objectType, objectId) {
    return `
        <button class="btn btn-sm btn-outline-primary"
                type="button"
                data-move-map-object
                data-object-type="${escapeHtml(objectType)}"
                data-object-id="${escapeHtml(String(objectId))}">
            Переместить
        </button>
    `;
}

function addArmyMarker(army) {
    const marker = L.marker([army.lat, army.lng], {
        icon: createArmyIcon(army)
    })
        .bindPopup(createArmyPopupHtml(army));

    marker.addTo(mapLayers.armies);
    pageState.featureLayers.push(marker);
}

function addControlZone(zone) {
    const style = getControlZoneStyle(zone.side);

    const polygon = L.polygon(zone.coordinates, style)
        .bindPopup(createControlZonePopupHtml(zone));

    polygon.addTo(mapLayers.controlZones);
    pageState.featureLayers.push(polygon);
}

function getControlZoneStyle(side) {
    const color = getSideColor(resolveSideValue(side));

    return {
        color,
        fillColor: color,
        fillOpacity: 0.25,
        opacity: 0.8,
        weight: 2
    };
}

function createArmyIcon(army) {
    const color = getSideColor(resolveSideValue(army.side ?? army.warSideId));
    const letter = army.markerLetter ?? getArmyMarkerLetter(army.type ?? army.title);

    return L.divIcon({
        className: "",
        html: `<div class="army-marker" style="border-color: ${color}; color: ${color};">${escapeHtml(letter)}</div>`,
        iconSize: [34, 34],
        iconAnchor: [17, 17],
        popupAnchor: [0, -17]
    });
}

function createEventIcon(event) {
    const color = getSideColor(resolveSideValue(event.side ?? event.warSideId));

    return L.divIcon({
        className: "",
        html: `<div class="event-marker" style="background: ${color};">${escapeHtml(event.markerLetter ?? "!")}</div>`,
        iconSize: [28, 28],
        iconAnchor: [14, 14],
        popupAnchor: [0, -14]
    });
}

function resolveSideValue(side) {
    if (typeof side === "number") {
        return findMapSideByWarSideId(side) ?? { warSideId: side };
    }

    if (typeof side === "string" && /^\d+$/.test(side)) {
        const warSideId = Number(side);
        return findMapSideByWarSideId(warSideId) ?? { warSideId };
    }

    if (side && typeof side === "object") {
        const mapSide = findMapSideByWarSideId(side.warSideId ?? side.id);
        return mapSide ? { ...side, ...mapSide } : side;
    }

    return side;
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
