<script setup lang="ts">
import { ref, computed, onMounted, onBeforeUnmount } from 'vue'
import { useRoute } from 'vue-router'
import { API_BASE, HUB_BASE } from '../utils/api'
import { ChevronLeft, Trophy, Swords, Clock, AlertCircle } from 'lucide-vue-next'
import * as signalR from '@microsoft/signalr'

const route   = useRoute()
const matches = ref<any[]>([])
const tournament = ref<any>(null)
const loading = ref(true)
const currentTime = ref(new Date())

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
let timer: ReturnType<typeof setInterval> | null = null

onMounted(async () => {
  await fetchData()

  timer = setInterval(() => {
    currentTime.value = new Date()
  }, 1000)

  const token = localStorage.getItem('token')
  connection = new signalR.HubConnectionBuilder()
    .withUrl(`${HUB_BASE}/ws/arena`, { accessTokenFactory: () => token! })
    .withAutomaticReconnect()
    .build()

  connection.on('BracketUpdated', () => {
    fetchData()
  })

  connection.on('TournamentCompleted', () => {
    fetchData()
  })

  try {
    await connection.start()
    await connection.invoke('JoinTournamentGroup', Number(route.params.id))
  } catch (err) {
    console.error('SignalR failed', err)
  }
})

onBeforeUnmount(() => {
  connection?.stop()
  if (timer) clearInterval(timer)
})

const rounds = computed(() => {
  const map = new Map<string, any[]>()
  for (const m of matches.value) {
    const round = (m.bracketPosition ?? 'Round 1').split('-')[0] || 'Round 1'
    if (!map.has(round)) map.set(round, [])
    map.get(round)!.push(m)
  }
  return [...map.entries()].sort((a, b) => a[0].localeCompare(b[0]))
})

const roundLabel = (key: string): string => {
  const labels: Record<string, string> = {
    R1: 'ROUND 1', R2: 'ROUND 2', R3: 'SEMI FINALS', R4: 'GRAND FINALS',
    'Round 1': 'ROUND 1', 'Round 2': 'ROUND 2',
    'Semi': 'SEMI FINALS', 'Final': 'GRAND FINALS',
  }
  return labels[key] ?? key.toUpperCase()
}

const getMatchWinner = (m: any) => {
  if (!m.winnerId) return null
  return m.winnerName ?? 'Winner'
}

const getCountdown = (deadlineStr: string | null) => {
  if (!deadlineStr) return null
  const deadline = new Date(deadlineStr)
  const diff = deadline.getTime() - currentTime.value.getTime()
  if (diff <= 0) return 'EXPIRED'
  
  const min = Math.floor(diff / 60000)
  const sec = Math.floor((diff % 60000) / 1000)
  return `${min}:${sec.toString().padStart(2, '0')}`
}

const champion = computed(() => {
  if (tournament.value?.status !== 'Completed') return null
  // The winner of the match in the last round
  const lastRound = rounds.value[rounds.value.length - 1]
  if (!lastRound) return null
  const finalMatch = lastRound[1][0]
  return finalMatch?.winnerName
})
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
          <h1 class="text-4xl font-black uppercase italic text-slate-100 flex items-center gap-4">
            {{ tournament?.title ?? 'Sơ Đồ Giải Đấu' }}
            <span v-if="tournament?.status === 'Completed'" class="text-yellow-400 not-italic flex items-center gap-2 text-xl">
              <Trophy class="w-6 h-6" /> CHAMPION: {{ champion }}
            </span>
          </h1>
        </div>
      </div>

      <div v-if="tournament" class="flex items-center gap-2 px-4 py-2 rounded-full text-xs font-bold"
        :style="tournament.status === 'Active' || tournament.status === 'Ongoing'
          ? 'background:rgba(16,185,129,0.1);color:#10B981;border:1px solid rgba(16,185,129,0.2);'
          : tournament.status === 'Completed'
            ? 'background:rgba(251,191,36,0.1);color:#FBBF24;border:1px solid rgba(251,191,36,0.2);'
            : 'background:rgba(71,85,105,0.1);color:#64748B;border:1px solid rgba(71,85,105,0.2);'">
        <div class="w-1.5 h-1.5 rounded-full"
          :style="tournament.status === 'Active' || tournament.status === 'Ongoing' ? 'background:#10B981;' : tournament.status === 'Completed' ? 'background:#FBBF24;' : 'background:#475569;'" />
        {{ tournament.status?.toUpperCase() === 'ONGOING' ? 'ĐANG DIỄN RA' : tournament.status?.toUpperCase() }}
      </div>
    </header>

    <!-- Loading -->
    <div v-if="loading" class="flex justify-center py-24">
      <div class="w-8 h-8 rounded-full border-2 animate-spin"
        style="border-color:rgba(16,185,129,0.2);border-top-color:#10B981;" />
    </div>

    <!-- Dynamic brackets -->
    <div v-else-if="matches.length > 0" class="overflow-x-auto pb-12">
      <div class="flex gap-16 items-start min-w-max px-4">
        <div v-for="([roundKey, roundMatches]) in rounds" :key="roundKey" class="flex flex-col gap-6">
          <div class="text-xs font-bold text-slate-500 uppercase tracking-[0.25em] text-center pb-2"
            style="border-bottom:1px solid rgba(255,255,255,0.06);">
            {{ roundLabel(roundKey) }}
          </div>

          <div v-for="m in roundMatches" :key="m.id"
            class="w-72 rounded-xl overflow-hidden shadow-2xl transition-all relative"
            :style="m.status === 'Forfeited' ? 'border:1px solid rgba(239,68,68,0.3);' : 'background:#0A1628;border:1px solid rgba(255,255,255,0.08);'">
            
            <!-- Match Status Overlay -->
            <div v-if="m.status === 'Forfeited'" class="absolute top-0 right-0 p-1 bg-red-500/20 text-red-500 text-[8px] font-bold px-2 rounded-bl-lg flex items-center gap-1">
              <AlertCircle class="w-2 h-2" /> FORFEITED
            </div>
            
            <!-- Player 1 -->
            <div class="px-4 py-4 flex items-center justify-between"
              :style="m.winnerId && m.winnerName === m.player1
                ? 'background:rgba(16,185,129,0.12);border-bottom:1px solid rgba(16,185,129,0.2);'
                : 'border-bottom:1px solid rgba(255,255,255,0.05);'">
              <div class="flex items-center gap-3">
                <div class="w-8 h-8 rounded-full flex items-center justify-center text-xs font-black flex-shrink-0"
                  :style="m.winnerId && m.winnerName === m.player1 ? 'background:#10B981;color:#000;' : 'background:rgba(16,185,129,0.1);color:#10B981;'">
                  {{ m.player1?.[0]?.toUpperCase() ?? '?' }}
                </div>
                <span class="font-bold text-sm"
                  :style="m.winnerId && m.winnerName === m.player1 ? 'color:#10B981;' : m.player1 === 'TBD' ? 'color:#475569;' : 'color:#F1F5F9;'">
                  {{ m.player1 ?? 'TBD' }}
                </span>
              </div>
              <Trophy v-if="m.winnerId && m.winnerName === m.player1" class="w-4 h-4 text-yellow-400" />
            </div>

            <!-- VS / Timer Divider -->
            <div class="flex items-center justify-center py-1 relative" style="background:#060D16;">
              <div v-if="m.status === 'Pending' && m.player1 !== 'TBD' && m.player2 !== 'TBD'" class="flex items-center gap-1.5 text-[10px] font-bold text-yellow-500 animate-pulse">
                <Clock class="w-2.5 h-2.5" /> {{ getCountdown(m.roundDeadlineUtc) }}
              </div>
              <Swords v-else class="w-3 h-3 text-slate-600" />
            </div>

            <!-- Player 2 -->
            <div class="px-4 py-4 flex items-center justify-between"
              :style="m.winnerId && m.winnerName === m.player2 ? 'background:rgba(16,185,129,0.12);' : ''">
              <div class="flex items-center gap-3">
                <div class="w-8 h-8 rounded-full flex items-center justify-center text-xs font-black flex-shrink-0"
                  :style="m.winnerId && m.winnerName === m.player2 ? 'background:#10B981;color:#000;' : 'background:rgba(239,68,68,0.1);color:#EF4444;'">
                  {{ m.player2?.[0]?.toUpperCase() ?? '?' }}
                </div>
                <span class="font-bold text-sm"
                  :style="m.winnerId && m.winnerName === m.player2 ? 'color:#10B981;' : m.player2 === 'TBD' ? 'color:#475569;' : 'color:#F1F5F9;'">
                  {{ m.player2 ?? 'TBD' }}
                </span>
              </div>
              <Trophy v-if="m.winnerId && m.winnerName === m.player2" class="w-4 h-4 text-yellow-400" />
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Empty State -->
    <div v-else class="max-w-2xl mx-auto">
      <div class="text-center py-16 rounded-3xl mb-8"
        style="background:#0A1628;border:1px solid rgba(255,255,255,0.06);">
        <Clock class="w-16 h-16 text-slate-700 mx-auto mb-4" />
        <h2 class="text-2xl font-bold text-slate-200 mb-2">Chưa khởi tạo sơ đồ</h2>
        <p class="text-slate-500">
          Chờ đủ số lượng chiến binh tham gia để kích hoạt Bracket tự động.<br/>
          Trạng thái: <span class="text-emerald-400 font-bold uppercase">{{ tournament?.status }}</span>
        </p>
      </div>

      <router-link to="/tournaments"
        class="flex items-center justify-center gap-2 px-8 py-4 rounded-2xl font-bold transition-all mx-auto w-fit"
        style="background:rgba(16,185,129,0.1);color:#10B981;border:1px solid rgba(16,185,129,0.2);">
        <ChevronLeft class="w-5 h-5" /> Danh sách giải đấu
      </router-link>
    </div>
  </div>
</template>

