<template>
  <v-container>
    <v-row justify="center" class="mb-4">
      <v-col cols="12" class="text-center">
        <h1 class="text-h4 mb-1">Leaderboard</h1>
        <p class="text-body-2 text-medium-emphasis">Top Grid Seeker scores</p>
      </v-col>
    </v-row>

    <v-row justify="center">
      <v-col cols="12" sm="10" md="8" lg="6">
        <v-card v-if="loading" class="pa-8 text-center">
          <v-progress-circular indeterminate color="primary" />
          <div class="mt-2">Loading leaderboard...</div>
        </v-card>

        <v-card v-else-if="error" class="pa-8 text-center">
          <v-icon color="error" size="48" class="mb-2">mdi-alert-circle</v-icon>
          <div class="text-body-1">{{ error }}</div>
          <v-btn color="primary" class="mt-3" @click="fetchLeaderboard">Retry</v-btn>
        </v-card>

        <v-card v-else-if="entries.length === 0" class="pa-8 text-center">
          <v-icon color="grey" size="48" class="mb-2">mdi-trophy-outline</v-icon>
          <div class="text-body-1">No scores yet. Be the first to play!</div>
          <v-btn color="primary" class="mt-3" to="/gridseeker">Play Now</v-btn>
        </v-card>

        <v-table v-else>
          <thead>
            <tr>
              <th class="text-center">Rank</th>
              <th>Player</th>
              <th class="text-center">High Score</th>
              <th class="text-center">Games</th>
              <th class="text-center">Avg Score</th>
              <th class="text-center">Treasures</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="(entry, index) in entries" :key="entry.playerName">
              <td class="text-center">
                <v-icon v-if="index === 0" color="amber" size="small">mdi-trophy</v-icon>
                <v-icon v-else-if="index === 1" color="grey" size="small">mdi-trophy</v-icon>
                <v-icon v-else-if="index === 2" color="brown" size="small">mdi-trophy</v-icon>
                <span v-else>{{ index + 1 }}</span>
              </td>
              <td>{{ entry.playerName }}</td>
              <td class="text-center font-weight-bold">{{ entry.highScore }}</td>
              <td class="text-center">{{ entry.gamesPlayed }}</td>
              <td class="text-center">{{ entry.averageScore }}</td>
              <td class="text-center">{{ entry.totalTreasuresFound }}</td>
            </tr>
          </tbody>
        </v-table>
      </v-col>
    </v-row>

    <v-row justify="center" class="mt-4">
      <v-col cols="auto">
        <v-btn color="primary" to="/gridseeker" prepend-icon="mdi-gamepad-variant">
          Play Game
        </v-btn>
      </v-col>
    </v-row>
  </v-container>
</template>

<script setup lang="ts">
import axios from 'axios'

const API_BASE = import.meta.env.VITE_API_URL || 'https://gridseeker-api-clayden-fshtdtbzfcb5crgk.eastus-01.azurewebsites.net'

interface LeaderboardEntry {
  playerName: string
  highScore: number
  gamesPlayed: number
  averageScore: number
  totalTreasuresFound: number
}

const entries = ref<LeaderboardEntry[]>([])
const loading = ref(true)
const error = ref('')

const fetchLeaderboard = async () => {
  loading.value = true
  error.value = ''
  try {
    const response = await axios.get(`${API_BASE}/api/leaderboard`)
    entries.value = response.data
  } catch {
    error.value = 'Could not load leaderboard. Make sure the API is running.'
  } finally {
    loading.value = false
  }
}

onMounted(() => {
  fetchLeaderboard()
})
</script>
