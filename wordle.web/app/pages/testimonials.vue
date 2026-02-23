<template>
  <v-container class="py-8">
    <v-row justify="center">
      <v-col cols="12" md="8" lg="6">
        <h1 class="text-h4 text-center mb-6">
          <v-icon size="large" class="mr-2">mdi-message-star</v-icon>
          Testimonials
        </h1>

        <!-- Submit Form -->
        <v-card class="mb-6" elevation="2">
          <v-card-title>Leave a Review</v-card-title>
          <v-card-text>
            <v-text-field
              v-model="newAuthor"
              label="Your Name"
              variant="outlined"
              density="compact"
              class="mb-3"
            />
            <v-textarea
              v-model="newContent"
              label="Your Review"
              variant="outlined"
              rows="3"
              class="mb-3"
            />
            <div class="d-flex align-center mb-2">
              <span class="text-body-2 mr-3">Rating:</span>
              <v-rating
                v-model="newRating"
                color="amber"
                hover
                density="compact"
              />
            </div>
          </v-card-text>
          <v-card-actions>
            <v-spacer />
            <v-btn
              color="primary"
              variant="elevated"
              :loading="submitting"
              :disabled="!newContent.trim()"
              @click="submitTestimonial"
            >
              Submit Review
            </v-btn>
          </v-card-actions>
        </v-card>

        <!-- Error/Success Messages -->
        <v-alert v-if="successMessage" type="success" variant="tonal" class="mb-4" closable @click:close="successMessage = ''">
          {{ successMessage }}
        </v-alert>
        <v-alert v-if="errorMessage" type="error" variant="tonal" class="mb-4" closable @click:close="errorMessage = ''">
          {{ errorMessage }}
        </v-alert>

        <!-- Loading -->
        <div v-if="loading" class="text-center py-8">
          <v-progress-circular indeterminate color="primary" />
        </div>

        <!-- Testimonial Cards -->
        <div v-else-if="testimonials.length">
          <v-card
            v-for="t in testimonials"
            :key="t.testimonialId"
            class="mb-3 testimonial-card"
            variant="outlined"
          >
            <v-card-text>
              <div class="d-flex align-center mb-2">
                <v-icon size="small" class="mr-2">mdi-account-circle</v-icon>
                <strong>{{ t.author }}</strong>
                <v-spacer />
                <v-rating
                  :model-value="t.rating"
                  color="amber"
                  density="compact"
                  size="small"
                  readonly
                />
              </div>
              <p class="text-body-1 mb-2">{{ t.content }}</p>
              <p class="text-caption text-medium-emphasis">
                {{ new Date(t.createdAt).toLocaleDateString() }}
              </p>
            </v-card-text>
          </v-card>
        </div>

        <!-- Empty State -->
        <v-card v-else variant="tonal" class="text-center pa-8">
          <v-icon size="48" color="grey" class="mb-3">mdi-message-outline</v-icon>
          <p class="text-body-1 text-medium-emphasis">No testimonials yet. Be the first!</p>
        </v-card>
      </v-col>
    </v-row>
  </v-container>
</template>

<script setup lang="ts">
import axios from 'axios'

const API_BASE = import.meta.env.VITE_API_URL || 'https://gridseeker-api-clayden-fshtdtbzfcb5crgk.eastus-01.azurewebsites.net'

interface TestimonialData {
  testimonialId: number
  author: string
  content: string
  rating: number
  createdAt: string
}

const testimonials = ref<TestimonialData[]>([])
const loading = ref(true)
const submitting = ref(false)
const successMessage = ref('')
const errorMessage = ref('')

const newAuthor = ref('')
const newContent = ref('')
const newRating = ref(5)

const fetchTestimonials = async () => {
  loading.value = true
  try {
    const response = await axios.get(`${API_BASE}/api/testimonial`)
    testimonials.value = response.data
  } catch {
    errorMessage.value = 'Could not load testimonials'
  } finally {
    loading.value = false
  }
}

const submitTestimonial = async () => {
  if (!newContent.value.trim()) return

  submitting.value = true
  errorMessage.value = ''
  try {
    await axios.post(`${API_BASE}/api/testimonial`, {
      author: newAuthor.value || 'Anonymous',
      content: newContent.value,
      rating: newRating.value
    })
    successMessage.value = 'Thanks for your review!'
    newAuthor.value = ''
    newContent.value = ''
    newRating.value = 5
    await fetchTestimonials()
  } catch {
    errorMessage.value = 'Failed to submit review'
  } finally {
    submitting.value = false
  }
}

onMounted(() => {
  fetchTestimonials()
})
</script>

<style scoped>
.testimonial-card {
  transition: transform 0.2s ease, box-shadow 0.2s ease;
}

.testimonial-card:hover {
  transform: translateY(-2px);
}
</style>
