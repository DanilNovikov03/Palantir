document.addEventListener("DOMContentLoaded", () => {
    const warId = Number(new URLSearchParams(window.location.search).get("warId"));
    const elements = {
        loading: document.getElementById("loadingMessage"),
        error: document.getElementById("errorMessage"),
        status: document.getElementById("statusMessage"),
        details: document.getElementById("conflictDetails"),
        name: document.getElementById("conflictName"),
        dates: document.getElementById("conflictDates"),
        note: document.getElementById("conflictNote"),
        warSidesList: document.getElementById("warSidesList"),
        editConflict: document.getElementById("editConflictButton"),
        conflictForm: document.getElementById("conflictForm"),
        conflictTitle: document.getElementById("conflictTitle"),
        conflictStartDate: document.getElementById("conflictStartDate"),
        conflictEndDate: document.getElementById("conflictEndDate"),
        conflictSummary: document.getElementById("conflictSummary"),
        cancelConflict: document.getElementById("cancelConflictForm"),
        theatersSection: document.getElementById("theatersSection"),
        theatersList: document.getElementById("theatersList"),
        addTheater: document.getElementById("addTheaterButton"),
        theaterForm: document.getElementById("theaterForm"),
        theaterFormTitle: document.getElementById("theaterFormTitle"),
        theaterId: document.getElementById("theaterId"),
        theaterTitle: document.getElementById("theaterTitle"),
        theaterSummary: document.getElementById("theaterSummary"),
        cancelTheater: document.getElementById("cancelTheaterForm")
    };

    let conflict = null;
    let theaters = [];

    if (!Number.isInteger(warId) || warId <= 0) {
        showFatalError("Не передан идентификатор конфликта.");
        return;
    }

    elements.editConflict.addEventListener("click", openConflictForm);
    elements.cancelConflict.addEventListener("click", () => elements.conflictForm.classList.add("d-none"));
    elements.conflictForm.addEventListener("submit", saveConflict);
    elements.addTheater.addEventListener("click", () => openTheaterForm());
    elements.cancelTheater.addEventListener("click", closeTheaterForm);
    elements.theaterForm.addEventListener("submit", saveTheater);
    elements.theatersList.addEventListener("click", handleTheaterAction);

    loadPage();
    loadWarSides();

    async function loadWarSides() {
        try {
            const sides = await getJson(`${API_BASE_URL}/wars/${warId}/sides`);

            elements.warSidesList.innerHTML = sides.length
                ? sides.map(side => `<span class="badge text-bg-secondary">${escapeHtml(side.title)}</span>`).join("")
                : '<span class="text-muted">Стороны конфликта не указаны.</span>';
        } catch (error) {
            elements.warSidesList.innerHTML = `<span class="text-danger">${escapeHtml(error.message)}</span>`;
        }
    }

    async function loadPage() {
        try {
            elements.loading.classList.remove("d-none");
            [conflict, theaters] = await Promise.all([
                getJson(`${API_BASE_URL}/war/${warId}`),
                getJson(`${API_BASE_URL}/theater/by-war/${warId}`)
            ]);
            renderConflict();
            renderTheaters();
            elements.details.classList.remove("d-none");
            elements.theatersSection.classList.remove("d-none");
            elements.error.classList.add("d-none");
        } catch (error) {
            showFatalError(error.message || "Не удалось загрузить данные конфликта.");
        } finally {
            elements.loading.classList.add("d-none");
        }
    }

    function renderConflict() {
        elements.name.textContent = conflict.title;
        elements.dates.textContent = formatDates(conflict.startDate, conflict.endDate);
        elements.note.textContent = conflict.summary ?? "Описание отсутствует.";
    }

    function renderTheaters() {
        if (!theaters?.length) {
            elements.theatersList.innerHTML = '<div class="col-12"><div class="alert alert-secondary">Для этого конфликта пока не добавлены театры</div></div>';
            return;
        }

        elements.theatersList.innerHTML = theaters.map(theater => `
            <div class="col-md-6 col-xl-4">
                <article class="card h-100">
                    <div class="card-body d-flex flex-column">
                        <h3 class="card-title h5">${escapeHtml(theater.title)}</h3>
                        <p class="card-text text-muted">${escapeHtml(theater.summary ?? "Описание отсутствует")}</p>
                        <div class="d-flex flex-wrap gap-2 mt-auto">
                            <a href="/theater.html?warId=${warId}&theaterId=${theater.id}" class="btn btn-primary">Открыть</a>
                            <button class="btn btn-outline-primary" type="button" data-action="edit" data-id="${theater.id}">Изменить</button>
                            <button class="btn btn-outline-danger" type="button" data-action="delete" data-id="${theater.id}">Удалить</button>
                        </div>
                    </div>
                </article>
            </div>
        `).join("");
    }

    function openConflictForm() {
        elements.conflictTitle.value = conflict.title ?? "";
        elements.conflictStartDate.value = conflict.startDate?.slice(0, 10) ?? "";
        elements.conflictEndDate.value = conflict.endDate?.slice(0, 10) ?? "";
        elements.conflictSummary.value = conflict.summary ?? "";
        elements.conflictForm.classList.remove("d-none");
        elements.conflictTitle.focus();
    }

    async function saveConflict(event) {
        event.preventDefault();
        const request = {
            title: elements.conflictTitle.value.trim(),
            startDate: elements.conflictStartDate.value,
            endDate: elements.conflictEndDate.value || null,
            summary: elements.conflictSummary.value.trim() || null
        };

        if (request.endDate && request.startDate > request.endDate) {
            showStatus("Дата начала не может быть позже даты окончания.", "warning");
            return;
        }

        try {
            await putJson(`${API_BASE_URL}/war/${warId}`, request);
            elements.conflictForm.classList.add("d-none");
            showStatus("Конфликт изменён.", "success");
            await loadPage();
        } catch (error) {
            showStatus(error.message, "danger");
        }
    }

    function handleTheaterAction(event) {
        const button = event.target.closest("[data-action]");
        if (!button) return;
        const theater = theaters.find(item => item.id === Number(button.dataset.id));
        if (!theater) return;
        if (button.dataset.action === "edit") openTheaterForm(theater);
        if (button.dataset.action === "delete") deleteTheater(theater);
    }

    function openTheaterForm(theater = null) {
        elements.theaterForm.reset();
        elements.theaterId.value = theater?.id ?? "";
        elements.theaterFormTitle.textContent = theater ? "Изменить театр" : "Добавить театр";
        elements.theaterTitle.value = theater?.title ?? "";
        elements.theaterSummary.value = theater?.summary ?? "";
        elements.theaterForm.classList.remove("d-none");
        elements.theaterTitle.focus();
    }

    function closeTheaterForm() {
        elements.theaterForm.classList.add("d-none");
        elements.theaterForm.reset();
        elements.theaterId.value = "";
    }

    async function saveTheater(event) {
        event.preventDefault();
        const request = {
            warId,
            title: elements.theaterTitle.value.trim(),
            summary: elements.theaterSummary.value.trim() || null
        };

        try {
            const theaterId = Number(elements.theaterId.value);
            if (theaterId) {
                await putJson(`${API_BASE_URL}/theater/${theaterId}`, request);
                showStatus("Театр изменён.", "success");
            } else {
                await postJson(`${API_BASE_URL}/theater`, request);
                showStatus("Театр добавлен.", "success");
            }
            closeTheaterForm();
            await loadPage();
        } catch (error) {
            showStatus(error.message, "danger");
        }
    }

    async function deleteTheater(theater) {
        if (!window.confirm("Вы действительно хотите удалить эту запись?")) return;
        try {
            await deleteJson(`${API_BASE_URL}/theater/${theater.id}`);
            showStatus("Театр удалён.", "success");
            await loadPage();
        } catch (error) {
            showStatus(error.message, "danger");
        }
    }

    function showStatus(message, type) {
        elements.status.className = `alert alert-${type}`;
        elements.status.textContent = message;
    }

    function showFatalError(message) {
        elements.loading.classList.add("d-none");
        elements.error.textContent = message;
        elements.error.classList.remove("d-none");
    }
});

function formatDates(startDate, endDate) {
    if (!endDate) return `Начало: ${formatDate(startDate)}`;
    return `${formatDate(startDate)} — ${formatDate(endDate)}`;
}

function formatDate(dateString) {
    return dateString ? new Date(`${dateString.slice(0, 10)}T00:00:00`).toLocaleDateString("ru-RU") : "не указана";
}
