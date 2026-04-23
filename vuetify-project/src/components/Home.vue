<script setup lang="ts">
import { ref, type Ref, onMounted, computed } from 'vue'
import InitializeTracking from '@/components/InitializeTracking.vue'

const stepperIsActive: Ref<boolean> = ref(false);
const stepperIsDisabled: Ref<boolean> = ref(false);

const canStartStepper = computed(() => !stepperIsActive.value && !stepperIsDisabled.value);
const canStopStepper = computed(() => stepperIsActive.value);

function startStepper2() {
  stepperIsActive.value = true;
}

function stopStepper2() {
  stepperIsActive.value = false;
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

            <div v-if="canStartStepper">
                <InitializeTracking />
            </div>

            <v-row v-if="canStartStepper" class="px-4 py-2" align="center">
                <v-col cols="12" class="text-center">
                    <v-btn color="primary" @click="startStepper2">
                        Duh
                    </v-btn>
                </v-col>
            </v-row>

            <v-row v-if="canStopStepper" class="px-4 py-2" align="center">
                <v-col cols="12" class="text-center">
                    <v-btn color="error" @click="stopStepper2">
                        daa
                    </v-btn>
                </v-col>
            </v-row>
        </v-card>
    </v-container>
</template>