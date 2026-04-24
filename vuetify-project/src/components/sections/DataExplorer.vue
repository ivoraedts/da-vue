<script setup lang="ts">
import { ref, type Ref, onMounted, computed } from 'vue'
const emit = defineEmits(['response'])

function goBackToOverview() {
    emit('response', { action: 'goBackToOverview' });
}

</script>

<template>
    <v-container>
        <v-card class="mx-auto mt-5" max-width="500">
            <v-row>
                <v-col cols="12" class="text-center">
                    <h1>Data Explorer</h1>
                    <v-btn variant="tonal" @click="goBackToOverview()" prepend-icon="mdi-undo" elevated color="red">
                        Back to overview
                    </v-btn>
                </v-col>
            </v-row>
        </v-card>
    </v-container>
</template>