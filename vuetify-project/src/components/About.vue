<template>
  <v-container class="fill-height d-flex flex-column justify-center" max-width="1100">
    <div>
      <v-img class="mb-4 font-weight-bold" height="150" src="@/assets/logo-medium.png" />

      <div class="mb-8 text-center">
        <div class="text-body-medium font-weight-light mb-n1">The app was built just to play around with</div>
        <div class="text-title-large font-weight-bold">Vuetify, Vue, .Net Core and Docker</div>
      </div>

      <v-row>
        <v-col cols="12">
          <v-card class="py-4" color="secondary" rounded="lg" variant="tonal">
            <template v-if="!$vuetify.display.xs" #prepend>
              <v-avatar class="ml-2 mr-4" icon="mdi-rocket-launch-outline" size="60" variant="tonal" />
            </template>

            <template #title>
              <div class="my-title my-uppercase text-headline-medium font-weight-bold">Got started with Vue
              </div>
            </template>

            <template #subtitle>
              <div class="text-body-large">
                <p class="text-wrap">
                  I got started to play around with Vue and then figured out that Vuetify is a nice way to quickly build
                  a nice looking UI in Material Design.
                  <br />
                  The source code of this project is available on GitHub and even on Docker Hub:
                </p>
                <v-row>
                  <v-col cols="12">
                    <v-card prepend-icon="mdi-github" title="Da Vue on Github"
                      subtitle="https://github.com/ivoraedts/da-vue" href="https://github.com/ivoraedts/da-vue"
                      target="_blank" rel="noopener noreferrer" color="grey-darken-4" rounded="lg"></v-card>
                  </v-col>
                  <v-col cols="12">
                    <v-card prepend-icon="mdi-docker" title="Docker Hub" subtitle="Da Vue Web.API"
                      href="https://hub.docker.com/repository/docker/ivoraedts/da-vue-theweb-api/general"
                      target="_blank" rel="noopener noreferrer" color="blue-accent-4" rounded="lg"></v-card>
                  </v-col>
                  <v-col cols="12">
                    <v-card prepend-icon="mdi-docker" title="Docker Hub" subtitle="Da Vue Web.UI" variant="elevated"
                      href="https://hub.docker.com/repository/docker/ivoraedts/da-vue-theweb-ui/general" target="_blank"
                      rel="noopener noreferrer" color="blue-accent-4" rounded="lg"></v-card>
                  </v-col>
                </v-row>
                <p class="text-wrap">
                  ...and when I was thinking about what I could display without too much hassle
                  , I came up with the idea to show the measured temperatures from my TADO smart thermostat.
                  <br />
                  I found the following library (open source on GitHub and available via NuGet)
                  and information that could help me with the implementation:
                </p>
                <v-row>
                  <v-col cols="12">
                    <v-card prepend-icon="mdi-github" title="Opensource library on Github"
                      subtitle="https://github.com/KoenZomers/TadoApi" href="https://github.com/KoenZomers/TadoApi"
                      target="_blank" rel="noopener noreferrer" color="grey-darken-4" rounded="lg"></v-card>
                  </v-col>
                  <v-col cols="12">
                    <v-card prepend-icon="mdi-home-thermometer-outline" title="Tado Article"
                      subtitle="Some article explaining the communication"
                      href="https://help.tado.com/en/articles/8565472-how-do-i-authenticate-to-access-the-rest-api"
                      target="_blank" rel="noopener noreferrer" color="blue-accent-4" rounded="lg"></v-card>
                  </v-col>
                </v-row>
              </div>
            </template>
          </v-card>
        </v-col>
      </v-row>
    </div>
  </v-container>
</template>
