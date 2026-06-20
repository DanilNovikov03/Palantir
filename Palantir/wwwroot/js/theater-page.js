document.addEventListener("DOMContentLoaded", () => {
    const params = new URLSearchParams(window.location.search);
    const warIdFromUrl = Number(params.get("warId"));
    const theaterId = Number(params.get("theaterId"));
    const elements = {
        back: document.getElementById("backToConflictButton"),
        loading: document.getElementById("loadingMessage"),
        error: document.getElementById("errorMessage"),
        status: document.getElementById("statusMessage"),
        details: document.getElementById("theaterDetails"),
        title: document.getElementById("theaterTitle"),
        summary: document.getElementById("theaterSummary"),
        operationsSection: document.getElementById("operationsSection"),
        list: document.getElementById("operationsList"),
        addButton: document.getElementById("addOperationButton"),
        form: document.getElementById("operationForm"),
        formTitle: document.getElementById("operationFormTitle"),
        id: document.getElementById("operationId"),
        operationTitle: document.getElementById("operationTitle"),
        startDate: document.getElementById("operationStartDate"),
        endDate: document.getElementById("operationEndDate"),
        operationSummary: document.getElementById("operationSummary"),
        cancelButton: document.getElementById("cancelOperationForm")
    };

    let theater = null;
    let operations = [];

    if (!Number.isInteger(theaterId) || theaterId <= 0) {
        showFatalError("Не передан идентификатор театра.");
        return;
    }

    elements.addButton.addEventListener("click", () => openForm());
    elements.cancelButton.addEventListener("click", closeForm);
    elements.form.addEventListener("submit", saveOperation);
    elements.list.addEventListener("click", handleListAction);

    loadPage();

    async function loadPage() {
        try {
            elements.loading.classList.remove("d-none");
            [theater, operations] = await Promise.all([
                getJson(`${API_BASE_URL}/theater/${theaterId}`),
                getJson(`${API_BASE_URL}/operation/by-theater/${theaterId}`)
            ]);

            const warId = theater.warId ?? warIdFromUrl;
            if (warId) elements.back.href = `/conflict.html?warId=${warId}`;

            elements.title.textContent = theater.title ?? "Без названия";
            elements.summary.textContent = theater.summary ?? "Описание отсутствует.";
            renderOperations(warId);
            elements.details.classList.remove("d-none");
            elements.operationsSection.classList.remove("d-none");
            elements.error.classList.add("d-none");
        } catch (error) {
            showFatalError(error.message || "Не удалось загрузить данные театра.");
        } finally {
            elements.loading.classList.add("d-none");
        }
    }

    function renderOperations(warId) {
        if (!operations?.length) {
            elements.list.innerHTML = '<div class="col-12"><div class="alert alert-secondary">Для этого театра пока не добавлены операции</div></div>';
            return;
        }

        elements.list.innerHTML = operations.map(operation => `
            <div class="col-md-6 col-xl-4">
                <article class="card h-100">
                    <div class="card-body d-flex flex-column">
                        <h3 class="card-title h5">${escapeHtml(operation.title ?? "Без названия")}</h3>
                        <p class="card-text text-muted">${escapeHtml(operation.summary ?? "Описание отсутствует")}</p>
                        <p class="small text-secondary">${escapeHtml(formatDates(operation.startDate, operation.endDate))}</p>
                        <div class="d-flex flex-wrap gap-2 mt-auto">
                            <a href="/operation.html?warId=${encodeURIComponent(warId)}&theaterId=${theaterId}&operationId=${operation.id}" class="btn btn-primary">Открыть</a>
                            <button class="btn btn-outline-primary" type="button" data-action="edit" data-id="${operation.id}">Изменить</button>
                            <button class="btn btn-outline-danger" type="button" data-action="delete" data-id="${operation.id}">Удалить</button>
                        </div>
                    </div>
                </article>
            </div>
        `).join("");
    }

    function handleListAction(event) {
        const button = event.target.closest("[data-action]");
        if (!button) return;
        const operation = operations.find(item => item.id === Number(button.dataset.id));
        if (!operation) return;
        if (button.dataset.action === "edit") openForm(operation);
        if (button.dataset.action === "delete") deleteOperation(operation);
    }

    function openForm(operation = null) {
        elements.form.reset();
        elements.id.value = operation?.id ?? "";
        elements.formTitle.textContent = operation ? "Изменить операцию" : "Добавить операцию";
        elements.operationTitle.value = operation?.title ?? "";
        elements.startDate.value = operation?.startDate?.slice(0, 10) ?? "";
        elements.endDate.value = operation?.endDate?.slice(0, 10) ?? "";
        elements.operationSummary.value = operation?.summary ?? "";
        elements.form.classList.remove("d-none");
        elements.operationTitle.focus();
    }

    function closeForm() {
        elements.form.classList.add("d-none");
        elements.form.reset();
        elements.id.value = "";
    }

    async function saveOperation(event) {
        event.preventDefault();
        const request = {
            theaterId,
            title: elements.operationTitle.value.trim(),
            startDate: elements.startDate.value,
            endDate: elements.endDate.value,
            summary: elements.operationSummary.value.trim() || null
        };

        if (request.startDate > request.endDate) {
            showStatus("Дата начала не может быть позже даты окончания.", "warning");
            return;
        }

        try {
            const operationId = Number(elements.id.value);
            if (operationId) {
                await putJson(`${API_BASE_URL}/operation/${operationId}`, request);
                showStatus("Операция изменена.", "success");
            } else {
                await postJson(`${API_BASE_URL}/operation`, request);
                showStatus("Операция добавлена.", "success");
            }
            closeForm();
            await loadPage();
        } catch (error) {
            showStatus(error.message, "danger");
        }
    }

    async function deleteOperation(operation) {
        if (!window.confirm("Вы действительно хотите удалить эту запись?")) return;
        try {
            await deleteJson(`${API_BASE_URL}/operation/${operation.id}`);
            showStatus("Операция удалена.", "success");
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
    return `${formatDate(startDate)} — ${formatDate(endDate)}`;
}

function formatDate(dateString) {
    return dateString ? new Date(`${dateString.slice(0, 10)}T00:00:00`).toLocaleDateString("ru-RU") : "не указана";
}
