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

      <v-menu>
        <template v-slot:activator="{ props }">
          <v-btn icon v-bind="props" aria-label="Menu">
            <v-icon>mdi-menu</v-icon>
          </v-btn>
        </template>
        <v-list>
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
