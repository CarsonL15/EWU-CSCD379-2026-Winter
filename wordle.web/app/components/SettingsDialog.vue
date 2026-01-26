<template>
  <v-dialog v-model="dialogVisible" max-width="400">
    <v-card>
      <v-card-title class="d-flex align-center">
        <v-icon class="mr-2">mdi-cog</v-icon>
        Settings
      </v-card-title>

      <v-card-text>
        <v-list>
          <v-list-item>
            <template v-slot:prepend>
              <v-icon>mdi-theme-light-dark</v-icon>
            </template>
            <v-list-item-title>Dark Mode</v-list-item-title>
            <template v-slot:append>
              <v-switch
                v-model="isDark"
                hide-details
                color="primary"
                @update:model-value="toggleTheme"
              />
            </template>
          </v-list-item>

          <v-list-item>
            <template v-slot:prepend>
              <v-icon>mdi-palette</v-icon>
            </template>
            <v-list-item-title>Color Scheme</v-list-item-title>
          </v-list-item>

          <v-list-item class="pl-8">
            <v-chip-group
              v-model="selectedScheme"
              mandatory
              @update:model-value="changeColorScheme"
            >
              <v-chip value="default" variant="outlined">
                Default
              </v-chip>
              <v-chip value="oceanic" variant="outlined" color="teal">
                Oceanic Breeze
              </v-chip>
              <v-chip value="sunset" variant="outlined" color="orange">
                Sunset Glow
              </v-chip>
            </v-chip-group>
          </v-list-item>
        </v-list>
      </v-card-text>

      <v-card-actions>
        <v-spacer />
        <v-btn color="primary" @click="dialogVisible = false">Close</v-btn>
      </v-card-actions>
    </v-card>
  </v-dialog>
</template>

<script setup lang="ts">
import { useTheme } from 'vuetify'

const props = defineProps<{
  modelValue: boolean
}>()

const emit = defineEmits<{
  'update:modelValue': [value: boolean]
}>()

const theme = useTheme()

const dialogVisible = computed({
  get: () => props.modelValue,
  set: (value) => emit('update:modelValue', value)
})

const isDark = ref(false)
const selectedScheme = ref('default')

const toggleTheme = () => {
  const baseName = selectedScheme.value === 'default' ? '' : selectedScheme.value
  if (isDark.value) {
    theme.global.name.value = baseName ? `${baseName}Dark` : 'dark'
  } else {
    theme.global.name.value = baseName ? `${baseName}Light` : 'light'
  }
}

const changeColorScheme = () => {
  const scheme = selectedScheme.value
  if (scheme === 'default') {
    theme.global.name.value = isDark.value ? 'dark' : 'light'
  } else if (scheme === 'oceanic') {
    theme.global.name.value = isDark.value ? 'oceanicDark' : 'oceanicLight'
  } else if (scheme === 'sunset') {
    theme.global.name.value = isDark.value ? 'sunsetDark' : 'sunsetLight'
  }
}

onMounted(() => {
  const currentTheme = theme.global.name.value
  isDark.value = currentTheme.includes('dark') || currentTheme.includes('Dark')
  if (currentTheme.includes('oceanic') || currentTheme.includes('Oceanic')) {
    selectedScheme.value = 'oceanic'
  } else if (currentTheme.includes('sunset') || currentTheme.includes('Sunset')) {
    selectedScheme.value = 'sunset'
  } else {
    selectedScheme.value = 'default'
  }
})
</script>
