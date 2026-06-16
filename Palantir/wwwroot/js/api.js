const API_BASE_URL = "/api";

async function getJson(url) {
    const response = await fetch(url);

    if (!response.ok) {
        throw new Error(`Ошибка запроса: ${response.status}`);
    }

    return await response.json();
}