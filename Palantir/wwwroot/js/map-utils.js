export function getQueryParam(name) {
    return new URLSearchParams(window.location.search).get(name);
}

export function formatDate(value) {
    if (!value) {
        return "не указана";
    }

    const date = new Date(value);

    if (Number.isNaN(date.getTime())) {
        return String(value);
    }

    return date.toLocaleDateString("ru-RU");
}

export function isValidCoordinate(value) {
    return value !== null 
    && value !== undefined 
    && value !== "" 
    && !Number.isNaN(Number(value));
}

export function escapeHtml(value) {
    return String(value)
        .replaceAll("&", "&amp;")
        .replaceAll("<", "&lt;")
        .replaceAll(">", "&gt;")
        .replaceAll('"', "&quot;")
        .replaceAll("'", "&#039;");
}