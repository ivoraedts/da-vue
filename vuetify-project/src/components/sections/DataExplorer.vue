<script setup lang="ts">
import { ref, type Ref, onMounted, computed } from 'vue'
import ActualMeasurements from '@/components/sections/ActualMeasurements.vue'
import LastDayMeasurements from '@/components/sections/LastDayMeasurements.vue'
import LastWeekMeasurements from '@/components/sections/LastWeekMeasurements.vue'
import LastMonthMeasurements from '@/components/sections/LastMonthMeasurements.vue'
const emit = defineEmits(['response'])

function goBackToOverview() {
    emit('response', { action: 'goBackToOverview' });
}

const showSection: Ref<string, string> = ref("none");

function showActualMeasurements() {
    showSection.value = "actual measurements";
}

function showLastDayMeasurements() {
    showSection.value = "last day measurements";
}

function showLastWeekMeasurements() {
    showSection.value = "last week measurements";
}

function showLastMonthMeasurements() {
    showSection.value = "last month measurements";
}
</script>

<template>
    <v-container>
                <v-card variant="elevated" color="primary" class="mx-auto mt-5" max-width="500">
            <v-row>
                <v-col cols="12" class="text-center">
                    <h1>Data Explorer</h1>
                </v-col>
            </v-row>
        <v-row>
            <v-col cols="12" class="text-center">
                <v-btn-toggle v-model="showSection" divided rounded="xl" border>
                    <v-btn value="actual measurements">
                        <span>Now</span>
                        <v-icon end>
                            mdi-border-radius
                        </v-icon>
                    </v-btn>
                    <v-btn value="last day measurements">
                        <span>Day</span>
                        <v-icon end>
                            mdi-hours-24
                        </v-icon>
                    </v-btn>
                    <v-btn value="last week measurements">
                        <span>Week</span>
                        <v-icon end>
                            mdi-calendar-week
                        </v-icon>
                    </v-btn>
                    <v-btn value="last month measurements">
                        <span>Month</span>
                        <v-icon end>
                            mdi-calendar-month
                        </v-icon>
                    </v-btn>
                </v-btn-toggle>
            </v-col>
        </v-row>
        <v-row></v-row>
        </v-card>
        <div v-if="showSection === 'actual measurements'">
            <ActualMeasurements />
        </div>
        <div v-else-if="showSection === 'last day measurements'">
            <LastDayMeasurements />
        </div>
        <div v-else-if="showSection === 'last week measurements'">
            <LastWeekMeasurements />
        </div>
        <div v-else-if="showSection === 'last month measurements'">
            <LastMonthMeasurements />
        </div>
        <v-row>
            <v-col cols="12" class="text-center">
                <v-btn variant="elevated" @click="goBackToOverview()" prepend-icon="mdi-undo" elevated color="red">
                    Back to overview
                </v-btn>
            </v-col>
        </v-row>
    </v-container>
</template>