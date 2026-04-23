export interface TadoRetrievalScheduleModel {
    scheduleId: number
    tokenId: number
    interval: number
    nextRetrievalTime: Date
    lastRetrievalTime: Date
    isActive: boolean
    homeId: number
    zoneName: string
    lastError: string
    consecutiveFailures: number
}