import { describe, it, expect } from 'vitest'
import { validWords, type Guess } from '../app/utils/wordList'

describe('validWords', () => {
  const testWords = ['apple', 'grape', 'peach', 'lemon', 'mango', 'melon', 'berry', 'plumb', 'crane', 'slate']

  it('returns all words when no guesses', () => {
    const result = validWords(testWords, [])
    expect(result).toEqual(testWords)
  })

  it('filters by correct letter position', () => {
    // If 'a' is correct at position 0, words must start with 'a'
    // The absent letters p, p, l, e would filter out words containing those
    const words = ['apple', 'angry', 'allow', 'azure', 'amaze']
    const guesses: Guess[] = [
      { word: 'axxxx', results: ['correct', 'absent', 'absent', 'absent', 'absent'] }
    ]
    const result = validWords(words, guesses)
    // All words start with 'a', none contain x
    expect(result).toEqual(['apple', 'angry', 'allow', 'azure', 'amaze'])
  })

  it('filters by absent letters', () => {
    const guesses: Guess[] = [
      { word: 'xxxyz', results: ['absent', 'absent', 'absent', 'absent', 'absent'] }
    ]
    const result = validWords(testWords, guesses)
    // No words should contain x, y, or z
    expect(result).toContain('apple')
    expect(result).toContain('grape')
    expect(result).toContain('peach')
    expect(result).not.toContain('berry') // contains y
  })

  it('filters by present letters (in word but wrong position)', () => {
    const guesses: Guess[] = [
      { word: 'exxxx', results: ['present', 'absent', 'absent', 'absent', 'absent'] }
    ]
    const result = validWords(testWords, guesses)
    // Words must contain 'e' but not at position 0, and not contain x
    expect(result).toContain('apple') // has 'e' at position 4
    expect(result).toContain('grape') // has 'e' at position 4
    expect(result).toContain('peach') // has 'e' at position 1
    expect(result).not.toContain('mango') // no 'e'
    expect(result).toContain('berry') // has 'e' at position 1
  })

  it('filters words not starting with e when e is present at position 0', () => {
    const guesses: Guess[] = [
      { word: 'exxxx', results: ['present', 'absent', 'absent', 'absent', 'absent'] }
    ]
    const result = validWords(testWords, guesses)
    // Words must contain 'e' but NOT at position 0
    result.forEach(word => {
      expect(word[0].toLowerCase()).not.toBe('e')
      expect(word.toLowerCase()).toContain('e')
    })
  })

  it('handles multiple guesses', () => {
    // peach = p-e-a-c-h (a at position 2)
    const words = ['peach', 'beach', 'reach', 'leach', 'teach', 'mocha', 'alpha', 'meant']
    const guesses: Guess[] = [
      { word: 'xxaxx', results: ['absent', 'absent', 'correct', 'absent', 'absent'] }
    ]
    const result = validWords(words, guesses)
    // Must have 'a' at position 2, must not contain x
    expect(result).toContain('peach') // p-e-a-c-h: 'a' at position 2
    expect(result).toContain('beach') // b-e-a-c-h: 'a' at position 2
    expect(result).toContain('reach') // r-e-a-c-h: 'a' at position 2
    expect(result).toContain('leach') // l-e-a-c-h: 'a' at position 2
    expect(result).toContain('teach') // t-e-a-c-h: 'a' at position 2
    expect(result).not.toContain('mocha') // m-o-c-h-a: 'a' at position 4, not 2
    expect(result).not.toContain('alpha') // a-l-p-h-a: 'a' at position 0 and 4, not 2
    expect(result).toContain('meant') // m-e-a-n-t: 'a' at position 2
  })

  it('handles case insensitivity', () => {
    const words = ['APPLE', 'Grape', 'peach']
    const guesses: Guess[] = [
      { word: 'AXXXX', results: ['correct', 'absent', 'absent', 'absent', 'absent'] }
    ]
    const result = validWords(words, guesses)
    expect(result).toContain('APPLE')
    expect(result).not.toContain('Grape')
    expect(result).not.toContain('peach')
  })

  it('handles duplicate letters correctly - absent duplicate', () => {
    const words = ['sweet', 'sheep', 'steel', 'speed', 'creek']
    const guesses: Guess[] = [
      { word: 'sleek', results: ['correct', 'absent', 'correct', 'correct', 'absent'] }
    ]
    const result = validWords(words, guesses)
    // s at pos 0, e at pos 2, e at pos 3
    // l is absent, k is absent
    expect(result).toContain('sweet') // s-w-e-e-t: s at 0, e at 2, e at 3, no l, no k
    expect(result).toContain('speed') // s-p-e-e-d: s at 0, e at 2, e at 3, no l, no k
    expect(result).not.toContain('steel') // has l
    expect(result).not.toContain('creek') // has k and doesn't start with s
  })

  it('handles letter that is both correct and absent in same guess', () => {
    // Word has one 'e', guessed two 'e's
    const words = ['trees', 'tease', 'teens']
    const guesses: Guess[] = [
      { word: 'teeth', results: ['correct', 'correct', 'correct', 'absent', 'absent'] }
    ]
    const result = validWords(words, guesses)
    // t at pos 0, e at pos 1, e at pos 2
    // Second 't' is absent (so only one 't' allowed), 'h' is absent
    expect(result).toContain('teens') // t-e-e-n-s: t at 0, e at 1, e at 2, no h
    expect(result).not.toContain('trees') // t-r-e-e-s: r at pos 1, not e
    expect(result).not.toContain('tease') // t-e-a-s-e: a at pos 2, not e
  })

  it('returns empty array when no words match', () => {
    const guesses: Guess[] = [
      { word: 'zzzzz', results: ['correct', 'correct', 'correct', 'correct', 'correct'] }
    ]
    const result = validWords(testWords, guesses)
    expect(result).toEqual([])
  })

  it('filters correctly with all correct letters', () => {
    const guesses: Guess[] = [
      { word: 'apple', results: ['correct', 'correct', 'correct', 'correct', 'correct'] }
    ]
    const result = validWords(testWords, guesses)
    expect(result).toEqual(['apple'])
  })
})
