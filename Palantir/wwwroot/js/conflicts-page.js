document.addEventListener("DOMContentLoaded", () => {
    const elements = {
        list: document.getElementById("conflictsList"),
        loading: document.getElementById("loadingMessage"),
        error: document.getElementById("errorMessage"),
        status: document.getElementById("statusMessage"),
        addButton: document.getElementById("addConflictButton"),
        form: document.getElementById("conflictForm"),
        formTitle: document.getElementById("conflictFormTitle"),
        id: document.getElementById("conflictId"),
        title: document.getElementById("conflictTitle"),
        startDate: document.getElementById("conflictStartDate"),
        endDate: document.getElementById("conflictEndDate"),
        summary: document.getElementById("conflictSummary"),
        cancelButton: document.getElementById("cancelConflictForm")
    };

    let conflicts = [];

    elements.addButton.addEventListener("click", () => openForm());
    elements.cancelButton.addEventListener("click", closeForm);
    elements.form.addEventListener("submit", saveConflict);
    elements.list.addEventListener("click", handleListAction);

    loadConflicts();

    async function loadConflicts() {
        try {
            elements.loading.classList.remove("d-none");
            elements.error.classList.add("d-none");
            conflicts = await getJson(`${API_BASE_URL}/war`);
            renderConflicts();
        } catch (error) {
            elements.error.textContent = error.message || "Не удалось загрузить список конфликтов.";
            elements.error.classList.remove("d-none");
        } finally {
            elements.loading.classList.add("d-none");
        }
    }

    function renderConflicts() {
        if (!conflicts.length) {
            elements.list.innerHTML = '<div class="col-12"><div class="alert alert-secondary">Конфликты пока не добавлены.</div></div>';
            return;
        }

        elements.list.innerHTML = conflicts.map(conflict => `
            <div class="col-md-6 col-xl-4">
                <article class="card h-100">
                    <div class="card-body d-flex flex-column">
                        <h3 class="card-title h5">${escapeHtml(conflict.title)}</h3>
                        <p class="card-text text-muted">${escapeHtml(conflict.summary ?? "Описание отсутствует")}</p>
                        <p class="small text-secondary">${escapeHtml(formatDates(conflict.startDate, conflict.endDate))}</p>
                        <div class="d-flex flex-wrap gap-2 mt-auto">
                            <a href="/conflict.html?warId=${encodeURIComponent(conflict.id)}" class="btn btn-primary">Открыть</a>
                            <button class="btn btn-outline-primary" type="button" data-action="edit" data-id="${conflict.id}">Изменить</button>
                            <button class="btn btn-outline-danger" type="button" data-action="delete" data-id="${conflict.id}">Удалить</button>
                        </div>
                    </div>
                </article>
            </div>
        `).join("");
    }

    function handleListAction(event) {
        const button = event.target.closest("[data-action]");
        if (!button) return;

        const conflict = conflicts.find(item => item.id === Number(button.dataset.id));
        if (!conflict) return;

        if (button.dataset.action === "edit") openForm(conflict);
        if (button.dataset.action === "delete") deleteConflict(conflict);
    }

    function openForm(conflict = null) {
        elements.form.reset();
        elements.id.value = conflict?.id ?? "";
        elements.formTitle.textContent = conflict ? "Изменить конфликт" : "Добавить конфликт";
        elements.title.value = conflict?.title ?? "";
        elements.startDate.value = conflict?.startDate?.slice(0, 10) ?? "";
        elements.endDate.value = conflict?.endDate?.slice(0, 10) ?? "";
        elements.summary.value = conflict?.summary ?? "";
        elements.form.classList.remove("d-none");
        elements.title.focus();
    }

    function closeForm() {
        elements.form.classList.add("d-none");
        elements.form.reset();
        elements.id.value = "";
    }

    async function saveConflict(event) {
        event.preventDefault();

        const request = {
            title: elements.title.value.trim(),
            startDate: elements.startDate.value,
            endDate: elements.endDate.value || null,
            summary: elements.summary.value.trim() || null
        };

        if (request.endDate && request.startDate > request.endDate) {
            showStatus("Дата начала не может быть позже даты окончания.", "warning");
            return;
        }

        try {
            const id = Number(elements.id.value);
            if (id) {
                await putJson(`${API_BASE_URL}/war/${id}`, request);
                showStatus("Конфликт изменён.", "success");
            } else {
                await postJson(`${API_BASE_URL}/war`, request);
                showStatus("Конфликт добавлен.", "success");
            }

            closeForm();
            await loadConflicts();
        } catch (error) {
            showStatus(error.message, "danger");
        }
    }

    async function deleteConflict(conflict) {
        if (!window.confirm("Вы действительно хотите удалить этот конфликт?")) return;

        try {
            await deleteJson(`${API_BASE_URL}/war/${conflict.id}`);
            showStatus("Конфликт удалён.", "success");
            await loadConflicts();
        } catch (error) {
            showStatus(error.message, "danger");
        }
    }

    function showStatus(message, type) {
        elements.status.className = `alert alert-${type}`;
        elements.status.textContent = message;
    }
});

function formatDates(startDate, endDate) {
    if (!endDate) return `Начало: ${formatDate(startDate)}`;
    return `${formatDate(startDate)} — ${formatDate(endDate)}`;
}

function formatDate(dateString) {
    return dateString ? new Date(`${dateString.slice(0, 10)}T00:00:00`).toLocaleDateString("ru-RU") : "не указана";
}
