<script setup lang="ts">
import { ref, onUnmounted } from 'vue'
import { HUB_BASE } from '../utils/api'
import * as signalR from '@microsoft/signalr'
import { Swords, Users, Zap, ShieldAlert } from 'lucide-vue-next'

const status = ref('Ready to Battle')
const matchmaking = ref(false)
const matchFound = ref(false)
const opponent = ref('')

let connection: signalR.HubConnection | null = null

const startMatchmaking = async () => {
  matchmaking.value = true
  status.value = 'Connecting to Arena...'
  
  connection = new signalR.HubConnectionBuilder()
    .withUrl(`${HUB_BASE}/ws/arena`)
    .withAutomaticReconnect()
    .build()

  connection.on('ReceiveStatus', (msg: string) => {
    status.value = msg
  })

  connection.on('MatchFound', (data: any) => {
    matchmaking.value = false
    matchFound.value = true
    opponent.value = data.Opponent
    status.value = 'Match Found!'
  })

  try {
    await connection.start()
    await connection.invoke('JoinQueue', 'Current_User_01')
  } catch (err) {
    status.value = 'Connection Failed'
    matchmaking.value = false
  }
}

onUnmounted(() => {
  connection?.stop()
})
</script>

<template>
  <div class="p-8 max-w-6xl mx-auto">
    <header class="mb-12 flex items-center justify-between">
      <div>
        <h1 class="text-4xl font-black text-brand-emerald mb-2">CODE ARENA</h1>
        <p class="text-slate-400">Thử thách tốc độ và kỹ năng 1vs1 thời gian thực.</p>
      </div>
      <div class="flex gap-4">
        <div class="bg-slate-800 p-4 rounded-xl border border-slate-700 text-center">
            <div class="text-xs text-slate-500 uppercase font-bold">Current Elo</div>
            <div class="text-2xl font-black text-white">1,250</div>
        </div>
        <div class="bg-slate-800 p-4 rounded-xl border border-slate-700 text-center">
            <div class="text-xs text-slate-500 uppercase font-bold">Rank</div>
            <div class="text-2xl font-black text-brand-indigo uppercase italic">Gold</div>
        </div>
      </div>
    </header>

    <div class="grid grid-cols-1 md:grid-cols-3 gap-8">
      <!-- Matchmaking Card -->
      <div class="md:col-span-2 bg-slate-900 border border-slate-800 rounded-3xl p-12 flex flex-col items-center justify-center relative overflow-hidden group">
        <div class="absolute inset-0 bg-brand-emerald/5 opacity-0 group-hover:opacity-100 transition-opacity"></div>
        
        <div v-if="!matchmaking && !matchFound" class="text-center z-10">
          <div class="w-24 h-24 bg-brand-emerald/10 text-brand-emerald rounded-full flex items-center justify-center mx-auto mb-8 animate-pulse">
            <Swords class="w-12 h-12" />
          </div>
          <h2 class="text-3xl font-bold mb-4">Sẵn sàng thi đấu?</h2>
          <button @click="startMatchmaking" class="bg-brand-emerald hover:bg-emerald-600 text-white px-12 py-4 rounded-2xl font-black text-lg transition-all shadow-xl shadow-brand-emerald/20 active:scale-95">
            BẮT ĐẦU GHÉP CẶP
          </button>
        </div>

        <div v-if="matchmaking" class="text-center z-10">
           <div class="relative w-32 h-32 mx-auto mb-8">
              <div class="absolute inset-0 border-4 border-brand-emerald/20 rounded-full"></div>
              <div class="absolute inset-0 border-4 border-brand-emerald border-t-transparent rounded-full animate-spin"></div>
              <Users class="absolute inset-0 m-auto w-12 h-12 text-brand-emerald" />
           </div>
           <h2 class="text-2xl font-bold mb-2">{{ status }}</h2>
           <p class="text-slate-500 italic">Ước tính thời gian: 5 giây</p>
        </div>

        <div v-if="matchFound" class="text-center z-10 w-full animate-in zoom-in duration-300">
           <div class="flex items-center justify-around mb-12">
              <div class="text-center">
                <div class="w-20 h-20 bg-slate-800 rounded-full border-4 border-brand-emerald mx-auto mb-4"></div>
                <div class="font-bold">BẠN</div>
              </div>
              <div class="text-5xl font-black italic text-brand-emerald">VS</div>
              <div class="text-center">
                <div class="w-20 h-20 bg-slate-800 rounded-full border-4 border-red-500 mx-auto mb-4"></div>
                <div class="font-bold text-red-400 uppercase">{{ opponent }}</div>
              </div>
           </div>
           <button class="bg-brand-indigo hover:bg-indigo-600 text-white px-12 py-4 rounded-2xl font-black text-lg transition-all shadow-xl shadow-brand-indigo/20">
             VÀO ĐẤU TRƯỜNG NGAY
           </button>
        </div>
      </div>

      <!-- Stats & Rules -->
      <div class="space-y-6">
        <div class="bg-slate-800 border border-slate-700 p-6 rounded-2xl">
          <h3 class="flex items-center gap-2 font-bold mb-4">
            <Zap class="text-yellow-400 w-5 h-5" /> Recent History
          </h3>
          <div class="space-y-3">
             <div v-for="i in 3" :key="i" class="flex items-center justify-between text-sm p-2 bg-slate-900/50 rounded-lg">
                <span class="text-brand-emerald font-bold">WIN</span>
                <span class="text-slate-400">vs user_abc</span>
                <span class="text-slate-500">+12 Elo</span>
             </div>
          </div>
        </div>

        <div class="bg-red-500/10 border border-red-500/20 p-6 rounded-2xl">
          <h3 class="flex items-center gap-2 font-bold mb-2 text-red-400">
            <ShieldAlert class="w-5 h-5" /> Quy định Đấu trường
          </h3>
          <ul class="text-xs text-slate-400 space-y-2 list-disc list-inside">
            <li>Không paste code từ bên ngoài.</li>
            <li>Rời trận giữa chừng sẽ bị trừ 50 Elo.</li>
            <li>Sử dụng AI trong Arena sẽ bị khóa tài khoản.</li>
          </ul>
        </div>
      </div>
    </div>
  </div>
</template>
