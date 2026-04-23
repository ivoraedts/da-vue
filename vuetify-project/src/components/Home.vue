<script setup lang="ts">
import { ref, type Ref, onMounted, computed } from 'vue'
import InitializeTracking from '@/components/InitializeTracking.vue'
import type { TadoRetrievalScheduleModel } from '@/models/TadoRetrievalScheduleModel';

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
    }
    activeSchedulesAreRetrieved.value = true;
}

const activeSchedulesAreRetrieved: Ref<boolean> = ref(false);
const activeScheduleExists: Ref<boolean> = ref(false);

const canInitializeTracking = computed(() => activeSchedulesAreRetrieved.value && !activeScheduleExists.value);
const canEditActiveSchedule = computed(() => activeSchedulesAreRetrieved.value && activeScheduleExists.value);

const activeSchedule: Ref<TadoRetrievalScheduleModel | null> = ref(null);


onMounted(() => {
    getActiveTrackingSchedule();
});

</script>

<template>
    <v-container>
        <v-card class="mx-auto mt-5" max-width="500">
            <v-row>
                <v-col cols="12" class="text-center">
                    <h1>Da Home Page</h1>
                </v-col>
            </v-row>

            <div v-if="canInitializeTracking">
                <InitializeTracking />
            </div>

            <div v-if="canEditActiveSchedule">
                <v-row class="px-4 py-2" align="center">
                    <v-col cols="12" class="text-center">
                        <h2>Active Schedule</h2>
                    </v-col>
                    <v-col cols="6">
                        <p>Schedule ID: {{ activeSchedule?.scheduleId }}</p>
                        <p>Interval: {{ activeSchedule?.interval }} minutes</p>
                        <p>Zone name: {{ activeSchedule?.zoneName }}</p>
                        <p>consecutiveFailures: {{ activeSchedule?.consecutiveFailures }}</p>
                    </v-col>
                    <v-col cols="6">
                        <p>Next Retrieval:</p><p>{{ activeSchedule?.nextRetrievalTimeString }}</p>
                        <p>Last Retrieval:</p><p>{{ activeSchedule?.lastRetrievalTimeString }}</p>                        
                    </v-col>
                    <v-col v-if="activeSchedule?.lastError" cols="12" class="text-center">
                        <p>Last Error: {{ activeSchedule?.lastError }}</p>
                    </v-col>
                </v-row>
                <v-row class="px-4 py-2" align="center">
                    <v-col cols="12" class="text-center">
                        <h2>Edit Active Schedule</h2>
                    </v-col>
                    <v-col cols="12" class="text-center">
                        <p>Editing functionality not implemented yet.</p>
                    </v-col>
                </v-row>
            </div>

        </v-card>
    </v-container>
</template>