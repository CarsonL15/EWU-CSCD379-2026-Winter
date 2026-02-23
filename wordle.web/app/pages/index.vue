<template>
  <v-container class="text-center py-8">
    <v-row justify="center">
      <v-col cols="12" sm="8" md="6">
        <h1 class="text-h3 mb-2">Word Wizardry</h1>
        <p class="text-body-1 text-medium-emphasis mb-6">Choose your game</p>

        <v-row justify="center" class="ga-4">
          <v-col cols="12" sm="6">
            <v-card
              to="/game"
              variant="outlined"
              class="pa-6 game-card"
              hover
            >
              <v-icon size="48" color="primary" class="mb-3">mdi-wizard-hat</v-icon>
              <h2 class="text-h5 mb-2">Wordle</h2>
              <p class="text-body-2 text-medium-emphasis">Guess the 5-letter word in 6 tries</p>
            </v-card>
          </v-col>

          <v-col cols="12" sm="6">
            <v-card
              to="/gridseeker"
              variant="outlined"
              class="pa-6 game-card"
              hover
            >
              <v-icon size="48" color="amber" class="mb-3">mdi-treasure-chest</v-icon>
              <h2 class="text-h5 mb-2">Grid Seeker</h2>
              <p class="text-body-2 text-medium-emphasis">Find hidden treasures on an 8x8 grid</p>
            </v-card>
          </v-col>
        </v-row>

        <v-btn
          class="mt-6"
          variant="text"
          to="/leaderboard"
          prepend-icon="mdi-trophy"
        >
          View Leaderboard
        </v-btn>
      </v-col>
    </v-row>
  </v-container>
</template>

<style scoped>
.game-card {
  transition: transform 0.2s ease, box-shadow 0.2s ease;
}

.game-card:hover {
  transform: translateY(-4px);
}
</style>
