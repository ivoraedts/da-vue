<script setup lang="ts">
import { ref, type Ref, onMounted, computed } from 'vue'
import type { TadoInitialization } from '@/models/TadoInitialization';
import type { ActualTadoData } from '@/models/ActualTadoData'

const stepperIsActive: Ref<boolean> = ref(false);
const stepperIsDisabled: Ref<boolean> = ref(false);

const canStartStepper = computed(() => !stepperIsActive.value && !stepperIsDisabled.value);
const canStopStepper = computed(() => stepperIsActive.value);

function startStepper() {
  stepperIsActive.value = true;
}

function stopStepper() {
  stepperIsActive.value = false;
}

const currentStep = ref(1);
const handleNext = () => {
  if (currentStep.value === 3) {
    alert("Form Submitted!");
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

async function fetchUrl()
{
    // Vite proxies '/api/tadotemperature/get-new-url' to 'http://localhost:5160/api/tadotemperature/get-new-url'
    const response = await fetch('/api/tadotemperature/get-new-url')
    
    if (!response.ok) {
      throw new Error(`API error: ${response.status}`)
    }

    const data = await response.json() as TadoInitialization;
    TadoInitializationData.value = data;
}

const canStartStepper3 = computed(() => !stepperIsActive.value && !stepperIsDisabled.value);
const ActualDataIsVisible = computed<boolean>(() => ActualTadoData.value !== null);    
const ActualTadoData: Ref<ActualTadoData | null> = ref(null);
    

async function authenticateCommunication()
{
    // Vite proxies '/api/tadotemperature/authenticate' to 'http://localhost:5160/api/tadotemperature/authenticate'
    const response = await fetch('/api/tadotemperature/authenticate/'+TadoInitializationData.value?.communicationId);

    if (!response.ok) {
        throw new Error(`API error: ${response.status}`)
    }

    const data = await response.json() as ActualTadoData;
    ActualTadoData.value = data;
}

</script>

<template>    
      <v-container>
        <v-card class="mx-auto mt-5" max-width="500">
            <v-row>
                <v-col cols="12" class="text-center">
                    <h1>Da Home Page</h1>
                </v-col>
            </v-row>

            <v-row v-if="canStartStepper" class="px-4 py-2" align="center">
                <v-col cols="12" class="text-center">
                    <v-btn color="primary" @click="startStepper">
                        Initialise Tado Temperature Tracking
                    </v-btn>
                </v-col>
            </v-row>

            <v-row v-if="canStopStepper" class="px-4 py-2" align="center">
                <v-col cols="12" class="text-center">
                    <v-btn color="error" @click="stopStepper">
                        Cancel Tado Temperature Tracking Initialisation
                    </v-btn>
                </v-col>
            </v-row>

            <v-row v-if="stepperIsActive">
                <v-col cols="12" class="text-center">
                    <v-stepper
                        alt-labels
                        v-model="currentStep"
                        prev-text="Previous"
                        next-text="Next"
                        :items="['Step 1', 'Step 2', 'Step 3']"
                        hide-actions>

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
                                    <p>To authorise your Tado account, click the link below and follow the instructions and click Next when done.</p>
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
                                        <p>This should show some data to prove correctness and an interval to specify for tracking.</p>
                                        <p>Click Finish to complete the initialisation process and start tracking your Tado temperatures.</p>
                                    </div>
                                    <div v-else>
                                        <p>We got some data. 
                                            The temperature is {{ ActualTadoData?.insideTemperatureCelsius }} degrees Celsius.</p>
                                        <p>The Humidity is {{ ActualTadoData?.humidityPercentage }} %</p>    
                                    </div>
                                </v-col>
                            </v-row>
                        </v-card>
                    </template>

                    <v-stepper-actions
                        :disabled="false"
                        :next-text="currentStep === 3 ? 'Finish' : 'Next'"
                        @click:next="handleNext"
                        @click:prev="currentStep--"
                    ></v-stepper-actions>
                    </v-stepper>
                </v-col>
            </v-row>
        </v-card>
    </v-container>
</template>