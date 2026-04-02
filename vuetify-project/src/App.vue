<script setup lang="ts">
  import HelloWorld from '@/components/HelloWorld.vue'
  import Links from '@/components/Links.vue'
  import { ref, type Ref, onMounted  } from 'vue'

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

  // Define the structure to match your C# TodoItem model
interface TodoItem {
  id: number
  task: string
  isCompleted: boolean
}

const todoItems = ref<TodoItem[]>([])
const loading = ref(true)
const error = ref<string | null>(null)

async function fetchTodos() {
  loading.value = true
  error.value = null
  
  try {
    // Vite proxies '/api/todo' to 'http://localhost:5160/api/todo'
    const response = await fetch('/api/todo')
    
    if (!response.ok) {
      throw new Error(`API error: ${response.status}`)
    }
    
    const data = await response.json()
    todoItems.value = data
  } catch (err) {
    error.value = "Could not load tasks. Is the Backend running?"
    console.error(err)
  } finally {
    loading.value = false
  }
}

onMounted(() => {
  fetchTodos()
})

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
        <v-container>
        <v-card class="mx-auto mt-5" max-width="500">
          <v-toolbar color="primary">
            <v-toolbar-title>My .NET Tasks</v-toolbar-title>
            <v-spacer></v-spacer>
            <v-btn icon @click="fetchTodos" :loading="loading">
              <v-icon>mdi-refresh</v-icon>
            </v-btn>
          </v-toolbar>

          <v-list v-if="todoItems.length > 0">
            <v-list-item v-for="item in todoItems" :key="item.id">
              <template v-slot:prepend>
                <v-checkbox-btn :model-value="item.isCompleted" readonly></v-checkbox-btn>
              </template>
              <v-list-item-title>{{ item.task }}</v-list-item-title>
            </v-list-item>
          </v-list>

          <v-card-text v-else-if="!loading" class="text-center">
            {{ error || 'No tasks found.' }}
          </v-card-text>
          
          <v-progress-linear v-if="loading" indeterminate></v-progress-linear>
        </v-card>
      </v-container>
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
