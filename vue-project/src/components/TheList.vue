<script setup lang="ts">
import { ref } from 'vue'
import type { ListItem } from '../models/listItem';
import ListItemComponent from './ListItemComponent.vue';
import DocumentationIcon from './icons/IconDocumentation.vue'
import VisualStudioCodeIcon from './icons/VisualStudioCodeIcon.vue'
import YouTubeIcon from './icons/YouTubeIcon.vue'
import VueIcon from './icons/VueIcon.vue';

const itemList = ref<ListItem[]>([
  {
    title: 'Vite',
    icon: 'documentation',
    style: 'style-1',
    ref: 'https://vite.dev/guide/features.html'
  },
  {
    title: 'Using Vue in VSCode',
    icon: 'vscode',
    style: 'style-2',
    ref: 'https://code.visualstudio.com/docs/nodejs/vuejs-tutorial'
  },
  {
    title: 'Middle of Vue tutorial',
    icon: 'icon-3',
    style: 'style-3',
    ref: 'https://vuejs.org/tutorial/#step-7'
  }
  ,
  {
    title: 'Some Youtube video about Vue with ASP.NET Core',
    icon: 'youtube',
    style: 'style-4',
    ref: 'https://www.youtube.com/watch?v=HIIWdxEk_ls'
  }
])
</script>

<template>
    <div v-for="listItem in itemList">        
        <ListItemComponent>
            <template #icon>
                <div v-if="listItem.icon==='documentation'">
                    <DocumentationIcon />
                </div>
                <div v-else-if="listItem.icon==='vscode'">
                    <VisualStudioCodeIcon />
                </div>
                <div v-else-if="listItem.icon==='youtube'">
                    <YouTubeIcon />
                </div>
                <div v-else>
                    <VueIcon />
                </div>
            </template>
            <template #linkingtext>
                <div :class="listItem.style">
                    <a :href="listItem.ref" target="_blank" rel="noopener noreferrer">
                        {{ listItem.title }}
                    </a>
                </div>
            </template>
        </ListItemComponent>
    </div>
</template>

<style scoped>
.style-1 a {
  color: greenyellow;
}

.style-1 a:hover{
  color: darkgreen;
}

.style-2 a {
  color: cadetblue;
}

.style-2 a:hover{
  color: blue;
}

.style-3 a {
  color: orange;
}

.style-3 a:hover{
  color: orangered;
}

.style-4 a {
  color: red;
}

.style-4 a:hover{
  color: darkred;
}
</style>