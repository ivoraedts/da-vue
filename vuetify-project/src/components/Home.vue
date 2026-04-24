<script setup lang="ts">
import { ref, type Ref, onMounted, computed } from 'vue'
import InitializeTracking from '@/components/InitializeTracking.vue'
import ScheduleEditor from '@/components/sections/ScheduleEditor.vue'
import type { TadoRetrievalScheduleModel } from '@/models/TadoRetrievalScheduleModel';
import type { LatestMeasurement } from '@/models/LatestMeasurement';

const showSection: Ref<string, string> = ref("overview");

function showOverview() {
    showSection.value = "overview";
}

function showOverviewWithComment(name: string) {
    console.log("Showing overview for " + name);
    showOverview();
}

function showDataExplorer() {
    showSection.value = "data explorer";
}

function showScheduleEditor() {
    showSection.value = "schedule editor";
}


async function getActiveTrackingSchedule() {
    // Vite proxies '/api/tadotemperature/getActiveSchedule' to 'http://localhost:5160/api/tadotemperature/getActiveSchedule'
    const response = await fetch('/api/tadotemperature/getActiveSchedule')

    if (!response.ok) {
        console.log("No active schedule");
        activeScheduleExists.value = false;
    }
    else {
        const data = await response.json() as TadoRetrievalScheduleModel;
        if (data !== null) {
            console.log("Active Schedule: " + data.scheduleId);
            activeScheduleExists.value = true;
            activeSchedule.value = data;
            activeSchedule.value.nextRetrievalTimeString = new Date(activeSchedule.value.nextRetrievalTime).toLocaleString();
            activeSchedule.value.lastRetrievalTimeString = new Date(activeSchedule.value.lastRetrievalTime).toLocaleString();
        } else {
            console.log("No active schedule");
            activeScheduleExists.value = false;
        }
        activeSchedulesAreRetrieved.value = true;
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
}

const activeSchedulesAreRetrieved: Ref<boolean> = ref(false);
const activeScheduleExists: Ref<boolean> = ref(false);

const canInitializeTracking = computed(() => activeSchedulesAreRetrieved.value && !activeScheduleExists.value);
const canEditActiveSchedule = computed(() => activeSchedulesAreRetrieved.value && activeScheduleExists.value);

const activeSchedule: Ref<TadoRetrievalScheduleModel | null> = ref(null);

const showLatestMeasurement: Ref<boolean> = ref(false);
const latestMeasurement: Ref<LatestMeasurement | null> = ref(null);

const latestMeasurementColor: Ref<string> = ref("grey");
function setColorBasedOnTemperature(temp: number) {
    if (latestMeasurement.value) {

        if (temp < 15) { latestMeasurementColor.value = "green-darken-4"; }
        else if (temp < 16) { latestMeasurementColor.value = "green-darken-1"; }
        else if (temp < 16.5) { latestMeasurementColor.value = "lime-accent-4"; }
        else if (temp <= 17.0) { latestMeasurementColor.value = "lime-accent-3"; }
        else if (temp <= 17.5) { latestMeasurementColor.value = "lime-accent-2"; }
        else if (temp <= 18.5) { latestMeasurementColor.value = "yellow-lighten-4"; }
        else if (temp <= 18.5) { latestMeasurementColor.value = "yellow-accent-4"; }
        else if (temp <= 19.0) { latestMeasurementColor.value = "orange-lighten-4"; }
        else if (temp <= 19.5) { latestMeasurementColor.value = "orange-lighten-2"; }
        else if (temp <= 20.0) { latestMeasurementColor.value = "orange"; }
        else if (temp <= 20.5) { latestMeasurementColor.value = "orange-darken-2"; }
        else if (temp <= 21.0) { latestMeasurementColor.value = "orange-darken-3"; }
        else if (temp <= 21.5) { latestMeasurementColor.value = "orange-darken-4"; }
        else if (temp <= 22.0) { latestMeasurementColor.value = "red"; }
        else if (temp <= 22.5) { latestMeasurementColor.value = "red-darken-2"; }
        else if (temp <= 23.0) { latestMeasurementColor.value = "red-darken-3"; }
        else if (temp <= 24.0) { latestMeasurementColor.value = "red-darken-4"; }
        else if (temp <= 25.0) { latestMeasurementColor.value = "purple-darken-3"; }
        else if (temp <= 26.0) { latestMeasurementColor.value = "purple-darken-4"; }
        else if (temp <= 27.0) { latestMeasurementColor.value = "gray-darken-3"; }
        else {
            latestMeasurementColor.value = "gray-darken-4";
        }
    }
}

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
                    <h2>Latest Measurement</h2>
                </v-col>
                <v-col cols="6">
                    <p>Inside Temperature: {{ latestMeasurement?.insideTemperatureCelsius }} °C</p>
                </v-col>
                <v-col cols="6">
                    <p>Humidity: {{ latestMeasurement?.humidityPercentage }} %</p>
                </v-col>
                <v-col cols="12" class="text-center">
                    <p>Retrieved At: {{ new Date(latestMeasurement?.retrievedAt ?? "").toLocaleString() }}</p>
                </v-col>
            </v-row>
        </v-card>

        <v-card variant="elevated" color="secondary" v-if="canEditActiveSchedule" class="mx-auto mt-5" max-width="500">
            <v-row class="px-4 py-2" align="center">
                <v-col cols="12" class="text-center">
                    <v-row class="px-4 py-2" align="center">
                        <v-col cols="6" class="text-right">
                            <h2>Active Schedule</h2>
                        </v-col>
                        <v-col cols="6" class="text-left">
                            <v-btn icon @click="showScheduleEditor()">
                                <v-icon>mdi-pencil</v-icon>
                            </v-btn>
                        </v-col>
                    </v-row>
                </v-col>
                <v-col cols="6">
                    <p>Schedule ID: {{ activeSchedule?.scheduleId }}</p>
                    <p>Interval: {{ activeSchedule?.interval }} minutes</p>
                    <p>Zone name: {{ activeSchedule?.zoneName }}</p>
                    <p>Fail streak: {{ activeSchedule?.consecutiveFailures }}</p>
                </v-col>
                <v-col cols="6">
                    <p>Next Retrieval:</p>
                    <p>{{ activeSchedule?.nextRetrievalTimeString }}</p>
                    <p>Last Retrieval:</p>
                    <p>{{ activeSchedule?.lastRetrievalTimeString }}</p>
                </v-col>
                <v-col v-if="activeSchedule?.lastError" cols="12" class="text-center">
                    <p>Last Error: {{ activeSchedule?.lastError }}</p>
                </v-col>
            </v-row>
        </v-card>

    </v-container>
    <div v-if="showSection === 'schedule editor'">
        <ScheduleEditor @response="(msg) => showOverviewWithComment(msg.action)" />
    </div>
</template>