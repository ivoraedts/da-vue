<script setup lang="ts">
import { ref, type Ref, onMounted, computed } from 'vue'
import InitializeTracking from '@/components/InitializeTracking.vue'
import ScheduleEditor from '@/components/sections/ScheduleEditor.vue'
import DataExplorer from '@/components/sections/DataExplorer.vue'
import type { TadoRetrievalScheduleModel } from '@/models/TadoRetrievalScheduleModel';
import type { DataMeasureMents } from '@/models/DataMeasureMents';
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
    if (comment == 'scheduleUpdated' || comment == 'scheduleActivated') {
        console.log("Updating schedule on request of child component...");
        getLatestMeasurement();
        getActiveTrackingSchedule();
    }
    console.log("Showing overview for " + comment);
    showOverview();
}

function showInitializeTracking() {
    showSection.value = "initialize tracking";
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
        const data = await response.json() as DataMeasureMents;
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
const latestMeasurement: Ref<DataMeasureMents | null> = ref(null);

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
        <v-card variant="elevated" color="primary" class="mx-auto" max-width="100%">
            <v-row>
                <v-col cols="12" class="text-center">
                    <v-img v-if="$vuetify.display.xs" height="150" src="@/assets/LogoIncludingTado-small.png" />
                    <v-img v-if="$vuetify.display.sm" height="200" src="@/assets/LogoIncludingTado-small.png" />
                    <v-img v-if="$vuetify.display.md" height="250" src="@/assets/LogoIncludingTado-small.png" />
                    <v-img v-if="$vuetify.display.lg" height="300" src="@/assets/LogoIncludingTado-medium.png" />
                    <v-img v-if="$vuetify.display.xlAndUp" height="350" src="@/assets/LogoIncludingTado-large.png" />
                </v-col>
            </v-row>
            <v-row class="negative-margin negative-bottom-margin">
                <v-col cols="12" class="text-center">
                    <h1>Da Home Page</h1>
                </v-col>
            </v-row>
        </v-card>

        <v-card v-if="canInitializeTracking" variant="elevated" color="secondary" class="mx-auto mt-5" max-width="100%">
            <v-row class="px-4 py-2" align="center">
                <v-col cols="12" class="text-center">
                    <v-btn color="primary" @click="showInitializeTracking()" min-width="80%" size="x-large">
                        Initialize Tado Temperature Tracking
                    </v-btn>
                </v-col>
            </v-row>
        </v-card>

        <v-card variant="elevated" :color="latestMeasurementColor" v-if="showLatestMeasurement" class="mx-auto mt-5"
            max-width="100%">
            <v-row class="px-4 py-2" align="center">
                <v-col cols="12" class="text-center">
                    <span class="text-headline-small">Latest Measurement</span>
                </v-col>
                <v-col cols="12" class="text-center">
                    <v-img v-if="$vuetify.display.xs" height="370" src="@/assets/tado-small.png" class="dont-print-on-front" />
                    <v-img v-if="$vuetify.display.sm" height="440" src="@/assets/tado-small.png" class="dont-print-on-front" />
                    <v-img v-if="$vuetify.display.md" height="450" src="@/assets/tado-small.png" class="dont-print-on-front" />
                    <v-img v-if="$vuetify.display.lg" height="600" src="@/assets/tado-medium.png" class="dont-print-on-front" />
                    <v-img v-if="$vuetify.display.xlAndUp" height="800" src="@/assets/tado-medium.png" class="dont-print-on-front" />
                </v-col>
            </v-row>
            <v-row v-if="$vuetify.display.xs" class="huge-negative-margin-xs"></v-row>
            <v-row v-if="$vuetify.display.sm" class="huge-negative-margin-sm"></v-row>
            <v-row v-if="$vuetify.display.md" class="huge-negative-margin-md"></v-row>
            <v-row v-if="$vuetify.display.lg" class="huge-negative-margin-lg"></v-row>
            <v-row v-if="$vuetify.display.xlAndUp" class="huge-negative-margin-xlAndUp"></v-row>
            <v-row>
                <v-col cols="3" sm="4" class="text-right">
                    <v-icon v-if="$vuetify.display.xs" size="2.5rem" icon="mdi-thermometer"></v-icon>
                    <v-icon v-if="$vuetify.display.sm" size="4rem" icon="mdi-thermometer"></v-icon>
                    <v-icon v-if="$vuetify.display.md" size="4rem" icon="mdi-thermometer"></v-icon>
                    <v-icon v-if="$vuetify.display.lg" size="5.5rem" icon="mdi-thermometer"></v-icon>
                    <v-icon v-if="$vuetify.display.xlAndUp" size="7.5rem" icon="mdi-thermometer"></v-icon>
                </v-col>
                <v-col cols="6" sm="4" class="d-flex align-left ">
                    <span v-if="$vuetify.display.xs" class="thermostat-display-xs">{{ latestMeasurement?.insideTemperatureCelsius }}</span>
                    <span v-if="$vuetify.display.sm" class="thermostat-display-sm">{{ latestMeasurement?.insideTemperatureCelsius }}</span>
                    <span v-if="$vuetify.display.md" class="thermostat-display-md">{{ latestMeasurement?.insideTemperatureCelsius }}</span>
                    <span v-if="$vuetify.display.lg" class="thermostat-display-lg">{{ latestMeasurement?.insideTemperatureCelsius }}</span>
                    <span v-if="$vuetify.display.xlAndUp" class="thermostat-display-xlAndUp">{{ latestMeasurement?.insideTemperatureCelsius }}</span>
                </v-col>
                <v-col cols="3" sm="4" class="text-left">
                    <v-icon v-if="$vuetify.display.xs" size="2.5rem" icon="mdi-temperature-celsius"></v-icon>
                    <v-icon v-if="$vuetify.display.sm" size="4rem" icon="mdi-temperature-celsius"></v-icon>
                    <v-icon v-if="$vuetify.display.md" size="4rem" icon="mdi-temperature-celsius"></v-icon>
                    <v-icon v-if="$vuetify.display.lg" size="5.5rem" icon="mdi-temperature-celsius"></v-icon>
                    <v-icon v-if="$vuetify.display.xlAndUp" size="7.5rem" icon="mdi-temperature-celsius"></v-icon>
                </v-col>
            </v-row>
            <v-row :class="$vuetify.display.xs ? 'reduce-margin-more' : 'reduce-margin'">
            </v-row>
            <v-row>
                <v-col cols="3" sm="4" class="text-right">
                    <v-icon v-if="$vuetify.display.xs" size="2.5rem" icon="mdi-water"></v-icon>
                    <v-icon v-if="$vuetify.display.sm" size="4rem" icon="mdi-water"></v-icon>
                    <v-icon v-if="$vuetify.display.md" size="4rem" icon="mdi-water"></v-icon>
                    <v-icon v-if="$vuetify.display.lg" size="5.5rem" icon="mdi-water"></v-icon>
                    <v-icon v-if="$vuetify.display.xlAndUp" size="7.5rem" icon="mdi-water"></v-icon>
                </v-col>
                <v-col cols="6" sm="4" class="d-flex align-left ">
                    <span v-if="$vuetify.display.xs" class="thermostat-display-xs">{{ latestMeasurement?.humidityPercentage }}</span>
                    <span v-if="$vuetify.display.sm" class="thermostat-display-sm">{{ latestMeasurement?.humidityPercentage }}</span>
                    <span v-if="$vuetify.display.md" class="thermostat-display-md">{{ latestMeasurement?.humidityPercentage }}</span>
                    <span v-if="$vuetify.display.lg" class="thermostat-display-lg">{{ latestMeasurement?.humidityPercentage }}</span>
                    <span v-if="$vuetify.display.xlAndUp" class="thermostat-display-xlAndUp">{{ latestMeasurement?.humidityPercentage }}</span>
                </v-col>
                <v-col cols="3" sm="4" class="text-left">
                    <v-icon v-if="$vuetify.display.xs"  size="2.5rem" icon="mdi-percent"></v-icon>
                    <v-icon v-if="$vuetify.display.sm"  size="4rem" icon="mdi-percent"></v-icon>
                    <v-icon v-if="$vuetify.display.md"  size="4rem" icon="mdi-percent"></v-icon>
                    <v-icon v-if="$vuetify.display.lg"  size="5.5rem" icon="mdi-percent"></v-icon>
                    <v-icon v-if="$vuetify.display.xlAndUp"  size="7.5rem" icon="mdi-percent"></v-icon>
                </v-col>
            </v-row>
            <v-row :class="$vuetify.display.xs ? 'reduce-margin-more' : 'reduce-margin'">
            </v-row>
            <v-row>
                <v-col cols="12" class="text-center">
                    <v-btn class="custom-flex-btn" @click="showDataExplorer()">
                        <v-icon  v-if="$vuetify.display.xs" size="1rem">mdi-chart-line</v-icon>
                        <v-icon  v-if="$vuetify.display.sm" size="2rem">mdi-chart-line</v-icon>
                        <v-icon  v-if="$vuetify.display.md" size="2.1rem">mdi-chart-line</v-icon>
                        <v-icon  v-if="$vuetify.display.lg" size="3.2rem">mdi-chart-line</v-icon>
                        <v-icon  v-if="$vuetify.display.xlAndUp" size="5rem">mdi-chart-line</v-icon>
                    </v-btn>
                </v-col>
                <v-col cols="12" class="text-center">
                    <br v-if="$vuetify.display.sm" ></br>
                    <br v-if="$vuetify.display.md" ></br>
                    <br v-if="$vuetify.display.lg" ></br>
                    <br v-if="$vuetify.display.xlAndUp" ></br>
                    <br v-if="$vuetify.display.xlAndUp" ></br>
                    <span v-if="$vuetify.display.xs" class="text-body-small">Retrieved At: {{ new Date(latestMeasurement?.retrievedAt ?? "").toLocaleString() }}</span>
                    <span v-if="$vuetify.display.sm" class="text-body-large">Retrieved At: {{ new Date(latestMeasurement?.retrievedAt ?? "").toLocaleString() }}</span>
                    <span v-if="$vuetify.display.md" class="text-headline-small">Retrieved At: {{ new Date(latestMeasurement?.retrievedAt ?? "").toLocaleString() }}</span>
                    <span v-if="$vuetify.display.lg" class="text-headline-large">Retrieved At: {{ new Date(latestMeasurement?.retrievedAt ?? "").toLocaleString() }}</span>
                    <span v-if="$vuetify.display.xlAndUp" class="text-display-small">Retrieved At: {{ new Date(latestMeasurement?.retrievedAt ?? "").toLocaleString() }}</span>
                </v-col>
            </v-row>
        </v-card>

        <v-card variant="elevated" :color="currentScheduleColor" v-if="canEditCurrentSchedule" class="mx-auto mt-5"
            max-width="100%">
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
    <div v-if="showSection === 'initialize tracking'">
        <InitializeTracking @response="(msg) => showOverviewWithComment(msg.action)" />
    </div>

    <div v-if="showSection === 'data explorer'">
        <DataExplorer @response="(msg) => showOverviewWithComment(msg.action)" />
    </div>

    <div v-if="showSection === 'schedule editor'">
        <ScheduleEditor @response="(msg) => showOverviewWithComment(msg.action)" />
    </div>
</template>

<style scoped>
.negative-margin {
    --v-col-gap-y: -35px;
}

.reduce-margin {
    --v-col-gap-y: 7px;
}

.reduce-margin-more {
    --v-col-gap-y: 1px;
}

.negative-bottom-margin {
    margin-bottom: -15px;
}

.huge-negative-margin-xs {
    --v-col-gap-y: -270px;
}
.huge-negative-margin-sm {
    --v-col-gap-y: -320px;
}
.huge-negative-margin-md {
    --v-col-gap-y: -370px;
}
.huge-negative-margin-lg {
    --v-col-gap-y: -490px;
}
.huge-negative-margin-xlAndUp {
    --v-col-gap-y: -630px;
}
.dont-print-on-front {
    z-index: -1;
}

.thermostat-display,
.thermostat-display-xs,
.thermostat-display-sm,
.thermostat-display-md,
.thermostat-display-lg,
.thermostat-display-xlAndUp {
    font-family: 'DotoDigital', sans-serif;
    font-size: 3rem;
    font-weight: 400;
    margin-bottom: -13px;
    margin-top: -18px;
    color: rgb(255, 255, 235);
    /* Extra styling for that LED look */
    letter-spacing: 0.1em;
    text-shadow: 0 0 5px yellow;
}

.thermostat-display-sm
{
    font-size: 4rem;
    margin-bottom: -13px;
    margin-top: -20px;
}
.thermostat-display-md
{
    font-size: 4.1rem;
    margin-bottom: 6px;
    margin-top: -20px;
}
.thermostat-display-lg
{
    font-size: 5.5rem;
    font-weight: 500;
    margin-bottom: 19px;
    margin-top: -23px;
}
.thermostat-display-xlAndUp
{
    font-size: 8rem;
    font-weight: 500;
    margin-bottom: 19px;
    margin-top: -36px;
}

.custom-flex-btn {
  /* Reset Vuetify's native button constraints */
  min-width: unset !important; 
  height: auto !important;
  padding: 1rem !important;
  border-radius: 50%; /* Re-creates the circular look */
}
</style>