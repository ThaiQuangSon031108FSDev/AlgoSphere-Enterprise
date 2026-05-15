<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRoute } from 'vue-router'
import { API_BASE } from '../utils/api'
import { ChevronLeft, Info, Trophy } from 'lucide-vue-next'

const route = useRoute()
const matches = ref<any[]>([])

onMounted(async () => {
  const id = route.params.id
  try {
    const res = await fetch(`${API_BASE}/tournaments/${id}/brackets`)
    matches.value = await res.json()
  } catch (err) {
    console.error(err)
  }
})
</script>

<template>
  <div class="p-8 h-screen flex flex-col">
    <header class="mb-12 flex items-center justify-between">
      <div class="flex items-center gap-4">
        <router-link to="/tournaments" class="p-2 hover:bg-slate-800 rounded-lg text-slate-400">
          <ChevronLeft class="w-5 h-5" />
        </router-link>
        <h1 class="text-3xl font-black text-white uppercase italic">SƠ ĐỒ GIẢI ĐẤU</h1>
      </div>
      <div class="flex items-center gap-2 text-brand-emerald font-bold text-sm bg-brand-emerald/10 px-4 py-2 rounded-full">
         <Info class="w-4 h-4" /> TOURNAMENT IN PROGRESS
      </div>
    </header>

    <div class="flex-1 overflow-x-auto pb-12">
      <div class="flex gap-24 items-center justify-center min-w-max p-8">
         <!-- Round 1 -->
         <div class="space-y-12">
            <div class="text-xs font-bold text-slate-500 uppercase tracking-widest text-center mb-8">ROUND 1</div>
            <div v-for="i in 4" :key="i" class="w-64 bg-slate-900 border border-slate-800 rounded-xl overflow-hidden shadow-xl">
               <div class="p-3 border-b border-slate-800 flex justify-between bg-slate-800/30">
                  <span class="text-slate-300 font-bold">Player {{ i*2-1 }}</span>
                  <span class="text-brand-emerald font-mono">100%</span>
               </div>
               <div class="p-3 flex justify-between">
                  <span class="text-slate-500">Player {{ i*2 }}</span>
                  <span class="text-slate-700 font-mono">--</span>
               </div>
            </div>
         </div>

         <!-- Connector -->
         <div class="w-12 h-0 border-t-2 border-brand-emerald/20"></div>

         <!-- Semi Finals -->
         <div class="space-y-32">
            <div class="text-xs font-bold text-slate-500 uppercase tracking-widest text-center mb-8">SEMI FINALS</div>
            <div v-for="i in 2" :key="i" class="w-64 bg-slate-900 border border-slate-800 rounded-xl overflow-hidden shadow-xl border-l-4 border-l-brand-emerald">
               <div class="p-3 border-b border-slate-800 flex justify-between bg-brand-emerald/10">
                  <span class="text-brand-emerald font-bold italic">TBD</span>
                  <span class="text-brand-emerald font-mono">--</span>
               </div>
               <div class="p-3 flex justify-between italic text-slate-600">
                  <span>TBD</span>
                  <span class="font-mono">--</span>
               </div>
            </div>
         </div>

         <!-- Connector -->
         <div class="w-12 h-0 border-t-2 border-brand-emerald/20"></div>

         <!-- Finals -->
         <div class="flex flex-col items-center">
            <div class="text-xs font-bold text-slate-500 uppercase tracking-widest text-center mb-8">GRAND FINALS</div>
            <div class="w-72 bg-slate-900 border-2 border-brand-emerald rounded-2xl p-6 shadow-2xl shadow-brand-emerald/10 text-center relative">
               <Trophy class="w-12 h-12 text-yellow-400 mx-auto mb-4" />
               <div class="text-2xl font-black text-white italic mb-1 uppercase">CHAMPION</div>
               <div class="text-slate-500 text-sm">Waiting for winner...</div>
            </div>
         </div>
      </div>
    </div>
  </div>
</template>
