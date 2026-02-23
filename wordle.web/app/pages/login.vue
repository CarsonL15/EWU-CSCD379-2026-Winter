<template>
  <v-container class="py-8">
    <v-row justify="center">
      <v-col cols="12" sm="8" md="5" lg="4">
        <v-card elevation="2">
          <v-card-title class="text-h5 text-center py-4">
            {{ isRegister ? 'Create Account' : 'Sign In' }}
          </v-card-title>

          <v-card-text>
            <v-alert v-if="errorMessage" type="error" variant="tonal" class="mb-4" closable @click:close="errorMessage = ''">
              {{ errorMessage }}
            </v-alert>

            <v-text-field
              v-model="emailInput"
              label="Email"
              type="email"
              variant="outlined"
              density="compact"
              class="mb-3"
              @keyup.enter="submit"
            />

            <v-text-field
              v-model="password"
              label="Password"
              type="password"
              variant="outlined"
              density="compact"
              class="mb-3"
              @keyup.enter="submit"
            />
          </v-card-text>

          <v-card-actions class="flex-column px-4 pb-4">
            <v-btn
              color="primary"
              variant="elevated"
              block
              :loading="loading"
              @click="submit"
            >
              {{ isRegister ? 'Register' : 'Login' }}
            </v-btn>

            <v-btn
              variant="text"
              class="mt-2"
              @click="isRegister = !isRegister"
            >
              {{ isRegister ? 'Already have an account? Sign in' : "Don't have an account? Register" }}
            </v-btn>
          </v-card-actions>
        </v-card>
      </v-col>
    </v-row>
  </v-container>
</template>

<script setup lang="ts">
definePageMeta({ public: true })

const { login, register, isAuthenticated } = useAuth()
const router = useRouter()

const emailInput = ref('')
const password = ref('')
const isRegister = ref(false)
const loading = ref(false)
const errorMessage = ref('')

// Redirect if already authenticated
watch(isAuthenticated, (val) => {
  if (val) router.push('/')
}, { immediate: true })

const submit = async () => {
  if (!emailInput.value || !password.value) {
    errorMessage.value = 'Email and password are required'
    return
  }

  loading.value = true
  errorMessage.value = ''

  try {
    if (isRegister.value) {
      await register(emailInput.value, password.value)
    } else {
      await login(emailInput.value, password.value)
    }
    router.push('/')
  } catch (e: any) {
    errorMessage.value = e.message || 'An error occurred'
  } finally {
    loading.value = false
  }
}
</script>
