<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import {
  LayoutDashboard, Code2, Trophy, Swords,
  MessageSquare, Target, ShieldCheck, Settings,
  LogOut, Zap, ChevronRight, GitBranch, User
} from 'lucide-vue-next'
import { API_BASE } from '../utils/api'

const route = useRoute()
const router = useRouter()
const isActive = (path: string) => route.path === path || route.path.startsWith(path + '/')

const navItems = [
  { to: '/dashboard', label: 'Dashboard', icon: LayoutDashboard, exact: true },
  { to: '/skill-tree', label: 'Skill Tree', icon: GitBranch },
  { to: '/profile', label: 'Hồ Sơ', icon: User },
  { to: '/topic/1', label: 'Bài Tập', icon: Code2 },
  { to: '/arena', label: 'Code Arena', icon: Swords, badge: 'LIVE' },
  { to: '/tournaments', label: 'Giải Đấu', icon: Target },
  { to: '/leaderboard', label: 'Xếp Hạng', icon: Trophy },
  { to: '/forum', label: 'Diễn Đàn', icon: MessageSquare },
  { to: '/admin', label: 'Admin CMS', icon: ShieldCheck },
]

const username = ref('Loading...')
const avatarChar = ref('?')
const xp = ref(0)
const level = ref(1)
const xpProgress = ref(0)
const rank = ref('Bronze')

onMounted(async () => {
  const token = localStorage.getItem('token')
  if (!token) return

  try {
    const res = await fetch(`${API_BASE}/users/me`, {
      headers: { Authorization: `Bearer ${token}` }
    })
    if (res.ok) {
      const data = await res.json()
      username.value = data.username
      avatarChar.value = data.username[0]?.toUpperCase()
      xp.value = data.xp
      level.value = data.level
      rank.value = data.rank
      xpProgress.value = data.nextLevelXp > 0 ? (data.xp / data.nextLevelXp) * 100 : 0
    }
  } catch (e) {
    console.error('Failed to load user data for sidebar')
  }
})

const handleLogout = () => {
  localStorage.removeItem('token')
  router.push('/login')
}
</script>

<template>
  <aside class="fixed left-0 top-0 h-screen w-64 flex flex-col z-50" style="background: #060D16; border-right: 1px solid rgba(16,185,129,0.12);">

    <!-- Logo -->
    <div class="px-6 pt-7 pb-6">
      <div class="flex items-center gap-3 mb-1">
        <div class="w-8 h-8 rounded-lg flex items-center justify-center glow-emerald-sm" style="background: linear-gradient(135deg, #10B981, #059669);">
          <Zap class="w-4 h-4 text-white fill-current" />
        </div>
        <div>
          <h1 class="text-lg font-bold tracking-tight leading-none" style="color: #10B981; text-shadow: 0 0 15px rgba(16,185,129,0.5);">ALGOSPHERE</h1>
          <span class="text-[9px] font-bold tracking-[0.2em] uppercase" style="color: #475569;">Enterprise Edition</span>
        </div>
      </div>
    </div>

    <!-- User Card -->
    <div @click="router.push('/profile')" class="mx-3 mb-5 p-4 rounded-xl relative overflow-hidden shine cursor-pointer" style="background: rgba(16,185,129,0.06); border: 1px solid rgba(16,185,129,0.15);">
      <div class="flex items-center gap-3 mb-3">
        <div class="relative">
          <div class="w-10 h-10 rounded-xl flex items-center justify-center font-bold text-sm" style="background: linear-gradient(135deg, #10B981 0%, #3B82F6 100%);">
            {{ avatarChar }}
          </div>
          <div class="absolute -bottom-0.5 -right-0.5 w-3 h-3 rounded-full bg-emerald-400 border-2" style="border-color: #060D16;"></div>
        </div>
        <div class="min-w-0">
          <div class="font-semibold text-sm text-slate-100 truncate">{{ username }}</div>
          <div class="text-[10px] font-bold uppercase tracking-wider" style="color: #10B981;">LV.{{ level }} · {{ rank }} Rank</div>
        </div>
      </div>
      <!-- XP Bar -->
      <div class="space-y-1">
        <div class="flex justify-between items-center">
          <span class="text-[10px] text-slate-500 font-bold uppercase tracking-wide">XP Progress</span>
          <span class="text-[10px] font-mono-stat" style="color: #10B981;">{{ xp.toLocaleString() }}</span>
        </div>
        <div class="h-1.5 rounded-full overflow-hidden" style="background: rgba(255,255,255,0.06);">
          <div class="h-full rounded-full transition-all duration-700 glow-emerald-sm" :style="`width: ${xpProgress}%; background: linear-gradient(90deg, #10B981, #34D399);`"></div>
        </div>
      </div>
    </div>

    <!-- Navigation -->
    <nav class="flex-1 px-3 space-y-0.5 overflow-y-auto">
      <template v-for="item in navItems" :key="item.to">
        <router-link
          :to="item.to"
          class="group flex items-center gap-3 px-3 py-2.5 rounded-xl transition-all duration-200 relative cursor-pointer"
          :class="isActive(item.to) && (item.exact ? route.path === item.to : true)
            ? 'nav-active'
            : 'text-slate-400 hover:text-slate-100 hover:bg-white/5'"
        >
          <component
            :is="item.icon"
            class="w-4 h-4 flex-shrink-0 transition-transform duration-200 group-hover:scale-110"
            :class="isActive(item.to) ? 'text-emerald-400' : ''"
          />
          <span class="flex-1 text-sm font-medium">{{ item.label }}</span>

          <!-- Live badge -->
          <span v-if="item.badge"
            class="text-[9px] font-bold px-1.5 py-0.5 rounded font-mono-stat"
            style="background: rgba(239,68,68,0.15); color: #F87171; border: 1px solid rgba(239,68,68,0.3);">
            {{ item.badge }}
          </span>

          <!-- Active arrow -->
          <ChevronRight v-if="isActive(item.to)"
            class="w-3 h-3 text-emerald-400 opacity-60"
          />
        </router-link>
      </template>
    </nav>

    <!-- Bottom section -->
    <div class="px-3 pb-6 pt-4 space-y-0.5" style="border-top: 1px solid rgba(255,255,255,0.05);">
      <router-link to="/settings"
        class="flex items-center gap-3 px-3 py-2.5 rounded-xl text-slate-500 hover:text-slate-200 hover:bg-white/5 transition-all text-sm font-medium cursor-pointer">
        <Settings class="w-4 h-4" /> Cài đặt
      </router-link>
      <button @click="handleLogout" class="w-full flex items-center gap-3 px-3 py-2.5 rounded-xl transition-all text-sm font-medium cursor-pointer"
        style="color: #F87171;"
        onmouseenter="this.style.background='rgba(239,68,68,0.08)'"
        onmouseleave="this.style.background='transparent'">
        <LogOut class="w-4 h-4" /> Đăng xuất
      </button>
    </div>
  </aside>
</template>
