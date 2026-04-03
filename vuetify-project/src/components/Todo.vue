<script setup lang="ts">
import { ref, type Ref, onMounted } from 'vue'
import type { TodoItem } from '@/models/todoItem';

const todoItems: Ref<TodoItem[]> = ref<TodoItem[]>([])
const loading: Ref<boolean> = ref(true)
const error: Ref<string | null> = ref(null)

async function fetchTodos() {
  loading.value = true
  error.value = null
  
  try {
    // Vite proxies '/api/todo' to 'http://localhost:5160/api/todo'
    const response = await fetch('/api/todo')
    
    if (!response.ok) {
      throw new Error(`API error: ${response.status}`)
    }
    
    const data = await response.json() as TodoItem[]
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
  <v-container>
  <v-card class="mx-auto mt-5" max-width="500">
    <v-toolbar color="primary">
      <v-toolbar-title>The TODO List</v-toolbar-title>
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
</template>
