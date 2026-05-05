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
            currentScheduleWasPasswordProtected.value = data.isPasswordProtected;
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
    if (!response.ok) {
        if (response.status === 401) {
            alert("Wrong password. Updating an active schedule that was password protected requires entering the old password.");
            console.error(`Wrong password. Updating an active schedule that was password protected requires entering the old password. API error details: ${response.status}`)
        }
        else {
            alert("Could not update task. Is the Backend running?");
            console.error(`API error: ${response.status}`)
            alert(response);
        }
    }
    else {
        emit('response', { action: 'scheduleUpdated' });
    }
}

function togglePasswordVisibility() {
    passwordIsVisible.value = !passwordIsVisible.value;
}

const currentSchedule: Ref<TadoRetrievalScheduleModel | null> = ref(null);
const currentScheduleWasPasswordProtected: Ref<boolean> = ref(false);
const passwordIsVisible: Ref<boolean> = ref(false);
const min = ref(1)
const max = ref(120)

const passwordInputType = computed(() => {
    if (passwordIsVisible.value) return "normal";
    return "password";
})

const showPasswordToggle = computed(() => {
    if (currentScheduleWasPasswordProtected.value) return true;
    if (currentSchedule != null && currentSchedule.value != null && currentSchedule.value?.isPasswordProtected) return true;

    return false;
})

const passwordToggleIcon = computed(() => {
    if (passwordIsVisible.value) return "mdi-eye-off";
    return "mdi-eye";
})

const passwordToggleText = computed(() => {
    if (passwordIsVisible.value) return "Hide passwords";
    return "Show passwords";
})

onMounted(() => {
    getActiveTrackingSchedule();
});
</script>

<template>
    <v-container>
        <v-card variant="elevated" color="primary" class="mx-auto mt-5" max-width="500">
            <v-row>
                <v-col cols="12" class="text-center">
                    <h1>Schedule editor</h1>
                </v-col>
            </v-row>
        </v-card>
        <v-card class="mx-auto mt-5" max-width="500">
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

            <v-row v-if="showPasswordToggle" class="px-4 py-2" align="center">
                <v-col cols="12" class="text-center">
                    <v-btn variant="elevated" @click="togglePasswordVisibility()" :prepend-icon="passwordToggleIcon"
                        elevated color="secondary" size="x-large" :text="passwordToggleText">
                    </v-btn>
                </v-col>
            </v-row>

            <v-row v-if="currentSchedule != null && currentScheduleWasPasswordProtected" class="px-4 py-2"
                align="center">
                <v-col cols="6" class="text-right"><span class="text-headline-small">Current Password:</span></v-col>
                <v-col cols="6">
                    <v-text-field label="Current Password" :type="passwordInputType"
                        v-model="currentSchedule.oldPassword"></v-text-field>
                </v-col>
            </v-row>

            <v-row v-if="!currentScheduleWasPasswordProtected" class="px-4 py-2" align="center">
                <v-col cols="12" class="text-center"><span class="text-headline-small">Currently No
                        Password</span></v-col>
            </v-row>

            <v-row v-if="currentSchedule != null" class="px-4 py-2" align="center">
                <v-col cols="6" class="text-right"><span class="text-headline-small">Set Password:</span></v-col>
                <v-col cols="6"><v-checkbox-btn v-model="currentSchedule.isPasswordProtected"></v-checkbox-btn></v-col>
            </v-row>

            <v-row v-if="currentSchedule != null && currentSchedule.isPasswordProtected" class="px-4 py-2"
                align="center">
                <v-col cols="2" class="text-right">
                    <v-icon color="orange-darken-2" icon="mdi-alert" size="large"></v-icon>
                </v-col>
                <v-col cols="10" class="text-left"><span class="text-body-medium font-weight-bold ">Only Active
                        Schedules are
                        Password-Protected</span></v-col>
            </v-row>

            <v-row v-if="currentSchedule != null && currentSchedule.isPasswordProtected" class="px-4 py-2"
                align="center">
                <v-col cols="6" class="text-right"><span class="text-headline-small">New Password:</span></v-col>
                <v-col cols="6"><v-text-field label="New Password" :type="passwordInputType"
                        v-model="currentSchedule.newPassword"></v-text-field></v-col>
            </v-row>

            <v-row v-if="currentSchedule != null" class="px-4 py-2" align="center">
                <v-col cols="12">
                    <v-divider :thickness="4" color="primary"></v-divider>
                </v-col>
            </v-row>

            <v-row v-if="currentSchedule != null" class="px-4 py-2" align="center">
                <v-col cols="6" class="text-center">
                    <v-btn variant="elevated" @click="editItem()" prepend-icon="mdi-content-save" elevated
                        color="primary" size="x-large">
                        Save
                    </v-btn>
                </v-col>
                <v-col cols="6" class="text-center">
                    <v-btn variant="elevated" @click="goBackToOverview()" prepend-icon="mdi-undo" elevated
                        color="secondary" size="x-large">
                        Cancel
                    </v-btn>
                </v-col>
            </v-row>
            <v-row v-if="currentSchedule == null" class="px-4 py-2" align="center">
                <v-col cols="12" class="text-center">
                    <v-btn variant="elevated" @click="goBackToOverview()" prepend-icon="mdi-undo" elevated
                        color="secondary" size="x-large">
                        Cancel
                    </v-btn>
                </v-col>
            </v-row>
        </v-card>
        <v-card v-if="currentSchedule != null" variant="elevated" color="secondary" class="mx-auto mt-5"
            max-width="500">
            <v-row></v-row>
            <v-row>
                <v-col cols="1"></v-col>
                <v-col cols="5">
                    Next Retrieval Time:
                </v-col>
                <v-col cols="6">
                    {{ currentSchedule.nextRetrievalTimeString }}
                </v-col>
            </v-row>
            <v-row>
                <v-col cols="1"></v-col>
                <v-col cols="5">
                    Last Retrieval Time:
                </v-col>
                <v-col cols="6">
                    {{ currentSchedule.lastRetrievalTimeString }}
                </v-col>
            </v-row>
            <v-row v-if="currentSchedule.consecutiveFailures > 0">
                <v-col cols="1"></v-col>
                <v-col cols="5">
                    Consecutive Failures:
                </v-col>
                <v-col cols="6">
                    {{ currentSchedule.consecutiveFailures }}
                </v-col>
            </v-row>
            <v-row v-if="currentSchedule.lastError != null && currentSchedule.lastError.length>0">
                <v-col cols="1"></v-col>
                <v-col cols="11">
                    Last Error: {{ currentSchedule.lastError }}
                </v-col>
            </v-row>
            <v-row></v-row>
        </v-card>
    </v-container>
</template>