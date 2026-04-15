<script setup lang="ts">
import { ref, type Ref, onMounted, computed } from 'vue'
import type { TodoItem } from '@/models/todoItem';
import { tr } from 'vuetify/locale';

const todoItems: Ref<TodoItem[]> = ref<TodoItem[]>([])
const oldTodoItems: Ref<TodoItem[]> = ref<TodoItem[]>([])
const loading: Ref<boolean> = ref(true)
const error: Ref<string | null> = ref(null)
const newTodo = ref('')
const canAddTodo = computed(() => newTodo.value.trim().length > 0)

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
    oldTodoItems.value = JSON.parse(JSON.stringify(data)) // Deep copy for edit cancellation
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

async function addTodo() {
  var newItem : TodoItem = {
    id: 0,
    task: newTodo.value.trim(),
    isCompleted: false,
  }

  const response = await fetch('/api/todo', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json'
    },
    body: JSON.stringify(newItem)
  })

  if (response.ok) {
    newTodo.value = ''
    fetchTodos() // Refresh the list after adding a new item
  } else {
    error.value = "Could not add task. Is the Backend running?"
    console.error(`API error: ${response.status}`)
  }
}

async function toggleItem(item: TodoItem) {
  item.isCompleted = !item.isCompleted
  const response = await fetch(`/api/todo/${item.id}`, {
    method: 'PUT',
    headers: {
      'Content-Type': 'application/json'
    },
    body: JSON.stringify(item)
  })
  if (!response.ok)
  {
    error.value = "Could not update task. Is the Backend running?"
    console.error(`API error: ${response.status}`)
  }
}

async function editItem(item: TodoItem) {
  item.editMode = false
  const response = await fetch(`/api/todo/${item.id}`, {
    method: 'PUT',
    headers: {
      'Content-Type': 'application/json'
    },
    body: JSON.stringify(item)
  })
  if (!response.ok)
  {
    error.value = "Could not update task. Is the Backend running?"
    console.error(`API error: ${response.status}`)
  }
}

async function deleteItem(item: TodoItem) {
  const response = await fetch(`/api/todo/${item.id}`, {
    method: 'DELETE'
  })
  if (response.ok) {
    fetchTodos() // Refresh the list after deleting an item
  } else {
    error.value = "Could not delete task. Is the Backend running?"
    console.error(`API error: ${response.status}`)
  }
}

function editMode(item: TodoItem) {
  item.editMode = true
}

function cancelEdit(item: TodoItem) {
  var oldVersion = oldTodoItems.value.find(i => i.id === item.id)
  if (oldVersion) {
    item.task = oldVersion.task
  }
  item.editMode = false
}

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
          <v-checkbox-btn :model-value="item.isCompleted" @click="() => toggleItem(item)"></v-checkbox-btn>
        </template>
        <v-list-item-title v-if="!item.editMode">{{ item.task }}</v-list-item-title>
        <v-text-field v-else v-model="item.task"></v-text-field>
        <template v-slot:append>          
          <v-icon v-if="!item.editMode" color="blue" @click="() => editMode(item)">mdi-pencil</v-icon>
          <v-icon v-if="item.editMode">mdi-bestaat-niet</v-icon>
          <v-icon v-if="item.editMode" color="blue" @click="() => cancelEdit(item)">mdi-undo</v-icon>
          <v-icon v-if="item.editMode">mdi-bestaat-niet</v-icon>
          <v-icon v-if="item.editMode" color="blue" @click="() => editItem(item)">mdi-content-save-edit-outline</v-icon>
          <v-icon>mdi-bestaat-niet</v-icon>
          <v-icon color="red" @click="() => deleteItem(item)">mdi-delete</v-icon>
        </template>
      </v-list-item>
    </v-list>

    <v-card-text v-else-if="!loading" class="text-center">
      {{ error || 'No tasks found.' }}
    </v-card-text>

    <v-divider :thickness="2" class="border-opacity-75" color="info" inset></v-divider>

    <v-row class="px-4 py-2" align="center">
      <v-col cols="9">
        <v-text-field v-model="newTodo" label="New Todo" placeholder="new todo" clearable></v-text-field>
      </v-col>
      <v-col cols="3">
        <v-btn color="primary" @click="addTodo" :disabled="!canAddTodo">
          Add Todo
        </v-btn>
      </v-col>
    </v-row>

    <v-progress-linear v-if="loading" indeterminate></v-progress-linear>
  </v-card>
</v-container>
</template>
