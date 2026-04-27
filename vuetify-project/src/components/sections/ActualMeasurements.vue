<script setup lang="ts">
import { ref, type Ref, onMounted, computed } from 'vue'
import type { DataMeasureMents } from '@/models/DataMeasureMents';

const showSection: Ref<string, string> = ref("none");
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
            lastMeasurements.value = data;
            console.log("Number of Last Measurements: " + data.length);
        } else {
            showLastMeasurements.value = false;
            console.log("No measurement data available");
        }
    }
}

onMounted(() => {
  getActualMeasurements();
})
</script>

<template>
        <v-row>
            <v-col cols="12" class="text-center">
                <span class="text-headline-small">Actual Measurements</span>
                <br/>
                <span class="text-body-medium">This section shows the current measurements.</span>
                <br/>
                <span class="text-body-medium">..and is not yet implemented.</span>
            </v-col>
        </v-row>
</template>