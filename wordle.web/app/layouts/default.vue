<template>
  <v-app>
    <v-app-bar color="primary" density="comfortable">
      <NuxtLink to="/" class="d-flex align-center text-decoration-none text-white ml-3">
        <svg width="36" height="36" viewBox="0 0 100 100" xmlns="http://www.w3.org/2000/svg">
          <!-- Wizard hat -->
          <polygon points="50,5 20,65 80,65" fill="#FFD54F" stroke="white" stroke-width="2"/>
          <rect x="12" y="65" width="76" height="12" rx="6" fill="#FFD54F" stroke="white" stroke-width="2"/>
          <!-- Star on hat -->
          <polygon points="50,22 53,32 63,32 55,38 58,48 50,42 42,48 45,38 37,32 47,32" fill="white" opacity="0.9"/>
          <!-- Letter tiles -->
          <rect x="25" y="80" width="16" height="16" rx="2" fill="white" opacity="0.9"/>
          <rect x="43" y="80" width="16" height="16" rx="2" fill="white" opacity="0.9"/>
          <rect x="61" y="80" width="16" height="16" rx="2" fill="white" opacity="0.9"/>
          <text x="33" y="93" font-size="11" font-weight="bold" fill="#1976D2" text-anchor="middle" font-family="sans-serif">W</text>
          <text x="51" y="93" font-size="11" font-weight="bold" fill="#6aaa64" text-anchor="middle" font-family="sans-serif">W</text>
          <text x="69" y="93" font-size="11" font-weight="bold" fill="#c9b458" text-anchor="middle" font-family="sans-serif">Z</text>
        </svg>
        <span class="text-h6 font-weight-bold ml-2">Word Wizardry</span>
      </NuxtLink>

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
          <v-list-item to="/testimonials" prepend-icon="mdi-message-star">
            <v-list-item-title>Testimonials</v-list-item-title>
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
