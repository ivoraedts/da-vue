<script setup lang="ts">
import { ref, type Ref, onMounted } from 'vue'
import type { Boundaries } from '@/models/Boundaries';
import { getGraficalHumidityData, getGraficalTemperatureData, type GraficalData } from '@/utils/TemperatureDisplay';

interface DayPartMeasurements {
  insideTemperatureCelsius: number
  humidityPercentage: number
  retrievedAt: string
  dayPart: string
}

const showMeasurements: Ref<boolean> = ref(false)
const measurements: Ref<DayPartMeasurements[]> = ref([])

const firstMeasurementDate: Ref<string> = ref('')
const lastMeasurementDate: Ref<string> = ref('')

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

const temperatures: Ref<number[]> = ref([])
const humidities: Ref<number[]> = ref([])
const dayParts: Ref<string[]> = ref([])
const timeLabels: Ref<string[]> = ref([])

const showSpecificDate: Ref<boolean> = ref(false)
const selectedDate: Ref<Date> = ref(new Date())
const mindate: Ref<string> = ref('2026-04-24')
const maxdate: Ref<string> = ref('2026-04-30')

const showDetails: Ref<boolean> = ref(false)

function getDateKey(dateString: string) {
  return new Date(dateString).toLocaleDateString(undefined, {
    month: 'short',
    day: 'numeric',
  })
}

function sortMeasurements(data: DayPartMeasurements[]) {
  return data.slice().sort((a, b) => new Date(a.retrievedAt).getTime() - new Date(b.retrievedAt).getTime())
}

function updateGraphicData(data: DayPartMeasurements[]) {
  const sorted = sortMeasurements(data)

  measurements.value = sorted
  temperatures.value = sorted.map(m => m.insideTemperatureCelsius)
  humidities.value = sorted.map(m => m.humidityPercentage)
  dayParts.value = sorted.map(m => m.dayPart)
  timeLabels.value = sorted.map(m => new Date(m.retrievedAt).toLocaleTimeString(undefined, { hour: '2-digit', minute: '2-digit' }))

  firstMeasurementDate.value = getDateKey(sorted[0].retrievedAt)
  lastMeasurementDate.value = getDateKey(sorted[sorted.length - 1].retrievedAt)

  const temperatureData: GraficalData = getGraficalTemperatureData(temperatures.value, 0)
  t_selectedGradient.value = temperatureData.gradient.reverse()
  minScaleTemperature.value = temperatureData.minScale
  maxScaleTemperature.value = temperatureData.maxScale

  const humidityData: GraficalData = getGraficalHumidityData(humidities.value, 0)
  h_selectedGradient.value = humidityData.gradient.reverse()
  minScaleHumidity.value = humidityData.minScale
  maxScaleHumidity.value = humidityData.maxScale

  maxTemperature.value = Math.max(...temperatures.value)
  minTemperature.value = Math.min(...temperatures.value)
  maxHumidity.value = Math.max(...humidities.value)
  minHumidity.value = Math.min(...humidities.value)
}

async function getLastWeeklyDayPartAggregations() {
  const response = await fetch('/api/measurements/daypart')

  if (!response.ok) {
    showMeasurements.value = false
    console.log('Could not retrieve weekly daypart aggregations')
    return
  }

  const data = await response.json() as DayPartMeasurements[]
  if (data && data.length > 0) {
    showMeasurements.value = true
    updateGraphicData(data)
  }
  else {
    showMeasurements.value = false
    console.log('No weekly daypart data available')
  }
}

async function getDayPartAggregationBoundaries() {
  const response = await fetch('/api/measurements/daypart/boundaries')

  if (!response.ok) {
    console.log('Could not retrieve daypart aggregation boundaries')
    return
  }

  const data = await response.json() as Boundaries
  if (data) {
    const minDate = new Date(data.oldestItem)
    minDate.setDate(minDate.getDate() + 1)
    mindate.value = minDate.toISOString().split('T')[0]
    const maxDate = new Date(data.newestItem)
    maxDate.setDate(maxDate.getDate() + 1)
    maxdate.value = maxDate.toISOString().split('T')[0]
  }
  else {
    console.log('No daypart boundaries available')
  }
}

async function getDayPartAggregationsForSpecifiedDate() {
  const dateString = selectedDate.value.toISOString().split('T')[0]
  const response = await fetch(`/api/measurements/daypart/${dateString}`)

  if (!response.ok) {
    showMeasurements.value = false
    console.log('Could not retrieve daypart aggregations for specified date')
    return
  }

  const data = await response.json() as DayPartMeasurements[]
  if (data && data.length > 0) {
    showMeasurements.value = true
    updateGraphicData(data)
  }
  else {
    showMeasurements.value = false
    console.log('No daypart data available for selected date')
  }
}

function hideDatePicker() {
  showSpecificDate.value = false
}

function showDatePicker() {
  showSpecificDate.value = true
}

function toggleDetails() {
  showDetails.value = !showDetails.value
}

const t_selectedGradient: Ref<string[]> = ref([])
const h_selectedGradient: Ref<string[]> = ref([])

onMounted(() => {
  getLastWeeklyDayPartAggregations()
  getDayPartAggregationBoundaries()
})
</script>

<template>
  <v-container>
    <v-card variant="elevated" color="primary" class="mx-auto mt-5" max-width="600">
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
          <v-date-picker v-model="selectedDate" :max="maxdate" :min="mindate" type="date"
            @update:model-value="getDayPartAggregationsForSpecifiedDate()"></v-date-picker>
        </v-col>
      </v-row>
      <v-row></v-row>
    </v-card>
  </v-container>

  <div v-if="showMeasurements">
    <v-row>
      <v-col cols="12" class="text-center">
        <br />
        <span class="text-headline-small">Weekly Daypart Data</span>
        <br class="hidden-md-and-up" />
        <span class="text-body-medium">({{ firstMeasurementDate }} - {{ lastMeasurementDate }})</span>
      </v-col>
    </v-row>

    <v-row>
      <v-col cols="12" class="text-center">
        <br />
        <span class="text-title-large">Temperature ({{ minTemperature }}°C - {{ maxTemperature }}°C)</span>
      </v-col>
    </v-row>

    <div class="d-flex align-stretch mb-4 small_border">
      <div class="d-flex flex-column justify-space-between pr-2 py-1 text-caption text-grey-darken-1">
        <div>{{ maxScaleTemperature }}°</div>
        <div>{{ minScaleTemperature }}°</div>
      </div>
      <div class="flex-grow-1">
        <v-sparkline :fill="fill" :gradient="t_selectedGradient" :line-width="lineWidth"
          :model-value="temperatures" :padding="padding" :smooth="smooth" auto-draw :min="minScaleTemperature"
          :max="maxScaleTemperature"></v-sparkline>
      </div>
    </div>

    <v-row>
      <v-col cols="12" class="text-center">
        <br />
        <span class="text-title-large">Humidity ({{ minHumidity }}% - {{ maxHumidity }}%)</span>
      </v-col>
    </v-row>

    <div class="d-flex align-stretch mb-4 small_border">
      <div class="d-flex flex-column justify-space-between pr-2 py-1 text-caption text-grey-darken-1">
        <div>{{ maxScaleHumidity }}%</div>
        <div>{{ minScaleHumidity }}%</div>
      </div>
      <div class="flex-grow-1">
        <v-sparkline :fill="fill" :gradient="h_selectedGradient" :line-width="lineWidth"
          :model-value="humidities" :padding="padding" :smooth="smooth" auto-draw :min="minScaleHumidity"
          :max="maxScaleHumidity"></v-sparkline>
      </div>
    </div>

    <v-row>
      <v-col cols="12" class="text-center">
        <span class="text-title-medium">Daypart details</span>
      </v-col>
    </v-row>

    <v-row>
      <v-col cols="12" class="text-center">
        <v-btn variant="elevated" @click="toggleDetails()" prepend-icon="mdi-table" elevated
          color="secondary">
          {{ showDetails ? 'Hide Details' : 'Show Details' }}
        </v-btn>
      </v-col>
    </v-row>

    <v-row v-if="showDetails">
      <v-col cols="12">
        <v-simple-table class="mx-auto" style="max-width: 800px;">
          <thead>
            <tr>
              <th class="text-left px-3">Date</th>
              <th class="text-left px-3">Time</th>
              <th class="text-left px-3">Day Part</th>
              <th class="text-right px-3">Temp °C</th>
              <th class="text-right px-3">Humidity %</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="item in measurements" :key="item.retrievedAt + item.dayPart">
              <td class="px-3">{{ new Date(item.retrievedAt).toLocaleDateString() }}</td>
              <td class="px-3">{{ new Date(item.retrievedAt).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' }) }}</td>
              <td class="px-3">{{ item.dayPart }}</td>
              <td class="text-right px-3">{{ item.insideTemperatureCelsius.toFixed(1) }}</td>
              <td class="text-right px-3">{{ item.humidityPercentage.toFixed(1) }}</td>
            </tr>
          </tbody>
        </v-simple-table>
      </v-col>
    </v-row>
    <v-row v-if="!showDetails"></v-row>
  </div>
</template>

<style scoped>
.small_border {
  border: 1px solid gray;
  border-radius: 8px;
  padding: 1px;
}
</style>