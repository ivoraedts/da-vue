<script setup lang="ts">
import { ref, type Ref, onMounted, computed } from 'vue'
import InitializeTracking from '@/components/InitializeTracking.vue'
import ScheduleEditor from '@/components/sections/ScheduleEditor.vue'
import DataExplorer from '@/components/sections/DataExplorer.vue'
import type { TadoRetrievalScheduleModel } from '@/models/TadoRetrievalScheduleModel';
import type { LatestMeasurement } from '@/models/LatestMeasurement';
import { getMaterialColorForTemperature } from '@/utils/TemperatureDisplay';

const showSection: Ref<string, string> = ref("overview");

function showOverview() {
    showSection.value = "overview";
    refreshDataIfNeeded();
}

function refreshDataIfNeeded() {
    //refresh overview data if last refresh was more than 2 minutes ago
    if (new Date().getTime() - lastRefreshTime.getTime() > 2 * 60 * 1000) {
        getLatestMeasurement();
        getActiveTrackingSchedule();
    }
}

function showOverviewWithComment(comment: string) {
    if (comment == 'scheduleUpdated') {
        console.log("Updating schedule on request of child component...");
        getLatestMeasurement();
        getActiveTrackingSchedule();
    }
    console.log("Showing overview for " + comment);
    showOverview();
}

function showDataExplorer() {
    showSection.value = "data explorer";
}

function showScheduleEditor() {
    showSection.value = "schedule editor";
}


async function getActiveTrackingSchedule() {
    // Vite proxies '/api/tadotemperature/getCurrentSchedule' to 'http://localhost:5160/api/tadotemperature/getCurrentSchedule'
    const response = await fetch('/api/tadotemperature/getCurrentSchedule')

    if (!response.ok) {
        if (response.status === 404) {
            console.log("No schedule found (404)");
            schedulesAreRetrieved.value = true;
            scheduleExists.value = false;
        } else {
            console.log("Error retrieving active schedule: " + response.statusText);
            schedulesAreRetrieved.value = false;
        }
    }
    else {
        const data = await response.json() as TadoRetrievalScheduleModel;
        if (data !== null) {
            scheduleExists.value = true;
            currentSchedule.value = data;
            currentSchedule.value.nextRetrievalTimeString = new Date(currentSchedule.value.nextRetrievalTime).toLocaleString();
            currentSchedule.value.lastRetrievalTimeString = new Date(currentSchedule.value.lastRetrievalTime).toLocaleString();
            scheduleIsActive.value = data.isActive;
        } else {
            //this should never happen!
            console.log("No active schedule");
            scheduleExists.value = false;
        }
        schedulesAreRetrieved.value = true;
    }
}

async function getLatestMeasurement() {
    // Vite proxies '/api/measurements/latest' to 'http://localhost:5160/api/measurements/latest'
    const response = await fetch('/api/measurements/latest')

    if (!response.ok) {
        showLatestMeasurement.value = false;
        console.log("Could not retrieve latest measurement");
    }
    else {
        const data = await response.json() as LatestMeasurement;
        if (data !== null) {
            showLatestMeasurement.value = true;
            latestMeasurement.value = data;
            console.log("Latest Measurement: " + data.insideTemperatureCelsius + "°C at " + data.retrievedAt);
            setColorBasedOnTemperature(data.insideTemperatureCelsius);
        } else {
            showLatestMeasurement.value = false;
            console.log("No measurement data available");
        }
    }
    lastRefreshTime = new Date();
}

const schedulesAreRetrieved: Ref<boolean> = ref(false);
const scheduleExists: Ref<boolean> = ref(false);
const scheduleIsActive: Ref<boolean> = ref(false);

const canInitializeTracking = computed(() => schedulesAreRetrieved.value && !scheduleIsActive.value);
const canEditCurrentSchedule = computed(() => schedulesAreRetrieved.value && scheduleExists.value);

const currentSchedule: Ref<TadoRetrievalScheduleModel | null> = ref(null);
const currentScheduleColor = computed(() => {
    if (!scheduleIsActive.value) {
        return "grey";
    } else if (currentSchedule.value?.consecutiveFailures && currentSchedule.value.consecutiveFailures > 0) {
        return "orange-darken-3";
    } else {
        return "secondary";
    }
});

const showLatestMeasurement: Ref<boolean> = ref(false);
const latestMeasurement: Ref<LatestMeasurement | null> = ref(null);

const latestMeasurementColor: Ref<string> = ref("grey");
function setColorBasedOnTemperature(temp: number) {
    if (latestMeasurement.value) {
        latestMeasurementColor.value = getMaterialColorForTemperature(temp);
    }
}

var lastRefreshTime: Date = new Date();

onMounted(() => {
    getActiveTrackingSchedule();
    getLatestMeasurement();
});

</script>

<template>
    <v-container v-if="showSection === 'overview'">
        <v-card variant="elevated" color="primary" class="mx-auto mt-5" max-width="500">
            <v-row>
                <v-col cols="12" class="text-center">
                    <h1>Da Home Page</h1>
                </v-col>
            </v-row>
        </v-card>

        <v-card v-if="canInitializeTracking" class="mx-auto mt-5" max-width="500">
            <InitializeTracking />
        </v-card>

        <v-card variant="elevated" :color="latestMeasurementColor" v-if="showLatestMeasurement" class="mx-auto mt-5"
            max-width="500">
            <v-row class="px-4 py-2" align="center">
                <v-col cols="12" class="text-center">
                    <span class="text-headline-small">Latest Measurement</span>

                </v-col>
                <v-col cols="5" class="d-flex align-center justify-center">
                    <v-icon size="x-large" icon="mdi-thermometer"></v-icon>
                    <span class="text-display-small">{{ latestMeasurement?.insideTemperatureCelsius }}</span>
                    <v-icon size="x-large" icon="mdi-temperature-celsius"></v-icon>
                </v-col>
                <v-col cols="5" class="d-flex align-center justify-center">
                    <v-icon size="x-large" icon="mdi-water"></v-icon>
                    <span class="text-display-small">{{ latestMeasurement?.humidityPercentage }}</span>
                    <v-icon size="x-large" icon="mdi-percent"></v-icon>
                </v-col>
                <v-col cols="2" class="text-left">
                    <v-btn icon @click="showDataExplorer()">
                        <v-icon>mdi-chart-line</v-icon>
                    </v-btn>
                </v-col>
                <v-col cols="12" class="text-center">
                    <span class="text-body-small">Retrieved At: {{ new Date(latestMeasurement?.retrievedAt ??
                        "").toLocaleString() }}</span>
                </v-col>
            </v-row>
        </v-card>

        <v-card variant="elevated" :color="currentScheduleColor" v-if="canEditCurrentSchedule" class="mx-auto mt-5"
            max-width="500">
            <v-row class="px-4 py-2" align="center">
                <v-col cols="12" class="text-center">
                    <v-row class="px-4 py-2" align="center">
                        <v-col cols="6" class="text-right">
                            <h2>Tracking Schedule</h2>
                        </v-col>
                        <v-col cols="6" class="text-left">
                            <v-btn icon @click="showScheduleEditor()">
                                <v-icon>mdi-pencil</v-icon>
                            </v-btn>
                        </v-col>
                    </v-row>
                </v-col>
                <v-col v-if="!scheduleIsActive" cols="6" class="text-right">
                    <v-icon color="yellow">mdi-alert</v-icon>
                </v-col>
                <v-col v-if="!scheduleIsActive" cols="6" class="text-left">
                    <h3>Deactivated</h3>
                </v-col>
                <v-col cols="6">
                    <p>Interval: {{ currentSchedule?.interval }} minutes</p>
                    <p>Last Retrieval:</p>
                    <p>{{ currentSchedule?.lastRetrievalTimeString }}</p>
                </v-col>
                <v-col cols="6">
                    <p>Zone name: {{ currentSchedule?.zoneName }}</p>
                    <p>Next Retrieval:</p>
                    <p>{{ currentSchedule?.nextRetrievalTimeString }}</p>
                </v-col>
                <v-col v-if="currentSchedule?.lastError" cols="12">
                    <p v-if="currentSchedule != null && currentSchedule.consecutiveFailures > 0">Fail streak: {{
                        currentSchedule?.consecutiveFailures }}</p>
                    <p>Last Error: {{ currentSchedule?.lastError }}</p>
                </v-col>
            </v-row>
        </v-card>

    </v-container>
    <div v-if="showSection === 'schedule editor'">
        <ScheduleEditor @response="(msg) => showOverviewWithComment(msg.action)" />
    </div>
    <div v-if="showSection === 'data explorer'">
        <DataExplorer @response="(msg) => showOverviewWithComment(msg.action)" />
    </div>
</template>