<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { Trophy, Calendar, Users, ChevronRight } from 'lucide-vue-next'
import { API_BASE } from '../utils/api'

const tournaments = ref<any[]>([])
const loading = ref(true)

onMounted(async () => {
  try {
    const res = await fetch(`${API_BASE}/tournaments`)
    tournaments.value = await res.json()
  } catch (err) {
    console.error(err)
  } finally {
    loading.value = false
  }
})

const statusConfig: Record<string, { label: string; color: string; bg: string; dot: string }> = {
  Active:    { label: 'LIVE', color: '#10B981', bg: 'rgba(16,185,129,0.12)', dot: '#10B981' },
  Upcoming:  { label: 'SẮPDIỄN RA', color: '#3B82F6', bg: 'rgba(59,130,246,0.12)', dot: '#3B82F6' },
  Completed: { label: 'ĐÃ KẾT THÚC', color: '#475569', bg: 'rgba(71,85,105,0.12)', dot: '#475569' },
}
</script>

<template>
  <div class="p-8 max-w-5xl mx-auto">

    <!-- Header -->
    <header class="mb-10">
      <p class="text-xs font-bold tracking-[0.25em] uppercase mb-2" style="color: #10B981;">E-SPORTS DIVISION</p>
      <div class="flex items-end justify-between">
        <div>
          <h1 class="text-4xl font-bold text-slate-100 mb-2">Giải Đấu</h1>
          <p class="text-slate-500 text-sm">Tham gia các cuộc thi coding để khẳng định vị thế.</p>
        </div>
        <div class="flex items-center gap-2 px-4 py-2 rounded-full text-xs font-bold"
          style="background: rgba(239,68,68,0.1); color: #F87171; border: 1px solid rgba(239,68,68,0.25);">
          <div class="w-1.5 h-1.5 bg-red-400 rounded-full animate-pulse"></div>
          {{ tournaments.filter(t => t.status === 'Active').length }} ĐANG DIỄN RA
        </div>
      </div>
    </header>

    <!-- Loading -->
    <div v-if="loading" class="flex justify-center py-20">
      <div class="w-8 h-8 border-2 rounded-full animate-spin" style="border-color: rgba(16,185,129,0.2); border-top-color: #10B981;"></div>
    </div>

    <!-- Tournament list -->
    <div v-else class="space-y-4">
      <div v-for="t in tournaments" :key="t.id"
        class="shine group rounded-2xl p-6 transition-all duration-300 cursor-pointer relative overflow-hidden"
        style="background: #080F1C; border: 1px solid rgba(255,255,255,0.06);"
        @mouseenter="($event.currentTarget as HTMLElement).style.borderColor = 'rgba(16,185,129,0.3)'"
        @mouseleave="($event.currentTarget as HTMLElement).style.borderColor = 'rgba(255,255,255,0.06)'">

        <!-- Accent bar -->
        <div class="absolute left-0 top-0 bottom-0 w-1 rounded-l-2xl opacity-0 group-hover:opacity-100 transition-opacity duration-300"
          :style="`background: linear-gradient(180deg, #10B981, #3B82F6);`"></div>

        <div class="flex items-center gap-6">
          <!-- Trophy icon -->
          <div class="w-16 h-16 rounded-2xl flex items-center justify-center flex-shrink-0 transition-all duration-300"
            style="background: rgba(16,185,129,0.08); border: 1px solid rgba(16,185,129,0.15);"
            @mouseenter="($event.currentTarget as HTMLElement).style.background = 'rgba(16,185,129,0.15)'"
            @mouseleave="($event.currentTarget as HTMLElement).style.background = 'rgba(16,185,129,0.08)'">
            <Trophy class="w-8 h-8 text-emerald-400" />
          </div>

          <!-- Info -->
          <div class="flex-1 min-w-0">
            <div class="flex items-center gap-3 mb-2 flex-wrap">
              <span class="inline-flex items-center gap-1.5 text-[10px] font-bold px-2.5 py-1 rounded-full uppercase tracking-wide"
                :style="`color: ${statusConfig[t.status]?.color ?? '#94A3B8'}; background: ${statusConfig[t.status]?.bg ?? 'rgba(148,163,184,0.1)'};`">
                <div class="w-1.5 h-1.5 rounded-full" :style="`background: ${statusConfig[t.status]?.dot ?? '#94A3B8'};`"></div>
                {{ statusConfig[t.status]?.label ?? t.status }}
              </span>
              <span class="text-xs text-slate-500 flex items-center gap-1.5">
                <Calendar class="w-3.5 h-3.5" />
                {{ new Date(t.startDate).toLocaleDateString('vi-VN') }}
              </span>
            </div>
            <h3 class="text-lg font-bold text-slate-100 mb-1 group-hover:text-emerald-400 transition-colors duration-200 truncate">{{ t.title }}</h3>
            <p class="text-slate-500 text-sm line-clamp-1">{{ t.description }}</p>
          </div>

          <!-- Stats + CTA -->
          <div class="flex items-center gap-8 flex-shrink-0">
            <div class="text-center hidden sm:block">
              <div class="flex items-center justify-center gap-1.5 text-xs text-slate-500 mb-1 uppercase tracking-wide font-bold">
                <Users class="w-3 h-3" /> Players
              </div>
              <div class="font-mono-stat text-xl font-bold text-slate-200">{{ t.participantCount }}</div>
            </div>

            <router-link :to="{ name: 'brackets', params: { id: t.id } }"
              class="flex items-center gap-2 px-5 py-2.5 rounded-xl font-bold text-sm transition-all duration-200 cursor-pointer whitespace-nowrap"
              style="background: rgba(16,185,129,0.1); color: #10B981; border: 1px solid rgba(16,185,129,0.2);"
              @mouseenter="($event.currentTarget as HTMLElement).style.background = '#10B981'; ($event.currentTarget as HTMLElement).style.color = '#000';"
              @mouseleave="($event.currentTarget as HTMLElement).style.background = 'rgba(16,185,129,0.1)'; ($event.currentTarget as HTMLElement).style.color = '#10B981';">
              Chi tiết <ChevronRight class="w-4 h-4" />
            </router-link>
          </div>
        </div>
      </div>

      <!-- Empty state -->
      <div v-if="!tournaments.length" class="text-center py-20">
        <Trophy class="w-12 h-12 text-slate-700 mx-auto mb-4" />
        <p class="text-slate-500 font-medium">Chưa có giải đấu nào</p>
      </div>
    </div>
  </div>
</template>
