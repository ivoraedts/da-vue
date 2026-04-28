<script setup lang="ts">
import { ref, type Ref, onMounted } from 'vue'
import type { DataMeasureMents } from '@/models/DataMeasureMents';
import { getGraficalHumidityData, getGraficalTemperatureData, type GraficalData } from '@/utils/TemperatureDisplay';

const showLastMeasurements: Ref<boolean> = ref(false);
const lastMeasurements: Ref<DataMeasureMents[] | null> = ref<DataMeasureMents[]>([]);

async function getActualMeasurements() {
    // Vite proxies '/api/measurements/last10' to 'http://localhost:5160/api/measurements/last10'
    const response = await fetch('/api/measurements/last10')

    if (!response.ok) {
        showLastMeasurements.value = false;
        console.log("Could not retrieve last 10 measurements");
    }
    else {
        const data = await response.json() as DataMeasureMents[];
        if (data !== null) {
            showLastMeasurements.value = true;
            lastMeasurements.value = data.reverse();
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

            console.log("Temperature Gradient: " + t_selectedGradient.value);
            console.log("Temperature Scale: " + minScaleTemperature.value + " - " + maxScaleTemperature.value);
            console.log("Humidity Gradient: " + h_selectedGradient.value);
            console.log("Humidity Scale: " + minScaleHumidity.value + " - " + maxScaleHumidity.value);

            maxTemperature.value = Math.max(...data.map(m => m.insideTemperatureCelsius));
            minTemperature.value = Math.min(...data.map(m => m.insideTemperatureCelsius));
            maxHumidity.value = Math.max(...data.map(m => m.humidityPercentage));
            minHumidity.value = Math.min(...data.map(m => m.humidityPercentage));
            lastMeasurementTime.value = new Date(data[0].retrievedAt).toLocaleTimeString();
            firstMeasurementTime.value = new Date(data[data.length - 1].retrievedAt).toLocaleTimeString();

        } else {
            showLastMeasurements.value = false;
            console.log("No measurement data available");
        }
    }
}

onMounted(() => {
    getActualMeasurements();
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
</script>

<template>
    <div v-if="showLastMeasurements">
        <v-row>
            <v-col cols="12" class="text-center">
                <br />
                <span class="text-headline-small">Actual Data Measurements ({{ firstMeasurementTime }} - {{
                    lastMeasurementTime }})</span>
            </v-col>
        </v-row>
        <v-row>
            <v-col cols="12" class="text-center">
                <br />
                <span class="text-title-medium">Actual Temperature Measurements ({{ minTemperature }}°C - {{
                    maxTemperature
                    }}°C)</span>
            </v-col>
        </v-row>
        <!-- Temperature Section -->
        <div class="d-flex align-stretch mb-4">
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
                <span class="text-title-medium">Actual Humidity Measurements ({{ minHumidity }}% - {{ maxHumidity
                    }}%)</span>
            </v-col>
        </v-row>
        <!-- Humidity Section -->
        <div class="d-flex align-stretch mb-4">
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