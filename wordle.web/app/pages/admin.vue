<template>
  <v-container class="py-8">
    <v-row justify="center">
      <v-col cols="12" md="10" lg="8">
        <h1 class="text-h4 text-center mb-6">
          <v-icon size="large" class="mr-2">mdi-shield-crown</v-icon>
          Admin Dashboard
        </h1>

        <!-- User Management -->
        <v-card class="mb-6" elevation="2">
          <v-card-title>
            <v-icon class="mr-2">mdi-account-group</v-icon>
            User Management
          </v-card-title>
          <v-card-text>
            <v-table v-if="users.length">
              <thead>
                <tr>
                  <th>Email</th>
                  <th>Roles</th>
                  <th>Actions</th>
                </tr>
              </thead>
              <tbody>
                <tr v-for="user in users" :key="user.id">
                  <td>{{ user.email }}</td>
                  <td>
                    <v-chip
                      v-for="role in user.roles"
                      :key="role"
                      size="small"
                      color="primary"
                      class="mr-1"
                    >
                      {{ role }}
                    </v-chip>
                    <span v-if="!user.roles.length" class="text-medium-emphasis">No roles</span>
                  </td>
                  <td>
                    <v-btn
                      v-if="!user.roles.includes('Admin')"
                      size="small"
                      color="success"
                      variant="tonal"
                      class="mr-1"
                      @click="addRole(user.id, 'Admin')"
                    >
                      Make Admin
                    </v-btn>
                    <v-btn
                      v-else
                      size="small"
                      color="warning"
                      variant="tonal"
                      @click="removeRole(user.id, 'Admin')"
                    >
                      Remove Admin
                    </v-btn>
                  </td>
                </tr>
              </tbody>
            </v-table>
            <div v-else class="text-center py-4 text-medium-emphasis">
              No users found
            </div>
          </v-card-text>
        </v-card>

        <!-- Testimonial Moderation -->
        <v-card elevation="2">
          <v-card-title>
            <v-icon class="mr-2">mdi-message-star</v-icon>
            Testimonial Moderation
          </v-card-title>
          <v-card-text>
            <div v-if="testimonials.length">
              <v-card
                v-for="t in testimonials"
                :key="t.testimonialId"
                class="mb-3"
                variant="outlined"
              >
                <v-card-text class="d-flex align-center">
                  <div class="flex-grow-1">
                    <strong>{{ t.author }}</strong>
                    <v-rating
                      :model-value="t.rating"
                      color="amber"
                      density="compact"
                      size="small"
                      readonly
                      class="ml-2"
                    />
                    <p class="text-body-2 mt-1 mb-0">{{ t.content }}</p>
                  </div>
                  <v-btn
                    icon
                    color="error"
                    variant="tonal"
                    size="small"
                    @click="deleteTestimonial(t.testimonialId)"
                  >
                    <v-icon>mdi-delete</v-icon>
                  </v-btn>
                </v-card-text>
              </v-card>
            </div>
            <div v-else class="text-center py-4 text-medium-emphasis">
              No testimonials to moderate
            </div>
          </v-card-text>
        </v-card>
      </v-col>
    </v-row>
  </v-container>
</template>

<script setup lang="ts">
const { isAdmin } = useAuth()
const { apiFetch } = useApiFetch()
const router = useRouter()

interface UserData {
  id: string
  email: string
  name: string
  roles: string[]
}

interface TestimonialData {
  testimonialId: number
  author: string
  content: string
  rating: number
  createdAt: string
}

const users = ref<UserData[]>([])
const testimonials = ref<TestimonialData[]>([])

// Redirect non-admins
watch(isAdmin, (val) => {
  if (!val) router.push('/')
}, { immediate: true })

const fetchUsers = async () => {
  try {
    users.value = await apiFetch<UserData[]>('/api/user/list')
  } catch {
    // silently fail
  }
}

const fetchTestimonials = async () => {
  try {
    testimonials.value = await apiFetch<TestimonialData[]>('/api/testimonial')
  } catch {
    // silently fail
  }
}

const addRole = async (userId: string, role: string) => {
  try {
    await apiFetch(`/api/user/${userId}/AddRole/${role}`, { method: 'POST' })
    await fetchUsers()
  } catch {
    // silently fail
  }
}

const removeRole = async (userId: string, role: string) => {
  try {
    await apiFetch(`/api/user/${userId}/RemoveRole/${role}`, { method: 'POST' })
    await fetchUsers()
  } catch {
    // silently fail
  }
}

const deleteTestimonial = async (id: number) => {
  try {
    await apiFetch(`/api/testimonial/${id}`, { method: 'DELETE' })
    await fetchTestimonials()
  } catch {
    // silently fail
  }
}

onMounted(() => {
  if (isAdmin.value) {
    fetchUsers()
    fetchTestimonials()
  }
})
</script>
