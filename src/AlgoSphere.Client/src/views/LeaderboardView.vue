<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { ArrowUp, Minus, Crown } from 'lucide-vue-next'
import { API_BASE } from '../utils/api'

const rankings = ref<any[]>([])
const loading = ref(true)

onMounted(async () => {
  try {
    const res = await fetch(`${API_BASE}/leaderboard/top?count=10`)
    rankings.value = await res.json()
  } catch (err) {
    console.error(err)
  } finally {
    loading.value = false
  }
})

const getRankConfig = (rank: number) => {
  if (rank === 1) return { color: '#FBBF24', bg: 'rgba(251,191,36,0.1)', border: 'rgba(251,191,36,0.25)', glow: '0 0 20px rgba(251,191,36,0.15)' }
  if (rank === 2) return { color: '#94A3B8', bg: 'rgba(148,163,184,0.08)', border: 'rgba(148,163,184,0.2)', glow: 'none' }
  if (rank === 3) return { color: '#F97316', bg: 'rgba(249,115,22,0.08)', border: 'rgba(249,115,22,0.2)', glow: 'none' }
  return { color: '#475569', bg: 'transparent', border: 'rgba(255,255,255,0.04)', glow: 'none' }
}
</script>

<template>
  <div class="p-8 max-w-4xl mx-auto">

    <!-- Header -->
    <header class="mb-10 text-center">
      <div class="inline-flex items-center justify-center w-16 h-16 rounded-2xl mb-5 glow-emerald"
        style="background: linear-gradient(135deg, rgba(251,191,36,0.2), rgba(249,115,22,0.2)); border: 1px solid rgba(251,191,36,0.3);">
        <Crown class="w-8 h-8 text-yellow-400" />
      </div>
      <p class="text-xs font-bold tracking-[0.25em] uppercase mb-2" style="color: #10B981;">GLOBAL RANKINGS</p>
      <h1 class="text-4xl font-bold text-slate-100 mb-3">
        Bảng Xếp Hạng
      </h1>
      <p class="text-slate-500 text-sm">Những bậc thầy thuật toán dẫn đầu hệ sinh thái AlgoSphere.</p>
    </header>

    <!-- TOP 3 Podium -->
    <div v-if="!loading && rankings.length >= 3" class="grid grid-cols-3 gap-4 mb-8 items-end">
      <!-- Rank 2 -->
      <div class="text-center">
        <div class="inline-flex w-14 h-14 rounded-2xl items-center justify-center font-bold text-lg mb-3"
          style="background: rgba(148,163,184,0.15); border: 1px solid rgba(148,163,184,0.3); color: #94A3B8;">
          {{ rankings[1]?.username?.charAt(0) }}
        </div>
        <div class="text-sm font-bold text-slate-300 mb-1 truncate">{{ rankings[1]?.username }}</div>
        <div class="font-mono-stat text-lg font-bold text-slate-400">{{ rankings[1]?.score?.toLocaleString() }}</div>
        <div class="mt-2 py-3 rounded-xl text-xs font-bold" style="background: rgba(148,163,184,0.08); color: #94A3B8; border: 1px solid rgba(148,163,184,0.2);">
          🥈 #2
        </div>
      </div>
      <!-- Rank 1 -->
      <div class="text-center -mt-4">
        <div class="relative inline-block mb-3">
          <div class="w-16 h-16 rounded-2xl flex items-center justify-center font-bold text-xl glow-emerald"
            style="background: linear-gradient(135deg, rgba(251,191,36,0.3), rgba(249,115,22,0.2)); border: 2px solid rgba(251,191,36,0.5); color: #FBBF24;">
            {{ rankings[0]?.username?.charAt(0) }}
          </div>
          <div class="absolute -top-3 left-1/2 -translate-x-1/2">
            <Crown class="w-5 h-5 text-yellow-400" style="filter: drop-shadow(0 0 6px rgba(251,191,36,0.8));" />
          </div>
        </div>
        <div class="text-sm font-bold text-yellow-300 mb-1 truncate">{{ rankings[0]?.username }}</div>
        <div class="font-mono-stat text-2xl font-bold text-yellow-400">{{ rankings[0]?.score?.toLocaleString() }}</div>
        <div class="mt-2 py-3 rounded-xl text-xs font-bold" style="background: rgba(251,191,36,0.1); color: #FBBF24; border: 1px solid rgba(251,191,36,0.3);">
          👑 CHAMPION
        </div>
      </div>
      <!-- Rank 3 -->
      <div class="text-center">
        <div class="inline-flex w-14 h-14 rounded-2xl items-center justify-center font-bold text-lg mb-3"
          style="background: rgba(249,115,22,0.1); border: 1px solid rgba(249,115,22,0.3); color: #F97316;">
          {{ rankings[2]?.username?.charAt(0) }}
        </div>
        <div class="text-sm font-bold text-slate-300 mb-1 truncate">{{ rankings[2]?.username }}</div>
        <div class="font-mono-stat text-lg font-bold text-orange-400">{{ rankings[2]?.score?.toLocaleString() }}</div>
        <div class="mt-2 py-3 rounded-xl text-xs font-bold" style="background: rgba(249,115,22,0.08); color: #F97316; border: 1px solid rgba(249,115,22,0.2);">
          🥉 #3
        </div>
      </div>
    </div>

    <!-- Full table -->
    <div class="rounded-2xl overflow-hidden" style="background: #080F1C; border: 1px solid rgba(255,255,255,0.06);">

      <!-- Table header -->
      <div class="grid grid-cols-12 px-6 py-3 text-[10px] font-bold uppercase tracking-[0.15em]" style="background: rgba(255,255,255,0.03); color: #475569; border-bottom: 1px solid rgba(255,255,255,0.05);">
        <div class="col-span-1">#</div>
        <div class="col-span-6">Player</div>
        <div class="col-span-3 text-right">Score</div>
        <div class="col-span-2 text-center">Trend</div>
      </div>

      <!-- Rows -->
      <div v-if="loading" class="p-16 text-center">
        <div class="inline-block w-8 h-8 border-2 border-t-emerald-400 rounded-full animate-spin" style="border-color: rgba(16,185,129,0.2); border-top-color: #10B981;"></div>
      </div>

      <div v-else>
        <div v-for="user in rankings" :key="user.rank"
          class="grid grid-cols-12 items-center px-6 py-4 group cursor-pointer transition-all duration-200"
          :style="`background: ${getRankConfig(user.rank).bg}; border-bottom: 1px solid rgba(255,255,255,0.03); box-shadow: ${getRankConfig(user.rank).glow};`"
          @mouseenter="($event.currentTarget as HTMLElement).style.background = 'rgba(16,185,129,0.04)'"
          @mouseleave="($event.currentTarget as HTMLElement).style.background = getRankConfig(user.rank).bg">

          <!-- Rank -->
          <div class="col-span-1">
            <div v-if="user.rank <= 3" class="w-7 h-7 rounded-lg flex items-center justify-center text-sm"
              :style="`background: ${getRankConfig(user.rank).bg}; border: 1px solid ${getRankConfig(user.rank).border};`">
              <span v-if="user.rank === 1">👑</span>
              <span v-else-if="user.rank === 2">🥈</span>
              <span v-else>🥉</span>
            </div>
            <span v-else class="font-mono-stat text-sm font-bold text-slate-500">{{ user.rank }}</span>
          </div>

          <!-- Player -->
          <div class="col-span-6 flex items-center gap-3">
            <div class="w-9 h-9 rounded-xl flex items-center justify-center font-bold text-sm flex-shrink-0 transition-all duration-200"
              :style="`background: linear-gradient(135deg, rgba(16,185,129,0.2), rgba(59,130,246,0.2)); border: 1px solid ${getRankConfig(user.rank).border}; color: ${getRankConfig(user.rank).color};`">
              {{ user.username?.charAt(0) }}
            </div>
            <div>
              <div class="font-bold text-sm text-slate-200 group-hover:text-emerald-400 transition-colors duration-200">{{ user.username }}</div>
              <div class="text-[10px] text-slate-600 font-medium">Lv.{{ Math.floor(user.score / 200) }} · Pro</div>
            </div>
          </div>

          <!-- Score -->
          <div class="col-span-3 text-right">
            <span class="font-mono-stat font-bold text-sm" :style="`color: ${getRankConfig(user.rank).color}`">
              {{ user.score?.toLocaleString() }}
            </span>
            <div class="text-[10px] text-slate-600">PTS</div>
          </div>

          <!-- Trend -->
          <div class="col-span-2 flex justify-center">
            <div class="flex items-center gap-1 px-2 py-0.5 rounded-full text-[10px] font-bold"
              :style="user.rank === 1 ? 'background: rgba(16,185,129,0.1); color: #10B981;' : 'background: rgba(255,255,255,0.03); color: #475569;'">
              <ArrowUp v-if="user.rank === 1" class="w-3 h-3" />
              <Minus v-else class="w-3 h-3" />
              <span>{{ user.rank === 1 ? '+12' : '0' }}</span>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>
