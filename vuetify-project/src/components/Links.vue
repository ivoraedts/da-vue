<script setup lang="ts">
  const links = [
    {
      href: 'https://vuetifyjs.com/',
      icon: 'mdi-text-box-outline',
      subtitle: 'Learn about all things Vuetify in our documentation.',
      title: 'Documentation',
      color: 'primary',
    },
    {
      href: 'https://vuetifyjs.com/introduction/why-vuetify/#feature-guides',
      icon: 'mdi-star-circle-outline',
      subtitle: 'Explore available framework Features.',
      title: 'Features',
      color: 'secondary',
    },
    {
      href: 'https://vuetifyjs.com/components/all',
      icon: 'mdi-widgets-outline',
      subtitle: 'Discover components in the API Explorer.',
      title: 'Components',
      color: 'indigo-darken-3',
    },
    {
      href: 'https://discord.vuetifyjs.com',
      icon: 'mdi-account-group-outline',
      subtitle: 'Connect with Vuetify developers.',
      title: 'Community',
      color: 'pink-darken-3',
    },
    {
      href: 'https://vite.dev/guide/features.html',
      icon: 'mdi-text-box-outline',
      subtitle: 'Documentation for Vite, the build tool used in this project.',
      title: 'Vite',
      color: 'brown-darken-3',
    },
    {
      href: 'https://code.visualstudio.com/docs/nodejs/vuejs-tutorial',
      icon: 'mdi-cast-education',
      subtitle: 'Learn how to use Vue in Visual Studio Code.',
      title: 'Using Vue in VSCode',
      color: 'blue-darken-3',
    },
    {
      href: 'https://vuejs.org/tutorial/#step-7',
      icon: 'mdi-book-open-page-variant-outline',
      subtitle: 'Learn about Vue.js features and concepts.',
      title: 'Middle of Vue tutorial',
      color: 'green-darken-3',
    },
    {
      href: 'https://www.youtube.com/watch?v=HIIWdxEk_ls',
      icon: 'mdi-youtube',
      subtitle: 'Some Youtube video about Vue with ASP.NET Core',
      title: 'Video',
      color: 'red-darken-3',
    },
  ]
</script>

<template>
  <v-container class="fill-height d-flex flex-column justify-center" max-width="1100">
    <div>
      <div class="mb-8 text-center">
        <div class="text-body-medium font-weight-light mb-n1">This page shows all the interesting</div>
        <div class="text-display-medium font-weight-bold">Links</div>
      </div>

      <v-row>
        <v-col v-for="link in links" :key="link.href" cols="12">
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
