export interface TadoRetrievalScheduleModel {
    scheduleId: number
    tokenId: number
    interval: number
    nextRetrievalTime: Date
    nextRetrievalTimeString: string
    lastRetrievalTime: Date
    lastRetrievalTimeString: string
    isActive: boolean
    homeId: number
    zoneName: string
    lastError: string
    consecutiveFailures: number
}