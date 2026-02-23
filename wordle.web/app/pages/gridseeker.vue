<template>
  <v-container class="game-container" fluid>
    <!-- Player Name Display -->
    <v-row justify="center" class="mb-1">
      <v-col cols="12" class="text-center">
        <h1 class="text-h4 mb-1">Grid Seeker</h1>
        <p class="text-body-2 text-medium-emphasis">Find the hidden treasures before you run out of scans!</p>
      </v-col>
    </v-row>

    <!-- Player name chip -->
    <v-row justify="center" class="mb-2">
      <v-col cols="auto">
        <v-chip
          color="primary"
          variant="outlined"
          @click="showNameDialog = true"
          prepend-icon="mdi-account"
        >
          {{ playerName }}
        </v-chip>
      </v-col>
    </v-row>

    <!-- Game Stats Bar -->
    <v-row justify="center" class="mb-3">
      <v-col cols="12" sm="8" md="6" lg="4">
        <v-card variant="tonal" class="pa-3">
          <div class="d-flex justify-space-around text-center">
            <div>
              <v-icon color="amber" size="small">mdi-diamond-stone</v-icon>
              <div class="text-h6">{{ treasuresFound }}/{{ totalTreasures }}</div>
              <div class="text-caption">Treasures</div>
            </div>
            <div>
              <v-icon color="blue" size="small">mdi-radar</v-icon>
              <div class="text-h6">{{ scansRemaining }}</div>
              <div class="text-caption">Scans</div>
            </div>
            <div>
              <v-icon color="red" size="small">mdi-heart</v-icon>
              <div class="text-h6">{{ livesRemaining }}</div>
              <div class="text-caption">Lives</div>
            </div>
            <div>
              <v-icon color="green" size="small">mdi-star</v-icon>
              <div class="text-h6">{{ score }}</div>
              <div class="text-caption">Score</div>
            </div>
          </div>
        </v-card>
      </v-col>
    </v-row>

    <!-- Game Grid -->
    <v-row justify="center">
      <v-col cols="12" class="d-flex justify-center">
        <div class="grid-board" :class="{ 'grid-loading': gridLoading }">
          <div
            v-for="row in 8"
            :key="row"
            class="grid-row"
          >
            <div
              v-for="col in 8"
              :key="col"
              class="grid-cell"
              :class="getCellClass(row - 1, col - 1)"
              @click="scanCell(row - 1, col - 1)"
            >
              <template v-if="revealed[row - 1][col - 1]">
                <v-icon v-if="cellResults[row - 1][col - 1] === 'treasure'" color="amber" size="small">mdi-diamond-stone</v-icon>
                <v-icon v-else-if="cellResults[row - 1][col - 1] === 'trap'" color="red" size="small">mdi-skull-crossbones</v-icon>
                <span v-else class="distance-text">{{ cellDistances[row - 1][col - 1] }}</span>
              </template>
              <v-icon v-else size="x-small" color="grey-lighten-1">mdi-help</v-icon>
            </div>
          </div>
        </div>
      </v-col>
    </v-row>

    <!-- Game Message -->
    <v-row justify="center" class="my-3">
      <v-col cols="auto">
        <v-alert
          v-if="gameMessage"
          :type="gameMessageType"
          variant="tonal"
          density="compact"
          closable
          @click:close="gameMessage = ''"
        >
          {{ gameMessage }}
        </v-alert>
      </v-col>
    </v-row>

    <!-- New Game Button -->
    <v-row justify="center" class="mb-4">
      <v-col cols="auto">
        <v-btn
          color="primary"
          @click="startNewGame"
          prepend-icon="mdi-refresh"
        >
          New Game
        </v-btn>
      </v-col>
    </v-row>

    <!-- Name Dialog -->
    <v-dialog v-model="showNameDialog" max-width="400" persistent>
      <v-card>
        <v-card-title>Enter Your Name</v-card-title>
        <v-card-text>
          <v-text-field
            v-model="nameInput"
            label="Player Name"
            variant="outlined"
            autofocus
            @keyup.enter="saveName"
          />
        </v-card-text>
        <v-card-actions>
          <v-spacer />
          <v-btn @click="showNameDialog = false">Cancel</v-btn>
          <v-btn color="primary" @click="saveName">Save</v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>

    <!-- Confetti -->
    <div v-if="showConfetti" class="confetti-container">
      <div v-for="i in 40" :key="i" class="confetti-piece" :style="confettiStyle(i)" />
    </div>

    <!-- Game Over Dialog -->
    <v-dialog v-model="showGameOver" max-width="400" persistent>
      <v-card>
        <v-card-title class="text-center">
          {{ gameWon ? 'You Won!' : 'Game Over' }}
        </v-card-title>
        <v-card-text class="text-center">
          <v-icon
            :color="gameWon ? 'amber' : 'red'"
            size="64"
            class="mb-3"
          >
            {{ gameWon ? 'mdi-trophy' : 'mdi-skull' }}
          </v-icon>
          <div class="text-h4 mb-2">Score: {{ score }}</div>
          <div class="text-body-1">
            Treasures Found: {{ treasuresFound }}/{{ totalTreasures }}<br/>
            Scans Remaining: {{ scansRemaining }}<br/>
            Lives Remaining: {{ livesRemaining }}
          </div>
        </v-card-text>
        <v-card-actions>
          <v-spacer />
          <v-btn color="primary" @click="showGameOver = false; startNewGame()">
            Play Again
          </v-btn>
          <v-btn to="/leaderboard">
            Leaderboard
          </v-btn>
          <v-spacer />
        </v-card-actions>
      </v-card>
    </v-dialog>
  </v-container>
</template>

<script setup lang="ts">
import axios from 'axios'

const API_BASE = import.meta.env.VITE_API_URL || 'https://gridseeker-api-clayden-fshtdtbzfcb5crgk.eastus-01.azurewebsites.net'

// Game state
const grid = ref<number[]>([])
const totalTreasures = ref(5)
const totalTraps = ref(4)
const scansRemaining = ref(15)
const livesRemaining = ref(3)
const treasuresFound = ref(0)
const score = ref(0)
const gameOver = ref(false)
const gameWon = ref(false)
const gameMessage = ref('')
const gameMessageType = ref<'success' | 'error' | 'info' | 'warning'>('info')
const startTime = ref(Date.now())
const gridLoading = ref(true)

// Cell tracking
const revealed = ref<boolean[][]>(Array.from({ length: 8 }, () => Array(8).fill(false)))
const cellResults = ref<string[][]>(Array.from({ length: 8 }, () => Array(8).fill('')))
const cellDistances = ref<(number | string)[][]>(Array.from({ length: 8 }, () => Array(8).fill('')))

// Player name
const playerName = ref('Guest')
const nameInput = ref('')
const showNameDialog = ref(false)
const showGameOver = ref(false)
const showConfetti = ref(false)

const confettiColors = ['#FFD700', '#FF6B6B', '#4ECDC4', '#45B7D1', '#96CEB4', '#FFEAA7', '#DDA0DD', '#98D8C8']

const confettiStyle = (i: number) => ({
  left: `${Math.random() * 100}%`,
  backgroundColor: confettiColors[i % confettiColors.length],
  animationDelay: `${Math.random() * 0.5}s`,
  animationDuration: `${1 + Math.random() * 1.5}s`,
})

// Load name from localStorage
onMounted(() => {
  if (typeof window !== 'undefined') {
    const saved = localStorage.getItem('gridseeker_name')
    if (saved) {
      playerName.value = saved
    } else {
      showNameDialog.value = true
    }
  }
  startNewGame()
})

const saveName = () => {
  if (nameInput.value.trim()) {
    playerName.value = nameInput.value.trim()
    if (typeof window !== 'undefined') {
      localStorage.setItem('gridseeker_name', playerName.value)
    }
  }
  showNameDialog.value = false
}

const startNewGame = async () => {
  gridLoading.value = true
  gameOver.value = false
  gameWon.value = false
  scansRemaining.value = 15
  livesRemaining.value = 3
  treasuresFound.value = 0
  score.value = 0
  gameMessage.value = ''
  startTime.value = Date.now()
  grid.value = []
  revealed.value = Array.from({ length: 8 }, () => Array(8).fill(false))
  cellResults.value = Array.from({ length: 8 }, () => Array(8).fill(''))
  cellDistances.value = Array.from({ length: 8 }, () => Array(8).fill(''))

  try {
    const response = await axios.get(`${API_BASE}/api/game/new`)
    grid.value = response.data.grid
    totalTreasures.value = response.data.treasureCount
    totalTraps.value = response.data.trapCount
  } catch {
    // Fallback: generate grid client-side if API is unavailable
    generateLocalGrid()
    gameMessage.value = 'Playing offline (API unavailable)'
    gameMessageType.value = 'warning'
  } finally {
    gridLoading.value = false
  }
}

const generateLocalGrid = () => {
  const cells = Array(64).fill(0)
  let placed = 0
  while (placed < 5) {
    const i = Math.floor(Math.random() * 64)
    if (cells[i] === 0) { cells[i] = 1; placed++ }
  }
  placed = 0
  while (placed < 4) {
    const i = Math.floor(Math.random() * 64)
    if (cells[i] === 0) { cells[i] = 2; placed++ }
  }
  grid.value = cells
  totalTreasures.value = 5
  totalTraps.value = 4
}

const getCellType = (row: number, col: number): number => {
  return grid.value[row * 8 + col] || 0
}

const scanCell = (row: number, col: number) => {
  if (gridLoading.value || gameOver.value || revealed.value[row][col] || scansRemaining.value <= 0) return

  revealed.value[row][col] = true
  scansRemaining.value--

  const cellType = getCellType(row, col)

  if (cellType === 1) {
    // Treasure
    cellResults.value[row][col] = 'treasure'
    treasuresFound.value++
    gameMessage.value = 'Treasure found!'
    gameMessageType.value = 'success'

    if (treasuresFound.value >= totalTreasures.value) {
      endGame(true)
      return
    }
  } else if (cellType === 2) {
    // Trap
    cellResults.value[row][col] = 'trap'
    livesRemaining.value--
    gameMessage.value = 'You hit a trap!'
    gameMessageType.value = 'error'

    if (livesRemaining.value <= 0) {
      endGame(false)
      return
    }
  } else {
    // Empty - calculate distance to nearest treasure
    cellResults.value[row][col] = 'empty'
    const dist = calcDistance(row, col)
    cellDistances.value[row][col] = dist
    gameMessage.value = `Nearest treasure is ${dist} cells away`
    gameMessageType.value = 'info'
  }

  // Check if out of scans
  if (scansRemaining.value <= 0 && treasuresFound.value < totalTreasures.value) {
    endGame(false)
  }
}

const calcDistance = (row: number, col: number): number => {
  let minDist = Infinity
  for (let r = 0; r < 8; r++) {
    for (let c = 0; c < 8; c++) {
      if (getCellType(r, c) === 1 && !revealed.value[r][c]) {
        const dist = Math.sqrt(Math.pow(row - r, 2) + Math.pow(col - c, 2))
        minDist = Math.min(minDist, dist)
      }
    }
  }
  // Also check already-found treasures for distance display
  if (minDist === Infinity) {
    for (let r = 0; r < 8; r++) {
      for (let c = 0; c < 8; c++) {
        if (getCellType(r, c) === 1) {
          const dist = Math.sqrt(Math.pow(row - r, 2) + Math.pow(col - c, 2))
          minDist = Math.min(minDist, dist)
        }
      }
    }
  }
  return Math.round(minDist * 10) / 10
}

const endGame = async (won: boolean) => {
  gameOver.value = true
  gameWon.value = won
  score.value = treasuresFound.value * (scansRemaining.value + 1)

  // Reveal all cells
  for (let r = 0; r < 8; r++) {
    for (let c = 0; c < 8; c++) {
      if (!revealed.value[r][c]) {
        revealed.value[r][c] = true
        const ct = getCellType(r, c)
        if (ct === 1) cellResults.value[r][c] = 'treasure'
        else if (ct === 2) cellResults.value[r][c] = 'trap'
        else cellResults.value[r][c] = 'empty'
      }
    }
  }

  const durationSeconds = Math.round((Date.now() - startTime.value) / 1000)

  // Save to API
  try {
    await axios.post(`${API_BASE}/api/game/save`, {
      playerName: playerName.value,
      treasuresFound: treasuresFound.value,
      scansRemaining: scansRemaining.value,
      livesRemaining: livesRemaining.value,
      score: score.value,
      won: won,
      durationSeconds: durationSeconds
    })
  } catch {
    // Silently fail if API is down
  }

  if (won) {
    showConfetti.value = true
    setTimeout(() => { showConfetti.value = false }, 3000)
  }
  showGameOver.value = true
}

const getCellClass = (row: number, col: number) => {
  if (!revealed.value[row][col]) return 'cell-hidden'
  const result = cellResults.value[row][col]
  if (result === 'treasure') return 'cell-treasure'
  if (result === 'trap') return 'cell-trap'
  return 'cell-empty'
}
</script>

<style scoped>
.game-container {
  max-width: 100%;
  padding-top: 8px;
}

.grid-board {
  display: inline-block;
}

.grid-row {
  display: flex;
}

.grid-cell {
  width: 52px;
  height: 52px;
  border: 2px solid rgba(128, 128, 128, 0.3);
  display: flex;
  align-items: center;
  justify-content: center;
  cursor: pointer;
  transition: all 0.2s ease;
  margin: 1px;
  border-radius: 6px;
  font-weight: bold;
  font-size: 0.85rem;
  user-select: none;
}

.grid-loading {
  opacity: 0.5;
  pointer-events: none;
}

.cell-hidden {
  background: rgba(100, 100, 100, 0.15);
}

.cell-hidden:hover {
  background: rgba(33, 150, 243, 0.25);
  border-color: #2196F3;
  transform: scale(1.05);
}

.cell-treasure {
  background: linear-gradient(135deg, #FFD54F 0%, #FFB300 100%);
  border-color: #FFB300;
  animation: pop 0.3s ease;
}

.cell-trap {
  background: linear-gradient(135deg, #EF5350 0%, #C62828 100%);
  border-color: #C62828;
  animation: shake 0.4s ease;
}

.cell-empty {
  background: linear-gradient(135deg, #E3F2FD 0%, #BBDEFB 100%);
  border-color: #90CAF9;
}

.distance-text {
  color: #1565C0;
  font-weight: bold;
  font-size: 0.8rem;
}

@keyframes pop {
  0% { transform: scale(1); }
  50% { transform: scale(1.15); }
  100% { transform: scale(1); }
}

@keyframes shake {
  0%, 100% { transform: translateX(0); }
  20%, 60% { transform: translateX(-3px); }
  40%, 80% { transform: translateX(3px); }
}

.confetti-container {
  position: fixed;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  pointer-events: none;
  z-index: 9999;
  overflow: hidden;
}

.confetti-piece {
  position: absolute;
  top: -10px;
  width: 10px;
  height: 10px;
  border-radius: 2px;
  animation: confetti-fall linear forwards;
}

@keyframes confetti-fall {
  0% {
    transform: translateY(0) rotate(0deg);
    opacity: 1;
  }
  100% {
    transform: translateY(100vh) rotate(720deg);
    opacity: 0;
  }
}

@media (max-width: 480px) {
  .grid-cell {
    width: 38px;
    height: 38px;
    font-size: 0.7rem;
  }
}
</style>
