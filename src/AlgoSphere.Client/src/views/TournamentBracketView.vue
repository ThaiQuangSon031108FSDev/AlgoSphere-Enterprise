<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRoute } from 'vue-router'
import { API_BASE, HUB_BASE } from '../utils/api'
import { ChevronLeft, Trophy, Swords, Clock } from 'lucide-vue-next'
import * as signalR from '@microsoft/signalr'

const route   = useRoute()
const matches = ref<any[]>([])
const tournament = ref<any>(null)
const loading = ref(true)

const fetchData = async () => {
  const id = route.params.id
  try {
    const [tRes, mRes] = await Promise.all([
      fetch(`${API_BASE}/tournaments`),
      fetch(`${API_BASE}/tournaments/${id}/brackets`),
    ])
    if (tRes.ok) {
      const list: any[] = await tRes.json()
      tournament.value = list.find(t => String(t.id) === String(id))
    }
    if (mRes.ok) matches.value = await mRes.json()
  } catch (err) {
    console.error(err)
  } finally {
    loading.value = false
  }
}

let connection: signalR.HubConnection | null = null

onMounted(async () => {
  await fetchData()

  // Setup SignalR for live updates
  const token = localStorage.getItem('token')
  connection = new signalR.HubConnectionBuilder()
    .withUrl(`${HUB_BASE}/ws/arena`, { accessTokenFactory: () => token! })
    .withAutomaticReconnect()
    .build()

  connection.on('BracketUpdated', () => {
    fetchData()
  })

  try {
    await connection.start()
    await connection.invoke('JoinTournamentGroup', Number(route.params.id))
  } catch (err) {
    console.error('SignalR failed', err)
  }
})

import { onBeforeUnmount } from 'vue'
onBeforeUnmount(() => {
  connection?.stop()
})

// Group matches by BracketPosition prefix: "R1-M1" → round 1
const rounds = computed(() => {
  const map = new Map<string, any[]>()
  for (const m of matches.value) {
    const round = (m.bracketPosition ?? 'Round 1').split('-')[0] || 'Round 1'
    if (!map.has(round)) map.set(round, [])
    map.get(round)!.push(m)
  }
  return [...map.entries()].sort((a, b) => a[0].localeCompare(b[0]))
})

// Placeholder for empty brackets logic if needed later
onMounted(async () => {
  // no-op
})

const roundLabel = (key: string): string => {
  const labels: Record<string, string> = {
    R1: 'ROUND 1', R2: 'ROUND 2', R3: 'SEMI FINALS', R4: 'GRAND FINALS',
    'Round 1': 'ROUND 1', 'Round 2': 'ROUND 2',
    'Semi': 'SEMI FINALS', 'Final': 'GRAND FINALS',
  }
  return labels[key] ?? key.toUpperCase()
}

const isWinner = (m: any, player: 'player1' | 'player2') => {
  if (!m.winner || m.winner === 'TBD') return false
  return m.winner === m[player]
}
</script>

<template>
  <div class="p-8 min-h-screen" style="background:#020408;">

    <!-- Header -->
    <header class="flex items-center justify-between mb-12">
      <div class="flex items-center gap-4">
        <router-link to="/tournaments" class="p-2 rounded-lg text-slate-500 hover:text-emerald-400 hover:bg-white/5 transition-all">
          <ChevronLeft class="w-5 h-5" />
        </router-link>
        <div>
          <p class="text-xs font-bold tracking-widest uppercase mb-0.5" style="color:#10B981;">E-SPORTS DIVISION</p>
          <h1 class="text-3xl font-black uppercase italic text-slate-100">
            {{ tournament?.title ?? 'Sơ Đồ Giải Đấu' }}
          </h1>
        </div>
      </div>

      <div v-if="tournament" class="flex items-center gap-2 px-4 py-2 rounded-full text-xs font-bold"
        :style="tournament.status === 'Active'
          ? 'background:rgba(16,185,129,0.1);color:#10B981;border:1px solid rgba(16,185,129,0.2);'
          : 'background:rgba(71,85,105,0.1);color:#64748B;border:1px solid rgba(71,85,105,0.2);'">
        <div class="w-1.5 h-1.5 rounded-full"
          :style="tournament.status === 'Active' ? 'background:#10B981;' : 'background:#475569;'" />
        {{ tournament.status === 'Active' ? 'ĐANG DIỄN RA' : tournament.status?.toUpperCase() }}
      </div>
    </header>

    <!-- Loading -->
    <div v-if="loading" class="flex justify-center py-24">
      <div class="w-8 h-8 rounded-full border-2 animate-spin"
        style="border-color:rgba(16,185,129,0.2);border-top-color:#10B981;" />
    </div>

    <!-- Dynamic brackets from API -->
    <div v-else-if="matches.length > 0" class="overflow-x-auto pb-12">
      <div class="flex gap-16 items-start min-w-max px-4">
        <div v-for="([roundKey, roundMatches]) in rounds" :key="roundKey" class="flex flex-col gap-6">
          <!-- Round label -->
          <div class="text-xs font-bold text-slate-500 uppercase tracking-[0.25em] text-center pb-2"
            style="border-bottom:1px solid rgba(255,255,255,0.06);">
            {{ roundLabel(roundKey) }}
          </div>

          <!-- Match cards -->
          <div v-for="m in roundMatches" :key="m.id"
            class="w-64 rounded-xl overflow-hidden shadow-xl transition-all"
            style="background:#0A1628;border:1px solid rgba(255,255,255,0.08);">

            <!-- Player 1 -->
            <div class="px-4 py-3 flex items-center justify-between"
              :style="isWinner(m, 'player1')
                ? 'background:rgba(16,185,129,0.12);border-bottom:1px solid rgba(16,185,129,0.2);'
                : 'border-bottom:1px solid rgba(255,255,255,0.05);'">
              <div class="flex items-center gap-2.5">
                <div class="w-6 h-6 rounded-full flex items-center justify-center text-[10px] font-black flex-shrink-0"
                  style="background:rgba(16,185,129,0.15);color:#10B981;">
                  {{ m.player1?.[0]?.toUpperCase() ?? '?' }}
                </div>
                <span class="font-bold text-sm"
                  :style="isWinner(m, 'player1') ? 'color:#10B981;' : m.player1 === 'TBD' ? 'color:#475569;' : 'color:#F1F5F9;'">
                  {{ m.player1 ?? 'TBD' }}
                </span>
              </div>
              <Trophy v-if="isWinner(m, 'player1')" class="w-3.5 h-3.5 text-yellow-400" />
            </div>

            <!-- VS divider -->
            <div class="flex items-center justify-center py-1" style="background:#060D16;">
              <Swords class="w-3 h-3 text-slate-600" />
            </div>

            <!-- Player 2 -->
            <div class="px-4 py-3 flex items-center justify-between"
              :style="isWinner(m, 'player2') ? 'background:rgba(16,185,129,0.12);' : ''">
              <div class="flex items-center gap-2.5">
                <div class="w-6 h-6 rounded-full flex items-center justify-center text-[10px] font-black flex-shrink-0"
                  style="background:rgba(239,68,68,0.15);color:#EF4444;">
                  {{ m.player2?.[0]?.toUpperCase() ?? '?' }}
                </div>
                <span class="font-bold text-sm"
                  :style="isWinner(m, 'player2') ? 'color:#10B981;' : m.player2 === 'TBD' ? 'color:#475569;' : 'color:#F1F5F9;'">
                  {{ m.player2 ?? 'TBD' }}
                </span>
              </div>
              <Trophy v-if="isWinner(m, 'player2')" class="w-3.5 h-3.5 text-yellow-400" />
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- No matches yet: show participants list and status -->
    <div v-else class="max-w-2xl mx-auto">
      <div class="text-center py-12 rounded-2xl mb-8"
        style="background:#0A1628;border:1px solid rgba(255,255,255,0.06);">
        <Clock class="w-12 h-12 text-slate-700 mx-auto mb-4" />
        <h2 class="text-xl font-bold text-slate-300 mb-2">Sơ đồ chưa được tạo</h2>
        <p class="text-slate-500 text-sm">
          Giải đấu cần đủ người tham gia trước khi chia bracket.<br/>
          Trạng thái hiện tại:
          <span class="font-bold" style="color:#10B981;">
            {{ tournament?.status ?? 'Scheduled' }}
          </span>
        </p>
        <div class="flex items-center justify-center gap-6 mt-6">
          <div class="text-center">
            <div class="text-2xl font-black text-slate-100 font-mono-stat">{{ tournament?.participantCount ?? 0 }}</div>
            <div class="text-xs text-slate-500 uppercase tracking-wide mt-1">Người tham gia</div>
          </div>
        </div>
      </div>

      <router-link to="/tournaments"
        class="flex items-center justify-center gap-2 px-6 py-3 rounded-xl font-bold transition-all mx-auto w-fit"
        style="background:rgba(16,185,129,0.1);color:#10B981;border:1px solid rgba(16,185,129,0.2);">
        <ChevronLeft class="w-4 h-4" /> Quay lại danh sách giải đấu
      </router-link>
    </div>

  </div>
</template>
