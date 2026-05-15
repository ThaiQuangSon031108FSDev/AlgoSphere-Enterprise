<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { Code2, ChevronRight, Lock, Star } from 'lucide-vue-next'
import { useRoute } from 'vue-router'
import { API_BASE } from '../utils/api'

const route = useRoute()
const exercises = ref<any[]>([])
const loading = ref(true)

onMounted(async () => {
  const topicId = route.params.id
  try {
    const res = await fetch(`${API_BASE}/exercises/topic/${topicId}`)
    exercises.value = await res.json()
  } catch (err) {
    console.error(err)
  } finally {
    loading.value = false
  }
})

const diffConfig: Record<string, { label: string; color: string; bg: string }> = {
  Easy:   { label: 'Easy',   color: '#10B981', bg: 'rgba(16,185,129,0.12)' },
  Medium: { label: 'Medium', color: '#F97316', bg: 'rgba(249,115,22,0.12)' },
  Hard:   { label: 'Hard',   color: '#EF4444', bg: 'rgba(239,68,68,0.12)' },
}
</script>

<template>
  <div class="p-8 max-w-4xl mx-auto">

    <!-- Header -->
    <header class="mb-10">
      <router-link to="/" class="inline-flex items-center gap-1.5 text-xs font-bold text-slate-500 hover:text-emerald-400 transition-colors mb-5 uppercase tracking-wide">
        ← Quay lại Topics
      </router-link>
      <p class="text-xs font-bold tracking-[0.25em] uppercase mb-2" style="color: #10B981;">PRACTICE ZONE</p>
      <h1 class="text-4xl font-bold text-slate-100 mb-2">Danh Sách Bài Tập</h1>
      <p class="text-slate-500 text-sm">Chọn một thử thách và bắt đầu tối ưu hóa thuật toán.</p>
    </header>

    <!-- Stats bar -->
    <div v-if="!loading && exercises.length" class="flex items-center gap-6 mb-8 px-5 py-3 rounded-xl" style="background: rgba(255,255,255,0.02); border: 1px solid rgba(255,255,255,0.05);">
      <div class="flex items-center gap-2 text-xs font-bold">
        <div class="w-2 h-2 bg-emerald-400 rounded-full"></div>
        <span class="text-slate-400">{{ exercises.filter(e => e.difficultyLevel === 'Easy').length }} Easy</span>
      </div>
      <div class="flex items-center gap-2 text-xs font-bold">
        <div class="w-2 h-2 bg-orange-400 rounded-full"></div>
        <span class="text-slate-400">{{ exercises.filter(e => e.difficultyLevel === 'Medium').length }} Medium</span>
      </div>
      <div class="flex items-center gap-2 text-xs font-bold">
        <div class="w-2 h-2 bg-red-400 rounded-full"></div>
        <span class="text-slate-400">{{ exercises.filter(e => e.difficultyLevel === 'Hard').length }} Hard</span>
      </div>
      <div class="ml-auto font-mono-stat text-xs text-slate-500 font-bold">{{ exercises.length }} TOTAL</div>
    </div>

    <!-- Loading skeleton -->
    <div v-if="loading" class="space-y-3">
      <div v-for="n in 5" :key="n" class="rounded-2xl p-5 animate-pulse h-20"
        style="background: #080F1C; border: 1px solid rgba(255,255,255,0.05);"></div>
    </div>

    <!-- Exercise list -->
    <div v-else class="space-y-3">
      <div v-for="(exercise, idx) in exercises" :key="exercise.id"
        class="shine group flex items-center gap-5 rounded-2xl p-5 transition-all duration-200 cursor-pointer relative overflow-hidden"
        style="background: #080F1C; border: 1px solid rgba(255,255,255,0.06);"
        @mouseenter="($event.currentTarget as HTMLElement).style.borderColor = 'rgba(16,185,129,0.25)'"
        @mouseleave="($event.currentTarget as HTMLElement).style.borderColor = 'rgba(255,255,255,0.06)'">

        <!-- Index -->
        <div class="w-9 h-9 rounded-xl flex items-center justify-center font-mono-stat text-xs font-bold flex-shrink-0 transition-all duration-200"
          style="background: rgba(16,185,129,0.08); color: #10B981; border: 1px solid rgba(16,185,129,0.15);"
          @mouseenter="($event.currentTarget as HTMLElement).style.background = 'rgba(16,185,129,0.2)'"
          @mouseleave="($event.currentTarget as HTMLElement).style.background = 'rgba(16,185,129,0.08)'">
          {{ String(idx + 1).padStart(2, '0') }}
        </div>

        <!-- Icon -->
        <div class="p-2.5 rounded-xl flex-shrink-0" style="background: rgba(59,130,246,0.1); border: 1px solid rgba(59,130,246,0.15);">
          <Code2 class="text-blue-400 w-4 h-4" />
        </div>

        <!-- Content -->
        <div class="flex-1 min-w-0">
          <div class="flex items-center gap-3 mb-1 flex-wrap">
            <h4 class="font-bold text-slate-100 text-sm group-hover:text-emerald-400 transition-colors duration-200">{{ exercise.title }}</h4>
            <span class="text-[10px] font-bold px-2 py-0.5 rounded-full uppercase tracking-wide"
              :style="`color: ${diffConfig[exercise.difficultyLevel]?.color ?? '#94A3B8'}; background: ${diffConfig[exercise.difficultyLevel]?.bg ?? 'rgba(148,163,184,0.1)'}`">
              {{ diffConfig[exercise.difficultyLevel]?.label ?? exercise.difficultyLevel }}
            </span>
          </div>
          <div class="flex items-center gap-4 text-[11px] text-slate-500">
            <span class="flex items-center gap-1 font-mono-stat font-bold" style="color: #FBBF24;">
              <Star class="w-3 h-3 fill-current" /> +{{ exercise.points }} XP
            </span>
          </div>
        </div>

        <!-- CTA -->
        <router-link :to="{ name: 'workspace', params: { id: exercise.id } }"
          class="flex items-center gap-1.5 px-4 py-2 rounded-xl text-xs font-bold transition-all duration-200 flex-shrink-0 cursor-pointer"
          style="background: rgba(16,185,129,0.1); color: #10B981; border: 1px solid rgba(16,185,129,0.2);"
          @mouseenter="($event.currentTarget as HTMLElement).style.background = '#10B981'; ($event.currentTarget as HTMLElement).style.color = '#000';"
          @mouseleave="($event.currentTarget as HTMLElement).style.background = 'rgba(16,185,129,0.1)'; ($event.currentTarget as HTMLElement).style.color = '#10B981';">
          Luyện tập <ChevronRight class="w-3.5 h-3.5" />
        </router-link>
      </div>

      <!-- Empty state -->
      <div v-if="!exercises.length" class="text-center py-16">
        <Lock class="w-10 h-10 text-slate-700 mx-auto mb-3" />
        <p class="text-slate-500">Chưa có bài tập nào trong topic này</p>
      </div>
    </div>
  </div>
</template>
