<template>
  <v-app>
    <v-app-bar color="primary" density="comfortable">
      <v-btn icon to="/" aria-label="Home">
        <v-icon>mdi-wizard-hat</v-icon>
      </v-btn>
      <v-toolbar-title>
        <NuxtLink to="/" class="text-decoration-none text-white">
          Word Wizardry
        </NuxtLink>
      </v-toolbar-title>

      <v-spacer />

      <v-btn variant="text" to="/leaderboard" class="text-none">
        <v-icon start>mdi-trophy</v-icon>
        Leaderboard
      </v-btn>

      <v-menu>
        <template v-slot:activator="{ props }">
          <v-btn icon v-bind="props" aria-label="Menu">
            <v-icon>mdi-menu</v-icon>
          </v-btn>
        </template>
        <v-list>
          <v-list-item to="/game" prepend-icon="mdi-wizard-hat">
            <v-list-item-title>Wordle</v-list-item-title>
          </v-list-item>
          <v-list-item to="/gridseeker" prepend-icon="mdi-treasure-chest">
            <v-list-item-title>Grid Seeker</v-list-item-title>
          </v-list-item>
          <v-list-item to="/leaderboard" prepend-icon="mdi-trophy">
            <v-list-item-title>Leaderboard</v-list-item-title>
          </v-list-item>
          <v-list-item to="/about" prepend-icon="mdi-information">
            <v-list-item-title>About</v-list-item-title>
          </v-list-item>
          <v-list-item @click="showSettings = true" prepend-icon="mdi-cog">
            <v-list-item-title>Settings</v-list-item-title>
          </v-list-item>
        </v-list>
      </v-menu>
    </v-app-bar>

    <v-main>
      <slot />
    </v-main>

    <SettingsDialog v-model="showSettings" />
  </v-app>
</template>

<script setup lang="ts">
const showSettings = ref(false)
</script>
