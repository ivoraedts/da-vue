<script setup lang="ts">
import { ref, type Ref, onMounted } from 'vue'
import type { DataMeasureMents } from '@/models/DataMeasureMents';

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
            temperatures.value = data.map(m => m.insideTemperatureCelsius);
            humidities.value = data.map(m => m.humidityPercentage);

            setTemperatureGradient(data[0].insideTemperatureCelsius);
            setHumidityGradient(data[0].humidityPercentage);
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

    function setTemperatureGradient(lastTemperature: number) {
        console.log("Last Temperature: " + lastTemperature);
        if (lastTemperature < 18) { t_selectedGradient.value = colder_temperature_gradients; }
        else if (lastTemperature < 20) { t_selectedGradient.value = cold_temperature_gradients; }        
        else if (lastTemperature < 22) { t_selectedGradient.value = medium_temperature_gradients; }
        else if (lastTemperature < 24) { t_selectedGradient.value = warm_temperature_gradients; }
        else {
            t_selectedGradient.value = hot_temperature_gradients;
        }
    }

    function setHumidityGradient(lastHumidity: number) {
        console.log("Last Humidity: " + lastHumidity);
        if (lastHumidity < 40) { h_selectedGradient.value = low_humidity_gradients; }
        else if (lastHumidity < 60) { h_selectedGradient.value = medium_humidity_gradients; }
        else {
            h_selectedGradient.value = high_humidity_gradients;
        }
    }

}

onMounted(() => {
  getActualMeasurements();
})

  var colder_temperature_gradients: string[] = ['#EEFF41','#C6FF00','#AEEA00', '#43A047', '#1B5E20'];
  var cold_temperature_gradients: string[] = ['#FF9800', '#FFC107', '#FFD54F', '#FFEB3B', '#FFF9C4'];
  var medium_temperature_gradients: string[] = ['red', 'orange'];
  var warm_temperature_gradients: string[] = ['#B71C1C', 'red'];
  var hot_temperature_gradients: string[] = ['#black', '#4A148C', '#6A1B9A', '#B71C1C'];

  var low_humidity_gradients: string[] = ['#1976D2', '#FFFF00'];
  var medium_humidity_gradients: string[] = ['#1976D2', '#2196F3', '#E3F2FD'];
  var high_humidity_gradients: string[] = ['#0D47A1', '#2196F3'];

  const t_selectedGradient: Ref<string[]> = ref(medium_temperature_gradients)
  const temperatures: Ref<number[]> = ref([0, 2, 5, 9, 5, 10, 3, 5, 0, 0, 1, 8, 2, 9, 0])

  const h_selectedGradient: Ref<string[]> = ref(medium_humidity_gradients)
  const humidities: Ref<number[]> = ref([0, 2, 5, 9, 5, 10, 3, 5, 0, 0, 1, 8, 2, 9, 0])

  const firstMeasurementTime: Ref<string> = ref("");
  const lastMeasurementTime: Ref<string> = ref("");

  const fill: Ref<boolean> = ref(true)
  const padding: Ref<number> = ref(2)
  const smooth: Ref<boolean> = ref(true)
  const lineWidth: Ref<number> = ref(2)

  const maxTemperature: Ref<number> = ref(2)
  const minTemperature: Ref<number> = ref(2)
  const maxHumidity: Ref<number> = ref(2)
  const minHumidity: Ref<number> = ref(2)
</script>

<template>
        <v-row>
            <v-col cols="12" class="text-center">
                <br/>
                <span class="text-headline-small">Actual Data Measurements ({{ firstMeasurementTime }} - {{ lastMeasurementTime }})</span>
            </v-col>
        </v-row>
        <v-row>
            <v-col cols="12" class="text-center">
                <br/>
                <span class="text-title-medium">Actual Temperature Measurements ({{ minTemperature }}°C - {{ maxTemperature }}°C)</span>
            </v-col>
        </v-row>
    <v-container fluid>
    <v-sparkline
      :fill="fill"
      :gradient="t_selectedGradient"
      :line-width="lineWidth"
      :model-value="temperatures"
      :padding="padding"
      :smooth="smooth"
      auto-draw
    ></v-sparkline>
    </v-container>
            <v-row>
            <v-col cols="12" class="text-center">
                <br/>
                <span class="text-title-medium">Actual Humidity Measurements ({{ minHumidity }}% - {{ maxHumidity }}%)</span>
            </v-col>
        </v-row>
    <v-container fluid>
    <v-sparkline
      :fill="fill"
      :gradient="h_selectedGradient"
      :line-width="lineWidth"
      :model-value="humidities"
      :padding="padding"
      :smooth="smooth"
      auto-draw
    ></v-sparkline>
    </v-container>
</template>