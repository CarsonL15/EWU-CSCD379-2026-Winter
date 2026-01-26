import { answerWords } from '~/data/answerWords'

export interface GameStats {
  gamesPlayed: number
  gamesWon: number
  currentStreak: number
  maxStreak: number
  guessDistribution: number[] // index 0 = won in 1, index 5 = won in 6
  totalGuesses: number // for calculating average
}

export interface WordOfDayState {
  date: string
  word: string
  completed: boolean
  won: boolean
}

const STATS_KEY = 'wordle-stats'
const WOTD_KEY = 'wordle-wotd'
const PLAYED_WORDS_KEY = 'wordle-played-words'

export function useGameState() {
  // Get today's date string
  const getTodayString = (): string => {
    const today = new Date()
    return `${today.getFullYear()}-${String(today.getMonth() + 1).padStart(2, '0')}-${String(today.getDate()).padStart(2, '0')}`
  }

  // Get word of the day based on date
  const getWordOfDay = (): string => {
    const today = new Date()
    const startDate = new Date(2024, 0, 1) // Jan 1, 2024
    const daysSinceStart = Math.floor((today.getTime() - startDate.getTime()) / (1000 * 60 * 60 * 24))
    const wordIndex = daysSinceStart % answerWords.length
    return answerWords[wordIndex].toUpperCase()
  }

  // Load stats from localStorage
  const loadStats = (): GameStats => {
    if (typeof window === 'undefined') {
      return getDefaultStats()
    }
    const stored = localStorage.getItem(STATS_KEY)
    if (stored) {
      try {
        return JSON.parse(stored)
      } catch {
        return getDefaultStats()
      }
    }
    return getDefaultStats()
  }

  // Save stats to localStorage
  const saveStats = (stats: GameStats): void => {
    if (typeof window === 'undefined') return
    localStorage.setItem(STATS_KEY, JSON.stringify(stats))
  }

  // Get default stats
  const getDefaultStats = (): GameStats => ({
    gamesPlayed: 0,
    gamesWon: 0,
    currentStreak: 0,
    maxStreak: 0,
    guessDistribution: [0, 0, 0, 0, 0, 0],
    totalGuesses: 0
  })

  // Load word of the day state
  const loadWordOfDayState = (): WordOfDayState | null => {
    if (typeof window === 'undefined') return null
    const stored = localStorage.getItem(WOTD_KEY)
    if (stored) {
      try {
        const state = JSON.parse(stored) as WordOfDayState
        // Check if it's still today's word
        if (state.date === getTodayString()) {
          return state
        }
      } catch {
        // Invalid data, return null
      }
    }
    return null
  }

  // Save word of the day state
  const saveWordOfDayState = (state: WordOfDayState): void => {
    if (typeof window === 'undefined') return
    localStorage.setItem(WOTD_KEY, JSON.stringify(state))
  }

  // Get list of played random words (to avoid repeats)
  const getPlayedWords = (): string[] => {
    if (typeof window === 'undefined') return []
    const stored = localStorage.getItem(PLAYED_WORDS_KEY)
    if (stored) {
      try {
        return JSON.parse(stored)
      } catch {
        return []
      }
    }
    return []
  }

  // Add word to played list
  const addPlayedWord = (word: string): void => {
    if (typeof window === 'undefined') return
    const played = getPlayedWords()
    if (!played.includes(word.toLowerCase())) {
      played.push(word.toLowerCase())
      // Keep only last 100 words to prevent storage bloat
      if (played.length > 100) {
        played.shift()
      }
      localStorage.setItem(PLAYED_WORDS_KEY, JSON.stringify(played))
    }
  }

  // Get a random word that hasn't been played recently
  const getRandomWord = (): string => {
    const played = getPlayedWords()
    const available = answerWords.filter(w => !played.includes(w.toLowerCase()))

    // If all words have been played, reset and use all words
    const wordPool = available.length > 0 ? available : answerWords
    const randomIndex = Math.floor(Math.random() * wordPool.length)
    return wordPool[randomIndex].toUpperCase()
  }

  // Check if word of the day is available (not completed)
  const isWordOfDayAvailable = (): boolean => {
    const state = loadWordOfDayState()
    return !state || !state.completed
  }

  // Record game result
  const recordGameResult = (won: boolean, attempts: number): void => {
    const stats = loadStats()
    stats.gamesPlayed++

    if (won) {
      stats.gamesWon++
      stats.currentStreak++
      stats.maxStreak = Math.max(stats.maxStreak, stats.currentStreak)
      stats.guessDistribution[attempts - 1]++
      stats.totalGuesses += attempts
    } else {
      stats.currentStreak = 0
    }

    saveStats(stats)
  }

  // Calculate average guesses to win
  const getAverageGuesses = (): number => {
    const stats = loadStats()
    if (stats.gamesWon === 0) return 0
    return Math.round((stats.totalGuesses / stats.gamesWon) * 10) / 10
  }

  // Calculate win percentage
  const getWinPercentage = (): number => {
    const stats = loadStats()
    if (stats.gamesPlayed === 0) return 0
    return Math.round((stats.gamesWon / stats.gamesPlayed) * 100)
  }

  return {
    getTodayString,
    getWordOfDay,
    loadStats,
    saveStats,
    getDefaultStats,
    loadWordOfDayState,
    saveWordOfDayState,
    getPlayedWords,
    addPlayedWord,
    getRandomWord,
    isWordOfDayAvailable,
    recordGameResult,
    getAverageGuesses,
    getWinPercentage
  }
}
