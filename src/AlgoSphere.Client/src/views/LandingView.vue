<script setup lang="ts">
import { ref, onMounted, onBeforeUnmount } from 'vue'
import { useRouter } from 'vue-router'
import { Zap, Code2, Trophy, Brain, Swords, ArrowRight, ChevronRight } from 'lucide-vue-next'

const router = useRouter()

// Animated counter
const stats = ref([
  { label: 'Sinh viên', value: 0, target: 12400, suffix: '+' },
  { label: 'Bài tập', value: 0, target: 480, suffix: '' },
  { label: 'Thuật toán', value: 0, target: 60, suffix: '+' },
  { label: 'Trận đấu/ngày', value: 0, target: 3200, suffix: '+' },
])

const features = [
  {
    icon: Code2,
    title: 'Visualizer Engine',
    desc: 'Trực quan hóa thuật toán real-time với D3.js. Xem từng bước thực thi, so sánh, hoán đổi.',
    color: '#10B981',
  },
  {
    icon: Brain,
    title: 'AI Tutor Mentor',
    desc: 'Gemini AI phân tích code của bạn, gợi ý hướng đi mà không giải hộ toàn bộ.',
    color: '#3B82F6',
  },
  {
    icon: Swords,
    title: 'Code Arena',
    desc: '1v1 đấu thuật toán real-time. Xếp hạng Elo, mùa giải, anti-cheat Record/Replay.',
    color: '#F97316',
  },
  {
    icon: Trophy,
    title: 'Gamification',
    desc: 'Skill Tree, Huy chương, Daily Quest. Học theo lộ trình có thưởng — không nhàm chán.',
    color: '#FBBF24',
  },
]

// Sort demo animation
const sortBars = ref([64, 34, 25, 12, 22, 11, 90])
const activeBars = ref<number[]>([])
let sortInterval: ReturnType<typeof setTimeout>

const runSortDemo = () => {
  const arr = [...sortBars.value]
  const steps: { swap?: [number, number]; compare: [number, number] }[] = []

  // Generate bubble sort steps
  for (let i = 0; i < arr.length; i++) {
    for (let j = 0; j < arr.length - i - 1; j++) {
      steps.push({ compare: [j, j + 1] })
      if (arr[j] > arr[j + 1]) {
        steps.push({ swap: [j, j + 1], compare: [j, j + 1] })
        ;[arr[j], arr[j + 1]] = [arr[j + 1], arr[j]]
      }
    }
  }

  let si = 0
  const localArr = [...sortBars.value]

  const tick = () => {
    if (si >= steps.length) {
      activeBars.value = []
      setTimeout(resetDemo, 1500)
      return
    }
    const step = steps[si++]
    activeBars.value = [...step.compare]
    if (step.swap) {
      ;[localArr[step.swap[0]], localArr[step.swap[1]]] = [localArr[step.swap[1]], localArr[step.swap[0]]]
      sortBars.value = [...localArr]
    }
    sortInterval = setTimeout(tick, 380)
  }
  tick()
}

const resetDemo = () => {
  sortBars.value = [64, 34, 25, 12, 22, 11, 90]
  activeBars.value = []
  setTimeout(runSortDemo, 600)
}

onMounted(() => {
  // Animate counters
  stats.value.forEach((s, i) => {
    const duration = 1800
    const steps = 60
    const increment = s.target / steps
    let current = 0
    const timer = setInterval(() => {
      current = Math.min(current + increment, s.target)
      stats.value[i].value = Math.floor(current)
      if (current >= s.target) clearInterval(timer)
    }, duration / steps)
  })

  setTimeout(runSortDemo, 800)
})

onBeforeUnmount(() => {
  clearTimeout(sortInterval)
})
</script>

<template>
  <div class="min-h-screen overflow-x-hidden" style="background:#020408; color:#E2E8F0;">

    <!-- ── Navbar ── -->
    <nav class="fixed top-0 left-0 right-0 z-50 flex items-center justify-between px-8 h-16"
      style="background:rgba(2,4,8,0.85); backdrop-filter:blur(12px); border-bottom:1px solid rgba(16,185,129,0.1);">
      <div class="flex items-center gap-3">
        <div class="w-8 h-8 rounded-lg flex items-center justify-center" style="background:linear-gradient(135deg,#10B981,#059669);">
          <Zap class="w-4 h-4 text-white fill-current" />
        </div>
        <span class="font-bold text-lg tracking-tight" style="color:#10B981;">ALGOSPHERE</span>
        <span class="text-[9px] font-bold tracking-[0.2em] text-slate-600 uppercase hidden sm:block">Enterprise</span>
      </div>

      <div class="flex items-center gap-3">
        <router-link to="/leaderboard"
          class="text-sm text-slate-400 hover:text-slate-200 transition-colors hidden md:block px-4 py-2">
          Xếp hạng
        </router-link>
        <router-link to="/login"
          class="text-sm text-slate-400 hover:text-slate-200 transition-colors px-4 py-2">
          Đăng nhập
        </router-link>
        <router-link to="/login"
          class="text-sm font-bold px-4 py-2 rounded-lg transition-all"
          style="background:#10B981; color:#fff;"
          @mouseenter="($event.target as HTMLElement).style.background='#059669'"
          @mouseleave="($event.target as HTMLElement).style.background='#10B981'">
          Bắt đầu miễn phí
        </router-link>
      </div>
    </nav>

    <!-- ── Hero ── -->
    <section class="min-h-screen flex flex-col items-center justify-center text-center px-6 pt-16 relative overflow-hidden">
      <!-- Background grid -->
      <div class="absolute inset-0 pointer-events-none"
        style="background-image:linear-gradient(rgba(16,185,129,0.03) 1px,transparent 1px),linear-gradient(90deg,rgba(16,185,129,0.03) 1px,transparent 1px);background-size:60px 60px;">
      </div>
      <!-- Radial glow -->
      <div class="absolute top-1/2 left-1/2 -translate-x-1/2 -translate-y-1/2 w-[600px] h-[600px] rounded-full pointer-events-none"
        style="background:radial-gradient(circle,rgba(16,185,129,0.06) 0%,transparent 70%);"></div>

      <div class="relative z-10 max-w-4xl">
        <div class="inline-flex items-center gap-2 px-3 py-1.5 rounded-full text-xs font-bold mb-6"
          style="background:rgba(16,185,129,0.08);border:1px solid rgba(16,185,129,0.2);color:#10B981;">
          <span class="w-1.5 h-1.5 rounded-full bg-emerald-400 animate-pulse"></span>
          Nền tảng Visualizer & Code Arena #1 Việt Nam
        </div>

        <h1 class="font-bold leading-none mb-6" style="font-size:clamp(2.5rem,8vw,5rem);">
          Thấu hiểu<br/>
          <span style="background:linear-gradient(90deg,#10B981 0%,#34D399 40%,#3B82F6 100%);-webkit-background-clip:text;background-clip:text;-webkit-text-fill-color:transparent;">
            Thuật Toán
          </span><br/>
          Từ Bên Trong
        </h1>

        <p class="text-slate-400 text-lg mb-10 max-w-2xl mx-auto leading-relaxed">
          Không chỉ đọc lý thuyết — <strong class="text-slate-200">xem thuật toán chạy từng dòng code</strong>,
          đấu 1v1 với bạn bè, và có AI Mentor hướng dẫn bạn 24/7.
        </p>

        <div class="flex items-center justify-center gap-4 flex-wrap">
          <button @click="router.push('/login')"
            class="flex items-center gap-2 px-6 py-3 rounded-xl font-bold text-base transition-all"
            style="background:linear-gradient(135deg,#10B981,#059669);color:#fff;box-shadow:0 0 30px rgba(16,185,129,0.3);"
            @mouseenter="($event.target as HTMLElement).style.transform='translateY(-2px)'"
            @mouseleave="($event.target as HTMLElement).style.transform='translateY(0)'">
            Học miễn phí ngay <ArrowRight class="w-4 h-4" />
          </button>
          <router-link to="/leaderboard"
            class="flex items-center gap-2 px-6 py-3 rounded-xl font-bold text-base transition-all text-slate-300"
            style="border:1px solid rgba(255,255,255,0.1);"
            @mouseenter="($event.currentTarget as HTMLElement).style.borderColor='rgba(16,185,129,0.3)'"
            @mouseleave="($event.currentTarget as HTMLElement).style.borderColor='rgba(255,255,255,0.1)'">
            Xem bảng xếp hạng <ChevronRight class="w-4 h-4" />
          </router-link>
        </div>
      </div>
    </section>

    <!-- ── Live Sort Demo ── -->
    <section class="py-20 px-6">
      <div class="max-w-3xl mx-auto">
        <div class="text-center mb-10">
          <p class="text-xs font-bold tracking-[0.3em] uppercase text-emerald-500 mb-2">LIVE DEMO</p>
          <h2 class="text-2xl font-bold text-slate-100">Visualizer Engine đang chạy</h2>
          <p class="text-slate-500 text-sm mt-2">Bubble Sort — mỗi cột là một phần tử trong mảng</p>
        </div>

        <div class="rounded-2xl p-6 relative" style="background:#060D16;border:1px solid rgba(16,185,129,0.12);">
          <!-- Action badge -->
          <div class="flex items-center gap-2 mb-4">
            <span v-if="activeBars.length" class="text-[11px] font-bold px-2.5 py-1 rounded font-mono-stat animate-pulse"
              style="background:rgba(16,185,129,0.15);color:#10B981;border:1px solid rgba(16,185,129,0.3);">
              ⚖ COMPARING [{{ activeBars.join(', ') }}]
            </span>
            <span class="text-[10px] text-slate-600 font-mono-stat ml-auto">BUBBLE SORT · LIVE</span>
          </div>

          <!-- Bars -->
          <div class="flex items-end justify-center gap-2" style="height:160px;">
            <div v-for="(val, i) in sortBars" :key="i"
              class="rounded-t-md transition-all duration-300 flex-1 relative"
              :style="{
                height: `${(val / 100) * 140}px`,
                background: activeBars.includes(i)
                  ? 'linear-gradient(180deg,#10B981,#059669)'
                  : '#1E293B',
                boxShadow: activeBars.includes(i) ? '0 0 15px rgba(16,185,129,0.4)' : 'none',
                border: activeBars.includes(i) ? '1px solid #10B981' : '1px solid transparent',
              }">
              <span class="absolute -top-5 left-1/2 -translate-x-1/2 text-[11px] font-mono-stat font-bold"
                :style="{ color: activeBars.includes(i) ? '#10B981' : '#475569' }">
                {{ val }}
              </span>
            </div>
          </div>
        </div>
      </div>
    </section>

    <!-- ── Stats ── -->
    <section class="py-16 px-6" style="border-top:1px solid rgba(255,255,255,0.04);border-bottom:1px solid rgba(255,255,255,0.04);">
      <div class="max-w-4xl mx-auto grid grid-cols-2 md:grid-cols-4 gap-8 text-center">
        <div v-for="s in stats" :key="s.label">
          <div class="text-3xl font-bold font-mono-stat mb-1" style="color:#10B981;">
            {{ s.value.toLocaleString() }}{{ s.suffix }}
          </div>
          <div class="text-sm text-slate-500 font-medium">{{ s.label }}</div>
        </div>
      </div>
    </section>

    <!-- ── Features ── -->
    <section class="py-20 px-6">
      <div class="max-w-5xl mx-auto">
        <div class="text-center mb-14">
          <h2 class="text-3xl font-bold text-slate-100 mb-3">Hệ sinh thái học thuật toán</h2>
          <p class="text-slate-500">Từ người mới đến Pro — mọi thứ bạn cần đều ở đây</p>
        </div>

        <div class="grid md:grid-cols-2 gap-5">
          <div v-for="f in features" :key="f.title"
            class="rounded-2xl p-6 transition-all duration-300 group cursor-default"
            style="background:#060D16;border:1px solid rgba(255,255,255,0.06);"
            @mouseenter="($event.currentTarget as HTMLElement).style.borderColor = f.color + '40'"
            @mouseleave="($event.currentTarget as HTMLElement).style.borderColor = 'rgba(255,255,255,0.06)'">
            <div class="w-10 h-10 rounded-xl flex items-center justify-center mb-4 transition-all duration-300"
              :style="`background:${f.color}15;`">
              <component :is="f.icon" class="w-5 h-5" :style="`color:${f.color}`" />
            </div>
            <h3 class="font-bold text-slate-100 mb-2">{{ f.title }}</h3>
            <p class="text-slate-500 text-sm leading-relaxed">{{ f.desc }}</p>
          </div>
        </div>
      </div>
    </section>

    <!-- ── CTA ── -->
    <section class="py-20 px-6 text-center">
      <div class="max-w-xl mx-auto">
        <h2 class="text-2xl font-bold text-slate-100 mb-4">Sẵn sàng chinh phục thuật toán?</h2>
        <p class="text-slate-500 mb-8">Đăng ký miễn phí — không cần thẻ tín dụng.</p>
        <button @click="router.push('/login')"
          class="px-8 py-3 rounded-xl font-bold text-base transition-all"
          style="background:linear-gradient(135deg,#10B981,#059669);color:#fff;box-shadow:0 0 30px rgba(16,185,129,0.25);">
          Bắt đầu học ngay →
        </button>
      </div>
    </section>

    <!-- ── Footer ── -->
    <footer class="py-8 px-6 text-center text-slate-600 text-sm" style="border-top:1px solid rgba(255,255,255,0.04);">
      <p>© 2026 AlgoSphere Enterprise · Được xây dựng với ❤️ bởi sinh viên Việt Nam</p>
    </footer>
  </div>
</template>
