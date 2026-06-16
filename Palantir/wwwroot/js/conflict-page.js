document.addEventListener("DOMContentLoaded", async () => {
    const params = new URLSearchParams(window.location.search);
    const warId = params.get("warId");

    const loadingMessage = document.getElementById("loadingMessage");
    const errorMessage = document.getElementById("errorMessage");

    const conflictDetails = document.getElementById("conflictDetails");
    const conflictName = document.getElementById("conflictName");
    const conflictDates = document.getElementById("conflictDates");
    const conflictNote = document.getElementById("conflictNote");

    const theatersSection = document.getElementById("theatersSection");
    const theatersList = document.getElementById("theatersList");

    if (!warId) {
        loadingMessage.classList.add("d-none");
        errorMessage.classList.remove("d-none");
        errorMessage.textContent = "Не передан идентификатор конфликта.";
        return;
    }

    try {
        const conflict = await getJson(`${API_BASE_URL}/war/${warId}`);

        conflictName.textContent = conflict.title;
        conflictNote.textContent = conflict.summary ?? "Описание отсутствует.";

        conflictDates.textContent = formatDates(conflict.startDate, conflict.endDate);

        conflictDetails.classList.remove("d-none");

        const theaters = await getJson(`${API_BASE_URL}/theater/by-war/${warId}`);

        renderTheaters(theaters, theatersList, warId);

        theatersSection.classList.remove("d-none");
        loadingMessage.classList.add("d-none");
    } catch (error) {
        loadingMessage.classList.add("d-none");
        errorMessage.classList.remove("d-none");

        console.error(error);
    }
});

function renderTheaters(theaters, container, warId) {
    container.innerHTML = "";

    if (!theaters || theaters.length === 0) {
        container.innerHTML = `
            <div class="col-12">
                <div class="alert alert-warning">
                    Для этого конфликта пока не добавлены театры военных действий.
                </div>
            </div>
        `;
        return;
    }

    theaters.forEach(theater => {
        const card = document.createElement("div");
        card.className = "col-md-4";

        card.innerHTML = `
            <div class="card h-100">
                <div class="card-body d-flex flex-column">
                    <h5 class="card-title">${theater.title}</h5>

                    <p class="card-text text-muted">
                        ${theater.summary ?? "Описание отсутствует"}
                    </p>

                    <a href="/theater.html?warId=${warId}&theaterId=${theater.id}" class="btn btn-primary mt-auto">
                        Открыть театр
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