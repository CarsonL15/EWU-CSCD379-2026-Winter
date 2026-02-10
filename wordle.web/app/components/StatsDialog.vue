<template>
  <v-dialog v-model="dialogVisible" max-width="400">
    <v-card>
      <v-card-title class="d-flex align-center">
        <v-icon class="mr-2">mdi-chart-bar</v-icon>
        Statistics
      </v-card-title>

      <v-card-text>
        <!-- Main Stats -->
        <v-row class="text-center mb-4">
          <v-col cols="3">
            <div class="text-h4 font-weight-bold">{{ stats.gamesPlayed }}</div>
            <div class="text-caption">Played</div>
          </v-col>
          <v-col cols="3">
            <div class="text-h4 font-weight-bold">{{ winPercentage }}</div>
            <div class="text-caption">Win %</div>
          </v-col>
          <v-col cols="3">
            <div class="text-h4 font-weight-bold">{{ stats.currentStreak }}</div>
            <div class="text-caption">Current Streak</div>
          </v-col>
          <v-col cols="3">
            <div class="text-h4 font-weight-bold">{{ stats.maxStreak }}</div>
            <div class="text-caption">Max Streak</div>
          </v-col>
        </v-row>

        <!-- Average Guesses -->
        <div class="text-center mb-4">
          <div class="text-body-1">
            Average Guesses: <strong>{{ averageGuesses }}</strong>
          </div>
        </div>

        <v-divider class="mb-4" />

        <!-- Guess Distribution -->
        <div class="text-subtitle-1 font-weight-bold mb-2">Guess Distribution</div>
        <div v-for="(count, index) in stats.guessDistribution" :key="index" class="d-flex align-center mb-1">
          <div class="text-body-2 mr-2" style="width: 20px;">{{ index + 1 }}</div>
          <v-progress-linear
            :model-value="getDistributionPercentage(count)"
            :color="count > 0 ? 'primary' : 'grey'"
            height="20"
            rounded
          >
            <template v-slot:default>
              <span class="text-caption font-weight-bold">{{ count }}</span>
            </template>
          </v-progress-linear>
        </div>
      </v-card-text>

      <v-card-actions>
        <v-spacer />
        <v-btn color="primary" @click="dialogVisible = false">Close</v-btn>
      </v-card-actions>
    </v-card>
  </v-dialog>
</template>

<script setup lang="ts">
import { useGameState, type GameStats } from '~/composables/useGameState'

const props = defineProps<{
  modelValue: boolean
}>()

const emit = defineEmits<{
  'update:modelValue': [value: boolean]
}>()

const { loadStats, getWinPercentage, getAverageGuesses } = useGameState()

const dialogVisible = computed({
  get: () => props.modelValue,
  set: (value) => emit('update:modelValue', value)
})

const stats = ref<GameStats>({
  gamesPlayed: 0,
  gamesWon: 0,
  currentStreak: 0,
  maxStreak: 0,
  guessDistribution: [0, 0, 0, 0, 0, 0],
  totalGuesses: 0
})

const winPercentage = ref(0)
const averageGuesses = ref(0)

const maxDistribution = computed(() => {
  return Math.max(...stats.value.guessDistribution, 1)
})

const getDistributionPercentage = (count: number): number => {
  return (count / maxDistribution.value) * 100
}

watch(dialogVisible, (newVal) => {
  if (newVal) {
    stats.value = loadStats()
    winPercentage.value = getWinPercentage()
    averageGuesses.value = getAverageGuesses()
  }
})
</script>
