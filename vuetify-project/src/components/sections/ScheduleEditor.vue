<script setup lang="ts">
import { ref, type Ref, onMounted, computed } from 'vue'
import type { TadoRetrievalScheduleModel } from '@/models/TadoRetrievalScheduleModel';

const emit = defineEmits(['response'])
function goBackToOverview() {
    emit('response', { action: 'goBackToOverview' });
}

async function getActiveTrackingSchedule() {
    // Vite proxies '/api/tadotemperature/getCurrentSchedule' to 'http://localhost:5160/api/tadotemperature/getCurrentSchedule'
    const response = await fetch('/api/tadotemperature/getCurrentSchedule')

    if (!response.ok) {
        if (response.status === 404) {
            console.log("No schedule found (404)");
        } else {
            console.log("Error retrieving active schedule: " + response.statusText);
        }
    }
    else {
        const data = await response.json() as TadoRetrievalScheduleModel;
        if (data !== null) {
            currentSchedule.value = data;
            currentSchedule.value.nextRetrievalTimeString = new Date(currentSchedule.value.nextRetrievalTime).toLocaleString();
            currentSchedule.value.lastRetrievalTimeString = new Date(currentSchedule.value.lastRetrievalTime).toLocaleString();
        } else {
            //this should never happen!
            console.log("Schedule data is null");
        }
    }
}

async function editItem() {
  const response = await fetch(`/api/tadotemperature/editSchedule/${currentSchedule.value?.scheduleId}`, {
    method: 'PUT',
    headers: {
      'Content-Type': 'application/json'
    },
    body: JSON.stringify(currentSchedule.value)
  })
  if (!response.ok)
  {
    alert("Could not update task. Is the Backend running?");
    console.error(`API error: ${response.status}`)
    alert(response);
  }
  else
  {
    emit('response', { action: 'scheduleUpdated' });
  }
}

const currentSchedule: Ref<TadoRetrievalScheduleModel | null> = ref(null);

const min = ref(1)
const max = ref(120)

onMounted(() => {
    getActiveTrackingSchedule();
});
</script>

<template>
    <v-container>
        <v-card class="mx-auto mt-5" max-width="500">
            <v-row>
                <v-col cols="12" class="text-center">
                    <h1>Schedule editor</h1>
                </v-col>
            </v-row>
            <v-row v-if="currentSchedule != null" class="px-4 py-2" align="center">
                <v-col cols="12">
                    <span class="text-headline-small">Interval in minutes:</span>
                    <v-slider v-model="currentSchedule.interval" :max="max" :min="min" :step="1" class="align-center"
                        hide-details color="green" thumb-color="orange">
                        <template v-slot:append>
                            <v-text-field v-model="currentSchedule.interval" density="compact" style="width: 70px"
                                type="number" hide-details single-line></v-text-field>
                        </template>
                    </v-slider>
                </v-col>
                <v-col cols="6" class="text-right"><span class="text-headline-small">Active:</span></v-col>
                <v-col cols="6"><v-checkbox-btn v-model="currentSchedule.isActive"></v-checkbox-btn></v-col>
            </v-row>
            <v-row v-if="currentSchedule != null" class="px-4 py-2" align="center">
                <v-col cols="12">
                    <v-divider :thickness="4" color="primary"></v-divider>
                </v-col>
            </v-row>
            <v-row v-if="currentSchedule != null" class="px-4 py-2" align="center">
                <v-col cols="6" class="text-center">
                    <v-btn variant="tonal" @click="editItem()" prepend-icon="mdi-undo" elevated color="red">
                        Save
                    </v-btn>
                </v-col>
                <v-col cols="6" class="text-center">
                    <v-btn variant="tonal" @click="goBackToOverview()" prepend-icon="mdi-undo" elevated color="red">
                        Cancel
                    </v-btn>
                </v-col>
            </v-row>
            <v-row v-if="currentSchedule == null" class="px-4 py-2" align="center">
                <v-col cols="12" class="text-center">
                    <v-btn variant="tonal" @click="goBackToOverview()" prepend-icon="mdi-undo" elevated color="red">
                        Cancel
                    </v-btn>
                </v-col>
            </v-row>
            <v-row v-if="currentSchedule != null" class="px-4 py-2" align="center">
                <v-col cols="12">
                    <v-divider :thickness="4" color="primary"></v-divider>
                </v-col>
            </v-row>
            <v-row v-if="currentSchedule != null" class="px-4 py-2" align="center">
                <v-col cols="12">
                    <p>Next Retrieval Time: {{ currentSchedule.nextRetrievalTimeString }}</p>
                    <p>Last Retrieval Time: {{ currentSchedule.lastRetrievalTimeString }}</p>
                    <p v-if="currentSchedule.consecutiveFailures > 0">Consecutive Failures: {{
                        currentSchedule.consecutiveFailures
                    }}</p>
                    <p v-if="currentSchedule.lastError != null">Last Error: {{ currentSchedule.lastError }}</p>
                </v-col>
            </v-row>
        </v-card>
    </v-container>
</template>