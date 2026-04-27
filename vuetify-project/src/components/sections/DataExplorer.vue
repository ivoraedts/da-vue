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
        <v-row>
            <v-col cols="12" class="text-center">
                <span class="text-headline-small">Data Explorer</span>
            </v-col>
        </v-row>
        <v-row>
            <v-col cols="12" class="text-center">
                <v-btn-toggle v-model="showSection" divided rounded="xl" border>
                    <v-btn value="actual measurements">
                        <span class="hidden-xs">Actual</span>
                        <v-icon end>
                            mdi-border-radius
                        </v-icon>
                    </v-btn>
                    <v-btn value="last day measurements">
                        <span class="hidden-xs">Day</span>
                        <v-icon end>
                            mdi-hours-24
                        </v-icon>
                    </v-btn>
                    <v-btn value="last week measurements">
                        <span class="hidden-xs">Week</span>
                        <v-icon end>
                            mdi-calendar-week
                        </v-icon>
                    </v-btn>
                    <v-btn value="last month measurements">
                        <span class="hidden-xs">Month</span>
                        <v-icon end>
                            mdi-calendar-month
                        </v-icon>
                    </v-btn>
                </v-btn-toggle>
            </v-col>
        </v-row>
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