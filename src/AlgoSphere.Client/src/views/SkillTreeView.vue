<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { Lock, CheckCircle2, Circle, ChevronRight, Zap } from 'lucide-vue-next'
import { API_BASE } from '../utils/api'

const router = useRouter()

interface SkillNode {
  topicId: number
  title: string
  subtitle: string
  status: 'completed' | 'unlocked' | 'locked'
  xp: number
  totalExercises: number
  completedExercises: number
  color: string
}

interface SkillLane {
  lane: string
  color: string
  nodes: SkillNode[]
}

interface SkillTreeData {
  lanes: SkillLane[]
  totalXp: number
  level: number
  levelXp: number
  nextLevelXp: number
}

const skillLanes = ref<SkillLane[]>([])
const totalXp = ref(0)
const levelXp = ref(0)
const userLevel = ref(1)
const nextLevelXp = ref(100)
const loading = ref(true)
const error = ref('')

onMounted(async () => {
  const token = localStorage.getItem('token')
  if (!token) { router.push('/login'); return }

  try {
    const res = await fetch(`${API_BASE}/learning/skill-tree`, {
      headers: { Authorization: `Bearer ${token}` }
    })
    if (res.status === 401) { router.push('/login'); return }
    if (!res.ok) throw new Error(`HTTP ${res.status}`)

    const data: SkillTreeData = await res.json()
    skillLanes.value = data.lanes
    totalXp.value = data.totalXp
    levelXp.value = data.levelXp
    userLevel.value = data.level
    nextLevelXp.value = data.nextLevelXp
  } catch (e) {
    error.value = 'Không tải được Skill Tree. Kiểm tra kết nối backend.'
  } finally {
    loading.value = false
  }
})

const handleNodeClick = (node: SkillNode) => {
  if (node.status === 'locked') return
  router.push({ name: 'exercises', params: { id: node.topicId } })
}

const progressPct = (n: SkillNode) =>
  n.totalExercises > 0 ? Math.round((n.completedExercises / n.totalExercises) * 100) : 0
</script>

<template>
  <div class="p-8 max-w-7xl mx-auto">
    <!-- Header -->
    <header class="mb-10">
      <p class="text-xs font-bold tracking-[0.25em] uppercase mb-2" style="color:#10B981;">LEARNING PATH</p>
      <div class="flex items-end justify-between">
        <div>
          <h1 class="text-4xl font-bold text-slate-100 mb-2">Skill Tree</h1>
          <p class="text-slate-500 text-sm">Hoàn thành từng nút để mở khóa chủ đề tiếp theo</p>
        </div>
        <!-- XP Bar -->
        <div class="hidden lg:block w-64">
          <div class="flex justify-between text-xs mb-1.5">
            <span class="font-bold text-slate-300">LV.{{ userLevel }}</span>
            <span class="font-mono-stat" style="color:#10B981;">{{ levelXp }} / {{ nextLevelXp }} XP</span>
          </div>
          <div class="h-2 rounded-full overflow-hidden" style="background:rgba(255,255,255,0.06);">
            <div class="h-full rounded-full transition-all duration-700" style="background:linear-gradient(90deg,#10B981,#34D399);"
              :style="`width:${nextLevelXp > 0 ? Math.min((levelXp / nextLevelXp) * 100, 100) : 0}%`"></div>
          </div>
        </div>
      </div>
    </header>

    <!-- Loading -->
    <div v-if="loading" class="flex items-center justify-center py-32">
      <div class="flex gap-1.5">
        <span v-for="i in 3" :key="i" class="w-2 h-2 rounded-full bg-emerald-500 animate-bounce"
          :style="`animation-delay:${(i - 1) * 0.15}s`"></span>
      </div>
    </div>

    <!-- Error -->
    <div v-else-if="error" class="text-center py-20 text-red-400 text-sm">⚠️ {{ error }}</div>

    <!-- Empty state -->
    <div v-else-if="!skillLanes.length" class="text-center py-20 text-slate-500 text-sm">
      Chưa có nội dung học nào. Admin cần thêm Category và Topic vào hệ thống.
    </div>

    <!-- Lanes -->
    <div v-else class="space-y-10">
      <div v-for="lane in skillLanes" :key="lane.lane">
        <!-- Lane header -->
        <div class="flex items-center gap-3 mb-5">
          <div class="w-1.5 h-6 rounded-full" :style="`background:${lane.color}`"></div>
          <h2 class="text-sm font-bold text-slate-300 uppercase tracking-widest">{{ lane.lane }}</h2>
          <div class="flex-1 h-px" style="background:rgba(255,255,255,0.05);"></div>
        </div>

        <!-- Nodes -->
        <div class="grid grid-cols-1 md:grid-cols-3 gap-4">
          <div
            v-for="node in lane.nodes"
            :key="node.topicId"
            :data-topic-id="node.topicId"
            @click="handleNodeClick(node)"
            class="relative p-5 rounded-2xl transition-all duration-300 select-none"
            :class="node.status !== 'locked' ? 'cursor-pointer hover:scale-[1.02] hover:-translate-y-0.5' : 'cursor-not-allowed opacity-40'"
            :style="{
              background: node.status === 'locked' ? '#0A1628' : `${node.color}08`,
              border: `1px solid ${node.status !== 'locked' ? node.color + '30' : 'rgba(255,255,255,0.06)'}`,
              boxShadow: node.status === 'unlocked' ? `0 0 20px ${node.color}15` : 'none',
            }"
          >
            <!-- Status icon -->
            <div class="flex items-center justify-between mb-3">
              <div class="w-8 h-8 rounded-lg flex items-center justify-center"
                :style="`background:${node.color}15`">
                <CheckCircle2 v-if="node.status === 'completed'" class="w-4 h-4" :style="`color:${node.color}`" />
                <Circle v-else-if="node.status === 'unlocked'" class="w-4 h-4" :style="`color:${node.color}`" />
                <Lock v-else class="w-4 h-4 text-slate-600" />
              </div>
              <div class="flex items-center gap-1 text-[10px] font-bold" :style="`color:${node.color}`">
                <Zap class="w-3 h-3" />{{ node.xp }} XP
              </div>
            </div>

            <h3 class="font-bold text-slate-100 mb-0.5">{{ node.title }}</h3>
            <p class="text-xs text-slate-500 mb-3">{{ node.subtitle }}</p>

            <!-- Progress -->
            <div>
              <div class="flex justify-between text-[10px] text-slate-600 mb-1">
                <span>{{ node.completedExercises }}/{{ node.totalExercises }} bài</span>
                <span class="font-mono-stat">{{ progressPct(node) }}%</span>
              </div>
              <div class="h-1 rounded-full overflow-hidden" style="background:rgba(255,255,255,0.06);">
                <div class="h-full rounded-full transition-all duration-500"
                  :style="`width:${progressPct(node)}%;background:${node.color}`"></div>
              </div>
            </div>

            <!-- Arrow for unlocked -->
            <ChevronRight v-if="node.status !== 'locked'"
              class="absolute right-4 top-1/2 -translate-y-1/2 w-4 h-4 opacity-30"
              :style="`color:${node.color}`" />
          </div>
        </div>
      </div>
    </div>
  </div>
</template>
