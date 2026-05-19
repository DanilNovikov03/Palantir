document.addEventListener("DOMContentLoaded", async () => {
    const params = new URLSearchParams(window.location.search);

    const warId = params.get("warId");
    const theaterId = params.get("theaterId");

    const backToConflictButton = document.getElementById("backToConflictButton");

    const loadingMessage = document.getElementById("loadingMessage");
    const errorMessage = document.getElementById("errorMessage");

    const theaterDetails = document.getElementById("theaterDetails");
    const theaterTitle = document.getElementById("theaterTitle");
    const theaterSummary = document.getElementById("theaterSummary");

    const operationsSection = document.getElementById("operationsSection");
    const operationsList = document.getElementById("operationsList");

    if (warId) {
        backToConflictButton.href = `/conflict.html?warId=${warId}`;
    }

    if (!theaterId) {
        showError("Не передан идентификатор театра.");
        return;
    }

    try {
        const theater = await getJson(`${API_BASE_URL}/theater/${theaterId}`);

        renderTheater(theater, theaterTitle, theaterSummary);

        theaterDetails.classList.remove("d-none");

        const operations = await getJson(`${API_BASE_URL}/operation/by-theater/${theaterId}`);

        renderOperations(operations, operationsList);

        operationsSection.classList.remove("d-none");
        loadingMessage.classList.add("d-none");
    } catch (error) {
        showError("Не удалось загрузить данные театра.");
        console.error(error);
    }
});

function renderTheater(theater, titleElement, summaryElement) {
    titleElement.textContent = theater.title ?? "Без названия";

    summaryElement.textContent =
        theater.summary ??
        "Описание отсутствует.";
}

function renderOperations(operations, container) {
    container.innerHTML = "";

    if (!operations || operations.length === 0) {
        container.innerHTML = `
            <div class="col-12">
                <div class="alert alert-warning">
                    Для этого театра пока не добавлены операции.
                </div>
            </div>
        `;
        return;
    }

    operations.forEach(operation => {
        const card = document.createElement("div");
        card.className = "col-md-4";

        const title = operation.title ?? "Без названия";
        const summary =
            operation.summary ??
            "Описание отсутствует.";

        const startDate = operation.startDate ? formatDate(operation.startDate) : null;
        const endDate = operation.endDate ? formatDate(operation.endDate) : null;
        const datesText = formatDates(startDate, endDate);

        card.innerHTML = `
            <div class="card h-100">
                <div class="card-body d-flex flex-column">
                    <h5 class="card-title">${title}</h5>

                    <p class="card-text text-muted">
                        ${summary}
                    </p>

                    <p class="small text-secondary">
                        ${datesText}
                    </p>

                    <a href="/operation.html?operationId=${operation.id}" class="btn btn-primary mt-auto">
                        Открыть операцию
                    </a>
                </div>
            </div>
        `;

        container.appendChild(card);
    });
}

function formatDates(startDate, endDate) {
    if (!startDate && !endDate) {
        return "Даты не указаны";
    }

    if (startDate && !endDate) {
        return `Начало: ${startDate}`;
    }

    if (!startDate && endDate) {
        return `Окончание: ${endDate}`;
    }

    return `${startDate} — ${endDate}`;
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