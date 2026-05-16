<script setup lang="ts">
import { ref, onUnmounted, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { HUB_BASE, API_BASE } from '../utils/api'
import * as signalR from '@microsoft/signalr'
import { Swords, Users, Zap, ShieldAlert, Trophy, Clock, X } from 'lucide-vue-next'

const router = useRouter()

type ArenaState = 'idle' | 'searching' | 'matched' | 'practice'
const state = ref<ArenaState>('idle')
const statusMsg = ref('')
const opponent = ref('')
const matchId = ref('')
const matchExerciseId = ref(1)
const myTeam = ref<'blue' | 'red'>('blue')
const isPractice = ref(false)
const userProfile = ref<{ username: string; rankPoints: number; rank: string } | null>(null)
const elapsedSeconds = ref(0)
const countdown = ref(5)
let elapsedTimer: ReturnType<typeof setInterval> | null = null
let countdownTimer: ReturnType<typeof setInterval> | null = null

let connection: signalR.HubConnection | null = null

onMounted(async () => {
  const token = localStorage.getItem('token')
  if (!token) { router.push('/login'); return }

  // Load user profile for Elo display
  try {
    const res = await fetch(`${API_BASE}/users/me`, { headers: { Authorization: `Bearer ${token}` } })
    if (res.ok) userProfile.value = await res.json()
  } catch { /* silent */ }
})

const startMatchmaking = async () => {
  const token = localStorage.getItem('token')
  if (!token) { router.push('/login'); return }

  state.value = 'searching'
  statusMsg.value = 'Đang kết nối...'
  elapsedSeconds.value = 0
  countdown.value = 5

  connection = new signalR.HubConnectionBuilder()
    .withUrl(`${HUB_BASE}/ws/arena`, {
      accessTokenFactory: () => token,
    })
    .withAutomaticReconnect()
    .build()

  connection.on('ReceiveStatus', (msg: string) => {
    statusMsg.value = msg
  })

  connection.on('MatchFound', (data: any) => {
    clearInterval(elapsedTimer!)
    opponent.value = data.opponent
    matchId.value = data.matchId
    matchExerciseId.value = data.exerciseId || 1
    myTeam.value = data.yourTeam ?? 'blue'
    isPractice.value = data.isPractice ?? false
    state.value = data.isPractice ? 'practice' : 'matched'
    
    // Start countdown for drama
    countdownTimer = setInterval(() => {
      countdown.value--
      if (countdown.value <= 0) {
        clearInterval(countdownTimer!)
        enterMatch()
      }
    }, 1000)
  })

  try {
    await connection.start()
    await connection.invoke('JoinQueue')

    // Elapsed timer
    elapsedTimer = setInterval(() => { elapsedSeconds.value++ }, 1000)
  } catch {
    statusMsg.value = 'Không kết nối được Arena Hub. Kiểm tra backend.'
    state.value = 'idle'
  }
}

const cancelSearch = async () => {
  clearInterval(elapsedTimer!)
  clearInterval(countdownTimer!)
  if (connection) {
    try { await connection.invoke('LeaveQueue') } catch { /* ignore */ }
    await connection.stop()
  }
  state.value = 'idle'
  statusMsg.value = ''
  elapsedSeconds.value = 0
}

const enterMatch = () => {
  router.push({ name: 'workspace', params: { id: matchExerciseId.value }, query: { matchId: matchId.value, team: myTeam.value } })
}

const formatElapsed = (s: number) => `${Math.floor(s / 60).toString().padStart(2, '0')}:${(s % 60).toString().padStart(2, '0')}`

onUnmounted(() => {
  clearInterval(elapsedTimer!)
  connection?.stop()
})

const rankColors: Record<string, string> = { Bronze: '#CD7F32', Silver: '#C0C0C0', Gold: '#FBBF24', Platinum: '#60A5FA', Diamond: '#A78BFA' }
</script>

<template>
  <div class="p-8 max-w-6xl mx-auto">
    <header class="mb-12 flex items-center justify-between">
      <div>
        <p class="text-xs font-bold tracking-[0.25em] uppercase mb-2" style="color:#EF4444;">PVP MODE</p>
        <h1 class="text-4xl font-black" style="color:#10B981;">CODE ARENA</h1>
        <p class="text-slate-400 mt-1">Thử thách tốc độ và kỹ năng 1vs1 thời gian thực.</p>
      </div>
      <!-- Player Stats -->
      <div v-if="userProfile" class="flex gap-4">
        <div class="p-4 rounded-xl text-center" style="background:#0A1628;border:1px solid rgba(255,255,255,0.08);">
          <div class="text-xs text-slate-500 uppercase font-bold mb-1">XP</div>
          <div class="text-2xl font-black text-white font-mono-stat">{{ userProfile.rankPoints.toLocaleString() }}</div>
        </div>
        <div class="p-4 rounded-xl text-center" style="background:#0A1628;border:1px solid rgba(255,255,255,0.08);">
          <div class="text-xs text-slate-500 uppercase font-bold mb-1">Rank</div>
          <div class="text-2xl font-black uppercase italic" :style="`color:${rankColors[userProfile.rank] ?? '#FBBF24'}`">{{ userProfile.rank }}</div>
        </div>
      </div>
    </header>

    <div class="grid grid-cols-1 md:grid-cols-3 gap-8">
      <!-- Matchmaking Card -->
      <div class="md:col-span-2 rounded-3xl p-12 flex flex-col items-center justify-center relative overflow-hidden"
        style="background:#0A1628;border:1px solid rgba(255,255,255,0.08);">

        <!-- IDLE: Ready -->
        <div v-if="state === 'idle'" class="text-center z-10">
          <div class="w-24 h-24 rounded-full flex items-center justify-center mx-auto mb-8 animate-pulse"
            style="background:rgba(16,185,129,0.1);border:1px solid rgba(16,185,129,0.2);">
            <Swords class="w-12 h-12" style="color:#10B981;" />
          </div>
          <h2 class="text-3xl font-bold mb-2 text-slate-100">Sẵn sàng thi đấu?</h2>
          <p class="text-slate-500 text-sm mb-8">Ghép cặp tự động dựa trên Rank Point của bạn</p>
          <button @click="startMatchmaking"
            class="px-12 py-4 rounded-2xl font-black text-lg transition-all active:scale-95 shadow-xl"
            style="background:#10B981;color:#000;box-shadow:0 0 40px rgba(16,185,129,0.3);">
            BẮT ĐẦU GHÉP CẶP
          </button>
        </div>

        <!-- SEARCHING -->
        <div v-else-if="state === 'searching'" class="text-center z-10 w-full">
          <div class="relative w-32 h-32 mx-auto mb-8">
            <div class="absolute inset-0 rounded-full" style="border:4px solid rgba(16,185,129,0.15);"></div>
            <div class="absolute inset-0 rounded-full animate-spin" style="border:4px solid transparent;border-top-color:#10B981;"></div>
            <Users class="absolute inset-0 m-auto w-12 h-12" style="color:#10B981;" />
          </div>
          <h2 class="text-2xl font-bold mb-2 text-slate-100">{{ statusMsg }}</h2>
          <div class="flex items-center justify-center gap-2 text-slate-500 mb-6">
            <Clock class="w-4 h-4" />
            <span class="font-mono-stat">{{ formatElapsed(elapsedSeconds) }}</span>
          </div>
          <button @click="cancelSearch" class="flex items-center gap-2 px-6 py-2.5 rounded-xl text-sm font-bold transition-all mx-auto"
            style="color:#EF4444;background:rgba(239,68,68,0.08);border:1px solid rgba(239,68,68,0.2);">
            <X class="w-4 h-4" /> Hủy tìm trận
          </button>
        </div>

        <!-- MATCHED -->
        <div v-else-if="state === 'matched' || state === 'practice'" class="text-center z-10 w-full">
          <div class="text-xs font-bold uppercase tracking-widest mb-6" style="color:#10B981;">
            {{ state === 'practice' ? '🤖 Chế độ luyện tập' : '⚡ ĐÃ TÌM THẤY ĐỐI THỦ!' }}
          </div>
          <div class="flex items-center justify-around mb-10">
            <div class="text-center">
              <div class="w-20 h-20 rounded-full flex items-center justify-center font-bold text-xl mx-auto mb-3 border-4"
                :style="myTeam === 'blue' ? 'border-color:#3B82F6;background:rgba(59,130,246,0.15);' : 'border-color:#10B981;background:rgba(16,185,129,0.15);'">
                {{ userProfile?.username?.[0]?.toUpperCase() ?? 'B' }}
              </div>
              <div class="font-bold text-slate-200">{{ userProfile?.username ?? 'BẠN' }}</div>
              <div class="text-xs mt-1 font-bold" :style="myTeam === 'blue' ? 'color:#60A5FA' : 'color:#10B981'">TEAM {{ myTeam.toUpperCase() }}</div>
            </div>
            <div class="text-5xl font-black italic" style="color:#EF4444;">VS</div>
            <div class="text-center">
              <div class="w-20 h-20 rounded-full flex items-center justify-center font-bold text-xl mx-auto mb-3 border-4"
                style="border-color:#EF4444;background:rgba(239,68,68,0.15);">
                {{ opponent[0]?.toUpperCase() ?? 'B' }}
              </div>
              <div class="font-bold text-red-400 uppercase">{{ opponent }}</div>
              <div class="text-xs mt-1 text-slate-600">ĐỐI THỦ</div>
            </div>
          </div>
          <div class="flex flex-col items-center gap-4">
            <div class="text-sm font-bold text-slate-500 animate-pulse">Tự động bắt đầu sau...</div>
            <div class="text-6xl font-black text-white drop-shadow-[0_0_20px_rgba(59,130,246,0.5)]">{{ countdown }}</div>
            <button @click="enterMatch"
              class="px-12 py-4 rounded-2xl font-black text-lg transition-all active:scale-95"
              style="background:linear-gradient(135deg,#3B82F6,#8B5CF6);color:#fff;box-shadow:0 0 40px rgba(59,130,246,0.3);">
              VÀO ĐẤU TRƯỜNG NGAY →
            </button>
          </div>
        </div>
      </div>

      <!-- Side Panel -->
      <div class="space-y-5">
        <!-- Rules -->
        <div class="p-5 rounded-2xl" style="background:#0A1628;border:1px solid rgba(255,255,255,0.06);">
          <h3 class="flex items-center gap-2 font-bold mb-3 text-sm text-slate-300">
            <ShieldAlert class="w-4 h-4 text-red-400" /> Quy định
          </h3>
          <ul class="text-xs text-slate-500 space-y-2 list-disc list-inside">
            <li>Không paste code từ bên ngoài.</li>
            <li>Rời trận giữa chừng trừ 50 XP.</li>
            <li>Sử dụng AI trong Arena sẽ bị khóa tài khoản.</li>
          </ul>
        </div>

        <!-- Prizes -->
        <div class="p-5 rounded-2xl" style="background:#0A1628;border:1px solid rgba(255,255,255,0.06);">
          <h3 class="flex items-center gap-2 font-bold mb-3 text-sm text-slate-300">
            <Trophy class="w-4 h-4 text-yellow-400" /> Phần thưởng
          </h3>
          <div class="space-y-2">
            <div v-for="prize in [
              { result: 'Thắng', xp: '+50 XP', color: '#10B981' },
              { result: 'Hòa',   xp: '+15 XP', color: '#FBBF24' },
              { result: 'Thua',  xp: '+5 XP',  color: '#F97316' },
            ]" :key="prize.result" class="flex justify-between text-xs">
              <span class="text-slate-400">{{ prize.result }}</span>
              <span class="font-bold font-mono-stat" :style="`color:${prize.color}`">{{ prize.xp }}</span>
            </div>
          </div>
        </div>

        <!-- How it works -->
        <div class="p-5 rounded-2xl" style="background:#0A1628;border:1px solid rgba(255,255,255,0.06);">
          <h3 class="flex items-center gap-2 font-bold mb-3 text-sm text-slate-300">
            <Zap class="w-4 h-4 text-yellow-400" /> Cách thức
          </h3>
          <ol class="text-xs text-slate-500 space-y-1.5 list-decimal list-inside">
            <li>Bấm "Bắt đầu ghép cặp"</li>
            <li>Hệ thống tìm đối thủ cùng trình độ</li>
            <li>Cùng giải 1 bài trong 15 phút</li>
            <li>Ai nhanh và đúng hơn thắng</li>
          </ol>
        </div>
      </div>
    </div>
  </div>
</template>
