<script setup lang="ts">
import { ref, type Ref, onMounted } from 'vue'
import type { DataMeasureMents } from '@/models/DataMeasureMents';
import type { Boundaries } from '@/models/Boundaries';
import { getGraficalHumidityData, getGraficalTemperatureData, type GraficalData } from '@/utils/TemperatureDisplay';

const showLastMeasurements: Ref<boolean> = ref(false);
const lastMeasurements: Ref<DataMeasureMents[] | null> = ref<DataMeasureMents[]>([]);

async function getLastHourlyAggregations() {
    // Vite proxies '/api/measurements/hourly' to 'http://localhost:5160/api/measurements/hourly'
    const response = await fetch('/api/measurements/hourly')

    if (!response.ok) {
        showLastMeasurements.value = false;
        console.log("Could not retrieve hourly aggregations");
    }
    else {
        const data = await response.json() as DataMeasureMents[];
        if (data !== null) {
            showLastMeasurements.value = true;
            firstMeasurementTime.value = new Date(data[0].retrievedAt).toLocaleTimeString();
            lastMeasurementTime.value = new Date(data[data.length - 1].retrievedAt).toLocaleTimeString();

            lastMeasurements.value = data;
            temperatures.value = data.map(m => m.insideTemperatureCelsius);
            humidities.value = data.map(m => m.humidityPercentage);

            var graficalTemperatureData: GraficalData = getGraficalTemperatureData(data.map(m => m.insideTemperatureCelsius), 0);
            t_selectedGradient.value = graficalTemperatureData.gradient.reverse();
            minScaleTemperature.value = graficalTemperatureData.minScale;
            maxScaleTemperature.value = graficalTemperatureData.maxScale;

            var graficalHumidityData: GraficalData = getGraficalHumidityData(data.map(m => m.humidityPercentage), 0);
            h_selectedGradient.value = graficalHumidityData.gradient.reverse();
            minScaleHumidity.value = graficalHumidityData.minScale;
            maxScaleHumidity.value = graficalHumidityData.maxScale;

            maxTemperature.value = Math.max(...data.map(m => m.insideTemperatureCelsius));
            minTemperature.value = Math.min(...data.map(m => m.insideTemperatureCelsius));
            maxHumidity.value = Math.max(...data.map(m => m.humidityPercentage));
            minHumidity.value = Math.min(...data.map(m => m.humidityPercentage));

        } else {
            showLastMeasurements.value = false;
            console.log("No measurement data available");
        }
    }
}

async function getLastHourlyDataAggregationBoundaries() {
    // Vite proxies '/api/measurements/hourly/boundaries' to 'http://localhost:5160/api/measurements/hourly/boundaries'
    const response = await fetch('/api/measurements/hourly/boundaries')

    if (!response.ok) {
        console.log("Could not retrieve hourly aggregation boundaries");
    }
    else {
        const data = await response.json() as Boundaries;
        if (data !== null) {
            mindate.value=""+data.oldestItem;
            maxdate.value=""+data.newestItem;
        } 
        else {
            console.log("No Boundaries available");
        }
    }
}

async function getLastHourlyAggregationsForSpecifiedDate() {
    var dateStr = JSON.stringify(date.value);
    var dateStrWithoutBrackets = dateStr.substring(1);
    dateStrWithoutBrackets = dateStrWithoutBrackets.substring(0, dateStrWithoutBrackets.length-1);

    // Vite proxies '/api/measurements/hourly' to 'http://localhost:5160/api/measurements/hourly'
    const response = await fetch('/api/measurements/hourly/'+dateStrWithoutBrackets)

    if (!response.ok) {
        showLastMeasurements.value = false;
        console.log("Could not get LastHourlyAggregationsForSpecifiedDate");
    }
    else {
        const data = await response.json() as DataMeasureMents[];
        if (data !== null) {
            showLastMeasurements.value = true;
            firstMeasurementTime.value = new Date(data[0].retrievedAt).toLocaleTimeString();
            lastMeasurementTime.value = new Date(data[data.length - 1].retrievedAt).toLocaleTimeString();

            lastMeasurements.value = data;
            temperatures.value = data.map(m => m.insideTemperatureCelsius);
            humidities.value = data.map(m => m.humidityPercentage);

            var graficalTemperatureData: GraficalData = getGraficalTemperatureData(data.map(m => m.insideTemperatureCelsius), 0);
            t_selectedGradient.value = graficalTemperatureData.gradient.reverse();
            minScaleTemperature.value = graficalTemperatureData.minScale;
            maxScaleTemperature.value = graficalTemperatureData.maxScale;

            var graficalHumidityData: GraficalData = getGraficalHumidityData(data.map(m => m.humidityPercentage), 0);
            h_selectedGradient.value = graficalHumidityData.gradient.reverse();
            minScaleHumidity.value = graficalHumidityData.minScale;
            maxScaleHumidity.value = graficalHumidityData.maxScale;

            maxTemperature.value = Math.max(...data.map(m => m.insideTemperatureCelsius));
            minTemperature.value = Math.min(...data.map(m => m.insideTemperatureCelsius));
            maxHumidity.value = Math.max(...data.map(m => m.humidityPercentage));
            minHumidity.value = Math.min(...data.map(m => m.humidityPercentage));

        } else {
            showLastMeasurements.value = false;
            console.log("No measurement data available");
        }
    }
}

onMounted(() => {
    getLastHourlyAggregations();
    getLastHourlyDataAggregationBoundaries();
})

const t_selectedGradient: Ref<string[]> = ref([])
const temperatures: Ref<number[]> = ref([])
const h_selectedGradient: Ref<string[]> = ref([])
const humidities: Ref<number[]> = ref([])

const firstMeasurementTime: Ref<string> = ref("");
const lastMeasurementTime: Ref<string> = ref("");

const fill: Ref<boolean> = ref(true)
const padding: Ref<number> = ref(2)
const smooth: Ref<boolean> = ref(true)
const lineWidth: Ref<number> = ref(2)

const minScaleTemperature: Ref<string | number | undefined> = ref(18)
const maxScaleTemperature: Ref<string | number | undefined> = ref(23)
const minScaleHumidity: Ref<string | number | undefined> = ref(30)
const maxScaleHumidity: Ref<string | number | undefined> = ref(60)

const maxTemperature: Ref<number> = ref(2)
const minTemperature: Ref<number> = ref(2)
const maxHumidity: Ref<number> = ref(2)
const minHumidity: Ref<number> = ref(2)

const showSpecificDate: Ref<boolean> = ref(false);
const date: Ref<Date> = ref(new Date());
const mindate: Ref<string> = ref("2026-04-26");
const maxdate: Ref<string> = ref("2026-04-29");

async function hideDatePicker() {
    showSpecificDate.value = false;
}

async function showDatePicker() {
    showSpecificDate.value = true;
}

</script>

<template>
    <v-container>
        <v-card variant="elevated" color="primary" class="mx-auto mt-5" max-width="500">
            <v-row></v-row>
            <v-row>
                <v-col cols="6" class="text-center">
                    <v-btn variant="elevated" @click="hideDatePicker()" prepend-icon="mdi-menu-up-outline" elevated
                        color="secondary" :disabled="!showSpecificDate">
                        Hide Date Picker
                    </v-btn>
                </v-col>
                <v-col cols="6" class="text-center">
                    <v-btn variant="elevated" @click="showDatePicker()" prepend-icon="mdi-calendar" elevated
                        color="secondary" :disabled="showSpecificDate">
                        Pick Date
                    </v-btn>                    
                </v-col>
                <v-col v-if="showSpecificDate" cols="2" class="hidden-sm-and-down"></v-col>
                <v-col v-if="showSpecificDate" cols="10" class="text-center">
                    <v-date-picker v-model="date" :max="maxdate" :min="mindate" @update:model-value="getLastHourlyAggregationsForSpecifiedDate()"></v-date-picker>
                </v-col>
            </v-row>
            <v-row></v-row>
        </v-card>
    </v-container>

    <div v-if="showLastMeasurements">
        <v-row>
            <v-col cols="12" class="text-center">
                <br />
                <span class="text-headline-small">24 Hour Data
                    <br class="hidden-md-and-up" />
                    ({{ firstMeasurementTime }} - {{ lastMeasurementTime }})</span>
            </v-col>
        </v-row>
        <v-row>
            <v-col cols="12" class="text-center">
                <br />
                <span class="text-title-large">Temperature ({{ minTemperature }}°C - {{
                    maxTemperature
                    }}°C)</span>
            </v-col>
        </v-row>
        <!-- Temperature Section -->
        <div class="d-flex align-stretch mb-4 small_border">
            <!-- Labels: They will now stretch/shrink to match the sparkline height -->
            <div class="d-flex flex-column justify-space-between pr-2 py-1 text-caption text-grey-darken-1">
                <div>{{ maxScaleTemperature }}°</div>
                <div>{{ minScaleTemperature }}°</div>
            </div>

            <!-- Sparkline Wrapper: flex-grow-1 ensures it uses all available width -->
            <div class="flex-grow-1">
                <v-sparkline :fill="fill" :gradient="t_selectedGradient" :line-width="lineWidth"
                    :model-value="temperatures" :padding="padding" :smooth="smooth" auto-draw :min="minScaleTemperature"
                    :max="maxScaleTemperature"></v-sparkline>
            </div>
        </div>
        <v-row>
            <v-col cols="12" class="text-center">
                <br />
                <span class="text-title-large">Humidity ({{ minHumidity }}% - {{ maxHumidity
                    }}%)</span>
            </v-col>
        </v-row>
        <!-- Humidity Section -->
        <div class="d-flex align-stretch mb-4 small_border">
            <!-- Labels: They will now stretch/shrink to match the sparkline height -->
            <div class="d-flex flex-column justify-space-between pr-2 py-1 text-caption text-grey-darken-1">
                <div>{{ maxScaleHumidity }}%</div>
                <div>{{ minScaleHumidity }}%</div>
            </div>
            <!-- Sparkline Wrapper: flex-grow-1 ensures it uses all available width -->
            <div class="flex-grow-1">

                <v-sparkline :fill="fill" :gradient="h_selectedGradient" :line-width="lineWidth"
                    :model-value="humidities" :padding="padding" :smooth="smooth" auto-draw :min="minScaleHumidity"
                    :max="maxScaleHumidity"></v-sparkline>
            </div>
        </div>
    </div>
</template>
<style scoped>
.small_border {
    border: 1px solid gray;
    border-radius: 8px;
    padding: 1px;
}
</style>