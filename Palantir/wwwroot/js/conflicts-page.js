document.addEventListener("DOMContentLoaded", async () => {
    const conflictsList = document.getElementById("conflictsList");
    const loadingMessage = document.getElementById("loadingMessage");
    const errorMessage = document.getElementById("errorMessage");

    try {
        const conflicts = await getJson(`${API_BASE_URL}/war`);

        loadingMessage.classList.add("d-none");

        if (conflicts.length === 0) {
            conflictsList.innerHTML = `
                <div class="col-12">
                    <div class="alert alert-warning">
                        Конфликты пока не добавлены.
                    </div>
                </div>
            `;
            return;
        }

        conflicts.forEach(conflict => {
            const card = document.createElement("div");
            card.className = "col-md-4";

            card.innerHTML = `
                <div class="card h-100">
                    <div class="card-body d-flex flex-column">
                        <h5 class="card-title">${conflict.title}</h5>

                        <p class="card-text text-muted">
                            ${conflict.summary ?? "Описание отсутствует"}
                        </p>

                        <a href="/conflict.html?warId=${conflict.id}" class="btn btn-primary mt-auto">
                            Открыть конфликт
                        </a>
                    </div>
                </div>
            `;

            conflictsList.appendChild(card);
        });
    } catch (error) {
        loadingMessage.classList.add("d-none");
        errorMessage.classList.remove("d-none");

        console.error(error);
    }
});