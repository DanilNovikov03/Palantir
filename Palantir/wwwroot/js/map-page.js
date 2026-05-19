// map-page.js
// Страница карты открывается так: /map.html?operationId=1
// Пока Event и ControlZones не реализованы, карта показывает временные слои-заглушки.

(function () {
    const DEFAULT_CENTER = [51.75, 36.19];
    const DEFAULT_ZOOM = 7;

    const pageState = {
        operationId: getQueryParam("operationId"),
        operation: null,
        featureLayers: []
    };

    const elements = {
        backToOperationBtn: document.getElementById("backToOperationBtn"),
        fitMapBtn: document.getElementById("fitMapBtn"),
        operationSubtitle: document.getElementById("operationSubtitle"),
        operationInfo: document.getElementById("operationInfo"),
        statusMessage: document.getElementById("statusMessage"),
        objectsList: document.getElementById("objectsList"),
        eventsLayerToggle: document.getElementById("eventsLayerToggle"),
        controlZonesLayerToggle: document.getElementById("controlZonesLayerToggle"),
        armiesLayerToggle: document.getElementById("armiesLayerToggle"),
        frontLinesLayerToggle: document.getElementById("frontLinesLayerToggle")
    };

    const map = L.map("map", {
        zoomControl: true,
        attributionControl: true
    }).setView(DEFAULT_CENTER, DEFAULT_ZOOM);

    map.attributionControl.setPrefix(false);

    const yandexTilesApiKey = window.PALANTIR_MAP_CONFIG?.YANDEX_TILES_API_KEY;

    if (!yandexTilesApiKey) {
        console.error("Не указан Yandex Tiles API key. Проверь файл js/map-config.local.js");
    }

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
        frontLines: L.layerGroup().addTo(map)
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

        if (!pageState.operationId) {
            setStatus("Не передан operationId. Открой страницу так: /map.html?operationId=1", "warning");
            renderOperationInfo(null);
            renderTemporaryMapObjects();
            return;
        }

        await loadOperation(pageState.operationId);
        renderTemporaryMapObjects();
    }

    function setupNavigation() {
        if (!pageState.operationId) {
            elements.backToOperationBtn.href = "/operations.html";
            return;
        }

        elements.backToOperationBtn.href = `/operation.html?operationId=${encodeURIComponent(pageState.operationId)}`;
    }

    function setupLayerToggles() {
        bindLayerToggle(elements.eventsLayerToggle, mapLayers.events);
        bindLayerToggle(elements.controlZonesLayerToggle, mapLayers.controlZones);
        bindLayerToggle(elements.armiesLayerToggle, mapLayers.armies);
        bindLayerToggle(elements.frontLinesLayerToggle, mapLayers.frontLines);
    }

    function setupFitButton() {
        elements.fitMapBtn.addEventListener("click", fitMapToObjects);
    }

    function bindLayerToggle(checkbox, layer) {
        checkbox.addEventListener("change", function () {
            if (checkbox.checked) {
                layer.addTo(map);
            } else {
                map.removeLayer(layer);
            }
        });
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

    async function fetchOperation(operationId) {
        // Если у тебя в контроллере другой маршрут, поменяй список ниже.
        // Чаще всего для OperationsController подходит /api/Operations/{id}.
        const urls = [
            `/api/Operations/${encodeURIComponent(operationId)}`,
            `/api/operations/${encodeURIComponent(operationId)}`,
            `/api/Operation/${encodeURIComponent(operationId)}`
        ];

        let lastError = null;

        for (const url of urls) {
            try {
                const response = await fetch(url);

                if (!response.ok) {
                    lastError = new Error(`Ошибка ${response.status} при запросе ${url}`);
                    continue;
                }

                return await response.json();
            } catch (error) {
                lastError = error;
            }
        }

        throw lastError ?? new Error("Не удалось загрузить операцию.");
    }

    function renderOperationInfo(operation) {
        if (!operation) {
            elements.operationSubtitle.textContent = pageState.operationId
                ? `Операция #${pageState.operationId}`
                : "Операция не выбрана";

            elements.operationInfo.innerHTML = `
                <div><strong>ID операции:</strong> ${escapeHtml(pageState.operationId ?? "не указан")}</div>
                <div class="mt-2">Данные операции не были получены из API.</div>
            `;
            return;
        }

        const title = operation.title
            ?? operation.name
            ?? operation.operationName
            ?? `Операция #${pageState.operationId}`;

        const summary = operation.summary
            ?? operation.description
            ?? operation.note
            ?? "Описание операции пока не указано.";

        const startDate = operation.startDate
            ?? operation.dateStart
            ?? operation.beginDate
            ?? operation.startedAt
            ?? null;

        const endDate = operation.endDate
            ?? operation.dateEnd
            ?? operation.finishDate
            ?? operation.finishedAt
            ?? null;

        elements.operationSubtitle.textContent = title;

        elements.operationInfo.innerHTML = `
            <div><strong>ID операции:</strong> ${escapeHtml(pageState.operationId)}</div>
            <div><strong>Название:</strong> ${escapeHtml(title)}</div>
            <div><strong>Дата начала:</strong> ${escapeHtml(formatDate(startDate))}</div>
            <div><strong>Дата окончания:</strong> ${escapeHtml(formatDate(endDate))}</div>
            <div class="mt-2"><strong>Описание:</strong><br>${escapeHtml(summary)}</div>
        `;
    }

    function setMapCenterFromOperation(operation) {
        const lat = operation.centerLat ?? operation.latitude ?? operation.lat;
        const lng = operation.centerLng ?? operation.longitude ?? operation.lng;

        if (isValidCoordinate(lat) && isValidCoordinate(lng)) {
            map.setView([Number(lat), Number(lng)], operation.zoom ?? DEFAULT_ZOOM);
        }
    }

    function renderTemporaryMapObjects() {
        clearMapLayers();

        const demoData = createTemporaryMapData();

        demoData.controlZones.forEach(addControlZone);
        demoData.frontLines.forEach(addFrontLine);
        demoData.armies.forEach(addArmyMarker);
        demoData.events.forEach(addEventMarker);

        renderObjectsList(demoData);
        fitMapToObjects();
    }

    function clearMapLayers() {
        pageState.featureLayers = [];

        Object.values(mapLayers).forEach(layer => {
            layer.clearLayers();
        });
    }

    function createTemporaryMapData() {
        return {
            events: [
                {
                    id: 1,
                    title: "Начало операции",
                    description: "Временная точка события. Позже здесь будет запись из модели Event.",
                    lat: 51.73,
                    lng: 36.19,
                    date: "1943-07-05"
                },
                {
                    id: 2,
                    title: "Ключевой район боевых действий",
                    description: "Демонстрационная точка для проверки pop-up окна и списка объектов.",
                    lat: 51.49,
                    lng: 36.09,
                    date: "1943-07-12"
                }
            ],
            armies: [
                {
                    id: 1,
                    title: "Соединение A",
                    description: "Временная позиция войскового соединения.",
                    lat: 51.84,
                    lng: 35.95
                },
                {
                    id: 2,
                    title: "Соединение B",
                    description: "Временная позиция войскового соединения.",
                    lat: 51.41,
                    lng: 36.39
                }
            ],
            frontLines: [
                {
                    id: 1,
                    title: "Демонстрационная линия фронта",
                    coordinates: [
                        [51.95, 35.80],
                        [51.80, 36.00],
                        [51.62, 36.18],
                        [51.42, 36.35],
                        [51.25, 36.55]
                    ]
                }
            ],
            controlZones: [
                {
                    id: 1,
                    title: "Зона контроля стороны A",
                    description: "Временный полигон. Позже будет загружаться из ControlZones.",
                    side: "A",
                    coordinates: [
                        [52.00, 35.70],
                        [51.95, 36.20],
                        [51.65, 36.12],
                        [51.68, 35.65]
                    ]
                },
                {
                    id: 2,
                    title: "Зона контроля стороны B",
                    description: "Временный полигон. Позже будет загружаться из ControlZones.",
                    side: "B",
                    coordinates: [
                        [51.58, 36.22],
                        [51.35, 36.38],
                        [51.18, 36.70],
                        [51.47, 36.85],
                        [51.70, 36.45]
                    ]
                }
            ]
        };
    }

    function addEventMarker(event) {
        const marker = L.marker([event.lat, event.lng], { icon: eventIcon })
            .bindPopup(`
                <strong>${escapeHtml(event.title)}</strong><br>
                <span>${escapeHtml(formatDate(event.date))}</span><br>
                <span>${escapeHtml(event.description)}</span>
            `);

        marker.addTo(mapLayers.events);
        pageState.featureLayers.push(marker);
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

    function addFrontLine(frontLine) {
        const line = L.polyline(frontLine.coordinates, {
            color: "#212529",
            weight: 4,
            dashArray: "8 6"
        }).bindPopup(`<strong>${escapeHtml(frontLine.title)}</strong>`);

        line.addTo(mapLayers.frontLines);
        pageState.featureLayers.push(line);
    }

    function addControlZone(zone) {
        const style = zone.side === "A"
            ? { color: "#0d6efd", fillColor: "#0d6efd", fillOpacity: 0.18, weight: 2 }
            : { color: "#dc3545", fillColor: "#dc3545", fillOpacity: 0.18, weight: 2 };

        const polygon = L.polygon(zone.coordinates, style)
            .bindPopup(`
                <strong>${escapeHtml(zone.title)}</strong><br>
                <span>${escapeHtml(zone.description)}</span>
            `);

        polygon.addTo(mapLayers.controlZones);
        pageState.featureLayers.push(polygon);
    }

    function renderObjectsList(demoData) {
        const items = [];

        demoData.events.forEach(event => {
            items.push({
                type: "Событие",
                title: event.title,
                coordinates: [event.lat, event.lng]
            });
        });

        demoData.armies.forEach(army => {
            items.push({
                type: "Соединение",
                title: army.title,
                coordinates: [army.lat, army.lng]
            });
        });

        if (items.length === 0) {
            elements.objectsList.innerHTML = `<div class="text-secondary">Объектов пока нет.</div>`;
            return;
        }

        elements.objectsList.innerHTML = "";

        items.forEach(item => {
            const button = document.createElement("button");
            button.type = "button";
            button.className = "list-group-item list-group-item-action object-list-item";
            button.innerHTML = `
                <div class="fw-semibold">${escapeHtml(item.title)}</div>
                <div class="text-secondary">${escapeHtml(item.type)}</div>
            `;

            button.addEventListener("click", function () {
                map.setView(item.coordinates, 10);
            });

            elements.objectsList.appendChild(button);
        });
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

    function getQueryParam(name) {
        return new URLSearchParams(window.location.search).get(name);
    }

    function formatDate(value) {
        if (!value) {
            return "не указана";
        }

        const date = new Date(value);

        if (Number.isNaN(date.getTime())) {
            return String(value);
        }

        return date.toLocaleDateString("ru-RU");
    }

    function isValidCoordinate(value) {
        return value !== null && value !== undefined && value !== "" && !Number.isNaN(Number(value));
    }

    function escapeHtml(value) {
        return String(value)
            .replaceAll("&", "&amp;")
            .replaceAll("<", "&lt;")
            .replaceAll(">", "&gt;")
            .replaceAll('"', "&quot;")
            .replaceAll("'", "&#039;");
    }
})();
