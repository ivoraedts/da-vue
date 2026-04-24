<script setup lang="ts">
import { ref, type Ref, onMounted, computed } from 'vue'
const emit = defineEmits(['response'])

function goBackToOverview() {
    emit('response', { action: 'goBackToOverview' });
}

const showSection: Ref<string, string> = ref("actual measurements");

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
        <v-card class="mx-auto mt-5" max-width="500">
            <v-row>
                <v-col cols="12" class="text-center">
                    <v-btn-toggle v-model="showSection" divided>
                        <v-btn value="actual measurements">
                            <span class="hidden-sm-and-down">Actual</span>
                            <v-icon end>
                                mdi-border-radius
                            </v-icon>
                        </v-btn>
                        <v-btn value="last day measurements">
                            <span class="hidden-sm-and-down">Day</span>

                            <v-icon end>
                                mdi-hours-24
                            </v-icon>
                        </v-btn>
                        <v-btn value="last week measurements">
                            <span class="hidden-sm-and-down">Week</span>

                            <v-icon end>
                                mdi-calendar-week
                            </v-icon>
                        </v-btn>
                        <v-btn value="last month measurements">
                            <span class="hidden-sm-and-down">Month</span>
                            <v-icon end>
                                mdi-calendar-month
                            </v-icon>
                        </v-btn>
                    </v-btn-toggle>
                </v-col>
            </v-row>
        </v-card>
        <p></p>
        <v-row>
            <v-col cols="12" class="text-center">
                <v-btn variant="elevated" @click="goBackToOverview()" prepend-icon="mdi-undo" elevated color="red">
                    Back to overview
                </v-btn>
            </v-col>
        </v-row>
    </v-container>
</template>