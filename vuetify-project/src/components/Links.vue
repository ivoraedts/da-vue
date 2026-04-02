<script setup lang="ts">
  import { ref, type Ref } from 'vue'
  import type { DisplayLink } from '@/models/displayLink';
  import { GetAllLinks } from '@/functions/VariousFunctions';

  const displayLinks: Ref<DisplayLink[]> = ref([] as DisplayLink[]);
  displayLinks.value = GetAllLinks();
</script>

<template>
  <v-container class="fill-height d-flex flex-column justify-center" max-width="1100">
    <div>
      <div class="mb-8 text-center">
        <div class="text-body-medium font-weight-light mb-n1">This page shows all the interesting</div>
        <div class="text-display-medium font-weight-bold">Links</div>
      </div>

      <v-row>
        <v-col v-for="link in displayLinks" :key="link.href" cols="12">
          <v-card
            append-icon="mdi-open-in-new"
            class="py-4"
            :color="link.color"
            :href="link.href"
            rel="noopener noreferrer"
            rounded="lg"
            :subtitle="link.subtitle"
            target="_blank"
            :title="link.title"
            variant="tonal"
          >
            <template #prepend>
              <v-avatar class="ml-2 mr-4" :icon="link.icon" size="60" variant="tonal" />
            </template>
          </v-card>
        </v-col>
      </v-row>
    </div>
  </v-container>
</template>
