<template>
  <v-dialog v-model="dialogVisible" max-width="400">
    <v-card>
      <v-card-title class="d-flex align-center">
        <v-icon class="mr-2">mdi-lightbulb</v-icon>
        Hint Generator
      </v-card-title>

      <v-card-text>
        <div v-if="hints.length === 0" class="text-center py-4">
          <p class="text-body-1 mb-4">
            Need a hint? Click below to reveal clues about the word.
          </p>
          <p class="text-caption text-medium-emphasis mb-4">
            Warning: Using hints may take away from the fun!
          </p>
          <v-btn color="warning" @click="revealNextHint" variant="elevated">
            <v-icon start>mdi-lightbulb-on</v-icon>
            Reveal Hint
          </v-btn>
        </div>

        <div v-else class="py-2">
          <!-- Show all revealed hints -->
          <v-alert
            v-for="(hint, index) in hints"
            :key="index"
            :type="hint.type"
            variant="tonal"
            class="mb-3"
            density="compact"
          >
            <div class="d-flex align-center">
              <span class="font-weight-bold mr-2">Hint {{ index + 1 }}:</span>
              {{ hint.text }}
            </div>
          </v-alert>

          <div class="text-center mt-4">
            <v-btn
              v-if="hints.length < 3"
              color="warning"
              variant="outlined"
              size="small"
              @click="revealNextHint"
            >
              <v-icon start>mdi-lightbulb-on</v-icon>
              Another Hint ({{ 3 - hints.length }} left)
            </v-btn>
            <p v-else class="text-caption text-medium-emphasis">
              No more hints available!
            </p>
          </div>
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
interface Hint {
  text: string
  type: 'info' | 'warning' | 'success'
}

const props = defineProps<{
  modelValue: boolean
  targetWord: string
}>()

const emit = defineEmits<{
  'update:modelValue': [value: boolean]
}>()

const dialogVisible = computed({
  get: () => props.modelValue,
  set: (value) => emit('update:modelValue', value)
})

const hints = ref<Hint[]>([])
const vowels = ['A', 'E', 'I', 'O', 'U']

const revealNextHint = () => {
  const word = props.targetWord.toUpperCase()
  const hintLevel = hints.value.length + 1

  let newHint: Hint

  switch (hintLevel) {
    case 1:
      // Hint 1: Count of vowels
      const vowelCount = word.split('').filter(c => vowels.includes(c)).length
      const uniqueVowels = [...new Set(word.split('').filter(c => vowels.includes(c)))]
      newHint = {
        text: `The word has ${vowelCount} vowel${vowelCount !== 1 ? 's' : ''} (${uniqueVowels.length} unique: ${uniqueVowels.join(', ') || 'none'})`,
        type: 'info'
      }
      break

    case 2:
      // Hint 2: First letter
      newHint = {
        text: `The word starts with the letter "${word[0]}"`,
        type: 'warning'
      }
      break

    case 3:
      // Hint 3: Last letter
      newHint = {
        text: `The word ends with the letter "${word[word.length - 1]}"`,
        type: 'success'
      }
      break

    default:
      return
  }

  hints.value.push(newHint)
}

// Reset hints when target word changes (new game)
watch(() => props.targetWord, () => {
  hints.value = []
})
</script>
