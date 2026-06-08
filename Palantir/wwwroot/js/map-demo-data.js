export function createTemporaryMapData() {
    return {
        events: [
            {
                id: 1,
                title: "Начало операции",
                description: "Временная точка события. Позже здесь будет запись из модели Event.",
                lat: 51.73,
                lng: 36.19,
                date: "1943-07-05"
            },
            {
                id: 2,
                title: "Ключевой район боевых действий",
                description: "Демонстрационная точка для проверки pop-up окна и списка объектов.",
                lat: 51.49,
                lng: 36.09,
                date: "1943-07-12"
            }
        ],

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