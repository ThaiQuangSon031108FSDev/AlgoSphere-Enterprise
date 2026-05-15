<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { Users, BookOpen, Server, Star, Building2, Search, Plus, RefreshCw, AlertCircle, Trash2, ShieldOff, ShieldCheck } from 'lucide-vue-next'
import { API_BASE } from '../utils/api'

const router = useRouter()

type Tab = 'overview' | 'users' | 'content' | 'sandbox' | 'gamification'
const activeTab = ref<Tab>('overview')

const tabs: { key: Tab; label: string; icon: any }[] = [
  { key: 'overview', label: 'Tổng Quan', icon: Star },
  { key: 'users', label: 'Người dùng', icon: Users },
  { key: 'content', label: 'Nội dung', icon: BookOpen },
  { key: 'sandbox', label: 'Sandbox', icon: Server },
  { key: 'gamification', label: 'Badges', icon: Star },
]

// ── State ─────────────────────────────────────────────────────────────────────
interface Analytics { totalUsers: number; activeUsers: number; totalExercises: number; totalSubmissions: number; acceptedSubmissions: number; acceptanceRate: number }
interface AdminUser { id: number; username: string; email: string; role: string; level: number; rankPoints: number; status: string; createdAt: string }
interface AdminExercise { id: number; title: string; topicName: string; difficultyLevel: string; points: number; timeLimitMs: number }

const analytics = ref<Analytics | null>(null)
const users = ref<AdminUser[]>([])
const exercises = ref<AdminExercise[]>([])
const searchQuery = ref('')
const loading = ref(false)
const error = ref('')

const difficultyColor: Record<string, string> = { Easy: '#10B981', Medium: '#F97316', Hard: '#EF4444' }

// Static sandbox pools (Docker daemon status — not stored in DB)
const sandboxPools = [
  { id: 1, language: 'JavaScript', image: 'node:22-alpine', warm: 3, max: 10, status: 'Running', memLimit: '128MB', cpuLimit: '0.5 core' },
  { id: 2, language: 'Python', image: 'python:3.12-alpine', warm: 2, max: 10, status: 'Running', memLimit: '128MB', cpuLimit: '0.5 core' },
]

// Static badge config (can be moved to DB later)
const badges = [
  { id: 1, name: 'First Blood', criteria: 'Giải bài đầu tiên', xpReward: 10, active: true },
  { id: 2, name: 'Streak Master', criteria: '7 ngày liên tiếp', xpReward: 50, active: true },
  { id: 3, name: 'Array Conqueror', criteria: 'Hoàn thành chủ đề Array', xpReward: 100, active: true },
  { id: 4, name: 'Arena Warrior', criteria: 'Thắng 10 trận 1v1', xpReward: 200, active: false },
]

// ── API helpers ───────────────────────────────────────────────────────────────
const getHeaders = () => ({
  'Content-Type': 'application/json',
  Authorization: `Bearer ${localStorage.getItem('token')}`,
})

const fetchAnalytics = async () => {
  const res = await fetch(`${API_BASE}/admin/analytics`, { headers: getHeaders() })
  if (res.ok) analytics.value = await res.json()
}

const fetchUsers = async () => {
  const url = searchQuery.value
    ? `${API_BASE}/admin/users?search=${encodeURIComponent(searchQuery.value)}`
    : `${API_BASE}/admin/users`
  const res = await fetch(url, { headers: getHeaders() })
  if (res.ok) users.value = await res.json()
}

const fetchExercises = async () => {
  const res = await fetch(`${API_BASE}/admin/exercises`, { headers: getHeaders() })
  if (res.ok) exercises.value = await res.json()
}

const setUserStatus = async (userId: number, status: string) => {
  await fetch(`${API_BASE}/admin/users/${userId}/status`, {
    method: 'PATCH',
    headers: getHeaders(),
    body: JSON.stringify({ status }),
  })
  await fetchUsers()
}

const deleteExercise = async (id: number) => {
  if (!confirm('Xác nhận xóa bài tập này?')) return
  await fetch(`${API_BASE}/admin/exercises/${id}`, { method: 'DELETE', headers: getHeaders() })
  await fetchExercises()
}

// ── Mount ─────────────────────────────────────────────────────────────────────
onMounted(async () => {
  const token = localStorage.getItem('token')
  if (!token) { router.push('/login'); return }

  // TODO: Check role from JWT or /users/me to verify is Admin
  // If not admin: router.push('/dashboard')

  loading.value = true
  try {
    await Promise.all([fetchAnalytics(), fetchUsers(), fetchExercises()])
  } catch (e) {
    error.value = 'Lỗi kết nối API. Kiểm tra backend.'
  } finally {
    loading.value = false
  }
})
</script>

<template>
  <div class="p-8 max-w-7xl mx-auto">
    <!-- Header -->
    <header class="mb-8">
      <p class="text-xs font-bold tracking-[0.25em] uppercase mb-2" style="color:#EF4444;">ADMIN CMS</p>
      <h1 class="text-3xl font-bold text-slate-100">Cổng Quản Trị</h1>
      <p class="text-slate-500 text-sm mt-1">Quản lý nội dung, người dùng và hạ tầng AlgoSphere</p>
    </header>

    <!-- Error -->
    <div v-if="error" class="mb-6 p-3 rounded-lg text-sm text-red-400" style="background:rgba(239,68,68,0.08);border:1px solid rgba(239,68,68,0.2);">
      ⚠️ {{ error }}
    </div>

    <!-- Tabs -->
    <div class="flex items-center gap-1 mb-8 p-1 rounded-xl w-fit" style="background:#0A1628;border:1px solid rgba(255,255,255,0.06);">
      <button v-for="tab in tabs" :key="tab.key" @click="activeTab = tab.key"
        class="flex items-center gap-2 px-4 py-2 rounded-lg text-sm font-medium transition-all duration-200"
        :class="activeTab === tab.key ? 'text-slate-100' : 'text-slate-500 hover:text-slate-300'"
        :style="activeTab === tab.key ? 'background:rgba(255,255,255,0.08)' : ''">
        <component :is="tab.icon" class="w-4 h-4" />
        {{ tab.label }}
      </button>
    </div>

    <!-- ── Tab: Overview ── -->
    <div v-if="activeTab === 'overview'">
      <div v-if="loading" class="flex gap-1.5 justify-center py-20">
        <span v-for="i in 3" :key="i" class="w-2 h-2 rounded-full bg-emerald-500 animate-bounce"
          :style="`animation-delay:${(i - 1) * 0.15}s`"></span>
      </div>
      <div v-else-if="analytics" class="grid grid-cols-2 md:grid-cols-3 gap-4">
        <div v-for="stat in [
          { label: 'Tổng User', value: analytics.totalUsers, color: '#10B981' },
          { label: 'User Active', value: analytics.activeUsers, color: '#3B82F6' },
          { label: 'Bài Tập', value: analytics.totalExercises, color: '#F97316' },
          { label: 'Submissions', value: analytics.totalSubmissions, color: '#FBBF24' },
          { label: 'Accepted', value: analytics.acceptedSubmissions, color: '#10B981' },
          { label: 'Acceptance Rate', value: `${analytics.acceptanceRate}%`, color: '#A78BFA' },
        ]" :key="stat.label"
          class="p-5 rounded-2xl" style="background:#0A1628;border:1px solid rgba(255,255,255,0.06);">
          <div class="text-2xl font-bold mb-1" :style="`color:${stat.color}`">{{ stat.value }}</div>
          <div class="text-xs text-slate-500 uppercase tracking-wide font-medium">{{ stat.label }}</div>
        </div>
      </div>
    </div>

    <!-- ── Tab: Users ── -->
    <div v-else-if="activeTab === 'users'">
      <div class="flex items-center justify-between mb-4">
        <div class="flex items-center gap-2 px-3 py-2 rounded-lg" style="background:#0A1628;border:1px solid rgba(255,255,255,0.08);">
          <Search class="w-4 h-4 text-slate-500" />
          <input v-model="searchQuery" @keyup.enter="fetchUsers"
            placeholder="Tìm người dùng..." class="bg-transparent text-sm text-slate-200 outline-none placeholder-slate-600 w-48" />
        </div>
        <span class="text-xs text-slate-500">{{ users.length }} người dùng</span>
      </div>
      <div class="rounded-xl overflow-hidden" style="border:1px solid rgba(255,255,255,0.06);">
        <table class="w-full text-sm">
          <thead>
            <tr style="background:#0A1628;border-bottom:1px solid rgba(255,255,255,0.06);">
              <th class="text-left px-4 py-3 text-xs font-bold text-slate-500 uppercase">Tên</th>
              <th class="text-left px-4 py-3 text-xs font-bold text-slate-500 uppercase">Email</th>
              <th class="text-left px-4 py-3 text-xs font-bold text-slate-500 uppercase">Role</th>
              <th class="text-left px-4 py-3 text-xs font-bold text-slate-500 uppercase">XP</th>
              <th class="text-left px-4 py-3 text-xs font-bold text-slate-500 uppercase">Trạng thái</th>
              <th class="px-4 py-3"></th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="u in users" :key="u.id" style="border-bottom:1px solid rgba(255,255,255,0.04);"
              class="hover:bg-white/[0.02] transition-colors">
              <td class="px-4 py-3 text-slate-200 font-medium">{{ u.username }}</td>
              <td class="px-4 py-3 text-slate-500">{{ u.email }}</td>
              <td class="px-4 py-3">
                <span class="text-xs font-bold px-2 py-0.5 rounded"
                  :style="u.role === 'Admin' ? 'background:rgba(239,68,68,0.1);color:#EF4444' : 'background:rgba(16,185,129,0.1);color:#10B981'">
                  {{ u.role }}
                </span>
              </td>
              <td class="px-4 py-3 text-slate-400 font-mono-stat">{{ u.rankPoints.toLocaleString() }}</td>
              <td class="px-4 py-3">
                <span class="text-xs font-bold px-2 py-0.5 rounded"
                  :style="u.status === 'Active' ? 'background:rgba(16,185,129,0.1);color:#10B981' : 'background:rgba(239,68,68,0.1);color:#EF4444'">
                  {{ u.status }}
                </span>
              </td>
              <td class="px-4 py-3 flex items-center gap-2 justify-end">
                <button @click="setUserStatus(u.id, u.status === 'Active' ? 'Banned' : 'Active')"
                  class="text-slate-500 hover:text-slate-200 transition-colors" :title="u.status === 'Active' ? 'Ban user' : 'Activate user'">
                  <ShieldOff v-if="u.status === 'Active'" class="w-4 h-4 text-red-400" />
                  <ShieldCheck v-else class="w-4 h-4 text-emerald-400" />
                </button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <!-- ── Tab: Content ── -->
    <div v-else-if="activeTab === 'content'">
      <div class="flex items-center justify-between mb-4">
        <p class="text-slate-500 text-sm">{{ exercises.length }} bài tập</p>
      </div>
      <div class="space-y-3">
        <div v-for="ex in exercises" :key="ex.id"
          class="flex items-center justify-between p-4 rounded-xl hover:bg-white/[0.02] transition-all"
          style="background:#0A1628;border:1px solid rgba(255,255,255,0.06);">
          <div>
            <h3 class="font-bold text-slate-200 mb-1">{{ ex.title }}</h3>
            <div class="flex items-center gap-3 text-xs text-slate-500">
              <span>{{ ex.topicName }}</span>
              <span class="font-bold" :style="`color:${difficultyColor[ex.difficultyLevel] ?? '#94A3B8'}`">{{ ex.difficultyLevel }}</span>
              <span class="font-mono-stat">{{ ex.timeLimitMs }}ms</span>
              <span class="font-mono-stat">+{{ ex.points }}pts</span>
            </div>
          </div>
          <button @click="deleteExercise(ex.id)" class="p-2 rounded-lg text-slate-600 hover:text-red-400 hover:bg-red-400/10 transition-all">
            <Trash2 class="w-4 h-4" />
          </button>
        </div>
      </div>
    </div>

    <!-- ── Tab: Sandbox ── -->
    <div v-else-if="activeTab === 'sandbox'">
      <div class="mb-4 p-3 rounded-lg flex items-center gap-2 text-sm"
        style="background:rgba(251,191,36,0.08);border:1px solid rgba(251,191,36,0.2);color:#FBBF24;">
        <AlertCircle class="w-4 h-4 flex-shrink-0" />
        Docker containers chạy ephemeral — mỗi submission tạo container mới và xóa ngay sau khi hoàn thành.
      </div>
      <div class="grid md:grid-cols-2 gap-4">
        <div v-for="pool in sandboxPools" :key="pool.id" class="p-5 rounded-2xl"
          style="background:#0A1628;border:1px solid rgba(255,255,255,0.06);">
          <div class="flex items-start justify-between mb-4">
            <div>
              <h3 class="font-bold text-slate-200">{{ pool.language }}</h3>
              <p class="text-xs text-slate-600 font-mono-stat mt-0.5">{{ pool.image }}</p>
            </div>
            <span class="text-xs font-bold px-2 py-0.5 rounded"
              :style="pool.status === 'Running' ? 'background:rgba(16,185,129,0.1);color:#10B981' : 'background:rgba(239,68,68,0.1);color:#EF4444'">
              {{ pool.status }}
            </span>
          </div>
          <div class="grid grid-cols-2 gap-2 text-xs text-slate-500">
            <div>RAM: <span class="text-slate-300">{{ pool.memLimit }}</span></div>
            <div>CPU: <span class="text-slate-300">{{ pool.cpuLimit }}</span></div>
            <div>Network: <span class="text-red-400">disabled</span></div>
            <div>Filesystem: <span class="text-red-400">read-only</span></div>
          </div>
        </div>
      </div>
    </div>

    <!-- ── Tab: Badges ── -->
    <div v-else-if="activeTab === 'gamification'">
      <div class="space-y-3">
        <div v-for="b in badges" :key="b.id" class="flex items-center justify-between p-4 rounded-xl"
          style="background:#0A1628;border:1px solid rgba(255,255,255,0.06);">
          <div class="flex items-center gap-3">
            <div class="w-8 h-8 rounded-lg flex items-center justify-center" style="background:rgba(251,191,36,0.1);">
              <Star class="w-4 h-4 text-yellow-400" />
            </div>
            <div>
              <p class="font-bold text-slate-200 text-sm">{{ b.name }}</p>
              <p class="text-xs text-slate-500">{{ b.criteria }}</p>
            </div>
          </div>
          <div class="flex items-center gap-4">
            <span class="text-xs font-mono-stat font-bold" style="color:#FBBF24;">+{{ b.xpReward }} XP</span>
            <span class="text-xs px-2 py-0.5 rounded"
              :style="b.active ? 'background:rgba(16,185,129,0.1);color:#10B981' : 'background:rgba(255,255,255,0.04);color:#475569'">
              {{ b.active ? 'Active' : 'Inactive' }}
            </span>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>
