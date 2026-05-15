<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { Zap, Trophy, Flame, Code2, Star, Shield, Target } from 'lucide-vue-next'
import { API_BASE } from '../utils/api'

const router = useRouter()

interface UserProfile {
  id: number
  username: string
  email: string
  avatarUrl: string | null
  rankPoints: number
  status: string
  totalSubmissions: number
  solvedCount: number
  level: number
  xp: number
  nextLevelXp: number
  rank: string
  badges: string[]
}

const profile = ref<UserProfile | null>(null)
const loading = ref(true)
const error   = ref('')

// Static badge metadata
const BADGE_META: Record<string, { icon: any; color: string; desc: string }> = {
  'First Blood':     { icon: Zap,     color: '#10B981', desc: 'Giải bài đầu tiên' },
  'Streak Master':   { icon: Flame,   color: '#F97316', desc: '7 ngày liên tục' },
  'Array Conqueror': { icon: Shield,  color: '#3B82F6', desc: 'Hoàn thành Array' },
  'Arena Warrior':   { icon: Trophy,  color: '#FBBF24', desc: 'Thắng 10 trận 1v1' },
  'Speed Coder':     { icon: Target,  color: '#EF4444', desc: 'Giải trong dưới 5 phút' },
  'DP Master':       { icon: Star,    color: '#A78BFA', desc: 'Giải 5 bài DP nâng cao' },
}

const ALL_BADGES = Object.keys(BADGE_META)

// Heatmap — 84 days of pseudo-activity (will connect to real submissions later)
const heatmap = ref(
  Array.from({ length: 84 }, () => Math.floor(Math.random() * 5))
)

const heatColor = (v: number) => {
  if (v === 0) return 'rgba(255,255,255,0.04)'
  if (v === 1) return 'rgba(16,185,129,0.2)'
  if (v === 2) return 'rgba(16,185,129,0.4)'
  if (v === 3) return 'rgba(16,185,129,0.65)'
  return '#10B981'
}

const xpProgressPct = (p: UserProfile) =>
  p.nextLevelXp > 0 ? Math.min((p.xp / p.nextLevelXp) * 100, 100) : 0

const rankColor: Record<string, string> = {
  Bronze: '#CD7F32', Silver: '#C0C0C0', Gold: '#FBBF24',
  Platinum: '#60A5FA', Diamond: '#A78BFA',
}

onMounted(async () => {
  const token = localStorage.getItem('token')
  if (!token) {
    router.push('/login')
    return
  }

  try {
    const res = await fetch(`${API_BASE}/users/me`, {
      headers: { Authorization: `Bearer ${token}` },
    })

    if (res.status === 401) {
      localStorage.removeItem('token')
      router.push('/login')
      return
    }
    if (!res.ok) throw new Error(`HTTP ${res.status}`)

    profile.value = await res.json()
  } catch (e) {
    error.value = 'Không tải được thông tin profile. Kiểm tra kết nối.'
  } finally {
    loading.value = false
  }
})
</script>

<template>
  <div class="p-8 max-w-5xl mx-auto">

    <!-- Loading -->
    <div v-if="loading" class="flex items-center justify-center py-32">
      <div class="flex gap-1.5">
        <span v-for="i in 3" :key="i"
          class="w-2 h-2 rounded-full bg-emerald-500 animate-bounce"
          :style="`animation-delay:${(i-1)*0.15}s`"></span>
      </div>
    </div>

    <!-- Error -->
    <div v-else-if="error" class="flex items-center justify-center py-32 text-red-400 text-sm">
      ⚠️ {{ error }}
    </div>

    <template v-else-if="profile">
      <!-- Profile Header -->
      <div class="flex items-start gap-6 mb-10 p-6 rounded-2xl"
        style="background:#0A1628;border:1px solid rgba(16,185,129,0.12);">
        <!-- Avatar -->
        <div class="relative flex-shrink-0">
          <div class="w-20 h-20 rounded-2xl flex items-center justify-center text-2xl font-black overflow-hidden"
            style="background:linear-gradient(135deg,#10B981,#3B82F6);">
            <img v-if="profile.avatarUrl" :src="profile.avatarUrl" class="w-full h-full object-cover" />
            <span v-else>{{ profile.username[0]?.toUpperCase() }}</span>
          </div>
          <div class="absolute -bottom-1 -right-1 w-7 h-7 rounded-full border-2 flex items-center justify-center text-[10px] font-black bg-slate-900"
            style="border-color:#0A1628;background:#10B981;color:#000;">
            {{ profile.level }}
          </div>
        </div>

        <!-- Info -->
        <div class="flex-1 min-w-0">
          <h1 class="text-xl font-bold text-slate-100">{{ profile.username }}</h1>
          <p class="text-sm text-slate-500 mb-3">{{ profile.email }}</p>
          <div class="flex items-center gap-3 flex-wrap">
            <span class="flex items-center gap-1.5 text-xs font-bold px-2.5 py-1 rounded-full"
              :style="`color:${rankColor[profile.rank] ?? '#FBBF24'};background:${rankColor[profile.rank] ?? '#FBBF24'}20;border:1px solid ${rankColor[profile.rank] ?? '#FBBF24'}30`">
              <Trophy class="w-3 h-3" /> {{ profile.rank }} Rank
            </span>
            <span class="flex items-center gap-1.5 text-xs text-slate-500">
              <span class="w-1.5 h-1.5 rounded-full"
                :style="profile.status === 'Active' ? 'background:#10B981' : 'background:#EF4444'"></span>
              {{ profile.status }}
            </span>
          </div>
        </div>

        <!-- XP Progress -->
        <div class="hidden md:block w-52">
          <div class="flex justify-between text-xs mb-1.5">
            <span class="text-slate-500 font-bold">LV.{{ profile.level }}</span>
            <span class="font-mono-stat" style="color:#10B981;">
              {{ profile.xp.toLocaleString() }} / {{ profile.nextLevelXp.toLocaleString() }}
            </span>
          </div>
          <div class="h-2 rounded-full overflow-hidden" style="background:rgba(255,255,255,0.06);">
            <div class="h-full rounded-full transition-all duration-700"
              style="background:linear-gradient(90deg,#10B981,#34D399);"
              :style="`width:${xpProgressPct(profile)}%`"></div>
          </div>
          <p class="text-[10px] text-slate-600 mt-1">
            {{ (profile.nextLevelXp - profile.xp).toLocaleString() }} XP đến LV.{{ profile.level + 1 }}
          </p>
        </div>
      </div>

      <!-- Stats -->
      <div class="grid grid-cols-2 md:grid-cols-4 gap-4 mb-10">
        <div v-for="item in [
          { label:'Bài đã giải', value: profile.solvedCount, icon: Code2, color:'#10B981' },
          { label:'Rank Points', value: profile.rankPoints.toLocaleString(), icon: Zap, color:'#FBBF24' },
          { label:'Submissions', value: profile.totalSubmissions, icon: Target, color:'#3B82F6' },
          { label:'Huy chương', value: profile.badges.length, icon: Trophy, color:'#F97316' },
        ]" :key="item.label"
          class="rounded-2xl p-5"
          style="background:#0A1628;border:1px solid rgba(255,255,255,0.06);">
          <div class="p-2 rounded-lg w-fit mb-3" :style="`background:${item.color}15`">
            <component :is="item.icon" class="w-4 h-4" :style="`color:${item.color}`" />
          </div>
          <div class="font-mono-stat text-2xl font-bold text-slate-100 mb-0.5">{{ item.value }}</div>
          <div class="text-xs text-slate-500 uppercase tracking-wide font-medium">{{ item.label }}</div>
        </div>
      </div>

      <div class="grid md:grid-cols-2 gap-6">
        <!-- Activity Heatmap -->
        <div class="rounded-2xl p-5" style="background:#0A1628;border:1px solid rgba(255,255,255,0.06);">
          <h2 class="text-sm font-bold text-slate-200 mb-4">Hoạt động (84 ngày)</h2>
          <div class="grid gap-1" style="grid-template-columns:repeat(12,1fr);">
            <div v-for="week in 12" :key="week" class="grid gap-1" style="grid-template-rows:repeat(7,1fr);">
              <div v-for="day in 7" :key="day"
                class="w-full aspect-square rounded-sm"
                :style="`background:${heatColor(heatmap[(week-1)*7+(day-1)]??0)}`">
              </div>
            </div>
          </div>
          <div class="flex items-center gap-1.5 mt-3 text-[10px] text-slate-600">
            <span>Ít</span>
            <div v-for="v in [0,1,2,3,4]" :key="v"
              class="w-3 h-3 rounded-sm" :style="`background:${heatColor(v)}`"></div>
            <span>Nhiều</span>
          </div>
        </div>

        <!-- Badges -->
        <div class="rounded-2xl p-5" style="background:#0A1628;border:1px solid rgba(255,255,255,0.06);">
          <h2 class="text-sm font-bold text-slate-200 mb-4">
            Huy Chương ({{ profile.badges.length }}/{{ ALL_BADGES.length }})
          </h2>
          <div class="grid grid-cols-3 gap-3">
            <div v-for="name in ALL_BADGES" :key="name"
              class="flex flex-col items-center gap-1.5 p-3 rounded-xl transition-all duration-200"
              :style="{
                background: profile.badges.includes(name) ? `${BADGE_META[name].color}10` : 'rgba(255,255,255,0.02)',
                border: `1px solid ${profile.badges.includes(name) ? BADGE_META[name].color + '30' : 'rgba(255,255,255,0.06)'}`,
                opacity: profile.badges.includes(name) ? '1' : '0.35',
              }"
              :title="BADGE_META[name].desc">
              <component :is="BADGE_META[name].icon" class="w-5 h-5"
                :style="`color:${profile.badges.includes(name) ? BADGE_META[name].color : '#475569'}`" />
              <span class="text-[10px] font-bold text-center leading-tight"
                :style="`color:${profile.badges.includes(name) ? BADGE_META[name].color : '#475569'}`">
                {{ name }}
              </span>
            </div>
          </div>
        </div>
      </div>
    </template>
  </div>
</template>
