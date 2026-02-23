<template>
  <v-dialog v-model="dialogVisible" max-width="500">
    <v-card>
      <v-card-title class="d-flex align-center">
        <v-icon class="mr-2" color="success">mdi-trophy</v-icon>
        You Won!
      </v-card-title>

      <v-card-text>
        <div class="text-center mb-4">
          <v-chip color="success" size="x-large" class="text-h5 font-weight-bold px-6">
            {{ word }}
          </v-chip>
          <p class="text-body-2 mt-2 text-medium-emphasis">
            Solved in {{ attempts }} attempt{{ attempts !== 1 ? 's' : '' }}
          </p>
        </div>

        <v-divider class="mb-4" />

        <div v-if="loading" class="text-center py-4">
          <v-progress-circular indeterminate color="primary" />
          <p class="text-body-2 mt-2">Loading definition...</p>
        </div>

        <div v-else-if="definition">
          <div class="text-subtitle-1 font-weight-bold mb-2">
            Definition
          </div>
          <div v-if="phonetic" class="text-caption text-medium-emphasis mb-2">
            {{ phonetic }}
          </div>
          <div v-for="(meaning, index) in definitions" :key="index" class="mb-3">
            <div class="text-body-2 font-weight-medium text-primary">
              {{ meaning.partOfSpeech }}
            </div>
            <ul class="text-body-2 pl-4">
              <li v-for="(def, defIndex) in meaning.definitions.slice(0, 2)" :key="defIndex">
                {{ def.definition }}
                <span v-if="def.example" class="text-caption text-medium-emphasis">
                  — "{{ def.example }}"
                </span>
              </li>
            </ul>
          </div>
        </div>

        <div v-else class="text-center py-4">
          <v-icon size="48" color="grey">mdi-book-off</v-icon>
          <p class="text-body-2 mt-2 text-medium-emphasis">
            Definition not available
          </p>
        </div>
      </v-card-text>

      <v-card-actions>
        <v-spacer />
        <v-btn color="primary" variant="elevated" @click="dialogVisible = false">
          Continue
        </v-btn>
      </v-card-actions>
    </v-card>
  </v-dialog>
</template>

<script setup lang="ts">
interface DefinitionMeaning {
  partOfSpeech: string
  definitions: {
    definition: string
    example?: string
  }[]
}

const props = defineProps<{
  modelValue: boolean
  word: string
  attempts: number
}>()

const emit = defineEmits<{
  'update:modelValue': [value: boolean]
}>()

const dialogVisible = computed({
  get: () => props.modelValue,
  set: (value) => emit('update:modelValue', value)
})

const loading = ref(false)
const definition = ref<any>(null)
const phonetic = ref('')
const definitions = ref<DefinitionMeaning[]>([])

const fetchDefinition = async (word: string) => {
  loading.value = true
  definition.value = null
  phonetic.value = ''
  definitions.value = []

  try {
    const response = await fetch(`https://api.dictionaryapi.dev/api/v2/entries/en/${word.toLowerCase()}`)
    if (response.ok) {
      const data = await response.json()
      if (data && data.length > 0) {
        definition.value = data[0]
        phonetic.value = data[0].phonetic || data[0].phonetics?.find((p: any) => p.text)?.text || ''
        definitions.value = data[0].meanings || []
      }
    }
  } catch (error) {
    console.error('Error fetching definition:', error)
  } finally {
    loading.value = false
  }
}

watch(dialogVisible, (newVal) => {
  if (newVal && props.word) {
    fetchDefinition(props.word)
  }
})
</script>
