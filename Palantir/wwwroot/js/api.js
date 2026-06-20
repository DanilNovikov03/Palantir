const API_BASE_URL = "/api";

async function getJson(url) {
    return await requestJson(url);
}

async function postJson(url, body) {
    return await requestJson(url, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(body)
    });
}

async function putJson(url, body) {
    return await requestJson(url, {
        method: "PUT",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(body)
    });
}

async function deleteJson(url) {
    return await requestJson(url, { method: "DELETE" });
}

async function requestJson(url, options = {}) {
    const response = await fetch(url, options);

    if (!response.ok) {
        const body = await response.json().catch(() => null);
        const message = body?.message ?? body?.title ?? `Ошибка запроса: ${response.status}`;
        throw new Error(message);
    }

    if (response.status === 204) {
        return null;
    }

    return await response.json().catch(() => null);
}

function escapeHtml(value) {
    return String(value ?? "")
        .replaceAll("&", "&amp;")
        .replaceAll("<", "&lt;")
        .replaceAll(">", "&gt;")
        .replaceAll('"', "&quot;")
        .replaceAll("'", "&#039;");
}
