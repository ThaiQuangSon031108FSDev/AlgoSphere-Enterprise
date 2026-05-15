<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { BookOpen, ChevronRight, Zap, Trophy, Flame, Target } from 'lucide-vue-next'
import { API_BASE } from '../utils/api'

const topics = ref<any[]>([])

onMounted(async () => {
  try {
    const res = await fetch(`${API_BASE}/topics`)
    topics.value = await res.json()
  } catch (err) {
    console.error(err)
  }
})

const stats = [
  { label: 'Bài đã giải', value: '48', icon: Zap, color: '#10B981' },
  { label: 'Streak hiện tại', value: '7 ngày', icon: Flame, color: '#F97316' },
  { label: 'Hạng toàn cầu', value: '#142', icon: Trophy, color: '#FBBF24' },
  { label: 'Điểm tháng này', value: '1.250', icon: Target, color: '#3B82F6' },
]

const difficultyMap: Record<string, { label: string; color: string; bg: string }> = {
  Easy: { label: 'DỄ', color: '#10B981', bg: 'rgba(16,185,129,0.1)' },
  Medium: { label: 'TB', color: '#F97316', bg: 'rgba(249,115,22,0.1)' },
  Hard: { label: 'KHÓ', color: '#EF4444', bg: 'rgba(239,68,68,0.1)' },
}
</script>

<template>
  <div class="p-8 max-w-7xl mx-auto">

    <!-- Hero header -->
    <header class="mb-10">
      <div class="flex items-end justify-between">
        <div>
          <p class="text-xs font-bold tracking-[0.25em] uppercase mb-2" style="color: #10B981;">WELCOME BACK</p>
          <h1 class="text-5xl font-bold leading-none mb-3" style="color: #F1F5F9;">
            Chinh phục<br/>
            <span style="background: linear-gradient(90deg, #10B981, #3B82F6); -webkit-background-clip: text; background-clip: text; -webkit-text-fill-color: transparent;">
              Thuật Toán
            </span>
          </h1>
          <p class="text-slate-500 text-sm">Hôm nay bạn muốn luyện gì? Hãy chọn một topic bên dưới.</p>
        </div>
        <div class="hidden lg:flex items-center gap-2 px-4 py-2 rounded-full text-xs font-bold"
          style="background: rgba(16,185,129,0.08); border: 1px solid rgba(16,185,129,0.2); color: #10B981;">
          <div class="w-2 h-2 rounded-full bg-emerald-400 animate-pulse"></div>
          SERVER ONLINE
        </div>
      </div>
    </header>

    <!-- Stats row -->
    <div class="grid grid-cols-2 lg:grid-cols-4 gap-4 mb-10">
      <div v-for="stat in stats" :key="stat.label"
        class="shine cursor-pointer rounded-2xl p-5 transition-all duration-300 relative overflow-hidden"
        style="background: #0A1628; border: 1px solid rgba(255,255,255,0.06);"
        @mouseenter="($event.currentTarget as HTMLElement).style.borderColor = stat.color + '40'"
        @mouseleave="($event.currentTarget as HTMLElement).style.borderColor = 'rgba(255,255,255,0.06)'">
        <div class="flex items-start justify-between mb-4">
          <div class="p-2 rounded-lg" :style="`background: ${stat.color}15`">
            <component :is="stat.icon" class="w-4 h-4" :style="`color: ${stat.color}`" />
          </div>
        </div>
        <div class="font-mono-stat text-2xl font-bold text-slate-100 mb-0.5">{{ stat.value }}</div>
        <div class="text-xs text-slate-500 font-medium uppercase tracking-wide">{{ stat.label }}</div>
      </div>
    </div>

    <!-- Topics section -->
    <div class="mb-6 flex items-center justify-between">
      <div>
        <h2 class="text-lg font-bold text-slate-100">Topics Luyện Tập</h2>
        <p class="text-xs text-slate-500 mt-0.5">Chọn chủ đề và bắt đầu ngay</p>
      </div>
      <span class="text-xs font-mono-stat font-bold px-3 py-1 rounded-full"
        style="background: rgba(16,185,129,0.1); color: #10B981; border: 1px solid rgba(16,185,129,0.2);">
        {{ topics.length }} TOPICS
      </span>
    </div>

    <section class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
      <router-link
        v-for="(topic, idx) in topics"
        :key="topic.id"
        :to="{ name: 'exercises', params: { id: topic.id } }"
        class="card-glass shine group rounded-2xl p-6 transition-all duration-300 cursor-pointer block"
      >
        <!-- Top row -->
        <div class="flex items-start justify-between mb-5">
          <div class="flex items-center gap-3">
            <div class="w-10 h-10 rounded-xl flex items-center justify-center font-bold text-sm font-mono-stat"
              style="background: linear-gradient(135deg, rgba(16,185,129,0.2), rgba(59,130,246,0.2)); border: 1px solid rgba(16,185,129,0.2); color: #10B981;">
              {{ String(idx + 1).padStart(2, '0') }}
            </div>
            <div class="p-2 rounded-lg" style="background: rgba(16,185,129,0.1);">
              <BookOpen class="text-emerald-400 w-4 h-4" />
            </div>
          </div>
          <span v-if="topic.difficulty" class="text-[10px] font-bold px-2 py-0.5 rounded uppercase tracking-wide"
            :style="`color: ${difficultyMap[topic.difficulty]?.color ?? '#94A3B8'}; background: ${difficultyMap[topic.difficulty]?.bg ?? 'rgba(148,163,184,0.1)'}`">
            {{ difficultyMap[topic.difficulty]?.label ?? topic.difficulty }}
          </span>
        </div>

        <!-- Content -->
        <h3 class="text-base font-bold mb-2 text-slate-100 group-hover:text-emerald-400 transition-colors duration-200">{{ topic.name }}</h3>
        <p class="text-slate-500 text-sm mb-5 line-clamp-2 leading-relaxed">{{ topic.description }}</p>

        <!-- Footer -->
        <div class="flex items-center justify-between">
          <div class="flex items-center gap-1.5 text-xs text-slate-500">
            <div class="w-1.5 h-1.5 rounded-full bg-emerald-400"></div>
            <span>{{ topic.exerciseCount ?? '—' }} bài tập</span>
          </div>
          <span class="flex items-center gap-1 text-xs font-bold text-emerald-400 opacity-0 group-hover:opacity-100 transition-all duration-200 translate-x-1 group-hover:translate-x-0">
            Bắt đầu <ChevronRight class="w-3 h-3" />
          </span>
        </div>
      </router-link>

      <!-- Placeholder skeleton nếu chưa load -->
      <template v-if="!topics.length">
        <div v-for="n in 6" :key="n"
          class="rounded-2xl p-6 animate-pulse"
          style="background: #0A1628; border: 1px solid rgba(255,255,255,0.04); height: 180px;">
        </div>
      </template>
    </section>
  </div>
</template>
