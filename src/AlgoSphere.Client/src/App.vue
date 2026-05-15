<script setup lang="ts">
import { computed } from 'vue'
import { useRoute } from 'vue-router'
import Sidebar from './components/Sidebar.vue'

const route = useRoute()

// Routes that should NOT show the sidebar (public/fullscreen layouts)
const PUBLIC_ROUTES = ['landing', 'login', 'register', 'leaderboard-public']

const showSidebar = computed(() =>
  !PUBLIC_ROUTES.includes(route.name as string) &&
  !route.meta?.public &&
  route.name !== 'workspace' // workspace has its own full-screen layout
)

</script>

<template>
  <div class="flex min-h-screen bg-brand-slate text-white antialiased">
    <!-- Sidebar: only for authenticated app pages -->
    <Sidebar v-if="showSidebar" />

    <!-- Main content area -->
    <main
      :class="[
        'flex-1 min-h-screen overflow-y-auto',
        showSidebar ? 'ml-64' : 'ml-0'
      ]"
    >
      <router-view />
    </main>
  </div>
</template>

<style>
main::-webkit-scrollbar { width: 6px; }
main::-webkit-scrollbar-thumb { background: #1e293b; border-radius: 10px; }
</style>
