<script setup lang="ts">
  import HelloWorld from '@/components/HelloWorld.vue'
</script>

<template>
  <v-app>
    <v-main>
      <HelloWorld />
    </v-main>
    <v-btn
      class="ma-2"
      icon="mdi-theme-light-dark"
      location="top right"
      position="absolute"
      @click="$vuetify.theme.cycle()"
    />
  </v-app>
</template>
