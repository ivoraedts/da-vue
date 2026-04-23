<script setup lang="ts">
  import About from '@/components/About.vue'
  import Links from '@/components/Links.vue'
  import Todo from '@/components/Todo.vue'
  import InitializeTracking from '@/components/InitializeTracking.vue'
  import { ref, type Ref  } from 'vue'

  const showPage: Ref<string, string> = ref("home");
  showPage.value = "hello world page";

  function showAboutPage() {
    showPage.value = "hello world page";
  }

  function showHomePage() {
    showPage.value = "home";
  }

  function showLinksPage() {
    showPage.value = "links page";
  }

  function showToDoPage() {
    showPage.value = "todo page";
  }
</script>

<template>
  <v-app>
    <v-main>
      <v-app-bar rounded>
        <template v-slot:prepend>
          <v-app-bar-nav-icon @click="showHomePage()"></v-app-bar-nav-icon>
        </template>
        <v-app-bar-title>Da Vue</v-app-bar-title>
        <v-btn append-icon="mdi-information-outline" color="green-darken-4" @click="showAboutPage()">
          Hello
        </v-btn>
        <v-btn append-icon="mdi-list-box" color="blue-darken-2" @click="showLinksPage()">
          Links
        </v-btn>
        <v-btn append-icon="mdi-check-circle" color="red-darken-2" @click="showToDoPage()">
          Todos
        </v-btn>
      </v-app-bar>

      <div v-if="showPage==='hello world page'" >
        <About />  
      </div>
      <div v-else-if="showPage==='home'">
        <InitializeTracking />
      </div>
      <div v-else-if="showPage==='links page'">
        <Links />
      </div>
      <div v-else-if="showPage==='todo page'">
        <Todo />
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
