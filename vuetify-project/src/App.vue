<script setup lang="ts">
  import HelloWorld from '@/components/HelloWorld.vue'
  import Links from '@/components/Links.vue'
  import { ref, type Ref } from 'vue'

  const showPage: Ref<string, string> = ref("home");
  showPage.value = "hello world page";

  function showHelloWorldPage() {
    showPage.value = "hello world page";
  }

  function showHomePage() {
    showPage.value = "home";
  }

  function showLinksPage() {
    showPage.value = "links page";
  }
</script>

<template>
  <v-app>
    <v-main>
      <v-app-bar rounded>
        <template v-slot:prepend>
          <v-app-bar-nav-icon @click="showHomePage()"></v-app-bar-nav-icon>
        </template>
        <v-app-bar-title>Some Vuetify App</v-app-bar-title>
        <v-btn icon @click="showHelloWorldPage()">
          <v-icon>mdi-web</v-icon>
        </v-btn>
        <v-btn icon @click="showLinksPage()">
          <v-icon>mdi-list-box</v-icon>
        </v-btn>
      </v-app-bar>

      <div v-if="showPage==='hello world page'" >
        <HelloWorld />  
      </div>
      <div v-else-if="showPage==='home'">
        <h1>Home Page</h1>
      </div>
      <div v-else-if="showPage==='links page'">
        <Links />
      </div>
      <div v-else>
        <h1>Page Not Found</h1>
      </div>

      
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
