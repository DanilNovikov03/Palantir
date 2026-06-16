const SIDE_COLOR_FALLBACKS = {
    "СССР": "#d32f2f",
    "Германия": "#424242",
    "США": "#1976d2",
    "Великобритания": "#1565c0",
    "Франция": "#0d47a1",
    "1": "#d32f2f",
    "2": "#424242",
    "3": "#1976d2",
    default: "#6c757d"
};

export const SIDE_OPTIONS = [
    { id: 1, title: "СССР" },
    { id: 2, title: "Германия" },
    { id: 3, title: "США" }
];

export function getSideById(sideId) {
    const normalizedId = Number(sideId);
    return SIDE_OPTIONS.find(side => side.id === normalizedId) ?? null;
}

export function getSideColor(side) {
    if (!side) {
        return SIDE_COLOR_FALLBACKS.default;
    }

    if (side.colorHex) {
        return side.colorHex;
    }

    if (side.title && SIDE_COLOR_FALLBACKS[side.title]) {
        return SIDE_COLOR_FALLBACKS[side.title];
    }

    if (side.name && SIDE_COLOR_FALLBACKS[side.name]) {
        return SIDE_COLOR_FALLBACKS[side.name];
    }

    if (side.id && SIDE_COLOR_FALLBACKS[String(side.id)]) {
        return SIDE_COLOR_FALLBACKS[String(side.id)];
    }

    if (side.warSideId && SIDE_COLOR_FALLBACKS[String(side.warSideId)]) {
        return SIDE_COLOR_FALLBACKS[String(side.warSideId)];
    }

    if (typeof side === "string" && SIDE_COLOR_FALLBACKS[side]) {
        return SIDE_COLOR_FALLBACKS[side];
    }

    return SIDE_COLOR_FALLBACKS.default;
}
