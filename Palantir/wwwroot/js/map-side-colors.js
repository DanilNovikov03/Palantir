const SIDE_COLOR_FALLBACKS = {
    "СССР": "#d32f2f",
    "Германия": "#424242",
    "США": "#1976d2",
    "Великобритания": "#1565c0",
    "Франция": "#0d47a1",
    "Союзники": "#388e3c",
    default: "#6c757d"
};

export function getSideColor(side) {
    if (!side) {
        return SIDE_COLOR_FALLBACKS.default;
    }

    if (isValidHexColor(side.colorHex)) {
        return side.colorHex;
    }

    const title = side.name ?? side.title ?? side.sideName ?? side.sideTitle;

    return SIDE_COLOR_FALLBACKS[title] ?? SIDE_COLOR_FALLBACKS.default;
}

function isValidHexColor(value) {
    return typeof value === "string" && /^#[0-9a-f]{6}$/i.test(value.trim());
}
