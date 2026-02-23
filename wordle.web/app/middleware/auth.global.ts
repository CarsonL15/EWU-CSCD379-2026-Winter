export default defineNuxtRouteMiddleware((to) => {
  const { isAuthenticated } = useAuth()

  const isPublic = to.meta.public === true

  if (!isPublic && !isAuthenticated.value) {
    return navigateTo('/login')
  }
})
