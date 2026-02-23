export interface Guess {
  word: string
  results: ('correct' | 'present' | 'absent')[]
}

export function validWords(allWords: string[], guesses: Guess[]): string[] {
  if (guesses.length === 0) {
    return allWords
  }

  return allWords.filter(word => {
    for (const guess of guesses) {
      if (!matchesGuess(word, guess)) {
        return false
      }
    }
    return true
  })
}

function matchesGuess(word: string, guess: Guess): boolean {
  const wordLower = word.toLowerCase()
  const guessLower = guess.word.toLowerCase()

  for (let i = 0; i < guess.results.length; i++) {
    const result = guess.results[i]
    const guessedLetter = guessLower[i]

    if (result === 'correct') {
      // Letter must be at this exact position
      if (wordLower[i] !== guessedLetter) {
        return false
      }
    } else if (result === 'absent') {
      // Letter must not be in the word (unless it's also marked correct/present elsewhere)
      const letterOccurrencesInGuess = countLetterOccurrences(guessLower, guessedLetter)
      const correctOrPresentCount = countCorrectOrPresent(guess, guessedLetter)

      if (correctOrPresentCount === 0) {
        // Letter truly absent - should not appear in word at all
        if (wordLower.includes(guessedLetter)) {
          return false
        }
      } else {
        // Letter appears elsewhere as correct/present, so it can be in the word
        // but not in more positions than indicated
        const letterCountInWord = countLetterOccurrences(wordLower, guessedLetter)
        if (letterCountInWord > correctOrPresentCount) {
          return false
        }
      }
    } else if (result === 'present') {
      // Letter must be in the word but NOT at this position
      if (wordLower[i] === guessedLetter) {
        return false
      }
      if (!wordLower.includes(guessedLetter)) {
        return false
      }
    }
  }

  return true
}

function countLetterOccurrences(word: string, letter: string): number {
  return word.split('').filter(c => c === letter).length
}

function countCorrectOrPresent(guess: Guess, letter: string): number {
  let count = 0
  const guessLower = guess.word.toLowerCase()
  for (let i = 0; i < guess.results.length; i++) {
    if (guessLower[i] === letter && (guess.results[i] === 'correct' || guess.results[i] === 'present')) {
      count++
    }
  }
  return count
}
