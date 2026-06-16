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

    try {
        const operation = await getJson(`${API_BASE_URL}/operation/${operationId}`);

        renderOperation(operation);

        const events = await loadOperationEventsStub(operationId);
        renderEvents(events);

        loadingMessage.classList.add("d-none");
        operationDetails.classList.remove("d-none");
    } catch (error) {
        showError("Не удалось загрузить данные операции.");
        console.error(error);
    }
});

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

async function loadOperationEventsStub(operationId) {
    // Заглушка.
    // Когда появится EventsController, эту функцию можно будет заменить на реальный запрос:
    //
    // return await getJson(`${API_BASE_URL}/events/by-operation/${operationId}`);
    //
    // Пока возвращаем пустой массив, чтобы страница работала без реализованных Event.
    return [];
}

function renderEvents(events) {
    const eventsList = document.getElementById("eventsList");

    eventsList.innerHTML = "";

    if (!events || events.length === 0) {
        eventsList.innerHTML = `
            <div class="alert alert-secondary">
                События для операции пока не реализованы. 
                Позже здесь будет отображаться хронология событий операции.
            </div>
        `;
        return;
    }

    events.forEach(event => {
        const eventTitle = getFirstValue(event, ["title", "name"], "Событие без названия");
        const eventDescription = getFirstValue(event, ["description", "summary", "note"], "Описание отсутствует.");
        const eventDate = event.eventDate ?? event.date ?? event.startDate;

        const item = document.createElement("div");
        item.className = "card mb-3";

        item.innerHTML = `
            <div class="card-body">
                <h5 class="card-title">${eventTitle}</h5>
                <p class="card-text text-muted">${eventDate ? formatDate(eventDate) : "Дата не указана"}</p>
                <p class="card-text">${eventDescription}</p>
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