<script setup lang="ts">
import { ref, type Ref, onMounted } from 'vue'
import type { DataMeasureMents } from '@/models/DataMeasureMents';
import type { Boundaries } from '@/models/Boundaries';
import { getGraficalHumidityData, getGraficalTemperatureData, type GraficalData } from '@/utils/TemperatureDisplay';

const showLastMeasurements: Ref<boolean> = ref(false);
const lastMeasurements: Ref<DataMeasureMents[] | null> = ref<DataMeasureMents[]>([]);

async function getLastMonthlyAggregations() {
    // Get current date and subtract one month
    const now = new Date();
    const lastMonth = new Date(now.getFullYear(), now.getMonth() - 1, 1);
    const year = lastMonth.getFullYear();
    const month = lastMonth.getMonth() + 1; // JS months are 0-based

    // Vite proxies '/api/measurements/daily/month/{year}/{month}' to backend
    const response = await fetch(`/api/measurements/daily/month/${year}/${month}`)

    if (!response.ok) {
        showLastMeasurements.value = false;
        console.log("Could not retrieve monthly aggregations");
    }
    else {
        const data = await response.json() as DataMeasureMents[];
        if (data !== null && data.length > 0) {
            showLastMeasurements.value = true;
            firstMeasurementDate.value = new Date(data[0].retrievedAt).toLocaleDateString();
            lastMeasurementDate.value = new Date(data[data.length - 1].retrievedAt).toLocaleDateString();

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
            console.log("No measurement data available for last month");
        }
    }
}

async function getLastMonthlyDataAggregationBoundaries() {
    // Vite proxies '/api/measurements/daily/boundaries' to backend
    const response = await fetch('/api/measurements/daily/boundaries')

    if (!response.ok) {
        console.log("Could not retrieve daily aggregation boundaries");
    }
    else {
        const data = await response.json() as Boundaries;
        if (data !== null) {
            mindate.value = new Date(data.oldestItem).toISOString().split('T')[0]; // YYYY-MM-DD
            maxdate.value = new Date(data.newestItem).toISOString().split('T')[0];
        } 
        else {
            console.log("No Boundaries available");
        }
    }
}

async function getLastMonthlyAggregationsForSpecifiedMonth() {
    const selectedDate = new Date(month.value);
    const year = selectedDate.getFullYear();
    const monthNum = selectedDate.getMonth() + 1;

    // Vite proxies '/api/measurements/daily/month/{year}/{month}' to backend
    const response = await fetch(`/api/measurements/daily/month/${year}/${monthNum}`)

    if (!response.ok) {
        showLastMeasurements.value = false;
        console.log("Could not get LastMonthlyAggregationsForSpecifiedMonth");
    }
    else {
        const data = await response.json() as DataMeasureMents[];
        if (data !== null && data.length > 0) {
            showLastMeasurements.value = true;
            firstMeasurementDate.value = new Date(data[0].retrievedAt).toLocaleDateString();
            lastMeasurementDate.value = new Date(data[data.length - 1].retrievedAt).toLocaleDateString();

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
            console.log("No measurement data available for selected month");
        }
    }
}

onMounted(() => {
    getLastMonthlyAggregations();
    getLastMonthlyDataAggregationBoundaries();
})

const t_selectedGradient: Ref<string[]> = ref([])
const temperatures: Ref<number[]> = ref([])
const h_selectedGradient: Ref<string[]> = ref([])
const humidities: Ref<number[]> = ref([])

const firstMeasurementDate: Ref<string> = ref("");
const lastMeasurementDate: Ref<string> = ref("");

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

const showSpecificMonth: Ref<boolean> = ref(false);
const month: Ref<Date> = ref(new Date());
const mindate: Ref<string> = ref("2026-04-01");
const maxdate: Ref<string> = ref("2026-04-29");

async function hideMonthPicker() {
    showSpecificMonth.value = false;
}

async function showMonthPicker() {
    showSpecificMonth.value = true;
}

</script>
<template>
    <v-container>
        <v-card variant="elevated" color="primary" class="mx-auto mt-5" max-width="500">
            <v-row></v-row>
            <v-row>
                <v-col cols="6" class="text-center">
                    <v-btn variant="elevated" @click="hideMonthPicker()" prepend-icon="mdi-menu-up-outline" elevated
                        color="secondary" :disabled="!showSpecificMonth">
                        Hide Month Picker
                    </v-btn>
                </v-col>
                <v-col cols="6" class="text-center">
                    <v-btn variant="elevated" @click="showMonthPicker()" prepend-icon="mdi-calendar" elevated
                        color="secondary" :disabled="showSpecificMonth">
                        Pick Month
                    </v-btn>                    
                </v-col>
                <v-col v-if="showSpecificMonth" cols="2" class="hidden-sm-and-down"></v-col>
                <v-col v-if="showSpecificMonth" cols="10" class="text-center">
                    <v-date-picker v-model="month" :max="maxdate" :min="mindate" type="month" @update:model-value="getLastMonthlyAggregationsForSpecifiedMonth()"></v-date-picker>
                </v-col>
            </v-row>
            <v-row></v-row>
        </v-card>
    </v-container>

    <div v-if="showLastMeasurements">
        <v-row>
            <v-col cols="12" class="text-center">
                <br />
                <span class="text-headline-small">Monthly Data
                    <br class="hidden-md-and-up" />
                    ({{ firstMeasurementDate }} - {{ lastMeasurementDate }})</span>
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