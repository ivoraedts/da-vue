export interface LatestMeasurement{
    retrievalId: number
    homeId: number
    zoneName: string
    insideTemperatureCelsius: number
    humidityPercentage: number
    retrievedAt: Date
}