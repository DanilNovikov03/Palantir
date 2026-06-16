export function createTemporaryMapData() {
    return {
        events: [],

        armies: [
            {
                id: 1,
                title: "Соединение A",
                description: "Временная позиция войскового соединения.",
                lat: 51.84,
                lng: 35.95
            },
            {
                id: 2,
                title: "Соединение B",
                description: "Временная позиция войскового соединения.",
                lat: 51.41,
                lng: 36.39
            }
        ],

        controlZones: []
    };
}
