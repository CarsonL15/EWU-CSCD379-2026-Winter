<template>
  <v-container class="game-container" fluid>
    <v-row justify="center" class="mb-2">
      <v-col cols="12" class="text-center">
        <h1 class="text-h4 mb-1">Word Wizardry</h1>
        <p class="text-body-2 text-medium-emphasis">Guess the 5-letter word in 6 tries</p>
      </v-col>
    </v-row>

    <!-- Game Mode Selector -->
    <v-row justify="center" class="mb-2">
      <v-col cols="auto">
        <v-btn-toggle
          v-model="gameMode"
          mandatory
          divided
          density="comfortable"
          color="primary"
          @update:model-value="handleModeChange"
        >
          <v-btn value="daily" :disabled="dailyCompleted && gameMode !== 'daily'">
            <v-icon start>mdi-calendar-today</v-icon>
            Daily
            <v-icon v-if="dailyCompleted" end color="success" size="small">mdi-check-circle</v-icon>
          </v-btn>
          <v-btn value="random">
            <v-icon start>mdi-shuffle-variant</v-icon>
            Random
          </v-btn>
        </v-btn-toggle>
      </v-col>
    </v-row>

    <!-- Action Buttons -->
    <v-row justify="center" class="mb-2">
      <v-col cols="auto" class="d-flex align-center ga-2 flex-wrap justify-center">
        <WordList :filtered-words="possibleWords" @select-word="fillCurrentGuess" />

        <v-btn
          color="warning"
          variant="tonal"
          size="small"
          @click="showHints = true"
          :disabled="gameOver"
        >
          <v-icon start size="small">mdi-lightbulb</v-icon>
          Hint
        </v-btn>

        <v-btn
          color="info"
          variant="tonal"
          size="small"
          @click="showStats = true"
        >
          <v-icon start size="small">mdi-chart-bar</v-icon>
          Stats
        </v-btn>

        <v-btn
          v-if="gameMode === 'random'"
          color="primary"
          variant="tonal"
          size="small"
          @click="startRandomGame"
        >
          <v-icon start size="small">mdi-refresh</v-icon>
          New Word
        </v-btn>
      </v-col>
    </v-row>

    <!-- Game Grid -->
    <v-row justify="center">
      <v-col cols="12" sm="10" md="8" lg="6" xl="4">
        <div class="game-board mx-auto">
          <v-row
            v-for="(guess, rowIndex) in guesses"
            :key="rowIndex"
            no-gutters
            justify="center"
            class="mb-1"
            :class="{ 'shake-row': shakeRow === rowIndex }"
          >
            <v-col
              v-for="(letter, colIndex) in guess.letters"
              :key="colIndex"
              cols="auto"
              class="pa-1"
            >
              <div
                class="tile"
                :class="getTileClass(rowIndex, colIndex)"
              >
                {{ letter }}
              </div>
            </v-col>
          </v-row>
        </div>
      </v-col>
    </v-row>

    <!-- Message Display -->
    <v-row justify="center" class="my-3">
      <v-col cols="auto">
        <v-alert
          v-if="message"
          :type="messageType"
          variant="tonal"
          closable
          density="compact"
          @click:close="message = ''"
        >
          {{ message }}
        </v-alert>
      </v-col>
    </v-row>

    <!-- Keyboard -->
    <v-row justify="center">
      <v-col cols="12" sm="10" md="8" lg="6" xl="4">
        <div class="keyboard mx-auto">
          <v-row
            v-for="(row, rowIndex) in keyboardRows"
            :key="rowIndex"
            justify="center"
            no-gutters
            class="mb-1"
          >
            <v-btn
              v-for="key in row"
              :key="key"
              :class="getKeyClass(key)"
              class="keyboard-key ma-1"
              :elevation="2"
              @click="handleKeyPress(key)"
              size="small"
            >
              <v-icon v-if="key === 'BACKSPACE'" size="small">mdi-backspace-outline</v-icon>
              <span v-else>{{ key }}</span>
            </v-btn>
          </v-row>
        </div>
      </v-col>
    </v-row>

    <!-- Dialogs -->
    <StatsDialog v-model="showStats" />
    <HintDialog v-model="showHints" :target-word="targetWord" />
    <DefinitionDialog
      v-model="showDefinition"
      :word="targetWord"
      :attempts="currentRow"
    />
  </v-container>
</template>

<script setup lang="ts">
import { words } from '~/data/words'
import { answerWords } from '~/data/answerWords'
import { validWords, type Guess } from '~/utils/wordList'
import { useGameState } from '~/composables/useGameState'

interface GuessRow {
  letters: string[]
  results: ('correct' | 'present' | 'absent' | 'empty')[]
  submitted: boolean
}

const {
  getWordOfDay,
  getTodayString,
  loadWordOfDayState,
  saveWordOfDayState,
  isWordOfDayAvailable,
  getRandomWord,
  addPlayedWord,
  recordGameResult
} = useGameState()

const keyboardRows = [
  ['Q', 'W', 'E', 'R', 'T', 'Y', 'U', 'I', 'O', 'P'],
  ['A', 'S', 'D', 'F', 'G', 'H', 'J', 'K', 'L'],
  ['ENTER', 'Z', 'X', 'C', 'V', 'B', 'N', 'M', 'BACKSPACE']
]

const targetWord = ref('')
const currentRow = ref(0)
const currentCol = ref(0)
const gameOver = ref(false)
const message = ref('')
const messageType = ref<'success' | 'error' | 'info'>('info')
const keyStates = ref<Record<string, 'correct' | 'present' | 'absent'>>({})
const shakeRow = ref<number | null>(null)

// Game mode: 'daily' or 'random'
const gameMode = ref<'daily' | 'random'>('daily')
const dailyCompleted = ref(false)

const showStats = ref(false)
const showHints = ref(false)
const showDefinition = ref(false)

// Computed for template compatibility
const isWordOfDay = computed(() => gameMode.value === 'daily')

const guesses = ref<GuessRow[]>([])

// Combine both word lists for validation (all words are valid guesses)
const allValidWords = computed(() => {
  const combined = new Set([...words.map(w => w.toLowerCase()), ...answerWords.map(w => w.toLowerCase())])
  return Array.from(combined)
})

const initializeGuesses = () => {
  return Array.from({ length: 6 }, () => ({
    letters: Array(5).fill(''),
    results: Array(5).fill('empty') as ('correct' | 'present' | 'absent' | 'empty')[],
    submitted: false
  }))
}

const submittedGuesses = computed((): Guess[] => {
  return guesses.value
    .filter(g => g.submitted)
    .map(g => ({
      word: g.letters.join(''),
      results: g.results.filter((r): r is 'correct' | 'present' | 'absent' => r !== 'empty')
    }))
})

const possibleWords = computed(() => {
  return validWords(answerWords, submittedGuesses.value)
})

const triggerShake = () => {
  shakeRow.value = currentRow.value
  setTimeout(() => {
    shakeRow.value = null
  }, 500)
}

// Extracted shared reset logic per code review feedback
const resetBoard = () => {
  guesses.value = initializeGuesses()
  currentRow.value = 0
  currentCol.value = 0
  gameOver.value = false
  message.value = ''
  keyStates.value = {}
}

const checkDailyCompleted = () => {
  const wotdState = loadWordOfDayState()
  dailyCompleted.value = !!(wotdState && wotdState.completed)
}

const handleModeChange = (newMode: 'daily' | 'random') => {
  if (newMode === 'daily') {
    playWordOfDay()
  } else {
    startRandomGame()
  }
}

const playWordOfDay = () => {
  const wotdState = loadWordOfDayState()

  // If already completed, show the completed state
  if (wotdState && wotdState.completed) {
    targetWord.value = wotdState.word
    resetBoard()
    gameOver.value = true
    message.value = wotdState.won
      ? "You already completed today's word!"
      : `Today's word was ${wotdState.word}`
    messageType.value = wotdState.won ? 'success' : 'info'
    dailyCompleted.value = true
    return
  }

  targetWord.value = getWordOfDay()
  resetBoard()
  gameMode.value = 'daily'
}

const startRandomGame = () => {
  targetWord.value = getRandomWord()
  resetBoard()
  gameMode.value = 'random'
  checkDailyCompleted()
}

const fillCurrentGuess = (word: string) => {
  if (gameOver.value) return

  const row = guesses.value[currentRow.value]
  const upperWord = word.toUpperCase()

  for (let i = 0; i < 5; i++) {
    row.letters[i] = upperWord[i] || ''
  }
  currentCol.value = 5
}

const handleKeyPress = (key: string) => {
  if (gameOver.value) return

  if (key === 'ENTER') {
    submitGuess()
  } else if (key === 'BACKSPACE') {
    deleteLetter()
  } else {
    addLetter(key)
  }
}

const addLetter = (letter: string) => {
  if (currentCol.value < 5) {
    guesses.value[currentRow.value].letters[currentCol.value] = letter
    currentCol.value++
  }
}

const deleteLetter = () => {
  if (currentCol.value > 0) {
    currentCol.value--
    guesses.value[currentRow.value].letters[currentCol.value] = ''
  }
}

const submitGuess = () => {
  const row = guesses.value[currentRow.value]
  const guessWord = row.letters.join('')

  if (guessWord.length !== 5) {
    message.value = 'Not enough letters'
    messageType.value = 'error'
    triggerShake()
    return
  }

  if (!allValidWords.value.includes(guessWord.toLowerCase())) {
    message.value = 'Not in word list'
    messageType.value = 'error'
    triggerShake()
    return
  }

  // Calculate results
  const targetLetters = targetWord.value.split('')
  const guessLetters = guessWord.split('')
  const results: ('correct' | 'present' | 'absent')[] = Array(5).fill('absent')
  const targetLetterCounts: Record<string, number> = {}

  // Count target letters
  targetLetters.forEach(letter => {
    targetLetterCounts[letter] = (targetLetterCounts[letter] || 0) + 1
  })

  // First pass: mark correct letters
  guessLetters.forEach((letter, i) => {
    if (letter === targetLetters[i]) {
      results[i] = 'correct'
      targetLetterCounts[letter]--
    }
  })

  // Second pass: mark present letters
  guessLetters.forEach((letter, i) => {
    if (results[i] !== 'correct' && targetLetterCounts[letter] > 0) {
      results[i] = 'present'
      targetLetterCounts[letter]--
    }
  })

  row.results = results
  row.submitted = true

  // Update keyboard states
  guessLetters.forEach((letter, i) => {
    const currentState = keyStates.value[letter]
    const newState = results[i]

    if (newState === 'correct') {
      keyStates.value[letter] = 'correct'
    } else if (newState === 'present' && currentState !== 'correct') {
      keyStates.value[letter] = 'present'
    } else if (!currentState) {
      keyStates.value[letter] = 'absent'
    }
  })

  // Check win/lose
  const won = guessWord === targetWord.value
  const lost = !won && currentRow.value === 5

  if (won) {
    message.value = 'Congratulations! You won!'
    messageType.value = 'success'
    gameOver.value = true
    currentRow.value++ // Increment for stats (attempts count)
    handleGameEnd(true, currentRow.value)
    // Show definition after a short delay
    setTimeout(() => {
      showDefinition.value = true
    }, 500)
  } else if (lost) {
    message.value = `Game Over! The word was ${targetWord.value}`
    messageType.value = 'error'
    gameOver.value = true
    handleGameEnd(false, 6)
  } else {
    currentRow.value++
    currentCol.value = 0
    message.value = ''
  }
}

const handleGameEnd = (won: boolean, attempts: number) => {
  // Record stats
  recordGameResult(won, attempts)

  // If word of the day, save completion state
  if (isWordOfDay.value) {
    saveWordOfDayState({
      date: getTodayString(),
      word: targetWord.value,
      completed: true,
      won: won
    })
    dailyCompleted.value = true
  } else {
    // Add to played words for random mode
    addPlayedWord(targetWord.value)
  }
}

const getTileClass = (rowIndex: number, colIndex: number) => {
  const row = guesses.value[rowIndex]
  if (!row.submitted) {
    return row.letters[colIndex] ? 'tile-filled' : 'tile-empty'
  }
  return `tile-${row.results[colIndex]}`
}

const getKeyClass = (key: string) => {
  const state = keyStates.value[key]
  if (state) {
    return `key-${state}`
  }
  return ''
}

// Handle physical keyboard
onMounted(() => {
  // Check daily completion status
  checkDailyCompleted()

  // Start with daily mode (will show completed state if already done)
  playWordOfDay()

  const handleKeydown = (event: KeyboardEvent) => {
    if (gameOver.value) return

    const key = event.key.toUpperCase()

    if (key === 'ENTER') {
      submitGuess()
    } else if (key === 'BACKSPACE') {
      deleteLetter()
    } else if (/^[A-Z]$/.test(key)) {
      addLetter(key)
    }
  }

  window.addEventListener('keydown', handleKeydown)

  onUnmounted(() => {
    window.removeEventListener('keydown', handleKeydown)
  })
})
</script>

<style scoped>
.game-container {
  max-width: 100%;
  padding-top: 8px;
}

.game-board {
  max-width: 350px;
}

.tile {
  width: 58px;
  height: 58px;
  border: 2px solid rgba(128, 128, 128, 0.3);
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 2rem;
  font-weight: bold;
  text-transform: uppercase;
  transition: all 0.2s ease;
}

.tile-empty {
  background: transparent;
}

.tile-filled {
  border-color: rgba(128, 128, 128, 0.6);
  animation: pop 0.1s ease;
}

.tile-correct {
  background: linear-gradient(135deg, #6aaa64 0%, #538d4e 100%);
  border-color: #6aaa64;
  color: white;
}

.tile-present {
  background: linear-gradient(135deg, #c9b458 0%, #b59f3b 100%);
  border-color: #c9b458;
  color: white;
}

.tile-absent {
  background: linear-gradient(135deg, #787c7e 0%, #3a3a3c 100%);
  border-color: #787c7e;
  color: white;
}

.keyboard {
  max-width: 500px;
}

.keyboard-key {
  min-width: 30px;
  height: 50px;
  font-weight: bold;
  text-transform: uppercase;
}

.key-correct {
  background: linear-gradient(135deg, #6aaa64 0%, #538d4e 100%) !important;
  color: white !important;
}

.key-present {
  background: linear-gradient(135deg, #c9b458 0%, #b59f3b 100%) !important;
  color: white !important;
}

.key-absent {
  background: linear-gradient(135deg, #787c7e 0%, #5a5a5a 100%) !important;
  color: white !important;
}

/* Shake animation for invalid words */
.shake-row {
  animation: shake 0.5s ease-in-out;
}

@keyframes shake {
  0%, 100% { transform: translateX(0); }
  10%, 30%, 50%, 70%, 90% { transform: translateX(-5px); }
  20%, 40%, 60%, 80% { transform: translateX(5px); }
}

@keyframes pop {
  0% { transform: scale(1); }
  50% { transform: scale(1.1); }
  100% { transform: scale(1); }
}

@media (max-width: 400px) {
  .tile {
    width: 48px;
    height: 48px;
    font-size: 1.5rem;
  }

  .keyboard-key {
    min-width: 24px;
    height: 45px;
    font-size: 0.75rem;
  }
}
</style>
