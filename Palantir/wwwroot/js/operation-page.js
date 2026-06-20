document.addEventListener("DOMContentLoaded", async () => {
    const params = new URLSearchParams(window.location.search);

    const warId = params.get("warId");
    const theaterId = params.get("theaterId");
    const operationId = params.get("operationId");

    const backToTheaterButton = document.getElementById("backToTheaterButton");
    const mapButton = document.getElementById("mapButton");

    const loadingMessage = document.getElementById("loadingMessage");
    const operationDetails = document.getElementById("operationDetails");

    if (warId && theaterId) {
        backToTheaterButton.href = `/theater.html?warId=${warId}&theaterId=${theaterId}`;
    }

    if (operationId) {
        mapButton.href = `/map.html?operationId=${operationId}`;
    }

    if (!operationId) {
        showError("Не передан идентификатор операции.");
        return;
    }

    loadOperationSides(operationId);

    try {
        const operation = await getJson(`${API_BASE_URL}/operation/${operationId}`);

        renderOperation(operation);

        const events = await loadOperationEvents(operationId);
        renderEvents(events);

        loadingMessage.classList.add("d-none");
        operationDetails.classList.remove("d-none");
    } catch (error) {
        showError("Не удалось загрузить данные операции.");
        console.error(error);
    }
});

async function loadOperationSides(operationId) {
    const container = document.getElementById("operationSidesList");

    try {
        const sides = await getJson(`${API_BASE_URL}/operations/${operationId}/sides`);

        container.innerHTML = sides.length
            ? sides.map(side => `<span class="badge text-bg-secondary">${escapeHtml(side.title)}</span>`).join("")
            : '<span class="text-muted">Стороны операции не указаны.</span>';
    } catch (error) {
        container.innerHTML = `<span class="text-danger">${escapeHtml(error.message)}</span>`;
    }
}

function renderOperation(operation) {
    const title = getFirstValue(operation, ["title", "name"], "Операция без названия");

    const description = getFirstValue(
        operation,
        ["summary", "description", "note"],
        "Описание операции отсутствует."
    );

    const goal = getFirstValue(operation, ["goal", "objective", "purpose"], null);
    const result = getFirstValue(operation, ["result", "outcome", "conclusion"], null);

    document.getElementById("operationTitle").textContent = title;
    document.getElementById("operationDescription").textContent = description;

    document.getElementById("operationDates").textContent = formatDates(
        operation.startDate,
        operation.endDate
    );

    setOptionalBlock("operationGoalBlock", "operationGoal", goal);
    setOptionalBlock("operationResultBlock", "operationResult", result);
}

async function loadOperationEvents(operationId) {
    const response = await getJson(`${API_BASE_URL}/events/by-operation/${operationId}`);

    const events = normalizeEventsResponse(response);

    return events
        .map(mapBackendEventToOperationEvent)
        .sort(compareEventsByDate);
}

function normalizeEventsResponse(response) {
    if (Array.isArray(response)) {
        return response;
    }

    if (Array.isArray(response?.items)) {
        return response.items;
    }

    if (Array.isArray(response?.events)) {
        return response.events;
    }

    return [];
}

function mapBackendEventToOperationEvent(event) {
    return {
        id: event.id,
        title: getFirstValue(event, ["title", "name"], `Событие #${event.id ?? ""}`.trim()),
        description: getFirstValue(
            event,
            ["text", "description", "summary", "note"],
            "Описание отсутствует."
        ),
        date: getFirstValue(event, ["date", "eventDate", "startDate", "dateEvent"], null),
        type: getFirstValue(event, ["type", "eventType"], null),
        sideTitle: getFirstValue(event, ["sideTitle", "warSideTitle", "sideName"], null)
    };
}

function compareEventsByDate(firstEvent, secondEvent) {
    const firstTime = firstEvent.date ? new Date(firstEvent.date).getTime() : Number.MAX_SAFE_INTEGER;
    const secondTime = secondEvent.date ? new Date(secondEvent.date).getTime() : Number.MAX_SAFE_INTEGER;

    if (firstTime !== secondTime) {
        return firstTime - secondTime;
    }

    const idDifference = Number(firstEvent.id ?? 0) - Number(secondEvent.id ?? 0);
    return idDifference || firstEvent.title.localeCompare(secondEvent.title, "ru");
}

function renderEvents(events) {
    const eventsList = document.getElementById("eventsList");

    eventsList.innerHTML = "";

    if (!events || events.length === 0) {
        eventsList.innerHTML = `
            <div class="alert alert-secondary">
                Для этой операции пока не добавлены события
            </div>
        `;
        return;
    }

    events.forEach(event => {
        const eventTitle = event.title || "Событие без названия";
        const eventDate = event.date;
        const details = [];

        if (event.sideTitle) details.push(`<div><strong>Сторона:</strong> ${escapeHtml(event.sideTitle)}</div>`);
        if (event.type) details.push(`<div><strong>Тип:</strong> ${escapeHtml(event.type)}</div>`);
        if (event.description && event.description !== "Описание отсутствует.") {
            details.push(`<div><strong>Описание:</strong> ${escapeHtml(event.description)}</div>`);
        }

        const item = document.createElement("div");
        item.className = "card mb-3";

        item.innerHTML = `
            <div class="card-body">
                <h3 class="h5 card-title">${escapeHtml(eventDate ? formatDate(eventDate) : "Дата не указана")} — ${escapeHtml(eventTitle)}</h3>
                <div class="card-text">${details.join("")}</div>
            </div>
        `;

        eventsList.appendChild(item);
    });
}

function setOptionalBlock(blockId, textElementId, value) {
    const block = document.getElementById(blockId);
    const textElement = document.getElementById(textElementId);

    if (!value) {
        block.classList.add("d-none");
        return;
    }

    textElement.textContent = value;
    block.classList.remove("d-none");
}

function getFirstValue(object, fieldNames, defaultValue) {
    for (const fieldName of fieldNames) {
        if (object[fieldName] !== undefined && object[fieldName] !== null && object[fieldName] !== "") {
            return object[fieldName];
        }
    }

    return defaultValue;
}

function formatDates(startDate, endDate) {
    if (!startDate && !endDate) {
        return "Даты не указаны";
    }

    if (startDate && !endDate) {
        return `Начало: ${formatDate(startDate)}`;
    }

    if (!startDate && endDate) {
        return `Окончание: ${formatDate(endDate)}`;
    }

    return `${formatDate(startDate)} — ${formatDate(endDate)}`;
}

function formatDate(dateString) {
    return new Date(dateString).toLocaleDateString("ru-RU");
}

function showError(message) {
    const loadingMessage = document.getElementById("loadingMessage");
    const errorMessage = document.getElementById("errorMessage");

    loadingMessage.classList.add("d-none");
    errorMessage.classList.remove("d-none");
    errorMessage.textContent = message;
}
