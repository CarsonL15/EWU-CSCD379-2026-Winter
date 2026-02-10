<template>
  <div>
    <v-chip
      color="primary"
      variant="elevated"
      @click="showDialog = true"
    >
      <v-icon start>mdi-format-list-bulleted</v-icon>
      {{ filteredWords.length }} possible words
    </v-chip>

    <v-dialog v-model="showDialog" max-width="500" scrollable>
      <v-card>
        <v-card-title class="d-flex align-center">
          <v-icon class="mr-2">mdi-book-alphabet</v-icon>
          Possible Words ({{ filteredWords.length }})
        </v-card-title>

        <v-divider />

        <v-card-text style="height: 400px;">
          <v-list density="compact">
            <v-hover v-for="word in filteredWords" :key="word" v-slot="{ isHovering, props }">
              <v-list-item
                v-bind="props"
                :color="isHovering ? 'primary' : undefined"
                :variant="isHovering ? 'tonal' : undefined"
                @click="selectWord(word)"
              >
                <v-list-item-title class="text-uppercase font-weight-medium">
                  {{ word }}
                </v-list-item-title>
              </v-list-item>
            </v-hover>
          </v-list>
        </v-card-text>

        <v-divider />

        <v-card-actions>
          <v-spacer />
          <v-btn color="primary" @click="showDialog = false">Close</v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>
  </div>
</template>

<script setup lang="ts">
const props = defineProps<{
  filteredWords: string[]
}>()

const emit = defineEmits<{
  selectWord: [word: string]
}>()

const showDialog = ref(false)

const selectWord = (word: string) => {
  emit('selectWord', word)
  showDialog.value = false
}
</script>

