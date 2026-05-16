<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { BarChart3, Users, CheckCircle2, TrendingUp, ShieldCheck, Trophy, Zap } from 'lucide-vue-next'
import { API_BASE } from '../utils/api'

const analytics = ref<any>(null)
const loading = ref(true)
const orgId = ref<number | null>(null)

onMounted(async () => {
  const token = localStorage.getItem('token')
  try {
    // 1. Get User Profile to find OrgId
    const profileRes = await fetch(`${API_BASE}/users/me`, {
      headers: { 'Authorization': `Bearer ${token}` }
    })
    if (!profileRes.ok) throw new Error('Unauthorized')
    const profile = await profileRes.json()
    
    // In our system, OrganizationId might be in profile.organizationId
    // If not present, we fallback to 1 for dev/demo purposes
    orgId.value = profile.organizationId || 1

    // 2. Fetch real analytics
    const res = await fetch(`${API_BASE}/b2b/analytics/${orgId.value}`, {
      headers: { 'Authorization': `Bearer ${token}` }
    })
    if (res.ok) {
      analytics.value = await res.json()
    }
  } catch (err) {
    console.error('Failed to load B2B Analytics:', err)
  } finally {
    loading.value = false
  }
})

const statCards = [
  { key: 'memberCount',        label: 'Thành Viên',      icon: Users,        color: '#10B981' },
  { key: 'totalSubmissions',   label: 'Tổng Submissions', icon: BarChart3,    color: '#3B82F6' },
  { key: 'averageSuccessRate', label: 'Tỷ Lệ Thành Công', icon: CheckCircle2, color: '#A78BFA', suffix: '%' },
]
</script>

<template>
  <div class="p-8 max-w-7xl mx-auto">

    <!-- Header -->
    <header class="mb-10 flex items-end justify-between">
      <div>
        <p class="text-xs font-bold tracking-[0.25em] uppercase mb-2" style="color: #10B981;">ENTERPRISE CONSOLE</p>
        <h1 class="text-4xl font-bold text-slate-100 mb-2">B2B Dashboard</h1>
        <p class="text-slate-500 text-sm">Hệ thống quản trị và phân tích dành cho đối tác tổ chức.</p>
      </div>
      <button class="flex items-center gap-2 px-4 py-2.5 rounded-xl font-bold text-sm transition-all duration-200 cursor-pointer"
        style="background: rgba(16,185,129,0.1); color: #10B981; border: 1px solid rgba(16,185,129,0.2);">
        <ShieldCheck class="w-4 h-4" /> Exam Mode
      </button>
    </header>

    <!-- Loading -->
    <div v-if="loading" class="flex justify-center py-20">
      <div class="w-8 h-8 border-2 rounded-full animate-spin" style="border-color: rgba(16,185,129,0.2); border-top-color: #10B981;"></div>
    </div>

    <div v-else class="space-y-8">

      <!-- Stats grid -->
      <div class="grid grid-cols-1 md:grid-cols-3 gap-5">
        <div v-for="stat in statCards" :key="stat.key"
          class="shine rounded-2xl p-6 transition-all duration-300 cursor-pointer relative overflow-hidden"
          style="background: #080F1C; border: 1px solid rgba(255,255,255,0.06);"
          @mouseenter="($event.currentTarget as HTMLElement).style.borderColor = stat.color + '40'"
          @mouseleave="($event.currentTarget as HTMLElement).style.borderColor = 'rgba(255,255,255,0.06)'">
          <div class="flex items-start justify-between mb-5">
            <div class="p-3 rounded-xl" :style="`background: ${stat.color}15; border: 1px solid ${stat.color}25;`">
              <component :is="stat.icon" class="w-5 h-5" :style="`color: ${stat.color}`" />
            </div>
            <div class="flex items-center gap-1 text-xs font-bold" style="color: #10B981;">
              <TrendingUp class="w-3 h-3" /> +12%
            </div>
          </div>
          <div class="font-mono-stat text-3xl font-bold text-slate-100 mb-1">
            {{ analytics[stat.key] }}{{ stat.suffix ?? '' }}
          </div>
          <div class="text-xs text-slate-500 font-bold uppercase tracking-wide">{{ stat.label }}</div>
        </div>
      </div>

      <!-- Main grid -->
      <div class="grid grid-cols-1 lg:grid-cols-3 gap-6">

        <!-- Top Students -->
        <div class="lg:col-span-2 rounded-2xl p-6" style="background: #080F1C; border: 1px solid rgba(255,255,255,0.06);">
          <div class="flex items-center justify-between mb-6">
            <div class="flex items-center gap-3">
              <div class="p-2 rounded-xl" style="background: rgba(251,191,36,0.1); border: 1px solid rgba(251,191,36,0.2);">
                <Trophy class="w-4 h-4 text-yellow-400" />
              </div>
              <div>
                <h3 class="font-bold text-slate-100">Bảng Vinh Danh</h3>
                <p class="text-xs text-slate-500">Top performers tháng này</p>
              </div>
            </div>
            <span class="text-[10px] font-bold px-2 py-0.5 rounded-full font-mono-stat" style="background: rgba(16,185,129,0.1); color: #10B981;">THIS MONTH</span>
          </div>

          <div class="space-y-2">
            <div v-for="(s, i) in analytics.topStudents" :key="i"
              class="flex items-center gap-4 p-3.5 rounded-xl transition-all duration-200 cursor-pointer group"
              style="background: rgba(255,255,255,0.02);"
              @mouseenter="($event.currentTarget as HTMLElement).style.background = 'rgba(16,185,129,0.05)'"
              @mouseleave="($event.currentTarget as HTMLElement).style.background = 'rgba(255,255,255,0.02)'">

              <!-- Rank -->
              <div class="w-8 h-8 rounded-lg flex items-center justify-center text-xs font-bold flex-shrink-0 font-mono-stat"
                :style="Number(i) === 0
                  ? 'background: rgba(251,191,36,0.15); color: #FBBF24; border: 1px solid rgba(251,191,36,0.3);'
                  : Number(i) === 1
                    ? 'background: rgba(148,163,184,0.1); color: #94A3B8; border: 1px solid rgba(148,163,184,0.2);'
                    : 'background: rgba(249,115,22,0.1); color: #F97316; border: 1px solid rgba(249,115,22,0.2);'"
              >
                {{ Number(i) + 1 }}
              </div>

              <!-- Avatar -->
              <div class="w-8 h-8 rounded-xl flex items-center justify-center font-bold text-xs flex-shrink-0"
                style="background: linear-gradient(135deg, rgba(16,185,129,0.3), rgba(59,130,246,0.3)); color: #10B981;">
                {{ s.username?.charAt(0) }}
              </div>

              <span class="flex-1 font-medium text-sm text-slate-300 group-hover:text-slate-100 transition-colors">{{ s.username }}</span>

              <div class="font-mono-stat text-sm font-bold text-emerald-400">{{ s.score?.toLocaleString() }} PTS</div>
            </div>
          </div>
        </div>

        <!-- Right column -->
        <div class="space-y-5">
          <!-- Exam Mode Card -->
          <div class="rounded-2xl p-6 relative overflow-hidden"
            style="background: linear-gradient(135deg, #0A1628, #0D1F3C); border: 1px solid rgba(59,130,246,0.25);">
            <div class="absolute top-0 right-0 w-32 h-32 rounded-full opacity-10"
              style="background: radial-gradient(circle, #3B82F6, transparent); transform: translate(30%, -30%);"></div>
            <div class="p-2.5 rounded-xl w-fit mb-4" style="background: rgba(59,130,246,0.15); border: 1px solid rgba(59,130,246,0.25);">
              <Zap class="w-5 h-5 text-blue-400" />
            </div>
            <h4 class="font-bold text-slate-100 mb-1">EXAM MODE</h4>
            <p class="text-xs text-slate-500 mb-5 leading-relaxed">Khóa AI và Internet cho kỳ thi tập trung của tổ chức.</p>
            <button class="w-full py-2.5 rounded-xl text-sm font-bold cursor-pointer transition-all duration-200"
              style="background: rgba(59,130,246,0.15); color: #60A5FA; border: 1px solid rgba(59,130,246,0.3);"
              @mouseenter="($event.currentTarget as HTMLElement).style.background = '#3B82F6'; ($event.currentTarget as HTMLElement).style.color = '#fff';"
              @mouseleave="($event.currentTarget as HTMLElement).style.background = 'rgba(59,130,246,0.15)'; ($event.currentTarget as HTMLElement).style.color = '#60A5FA';">
              KÍCH HOẠT NGAY
            </button>
          </div>

          <!-- Monthly Growth -->
          <div class="rounded-2xl p-5" style="background: #080F1C; border: 1px solid rgba(255,255,255,0.06);">
            <div class="flex items-center gap-2 mb-4">
              <TrendingUp class="w-4 h-4 text-emerald-400" />
              <span class="text-sm font-bold text-slate-300">Tăng trưởng tháng</span>
            </div>
            <div class="font-mono-stat text-3xl font-bold text-emerald-400 mb-1">+12.5%</div>
            <p class="text-xs text-slate-500">So với tháng trước</p>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>
