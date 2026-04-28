export function getMaterialColorForTemperature(temp: number): string {
    if (temp < 15) { return "green-darken-4"; }
    else if (temp < 16) { return "green-darken-1"; }
    else if (temp < 16.5) { return "lime-accent-4"; }
    else if (temp <= 17.0) { return "lime-accent-3"; }
    else if (temp <= 17.5) { return "lime-accent-2"; }
    else if (temp <= 18.5) { return "yellow-lighten-4"; }
    else if (temp <= 18.5) { return "yellow-accent-4"; }
    else if (temp <= 19.0) { return "orange-lighten-4"; }
    else if (temp <= 19.5) { return "orange-lighten-2"; }
    else if (temp <= 20.0) { return "orange"; }
    else if (temp <= 20.5) { return "orange-darken-2"; }
    else if (temp <= 21.0) { return "orange-darken-3"; }
    else if (temp <= 21.5) { return "orange-darken-4"; }
    else if (temp <= 22.0) { return "red"; }
    else if (temp <= 22.5) { return "red-darken-2"; }
    else if (temp <= 23.0) { return "red-darken-3"; }
    else if (temp <= 24.0) { return "red-darken-4"; }
    else if (temp <= 25.0) { return "purple-darken-3"; }
    else if (temp <= 26.0) { return "purple-darken-4"; }
    else if (temp <= 27.0) { return "gray-darken-3"; }
    else {
        return "gray-darken-4";
    }    
}

export interface ValueGradient {
  numValue: number
  gradient: string
}

const temperatureGradients: ValueGradient[] = [
    { numValue: 15, gradient: "#1B5E20" }, // green-darken-4
    { numValue: 16, gradient: "#43A047" }, // green-darken-1
    { numValue: 16.5, gradient: "#AEEA00" }, // lime-accent-4
    { numValue: 17.0, gradient: "#C6FF00" }, // lime-accent-3
    { numValue: 17.5, gradient: "#E4FF1A" }, // lime-accent-2
    { numValue: 18.5, gradient: "#FFF9C4" }, // yellow-lighten-4
    { numValue: 19.0, gradient: "#FFE0B2" }, // orange-lighten-4
    { numValue: 19.5, gradient: "#FFCC80" }, // orange-lighten-2
    { numValue: 20.0, gradient: "#FF9800" }, // orange
    { numValue: 20.5, gradient: "#F57C00" }, // orange-darken-2
    { numValue: 21.0, gradient: "#EF6C00" }, // orange-darken-3
    { numValue: 21.5, gradient: "#E65100" }, // orange-darken-4
    { numValue: 22.0, gradient: "#F44336" }, // red
    { numValue: 22.5, gradient: "#D32F2F" }, // red-darken-2
    { numValue: 23.0, gradient: "#C62828" }, // red-darken-3
    { numValue: 24.0, gradient: "#B71C1C" }, // red-darken-4
    { numValue: 25.0, gradient: "#6A1B9A" }, // purple-darken-3
    { numValue: 26.0, gradient: "#4A148C" }, // purple-darken-4
    { numValue: 27.0, gradient: "#424242" }  // grey-darken-3
];

const humidityGradients: ValueGradient[] = [
    { numValue: 10, gradient: "#CDDC39" }, // lime
    { numValue: 15, gradient: "#E6EE9C" }, // lime-lighten-2
    { numValue: 20, gradient: "#F9FBE7" }, // lime-lighten-4
    { numValue: 25, gradient: "#F9FBE7" }, // lime-lighten-5 (Note: Vuetify lighten-5 is very close to white)
    { numValue: 30, gradient: "#E0F2F1" }, // teal-lighten-5
    { numValue: 35, gradient: "#80CBC4" }, // teal-lighten-3
    { numValue: 40, gradient: "#00BFA5" }, // teal-accent-4
    { numValue: 45, gradient: "#00E5FF" }, // cyan-accent-4
    { numValue: 50, gradient: "#00B0FF" }, // light-blue-accent-4
    { numValue: 55, gradient: "#448AFF" }, // blue-accent-2
    { numValue: 60, gradient: "#1E88E5" }, // blue-darken-1
    { numValue: 65, gradient: "#1976D2" }, // blue-darken-2
    { numValue: 70, gradient: "#1565C0" }, // blue-darken-3
    { numValue: 75, gradient: "#0D47A1" }, // blue-darken-4
    { numValue: 80, gradient: "#283593" }, // indigo-darken-3
    { numValue: 85, gradient: "#1A237E" }, // indigo-darken-4
    { numValue: 90, gradient: "#4A148C" }  // purple-darken-4
];

export interface GraficalData {
  minScale: number
  maxScale: number
  gradient: string[]
}

export function getGraficalTemperatureData(temps: number[], margin: number): GraficalData {

    const minTemp = Math.min(...temps);
    const maxTemp = Math.max(...temps);

    const minScale = Math.floor(minTemp - margin);
    const maxScale = Math.ceil(maxTemp + margin);

    return {
        minScale,
        maxScale,
        gradient: temperatureGradients
            .filter(g => g.numValue >= minScale && g.numValue <= maxScale)
            .map(g => g.gradient)
    };
}

export function getGraficalHumidityData(temps: number[], margin: number): GraficalData {

    const minTemp = Math.min(...temps);
    const maxTemp = Math.max(...temps);

    const minScale = Math.floor((minTemp - margin) / 5) * 5;
    const maxScale = Math.ceil((maxTemp + margin) / 5) * 5;

    return {
        minScale,
        maxScale,
        gradient: humidityGradients
            .filter(g => g.numValue >= minScale && g.numValue <= maxScale)
            .map(g => g.gradient)
    };
}