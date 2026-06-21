document.addEventListener("DOMContentLoaded", async () => {
    const params = new URLSearchParams(window.location.search);

    let warId = params.get("warId");
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

    try {
        const operation = await getJson(`${API_BASE_URL}/operation/${operationId}`);
        warId = String(operation.warId ?? warId ?? "");

        renderOperation(operation);
        setupOperationSides(operationId, warId);

        const [events] = await Promise.all([
            loadOperationEvents(operationId),
            loadOperationSides(operationId)
        ]);
        renderEvents(events);

        loadingMessage.classList.add("d-none");
        operationDetails.classList.remove("d-none");
    } catch (error) {
        showError("Не удалось загрузить данные операции.");
        console.error(error);
    }
});

let operationSides = [];
let conflictSides = [];

function setupOperationSides(operationId, warId) {
    const addButton = document.getElementById("addOperationSideButton");
    const form = document.getElementById("operationSideForm");
    const cancelButton = document.getElementById("cancelOperationSideForm");
    const list = document.getElementById("operationSidesList");

    addButton.addEventListener("click", () => openOperationSideForm(warId));
    cancelButton.addEventListener("click", () => form.classList.add("d-none"));
    form.addEventListener("submit", event => saveOperationSide(event, operationId));
    list.addEventListener("click", event => deleteOperationSide(event, operationId));
}

async function loadOperationSides(operationId) {
    const container = document.getElementById("operationSidesList");

    try {
        operationSides = await getJson(`${API_BASE_URL}/operations/${operationId}/sides`);

        container.innerHTML = operationSides.length
            ? operationSides.map(side => `
                <span class="badge d-inline-flex align-items-center gap-2" style="background-color:${safeOperationSideColor(side.colorHex)}">
                    ${escapeHtml(side.title)}
                    <button class="btn-close btn-close-white" type="button" aria-label="Удалить ${escapeHtml(side.title)}" data-operation-side-id="${side.operationSideId}"></button>
                </span>`).join("")
            : '<span class="text-muted">Стороны операции не указаны.</span>';
    } catch (error) {
        container.innerHTML = `<span class="text-danger">${escapeHtml(error.message)}</span>`;
    }
}

async function openOperationSideForm(warId) {
    const form = document.getElementById("operationSideForm");
    const select = document.getElementById("operationSideSelect");

    try {
        conflictSides = await getJson(`${API_BASE_URL}/wars/${warId}/sides`);
        const usedIds = new Set(operationSides.map(side => side.warSideId));
        const availableSides = conflictSides.filter(side => !usedIds.has(side.warSideId));

        select.innerHTML = availableSides.length
            ? availableSides.map(side => `<option value="${side.warSideId}">${escapeHtml(side.title)}</option>`).join("")
            : '<option value="">Все стороны конфликта уже добавлены</option>';
        select.disabled = !availableSides.length;
        form.querySelector('[type="submit"]').disabled = !availableSides.length;
        form.classList.remove("d-none");
        if (availableSides.length) select.focus();
    } catch (error) {
        showOperationSidesStatus(error.message, "danger");
    }
}

async function saveOperationSide(event, operationId) {
    event.preventDefault();
    const form = event.currentTarget;
    const select = document.getElementById("operationSideSelect");

    try {
        await postJson(`${API_BASE_URL}/operations/${operationId}/sides`, {
            warSideId: Number(select.value)
        });
        form.classList.add("d-none");
        showOperationSidesStatus("Сторона добавлена к операции.", "success");
        await loadOperationSides(operationId);
    } catch (error) {
        showOperationSidesStatus(error.message, "danger");
    }
}

async function deleteOperationSide(event, operationId) {
    const button = event.target.closest("[data-operation-side-id]");
    if (!button || !window.confirm("Удалить сторону из этой операции?")) return;

    try {
        await deleteJson(`${API_BASE_URL}/operations/${operationId}/sides/${button.dataset.operationSideId}`);
        showOperationSidesStatus("Сторона удалена из операции.", "success");
        await loadOperationSides(operationId);
    } catch (error) {
        showOperationSidesStatus(error.message, "danger");
    }
}

function showOperationSidesStatus(message, type) {
    const status = document.getElementById("operationSidesStatus");
    status.className = `alert alert-${type} mt-3 mb-0`;
    status.textContent = message;
}

function safeOperationSideColor(colorHex) {
    return /^#[0-9a-f]{6}$/i.test(colorHex ?? "") ? colorHex : "#6c757d";
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
