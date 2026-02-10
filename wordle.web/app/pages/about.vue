<template>
  <v-container class="py-8">
    <v-row justify="center">
      <v-col cols="12" md="8" lg="6">
        <v-card elevation="2">
          <v-card-title class="text-h4 text-center py-6">
            <v-icon size="large" class="mr-2">mdi-information</v-icon>
            About Word Wizardry
          </v-card-title>

          <v-card-text class="text-body-1">
            <p class="mb-4">
              Word Wizardry is a Wordle-inspired word guessing game built as part of
              <strong>CSCD 379 - Web Applications</strong> at Eastern Washington University.
            </p>

            <p class="mb-4">
              This project demonstrates modern web development practices using
              <strong>Nuxt 4</strong>, <strong>Vue 3</strong>, <strong>Vuetify 3</strong>,
              and <strong>TypeScript</strong>. The application is deployed to Azure Static Web Apps
              with automated CI/CD pipelines.
            </p>

            <p class="mb-4">
              Special thanks to <strong>Meg</strong> for being an amazing instructor!
              Her guidance and expertise have been invaluable in learning web development
              concepts and best practices.
            </p>

            <v-divider class="my-4" />

            <p class="text-caption text-center">
              Built with Vue.js and Vuetify
            </p>
          </v-card-text>

          <v-card-actions class="justify-center pb-6">
            <v-btn
              color="primary"
              variant="elevated"
              prepend-icon="mdi-arrow-left"
              @click="$router.back()"
            >
              Go Back
            </v-btn>
          </v-card-actions>
        </v-card>
      </v-col>
    </v-row>
  </v-container>
</template>
