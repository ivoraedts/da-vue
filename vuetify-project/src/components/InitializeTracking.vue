<script setup lang="ts">
import { ref, type Ref, computed } from 'vue'
import type { TadoInitialization } from '@/models/TadoInitialization';
import type { ActualTadoData } from '@/models/ActualTadoData'
import type { SetSchedule } from '@/models/SetSchedule';

const emit = defineEmits(['response'])
function goBackToOverview() {
    emit('response', { action: 'goBackToOverview' });
}

const currentStep = ref(1);
const handleNext = () => {
    if (currentStep.value === 3) {
        addTrackingSchedule();
    } else {
        if (currentStep.value === 1) {
            fetchUrl();
        } else if (currentStep.value === 2) {
            authenticateCommunication();
        }

        currentStep.value++
    }
}

const TadoInitializationData: Ref<TadoInitialization | null> = ref(null);
const InitializationUrl = computed<string>(() => TadoInitializationData.value ? TadoInitializationData.value.verificationUriComplete : 'not available');

async function fetchUrl() {
    // Vite proxies '/api/tadotemperature/get-new-url' to 'http://localhost:5160/api/tadotemperature/get-new-url'
    const response = await fetch('/api/tadotemperature/get-new-url')

    if (!response.ok) {
        throw new Error(`API error: ${response.status}`)
    }

    const data = await response.json() as TadoInitialization;
    TadoInitializationData.value = data;
}

const ActualDataIsVisible = computed<boolean>(() => ActualTadoData.value !== null);
const ActualTadoData: Ref<ActualTadoData | null> = ref(null);

async function authenticateCommunication() {
    // Vite proxies '/api/tadotemperature/authenticate' to 'http://localhost:5160/api/tadotemperature/authenticate'
    const response = await fetch('/api/tadotemperature/authenticate/' + TadoInitializationData.value?.communicationId);

    if (!response.ok) {
        throw new Error(`API error: ${response.status}`)
    }

    const data = await response.json() as ActualTadoData;
    ActualTadoData.value = data;
}

const min = ref(1)
const max = ref(120)
const slider = ref(60)

async function addTrackingSchedule() {
    var newSchedule: SetSchedule = {
        tokenId: ActualTadoData.value?.tokenId ?? 0,
        intervalInMinutes: slider.value
    }

    const response = await fetch('/api/tadotemperature/addSchedule', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(newSchedule)
    })

    if (!response.ok) {
        throw new Error(`API error: ${response.status}`)
    }

    alert("Tracking schedule added successfully! The backend will now track your Tado temperatures at the specified interval.");
    emit('response', { action: 'scheduleActivated' });
}

</script>

<template>
    <v-container>
        <v-card variant="elevated" color="primary" class="mx-auto mt-5" max-width="500">
            <v-row>
                <v-col cols="12" class="text-center">
                    <h1>Initialize Tracking</h1>
                </v-col>
            </v-row>
        </v-card>
        <v-card variant="elevated" color="primary" class="mx-auto mt-5" max-width="500">
            <v-row class="px-4 py-2" align="center">
                <v-col cols="12" class="text-center">
                    <v-btn color="warning" @click="goBackToOverview" size="x-large">
                        Cancel Tado Temperature Tracking Initialisation
                    </v-btn>
                </v-col>
            </v-row>
        </v-card>
        <v-card variant="elevated" color="primary" class="mx-auto mt-5" max-width="500">
            <v-row>
                <v-col cols="12" class="text-center">
                    <v-stepper alt-labels v-model="currentStep" prev-text="Previous" next-text="Next"
                        :items="['Step 1', 'Step 2', 'Step 3']" hide-actions>
                        <template v-slot:item.1>
                            <v-card title="Step One" flat>
                                <v-row>
                                    <v-col cols="12" class="text-center">
                                        <h2>Step One: Get Activation URL</h2>
                                        <p>Click next to get the activation URL for your Tado account.</p>
                                    </v-col>
                                </v-row>
                            </v-card>
                        </template>

                        <template v-slot:item.2>
                            <v-card title="Step Two" flat>
                                <v-row>
                                    <v-col cols="12" class="text-center">
                                        <h2>Step Two: Authorise Tado Account</h2>
                                        <p>To authorise your Tado account, click the link below and follow the
                                            instructions and
                                            click Next when done.</p>
                                        <a :href="InitializationUrl" target="_blank">{{ InitializationUrl }}</a>
                                    </v-col>
                                </v-row>
                            </v-card>
                        </template>

                        <template v-slot:item.3>
                            <v-card title="Step Three" flat>
                                <v-row>
                                    <v-col cols="12" class="text-center">
                                        <h2>Step Three: Complete Initialisation</h2>
                                        <div v-if="!ActualDataIsVisible">
                                            <p>This should show some data to prove correctness and an interval to
                                                specify for
                                                tracking.</p>
                                            <p>Click Finish to complete the initialisation process and start tracking
                                                your Tado
                                                temperatures.</p>
                                        </div>
                                        <div v-else>
                                            <p>We successfully retrieved the Tado data. See here some actual values:</p>
                                            <li>The Zone Name is {{ ActualTadoData?.zoneName }}</li>
                                            <li>The temperature is {{ ActualTadoData?.insideTemperatureCelsius }}
                                                degrees
                                                Celsius.</li>
                                            <li>The Humidity is {{ ActualTadoData?.humidityPercentage }} %</li>
                                            <li>The (Tado) Home ID is {{ ActualTadoData?.homeId }}</li>
                                            <p>Set the tracking interval and click Finish to complete the initialisation
                                                process
                                                and start tracking your Tado temperatures.</p>
                                            <v-slider v-model="slider" :max="max" :min="min" :step="1"
                                                class="align-center" hide-details color="green" thumb-color="orange">
                                                <template v-slot:append>
                                                    <v-text-field v-model="slider" density="compact" style="width: 70px"
                                                        type="number" hide-details single-line></v-text-field>
                                                </template>
                                            </v-slider>
                                        </div>
                                    </v-col>
                                </v-row>
                            </v-card>
                        </template>

                        <v-stepper-actions :disabled="false" :next-text="currentStep === 3 ? 'Finish' : 'Next'"
                            @click:next="handleNext" @click:prev="currentStep--"></v-stepper-actions>
                    </v-stepper>
                </v-col>
            </v-row>
        </v-card>
    </v-container>
</template>