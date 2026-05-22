<script setup lang="ts">
import { ref, type Ref, onMounted, computed } from 'vue'
import { useDisplay } from 'vuetify'
import InitializeTracking from '@/components/InitializeTracking.vue'
import ScheduleEditor from '@/components/sections/ScheduleEditor.vue'
import DataExplorer from '@/components/sections/DataExplorer.vue'
import type { TadoRetrievalScheduleModel } from '@/models/TadoRetrievalScheduleModel';
import type { DataMeasureMents } from '@/models/DataMeasureMents';
import { getMaterialColorForTemperature } from '@/utils/TemperatureDisplay';
import logoSmall from '@/assets/LogoIncludingTado-small.png'
import logoMedium from '@/assets/LogoIncludingTado-medium.png'
import logoLarge from '@/assets/LogoIncludingTado-large.png'
import tadoSmall from '@/assets/tado-small.png'
import tadoMedium from '@/assets/tado-medium.png'
import tadoLarge from '@/assets/tado-large.png'

const { name } = useDisplay()

const logoSrc: Ref<string> = computed(() => {
    switch (name.value) {
        case 'xs':
        case 'sm':
        case 'md':
            return logoSmall
        case 'lg':
            return logoMedium
        case 'xl':
        case 'xxl':
        default:
            return logoLarge
    }
})

const tadoSrc: Ref<string> = computed(() => {
    switch (name.value) {
        case 'xs':
        case 'sm':
        case 'md':
            return tadoSmall
        case 'lg':
        case 'xl':
            return tadoMedium
        case 'xxl':
        default:
            return tadoLarge
    }
})

const displayItemSize: Ref<string> = computed(() => {
    switch (name.value) {
        case 'xs':
            return "2.5rem";
        case 'sm':
            return "4rem";
        case 'md':
            return "4rem";
        case 'lg':
            return "5.5rem";
        case 'xl':
            return "7.5rem";
        case 'xxl':
        default:
            return "10.5rem";
    }
})

const flexButtonSize: Ref<string> = computed(() => {
    switch (name.value) {
        case 'xs':
            return "1.4rem";
        case 'sm':
            return "2rem";
        case 'md':
            return "2.1rem";
        case 'lg':
            return "3.2rem";
        case 'xl':
            return "5rem";
        case 'xxl':
        default:
            return "8rem";
    }
})

const showSection: Ref<string> = ref("overview");

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
                    <v-img class="responsive-logo" :src="logoSrc" />
                </v-col>
            </v-row>
            <v-row class="pre-header-margin"></v-row>
            <v-row>
                <v-col cols="12" class="text-center">
                    <span class="responsive-home-title">Da Home Page</span>
                </v-col>
            </v-row>
        </v-card>

        <v-card v-if="canInitializeTracking" variant="elevated" color="secondary" class="mx-auto mt-5" max-width="100%">
            <v-row class="px-4 py-2" align="center">
                <v-col cols="12" class="text-center">
                    <v-btn color="primary" @click="showInitializeTracking()" min-width="80%" size="x-large">
                        <span class="responsive-sub-title">
                        Initialize Tado Temperature Tracking
                        </span>
                    </v-btn>
                </v-col>
            </v-row>
        </v-card>

        <v-card variant="elevated" :color="latestMeasurementColor" v-if="showLatestMeasurement" class="mx-auto mt-5">
            <v-row class="px-4 py-2" align="center">
                <v-col cols="12" class="text-center">
                    <span class="responsive-sub-title">Latest Measurement</span>
                </v-col>
            </v-row>
            <v-row class="responsive-small-negative-margin"></v-row>
            <v-row>
                <v-col cols="12" class="text-center">
                    <v-img class="tado-display dont-print-on-front" :src="tadoSrc" />
                </v-col>
            </v-row>
            <v-row class="responsive-print-over-image-negative-margin"></v-row>
            <v-row>
                <v-col cols="3" sm="4" class="text-right">
                    <v-icon :size="displayItemSize" icon="mdi-thermometer"></v-icon>
                </v-col>
                <v-col cols="6" sm="4" class="d-flex align-left ">
                    <span class="thermostat-display">{{ latestMeasurement?.insideTemperatureCelsius }}</span>
                </v-col>
                <v-col cols="3" sm="4" class="text-left">
                    <v-icon :size="displayItemSize" icon="mdi-temperature-celsius"></v-icon>
                </v-col>
            </v-row>
            <v-row :class="$vuetify.display.xs ? 'reduce-margin-more' : 'reduce-margin'">
            </v-row>
            <v-row>
                <v-col cols="3" sm="4" class="text-right">
                    <v-icon :size="displayItemSize" icon="mdi-water"></v-icon>
                </v-col>
                <v-col cols="6" sm="4" class="d-flex align-left ">
                    <span class="thermostat-display">{{ latestMeasurement?.humidityPercentage }}</span>
                </v-col>
                <v-col cols="3" sm="4" class="text-left">
                    <v-icon :size="displayItemSize" icon="mdi-percent"></v-icon>
                </v-col>
            </v-row>
            <v-row :class="$vuetify.display.xs ? 'reduce-margin-more' : 'reduce-margin'">
            </v-row>
            <v-row>
                <v-col cols="12" class="text-center">
                    <v-btn class="custom-flex-btn" @click="showDataExplorer()">
                        <v-icon :size="flexButtonSize">mdi-chart-line</v-icon>
                    </v-btn>
                </v-col>
            </v-row>
            <v-row class="responsive-go-after-image-margin"></v-row>
            <v-row>
                <v-col cols="12" class="text-center">
                    <span class="responsive-footer-text">Retrieved At: {{ new Date(latestMeasurement?.retrievedAt ??
                        "").toLocaleString() }}</span>
                </v-col>
            </v-row>
        </v-card>

        <v-card variant="elevated" :color="currentScheduleColor" v-if="canEditCurrentSchedule" class="mx-auto mt-5"
            max-width="100%">
            <v-row class="px-4 py-2" align="center">
                <v-col cols="12" class="text-center">
                    <v-row class="px-4 py-2" align="center">
                        <v-col cols="6" class="text-right">
                            <span class="responsive-sub-title">Tracking Schedule</span>
                        </v-col>
                        <v-col cols="6" class="text-left">
                            <v-btn icon @click="showScheduleEditor()">
                                <v-icon>mdi-pencil</v-icon>
                            </v-btn>
                        </v-col>
                    </v-row>
                </v-col>
                <v-col v-if="!scheduleIsActive" cols="6" class="text-right">
                    <v-icon :size="displayItemSize" color="yellow">mdi-alert</v-icon>
                </v-col>
                <v-col v-if="!scheduleIsActive" cols="6" class="text-left">
                    <spam class="responsive-sub-title">Deactivated</spam>
                </v-col>
                <v-col cols="6">
                    <p>
                        <span class="responsive-body-text">Interval: </span><br />
                        <span class="responsive-body-value-text">{{ currentSchedule?.interval }} minutes</span>
                    </p>
                                        <p>
                        <span class="responsive-body-text">Last Retrieval: </span><br />
                        <span class="responsive-body-value-text">{{ currentSchedule?.lastRetrievalTimeString }}</span>
                    </p>
                </v-col>
                <v-col cols="6">
                    <p>
                        <span class="responsive-body-text">Zone name: </span><br />
                        <span class="responsive-body-value-text">{{ currentSchedule?.zoneName }}</span>
                    </p>
                    <p>
                        <span class="responsive-body-text">Next Retrieval:</span><br />
                        <span class="responsive-body-value-text">{{ currentSchedule?.nextRetrievalTimeString }}</span>
                    </p>
                </v-col>
                <v-col v-if="currentSchedule?.lastError" cols="12">
                    <p v-if="currentSchedule != null && currentSchedule.consecutiveFailures > 0">Fail streak: {{
                        currentSchedule?.consecutiveFailures }}</p>
                    <p>
                        <span class="responsive-body-text">Last Error: </span><br />
                        <span class="responsive-body-value-text">{{ currentSchedule?.lastError }}</span>
                    </p>
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
/* Mobile First Defaults (xs) */
.pre-header-margin {
    --v-col-gap-y: -30px;
}

.responsive-home-title {
    font-size: 2rem;
    font-weight: 400;
    line-height: 2.5rem;
    letter-spacing: 0;
}

.responsive-sub-title {
    font-size: 0.95rem;
    font-weight: 550;
    line-height: 1.25rem;
    letter-spacing: 0rem;
}

.responsive-body-text {
    opacity: 0.7;
    font-size: 0.875rem;
    font-weight: 500;
    line-height: 1.25rem;
    letter-spacing: 0.00625rem;
}

.responsive-body-value-text {
    font-size: 0.875rem;
    font-weight: 500;
    line-height: 1.25rem;
    letter-spacing: 0.00625rem;
}

.responsive-footer-text {
    font-size: 0.75rem;
    font-weight: 400;
    line-height: 1rem;
    letter-spacing: 0.025rem;
}


.responsive-logo {
    height: 150px;
}

.responsive-small-negative-margin {
    --v-col-gap-y: -65px;
}

.tado-display {
    height: 390px;
}

.responsive-print-over-image-negative-margin {
    --v-col-gap-y: -318px;
}

.responsive-go-after-image-margin {
    --v-col-gap-y: 20px;
}

.thermostat-display {
    font-family: 'DotoDigital', sans-serif;
    font-weight: 400;
    color: rgb(255, 255, 235);
    /* Extra styling for that LED look */
    letter-spacing: 0.1em;
    text-shadow: 0 0 5px yellow;

    font-size: 3rem;
    margin-bottom: 14px;
    margin-top: -18px;
}

/* Small screens (sm) and up */
@media (min-width: 600px) {
    .pre-header-margin {
        --v-col-gap-y: -30px;
    }

    .responsive-home-title {
        font-size: 2.25rem;
        font-weight: 400;
        line-height: 2.75rem;
        letter-spacing: 0;
    }

    .responsive-sub-title {
        font-size: 1.375rem;
        font-weight: 400;
        line-height: 1.75rem;
        letter-spacing: 0;
    }

    .responsive-body-text {
    opacity: 0.7;
    font-size: 1.1rem;
    font-weight: 400;
    line-height: 1.25rem;
    letter-spacing: 0;
}

.responsive-body-value-text {
    font-size: 1.1rem;
    font-weight: 400;
    line-height: 1.25rem;
    letter-spacing: 0;
}

    .responsive-footer-text {
        font-size: 1rem;
        font-weight: 400;
        line-height: 1.5rem;
        letter-spacing: 0.03125rem;
    }

    .responsive-logo {
        height: 200px;
    }

    .responsive-small-negative-margin {
        --v-col-gap-y: -37px;
    }

    .tado-display {
        height: 440px;
    }

    .responsive-print-over-image-negative-margin {
        --v-col-gap-y: -320px;
    }

    .responsive-go-after-image-margin {
        --v-col-gap-y: 15px;
    }

    .thermostat-display {
        font-size: 4rem;
        margin-bottom: -13px;
        margin-top: -20px;
    }
}

/* Medium screens (md) and up */
@media (min-width: 960px) {
    .pre-header-margin {
        --v-col-gap-y: -30px;
    }

    .responsive-home-title {
        font-size: 2.8125rem;
        font-weight: 400;
        line-height: 3.25rem;
        letter-spacing: 0;
    }

    .responsive-sub-title {
        font-size: 1.8rem;
        font-weight: 400;
        line-height: 2rem;
        letter-spacing: 0;
    }

        .responsive-body-text {
    opacity: 0.7;
    font-size: 1.2rem;
    font-weight: 400;
    line-height: 1.5rem;
    letter-spacing: 0;
}

.responsive-body-value-text {
    font-size: 1.2rem;
    font-weight: 400;
    line-height: 1.5rem;
    letter-spacing: 0;
}

    .responsive-footer-text {
        font-size: 1.375rem;
        font-weight: 400;
        line-height: 1.75rem;
        letter-spacing: 0;
    }

    .responsive-logo {
        height: 250px;
    }

    .responsive-small-negative-margin {
        --v-col-gap-y: -37px;
    }

    .tado-display {
        height: 450px;
    }

    .responsive-print-over-image-negative-margin {
        --v-col-gap-y: -360px;
    }

    .thermostat-display {
        font-size: 4.1rem;
        margin-bottom: 6px;
        margin-top: -20px;
    }

    .responsive-go-after-image-margin {
        --v-col-gap-y: 10px;
    }
}

/* Large screens (lg) and up */
@media (min-width: 1280px) {
    .pre-header-margin {
        --v-col-gap-y: -30px;
    }

    .responsive-home-title {
        font-size: 3.5625rem;
        font-weight: 400;
        line-height: 4rem;
        letter-spacing: -0.015625rem;
    }

    .responsive-sub-title {
        font-size: 2.5rem;
        font-weight: 400;
        line-height: 2.5rem;
        letter-spacing: 0;
    }

            .responsive-body-text {
    opacity: 0.7;
    font-size: 1.8rem;
    font-weight: 400;
    line-height: 2.5rem;
    letter-spacing: 0;
}

.responsive-body-value-text {
    font-size: 1.8rem;
    font-weight: 400;
    line-height: 2.5rem;
    letter-spacing: 0;
}

    .responsive-footer-text {
        font-size: 1.5rem;
        font-weight: 400;
        line-height: 2rem;
        letter-spacing: 0;
    }

    .responsive-logo {
        height: 300px;
    }

    .responsive-small-negative-margin {
        --v-col-gap-y: -42px;
    }

    .tado-display {
        height: 600px;
    }

    .responsive-print-over-image-negative-margin {
        --v-col-gap-y: -500px;
    }

    .responsive-go-after-image-margin {
        --v-col-gap-y: 22px;
    }

    .thermostat-display {
        font-size: 5.5rem;
        font-weight: 500;
        margin-bottom: 28px;
        margin-top: -23px;
    }
}

/* Extra Large (xl) and up */
@media (min-width: 1904px) {
    .pre-header-margin {
        --v-col-gap-y: -30px;
    }

    .responsive-home-title {
        font-size: 4rem;
        font-weight: 450;
        line-height: 4.5rem;
    }

    .responsive-sub-title {
        font-size: 3rem;
        font-weight: 450;
        line-height: 3.5rem;
        letter-spacing: 0;
    }

                .responsive-body-text {
    opacity: 0.7;
    font-size: 2.5rem;
    font-weight: 400;
    line-height: 3rem;
    letter-spacing: 0.05rem;
}

.responsive-body-value-text {
    font-size: 2.5rem;
    font-weight: 400;
    line-height: 3rem;
    letter-spacing: 0.05rem;
}

    .responsive-footer-text {
        font-size: 2rem;
        font-weight: 400;
        line-height: 2.5rem;
        letter-spacing: 0rem;
    }

    .responsive-logo {
        height: 350px;
    }

    .responsive-small-negative-margin {
        --v-col-gap-y: -45px;
    }

    .tado-display {
        height: 800px;
    }

    .responsive-print-over-image-negative-margin {
        --v-col-gap-y: -630px;
    }

    .responsive-go-after-image-margin {
        --v-col-gap-y: 75px;
    }

    .thermostat-display {
        font-size: 8rem;
        font-weight: 500;
        margin-bottom: 19px;
        margin-top: -36px;
    }
}

/* Ultra Large (xxl) */
@media (min-width: 2560px) {
    .pre-header-margin {
        --v-col-gap-y: -35px;
    }

    .responsive-home-title {
        font-size: 5.5rem;
        font-weight: 500;
        line-height: 6rem;
    }

    .responsive-sub-title {
        font-size: 3.5625rem;
        font-weight: 400;
        line-height: 4rem;
        letter-spacing: -0.015625rem;
    }

    .responsive-body-text {
    opacity: 0.7;
    font-size: 2.8rem;
    font-weight: 400;
    line-height: 3.5rem;
    letter-spacing: 0.05rem;
}

.responsive-body-value-text {
    font-size: 2.8rem;
    font-weight: 400;
    line-height: 3.5rem;
    letter-spacing: 0.05rem;
}

    .responsive-footer-text {
        font-size: 2.25rem;
        font-weight: 400;
        line-height: 2.75rem;
        letter-spacing: 0;
    }

    .responsive-logo {
        height: 400px;
    }

    .responsive-small-negative-margin {
        --v-col-gap-y: -55px;
    }

    .tado-display {
        height: 1100px;
    }

    .responsive-print-over-image-negative-margin {
        --v-col-gap-y: -850px;
    }

    .thermostat-display {
        font-size: 11rem;
        font-weight: 600;
        margin-bottom: 29px;
        margin-top: -42px;
    }
}

.reduce-margin {
    --v-col-gap-y: 7px;
}

.reduce-margin-more {
    --v-col-gap-y: 1px;
}



.dont-print-on-front {
    z-index: -1;
}

.custom-flex-btn {
    /* Reset Vuetify's native button constraints */
    min-width: unset !important;
    height: auto !important;
    padding: 1rem !important;
    border-radius: 50%;
    /* Re-creates the circular look */
}
</style>