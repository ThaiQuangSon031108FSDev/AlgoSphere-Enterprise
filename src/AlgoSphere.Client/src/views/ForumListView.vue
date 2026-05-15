<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { Users, ChevronRight, Plus, Hash } from 'lucide-vue-next'
import { API_BASE } from '../utils/api'

const forums = ref<any[]>([])
const loading = ref(true)

onMounted(async () => {
  try {
    const res = await fetch(`${API_BASE}/forums`)
    forums.value = await res.json()
  } catch (err) {
    console.error(err)
  } finally {
    loading.value = false
  }
})

const accentColors = [
  { icon: '#10B981', bg: 'rgba(16,185,129,0.1)', border: 'rgba(16,185,129,0.2)' },
  { icon: '#3B82F6', bg: 'rgba(59,130,246,0.1)', border: 'rgba(59,130,246,0.2)' },
  { icon: '#A78BFA', bg: 'rgba(167,139,250,0.1)', border: 'rgba(167,139,250,0.2)' },
  { icon: '#F97316', bg: 'rgba(249,115,22,0.1)', border: 'rgba(249,115,22,0.2)' },
]
</script>

<template>
  <div class="p-8 max-w-6xl mx-auto">

    <!-- Header -->
    <header class="mb-10 flex items-end justify-between">
      <div>
        <p class="text-xs font-bold tracking-[0.25em] uppercase mb-2" style="color: #10B981;">COMMUNITY HUB</p>
        <h1 class="text-4xl font-bold text-slate-100 mb-2">Diễn Đàn</h1>
        <p class="text-slate-500 text-sm">Trao đổi, học hỏi và giải đáp thắc mắc về thuật toán.</p>
      </div>
      <button class="shine flex items-center gap-2 px-5 py-2.5 rounded-xl font-bold text-sm cursor-pointer transition-all duration-200"
        style="background: rgba(16,185,129,0.1); color: #10B981; border: 1px solid rgba(16,185,129,0.2);"
        @mouseenter="($event.currentTarget as HTMLElement).style.background = '#10B981'; ($event.currentTarget as HTMLElement).style.color = '#000';"
        @mouseleave="($event.currentTarget as HTMLElement).style.background = 'rgba(16,185,129,0.1)'; ($event.currentTarget as HTMLElement).style.color = '#10B981';">
        <Plus class="w-4 h-4" /> Tạo Thảo Luận
      </button>
    </header>

    <!-- Loading -->
    <div v-if="loading" class="grid grid-cols-1 md:grid-cols-2 gap-5">
      <div v-for="n in 4" :key="n" class="rounded-2xl p-6 animate-pulse h-48"
        style="background: #080F1C; border: 1px solid rgba(255,255,255,0.05);"></div>
    </div>

    <!-- Forum grid -->
    <div v-else class="grid grid-cols-1 md:grid-cols-2 gap-5">
      <div v-for="(forum, idx) in forums" :key="forum.id"
        class="shine group rounded-2xl p-6 transition-all duration-300 cursor-pointer relative overflow-hidden"
        style="background: #080F1C; border: 1px solid rgba(255,255,255,0.06);"
        @mouseenter="($event.currentTarget as HTMLElement).style.borderColor = accentColors[idx % 4].border"
        @mouseleave="($event.currentTarget as HTMLElement).style.borderColor = 'rgba(255,255,255,0.06)'">

        <!-- Top row -->
        <div class="flex items-start justify-between mb-5">
          <div class="w-12 h-12 rounded-xl flex items-center justify-center transition-all duration-300"
            :style="`background: ${accentColors[idx % 4].bg}; border: 1px solid ${accentColors[idx % 4].border};`">
            <Hash class="w-5 h-5" :style="`color: ${accentColors[idx % 4].icon}`" />
          </div>
          <div class="flex items-center gap-1.5 text-xs font-bold font-mono-stat px-3 py-1 rounded-full"
            style="background: rgba(255,255,255,0.04); color: #475569;">
            <Users class="w-3 h-3" />
            {{ forum.discussionCount ?? '—' }}
          </div>
        </div>

        <!-- Content -->
        <h3 class="text-base font-bold mb-2 text-slate-100 transition-colors duration-200"
          :style="`color: var(--c-${idx})`"
          @mouseenter="($event.currentTarget as HTMLElement).style.color = accentColors[idx % 4].icon"
          @mouseleave="($event.currentTarget as HTMLElement).style.color = '#F1F5F9'">
          {{ forum.title }}
        </h3>
        <p class="text-slate-500 text-sm mb-6 leading-relaxed line-clamp-2">{{ forum.description }}</p>

        <!-- CTA -->
        <router-link :to="{ name: 'forum-detail', params: { id: forum.id } }"
          class="w-full py-3 rounded-xl text-sm font-bold flex items-center justify-center gap-2 transition-all duration-200 cursor-pointer"
          style="background: rgba(255,255,255,0.04); color: #94A3B8; border: 1px solid rgba(255,255,255,0.06);"
          @mouseenter="($event.currentTarget as HTMLElement).style.background = accentColors[idx % 4].bg; ($event.currentTarget as HTMLElement).style.color = accentColors[idx % 4].icon; ($event.currentTarget as HTMLElement).style.borderColor = accentColors[idx % 4].border;"
          @mouseleave="($event.currentTarget as HTMLElement).style.background = 'rgba(255,255,255,0.04)'; ($event.currentTarget as HTMLElement).style.color = '#94A3B8'; ($event.currentTarget as HTMLElement).style.borderColor = 'rgba(255,255,255,0.06)';">
          Xem bài viết <ChevronRight class="w-4 h-4" />
        </router-link>
      </div>
    </div>
  </div>
</template>
