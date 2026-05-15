<script setup lang="ts">
import { ref, computed, onMounted, onBeforeUnmount } from 'vue'
import { useRoute } from 'vue-router'
import { API_BASE } from '../utils/api'
import { Play, Pause, RotateCcw, ChevronLeft, ChevronRight, Save, Sparkles, Send, X, BookOpen } from 'lucide-vue-next'
import * as d3 from 'd3'
import { TracePlayer, type TraceLog, type TraceStep } from '../utils/TracePlayer'
import MonacoEditor from '../components/MonacoEditor.vue'

// ── Solution Modal ────────────────────────────────────────────────
const showSolutionModal = ref(false)

interface SolutionData { code: string; steps: string[]; complexity: string; spaceComplexity: string }

const SOLUTIONS: Record<string, SolutionData> = {
  'two sum': {
    complexity: 'O(n)', spaceComplexity: 'O(n)',
    code: `function twoSum(nums, target) {
  const map = new Map();
  for (let i = 0; i < nums.length; i++) {
    const complement = target - nums[i];
    if (map.has(complement)) {
      return [map.get(complement), i];
    }
    map.set(nums[i], i);
  }
  return [];
}`,
    steps: [
      '1️⃣ Khởi tạo một HashMap (Map) trống để lưu giá trị → chỉ số.',
      '2️⃣ Duyệt từng phần tử nums[i] trong mảng.',
      '3️⃣ Tính complement = target - nums[i].',
      '4️⃣ Nếu complement đã có trong Map → trả về cặp [map.get(complement), i].',
      '5️⃣ Nếu chưa có → thêm nums[i] vào Map với chỉ số i và tiếp tục.',
    ]
  },
  'contains duplicate': {
    complexity: 'O(n)', spaceComplexity: 'O(n)',
    code: `function containsDuplicate(nums) {
  const seen = new Set();
  for (const n of nums) {
    if (seen.has(n)) return true;
    seen.add(n);
  }
  return false;
}`,
    steps: [
      '1️⃣ Dùng Set để lưu các số đã gặp.',
      '2️⃣ Với mỗi số n, kiểm tra xem Set đã chứa n chưa.',
      '3️⃣ Nếu có → trả về true (có phần tử trùng).',
      '4️⃣ Nếu chưa → thêm n vào Set và tiếp tục.',
      '5️⃣ Duyệt hết mà không tìm thấy → trả về false.',
    ]
  },
  'bubble sort': {
    complexity: 'O(n²)', spaceComplexity: 'O(1)',
    code: `function bubbleSort(arr) {
  const n = arr.length;
  for (let i = 0; i < n; i++) {
    for (let j = 0; j < n - i - 1; j++) {
      if (arr[j] > arr[j + 1]) {
        [arr[j], arr[j + 1]] = [arr[j + 1], arr[j]];
      }
    }
  }
  return arr;
}`,
    steps: [
      '1️⃣ Duyệt mảng n lần (vòng ngoài i).',
      '2️⃣ Trong mỗi lần, duyệt từ đầu đến phần tử chưa sắp xếp (vòng trong j).',
      '3️⃣ So sánh arr[j] với arr[j+1].',
      '4️⃣ Nếu arr[j] > arr[j+1] → hoán đổi (swap).',
      '5️⃣ Phần tử lớn nhất sẽ "nổi" dần về cuối sau mỗi lượt.',
    ]
  },
  'binary search': {
    complexity: 'O(log n)', spaceComplexity: 'O(1)',
    code: `function binarySearch(arr, target) {
  let left = 0, right = arr.length - 1;
  while (left <= right) {
    const mid = Math.floor((left + right) / 2);
    if (arr[mid] === target) return mid;
    if (arr[mid] < target) left = mid + 1;
    else right = mid - 1;
  }
  return -1;
}`,
    steps: [
      '1️⃣ Khởi tạo con trỏ left = 0, right = n-1.',
      '2️⃣ Tính mid = (left + right) / 2.',
      '3️⃣ Nếu arr[mid] == target → tìm thấy, trả về mid.',
      '4️⃣ Nếu arr[mid] < target → tìm nửa phải, left = mid + 1.',
      '5️⃣ Nếu arr[mid] > target → tìm nửa trái, right = mid - 1.',
    ]
  },
  'valid palindrome': {
    complexity: 'O(n)', spaceComplexity: 'O(1)',
    code: `function isPalindrome(s) {
  s = s.toLowerCase().replace(/[^a-z0-9]/g, '');
  let left = 0, right = s.length - 1;
  while (left < right) {
    if (s[left] !== s[right]) return false;
    left++; right--;
  }
  return true;
}`,
    steps: [
      '1️⃣ Chuẩn hóa: chuyển về lowercase, loại bỏ ký tự đặc biệt.',
      '2️⃣ Đặt con trỏ left ở đầu, right ở cuối chuỗi.',
      '3️⃣ So sánh s[left] với s[right].',
      '4️⃣ Nếu khác nhau → không phải palindrome, trả về false.',
      '5️⃣ Nếu giống nhau → tiến left lên, lùi right về cho đến khi gặp nhau.',
    ]
  },
  'climbing stairs': {
    complexity: 'O(n)', spaceComplexity: 'O(1)',
    code: `function climbStairs(n) {
  if (n <= 2) return n;
  let a = 1, b = 2;
  for (let i = 3; i <= n; i++) {
    [a, b] = [b, a + b];
  }
  return b;
}`,
    steps: [
      '1️⃣ Base case: n=1 → 1 cách, n=2 → 2 cách.',
      '2️⃣ Nhận ra: f(n) = f(n-1) + f(n-2) (giống Fibonacci).',
      '3️⃣ Dùng 2 biến a, b thay vì mảng để tiết kiệm bộ nhớ O(1).',
      '4️⃣ Mỗi bước: a = b (bước trước), b = a + b (bước hiện tại).',
      '5️⃣ Sau n-2 bước lặp, b chính là đáp án.',
    ]
  },
  'single number': {
    complexity: 'O(n)', spaceComplexity: 'O(1)',
    code: `function singleNumber(nums) {
  return nums.reduce((xor, n) => xor ^ n, 0);
}`,
    steps: [
      '1️⃣ Tính chất XOR: a ^ a = 0, a ^ 0 = a.',
      '2️⃣ Mọi số xuất hiện 2 lần sẽ triệt tiêu nhau khi XOR.',
      '3️⃣ Chỉ còn lại số xuất hiện 1 lần.',
      '4️⃣ Dùng reduce để XOR toàn bộ mảng trong 1 dòng.',
    ]
  },
}

const currentSolution = computed<SolutionData | null>(() => {
  if (!exercise.value?.title) return null
  const key = exercise.value.title.toLowerCase()
  return SOLUTIONS[key] ?? null
})

const applySolution = () => {
  if (!currentSolution.value) return
  code.value = currentSolution.value.code
  showSolutionModal.value = false
}

const route = useRoute()
const exercise = ref<any>(null)
const code = ref('// Viết thuật toán Bubble Sort của bạn...\nfunction bubbleSort(arr) {\n  for (let i = 0; i < arr.length; i++) {\n    for (let j = 0; j < arr.length - i - 1; j++) {\n      if (arr[j] > arr[j + 1]) {\n        [arr[j], arr[j + 1]] = [arr[j + 1], arr[j]]\n      }\n    }\n  }\n  return arr\n}')
const loading = ref(false)
const aiLoading = ref(false)
const userAiInput = ref('')
const currentStep = ref<TraceStep | null>(null)
const currentIndex = ref(-1)
const totalSteps = ref(0)
const visualizerData = ref<number[]>([])
const highlightLine = ref<number | null>(null)
// Full TraceLog — passed to AI for rich context
const lastTraceLog = ref<TraceLog | null>(null)
const aiMessages = ref<{ role: 'ai' | 'user'; text: string }[]>([
  { role: 'ai', text: 'Chào bạn! Tôi là AI Mentor. Viết thuật toán → Run Code để bắt đầu trực quan hóa. Sau đó hỏi tôi bất cứ điều gì! 💡' },
])

const SPEEDS = [0.25, 0.5, 1, 2, 4]
const speedIdx = ref(2) // default 1x
const currentSpeed = computed(() => SPEEDS[speedIdx.value])
const isPlaying = ref(false)

let player: TracePlayer | null = null

// ── D3 Visualizer ────────────────────────────────────────────────
const svgRef = ref<SVGSVGElement | null>(null)

const ACTION_COLORS: Record<string, string> = {
  cmp: '#F97316',  // orange — comparing
  swp: '#10B981',  // emerald — swapping
  vis: '#3B82F6',  // blue — visiting
  fnd: '#FBBF24',  // gold — found
  done: '#6EE7B7', // light emerald — sorted
}

const renderVisualizer = (data: number[], step: TraceStep | null) => {
  if (!svgRef.value || !data.length) return
  const svg = d3.select(svgRef.value)
  svg.selectAll('*').remove()

  const W = svgRef.value.clientWidth || 600
  const H = svgRef.value.clientHeight || 280

  const x = d3.scaleBand()
    .domain(data.map((_, i) => i.toString()))
    .range([12, W - 12])
    .padding(0.15)

  const y = d3.scaleLinear()
    .domain([0, (d3.max(data) ?? 100) * 1.15])
    .range([H - 28, 8])

  const getColor = (i: number) => {
    if (!step) return '#1E293B'
    if (step.t.includes(i)) return ACTION_COLORS[step.a] ?? '#10B981'
    return '#1E293B'
  }

  const bars = svg.selectAll('g.bar')
    .data(data)
    .enter()
    .append('g')
    .attr('class', 'bar')

  // Shadow glow for active bars
  const defs = svg.append('defs')
  const filter = defs.append('filter').attr('id', 'bar-glow')
  filter.append('feGaussianBlur').attr('stdDeviation', '4').attr('result', 'coloredBlur')
  const merge = filter.append('feMerge')
  merge.append('feMergeNode').attr('in', 'coloredBlur')
  merge.append('feMergeNode').attr('in', 'SourceGraphic')

  bars.append('rect')
    .attr('x', (_, i) => x(i.toString())!)
    .attr('y', d => y(d))
    .attr('width', x.bandwidth())
    .attr('height', d => H - 28 - y(d))
    .attr('rx', 4)
    .attr('fill', (_, i) => getColor(i))
    .attr('filter', (_, i) => step?.t.includes(i) ? 'url(#bar-glow)' : '')
    .attr('stroke', (_, i) => step?.t.includes(i) ? ACTION_COLORS[step.a] ?? '#10B981' : 'none')
    .attr('stroke-width', 1.5)

  bars.append('text')
    .attr('x', (_, i) => x(i.toString())! + x.bandwidth() / 2)
    .attr('y', d => y(d) - 6)
    .attr('text-anchor', 'middle')
    .attr('fill', (_, i) => step?.t.includes(i) ? '#FFF' : '#475569')
    .attr('font-size', '11px')
    .attr('font-family', 'JetBrains Mono, monospace')
    .attr('font-weight', '700')
    .text(d => d)
}

// ── Run Code ─────────────────────────────────────────────────────
const handleRunCode = async () => {
  loading.value = true
  player?.pause()
  isPlaying.value = false

  try {
    const token = localStorage.getItem('token')
    const res = await fetch(`${API_BASE}/exercises/execute`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${token}`
      },
      body: JSON.stringify({
        exerciseId: Number(route.params.id),
        code: code.value,
        language: 'javascript',
      }),
    })
    const result = await res.json()
    const log: TraceLog = JSON.parse(result.traceLog)
    lastTraceLog.value = log  // Store full log for AI context

    player = TracePlayer.create(log, (state, step, idx, total) => {
      visualizerData.value = state
      currentStep.value = step
      currentIndex.value = idx
      totalSteps.value = total
      highlightLine.value = step?.l ?? null
      renderVisualizer(state, step)
      isPlaying.value = player?.isPlaying ?? false
    })

    player.setSpeed(currentSpeed.value)
    visualizerData.value = log.initialState
    totalSteps.value = log.trace.length
    renderVisualizer(log.initialState, null)

    // Auto-prompt AI after successful run
    aiMessages.value.push({
      role: 'ai',
      text: `✅ Code đã chạy! Tôi đã phân tích ${log.trace.length} bước thực thi trên mảng [${log.initialState.join(', ')}]. Bạn muốn tôi giải thích bước nào?`
    })
  } catch (err) {
    console.error(err)
    aiMessages.value.push({ role: 'ai', text: '⚠️ Lỗi kết nối server. Kiểm tra backend đã chạy chưa?' })
  } finally {
    loading.value = false
  }
}

// ── Playback controls ─────────────────────────────────────────────
const togglePlay = () => {
  if (!player) return
  player.togglePlay()
  isPlaying.value = player.isPlaying
}

const stepBack = () => { player?.stepBackward(); isPlaying.value = false }
const stepFwd  = () => { player?.stepForward();  isPlaying.value = false }
const resetPlayer = () => { player?.reset(); isPlaying.value = false; highlightLine.value = null }

const cycleSpeed = () => {
  speedIdx.value = (speedIdx.value + 1) % SPEEDS.length
  player?.setSpeed(currentSpeed.value)
}

const seekProgress = (e: MouseEvent) => {
  if (!player || totalSteps.value === 0) return
  const bar = e.currentTarget as HTMLElement
  const ratio = e.offsetX / bar.clientWidth
  const idx = Math.floor(ratio * totalSteps.value) - 1
  player.seekTo(idx)
}

const progressPct = computed(() => {
  if (totalSteps.value === 0) return 0
  return ((currentIndex.value + 1) / totalSteps.value) * 100
})

// ── AI Tutor ─────────────────────────────────────────────────────
const aiPanelOpen = ref(true)
const aiChatRef = ref<HTMLDivElement | null>(null)

const sendAiMessage = async () => {
  const msg = userAiInput.value.trim() || 'Cho tôi gợi ý về bài tập này.'
  userAiInput.value = ''
  aiMessages.value.push({ role: 'user', text: msg })

  const aiMsgIdx = aiMessages.value.length
  aiMessages.value.push({ role: 'ai', text: '' })
  aiLoading.value = true

  // Pass full TraceLog for rich AI context; fallback to visualizer array
  const stateJson = lastTraceLog.value
    ? JSON.stringify(lastTraceLog.value)
    : JSON.stringify({ initialState: visualizerData.value })

  try {
    const token = localStorage.getItem('token')
    const res = await fetch(`${API_BASE}/ai/hint/stream`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        ...(token ? { 'Authorization': `Bearer ${token}` } : {})
      },
      body: JSON.stringify({
        currentCode: code.value,
        stateJson,
        errorMessage: msg,
      }),
    })

    if (!res.ok || !res.body) throw new Error('SSE stream failed')

    const reader = res.body.getReader()
    const decoder = new TextDecoder()
    let buffer = ''

    while (true) {
      const { done, value } = await reader.read()
      if (done) break
      buffer += decoder.decode(value, { stream: true })

      // Parse SSE lines
      const lines = buffer.split('\n')
      buffer = lines.pop() ?? ''

      for (const line of lines) {
        if (!line.startsWith('data:')) continue
        const payload = line.slice('data:'.length).trim()
        if (payload === '[DONE]') { aiLoading.value = false; break }
        try {
          const chunk = JSON.parse(payload) as string
          aiMessages.value[aiMsgIdx].text += chunk
          // Auto-scroll
          if (aiChatRef.value) aiChatRef.value.scrollTop = aiChatRef.value.scrollHeight
        } catch { /* skip malformed */ }
      }
    }
  } catch {
    aiMessages.value[aiMsgIdx].text = '⚠️ AI tạm thời không phản hồi.'
  } finally {
    aiLoading.value = false
    setTimeout(() => {
      if (aiChatRef.value) aiChatRef.value.scrollTop = aiChatRef.value.scrollHeight
    }, 50)
  }
}

const handleAiKeydown = (e: KeyboardEvent) => {
  if (e.key === 'Enter' && !e.shiftKey) { e.preventDefault(); sendAiMessage() }
}

onMounted(async () => {
  try {
    const res = await fetch(`${API_BASE}/exercises/${route.params.id}`)
    exercise.value = await res.json()
    
    // Dynamic initial code based on exercise title
    if (exercise.value?.title) {
      const title = exercise.value.title.toLowerCase()
      if (title.includes('two sum')) {
        code.value = '// Viết thuật toán Two Sum\nfunction twoSum(nums, target) {\n  \n}\n\n// Gợi ý: Dùng Hash Map để tối ưu O(n)'
      } else if (title.includes('bubble sort')) {
        code.value = '// Viết thuật toán Bubble Sort của bạn...\nfunction bubbleSort(arr) {\n  for (let i = 0; i < arr.length; i++) {\n    for (let j = 0; j < arr.length - i - 1; j++) {\n      if (arr[j] > arr[j + 1]) {\n        [arr[j], arr[j + 1]] = [arr[j + 1], arr[j]]\n      }\n    }\n  }\n  return arr\n}'
      } else if (title.includes('binary search')) {
        code.value = '// Viết thuật toán Binary Search\nfunction binarySearch(arr, target) {\n  let left = 0;\n  let right = arr.length - 1;\n  // code...\n}'
      } else if (title.includes('valid palindrome')) {
        code.value = '// Kiểm tra chuỗi đối xứng\nfunction isPalindrome(s) {\n  // Dùng 2 con trỏ left, right\n}'
      } else {
        code.value = `// Hoàn thành bài tập: ${exercise.value.title}\nfunction solve() {\n  \n}`
      }
    }
  } catch { /* offline */ }
})

onBeforeUnmount(() => { player?.pause() })
</script>

<template>
  <div class="h-screen flex flex-col overflow-hidden" style="background:#020408">

    <!-- ── Header ── -->
    <header class="h-13 flex items-center justify-between px-4 flex-shrink-0"
      style="background:#060D16; border-bottom:1px solid rgba(16,185,129,0.12);">
      <div class="flex items-center gap-3">
        <router-link to="/" class="p-1.5 rounded-lg text-slate-500 hover:text-emerald-400 hover:bg-white/5 transition-all">
          <ChevronLeft class="w-5 h-5" />
        </router-link>
        <div class="w-px h-4" style="background:rgba(255,255,255,0.08)"></div>
        <h1 class="font-bold text-slate-100 text-sm">{{ exercise?.title || 'Workspace' }}</h1>
        <span v-if="exercise?.difficultyLevel"
          class="text-[10px] font-bold px-2 py-0.5 rounded uppercase tracking-wide"
          :style="exercise.difficultyLevel==='Easy' ? 'color:#10B981;background:rgba(16,185,129,0.1)' :
                  exercise.difficultyLevel==='Hard' ? 'color:#EF4444;background:rgba(239,68,68,0.1)' :
                  'color:#F97316;background:rgba(249,115,22,0.1)'">
          {{ exercise.difficultyLevel }}
        </span>
      </div>

      <div class="flex items-center gap-2">
        <!-- Step info -->
        <span v-if="totalSteps > 0" class="text-xs font-mono-stat text-slate-500 hidden md:block">
          {{ currentIndex + 1 }} / {{ totalSteps }} bước
        </span>

        <!-- Solution hint button -->
        <button @click="showSolutionModal = true"
          class="flex items-center gap-1.5 px-3 py-1.5 rounded-lg font-bold text-sm transition-all"
          style="background:rgba(99,102,241,0.15); color:#818CF8; border:1px solid rgba(99,102,241,0.25);"
          @mouseenter="($event.currentTarget as HTMLElement).style.background='rgba(99,102,241,0.25)'"
          @mouseleave="($event.currentTarget as HTMLElement).style.background='rgba(99,102,241,0.15)'">
          <BookOpen class="w-3.5 h-3.5" />
          Code Mẫu
        </button>

        <button @click="handleRunCode" :disabled="loading"
          class="flex items-center gap-2 px-4 py-1.5 rounded-lg font-bold text-sm transition-all disabled:opacity-50"
          style="background:#10B981; color:#fff;"
          @mouseenter="($event.currentTarget as HTMLElement).style.background='#059669'"
          @mouseleave="($event.currentTarget as HTMLElement).style.background='#10B981'">
          <Play class="w-3.5 h-3.5 fill-current" />
          {{ loading ? 'RUNNING...' : 'RUN CODE' }}
        </button>
        <button class="p-2 rounded-lg text-slate-500 hover:text-slate-200 hover:bg-white/5 transition-all">
          <Save class="w-4 h-4" />
        </button>
      </div>
    </header>

    <!-- ── Body ── -->
    <div class="flex-1 flex overflow-hidden">

      <!-- ── Left: Visualizer + Controls ── -->
      <div class="flex-1 flex flex-col min-w-0" style="border-right:1px solid rgba(255,255,255,0.05);">

        <!-- Visualizer canvas -->
        <div class="flex-1 relative flex items-center justify-center overflow-hidden" style="background:#020408;">
          <svg ref="svgRef" width="100%" height="100%" style="overflow:visible;"></svg>

          <!-- Empty state -->
          <div v-if="!visualizerData.length" class="absolute inset-0 flex flex-col items-center justify-center gap-3 text-slate-600">
            <div class="w-16 h-16 rounded-2xl flex items-center justify-center" style="background:rgba(16,185,129,0.05);border:1px solid rgba(16,185,129,0.1);">
              <Play class="w-7 h-7 text-emerald-900" />
            </div>
            <p class="text-sm font-medium">Viết code và bấm RUN để bắt đầu</p>
          </div>

          <!-- Action overlay -->
          <div v-if="currentStep" class="absolute top-3 left-3 flex items-center gap-2">
            <span class="text-[11px] font-bold px-2.5 py-1 rounded font-mono-stat uppercase tracking-widest"
              :style="`background:${ACTION_COLORS[currentStep.a]}20; color:${ACTION_COLORS[currentStep.a]}; border:1px solid ${ACTION_COLORS[currentStep.a]}40`">
              {{ currentStep.a === 'cmp' ? '⚖ COMPARE' : currentStep.a === 'swp' ? '↔ SWAP' : currentStep.a === 'fnd' ? '✓ FOUND' : currentStep.a.toUpperCase() }}
            </span>
            <span v-if="currentStep.l" class="text-[11px] text-slate-500 font-mono-stat">LINE {{ currentStep.l }}</span>
          </div>

          <!-- Variable tracker -->
          <div v-if="currentStep?.v && Object.keys(currentStep.v).length"
            class="absolute top-3 right-3 text-[11px] font-mono-stat rounded-lg p-2.5 space-y-1"
            style="background:rgba(6,13,22,0.9); border:1px solid rgba(16,185,129,0.15);">
            <div class="text-slate-500 uppercase tracking-widest text-[9px] font-bold mb-1.5">Variables</div>
            <div v-for="(val, key) in currentStep.v" :key="key" class="flex items-center gap-2">
              <span class="text-emerald-400">{{ key }}</span>
              <span class="text-slate-500">=</span>
              <span class="text-slate-200">{{ val }}</span>
            </div>
          </div>
        </div>

        <!-- Progress bar -->
        <div class="h-1 cursor-pointer" style="background:rgba(255,255,255,0.04);" @click="seekProgress">
          <div class="h-full transition-all duration-200" style="background:linear-gradient(90deg,#10B981,#34D399);"
            :style="`width:${progressPct}%`"></div>
        </div>

        <!-- Playback controls -->
        <div class="h-14 flex items-center justify-center gap-4 flex-shrink-0"
          style="background:#060D16; border-top:1px solid rgba(255,255,255,0.04);">
          <button @click="resetPlayer" class="p-2 rounded-lg text-slate-500 hover:text-slate-200 hover:bg-white/5 transition-all" title="Reset">
            <RotateCcw class="w-4 h-4" />
          </button>
          <button @click="stepBack" class="p-2 rounded-lg text-slate-500 hover:text-emerald-400 hover:bg-white/5 transition-all" title="Bước lùi">
            <ChevronLeft class="w-5 h-5" />
          </button>

          <!-- Play/Pause -->
          <button @click="togglePlay"
            class="w-10 h-10 rounded-xl flex items-center justify-center transition-all active:scale-95"
            style="background:rgba(16,185,129,0.15);" title="Play/Pause"
            @mouseenter="($event.target as HTMLElement).style.background='rgba(16,185,129,0.25)'"
            @mouseleave="($event.target as HTMLElement).style.background='rgba(16,185,129,0.15)'">
            <component :is="isPlaying ? Pause : Play" class="w-5 h-5 text-emerald-400 fill-current" />
          </button>

          <button @click="stepFwd" class="p-2 rounded-lg text-slate-500 hover:text-emerald-400 hover:bg-white/5 transition-all" title="Bước tiến">
            <ChevronRight class="w-5 h-5" />
          </button>

          <!-- Speed -->
          <button @click="cycleSpeed"
            class="px-3 py-1.5 rounded-lg text-xs font-mono-stat font-bold transition-all"
            style="background:rgba(255,255,255,0.04); color:#10B981; border:1px solid rgba(16,185,129,0.2);"
            title="Thay đổi tốc độ">
            {{ currentSpeed }}x
          </button>
        </div>
      </div>

      <!-- ── Right: Monaco Editor + AI Chat ── -->
      <div class="flex flex-col" style="width:440px; background:#020408;">

        <!-- Editor tab bar -->
        <div class="h-9 flex items-center gap-1 px-3 flex-shrink-0"
          style="background:#060D16; border-bottom:1px solid rgba(255,255,255,0.05);">
          <span class="text-[11px] font-bold px-3 py-1 rounded-md" style="color:#10B981; background:rgba(16,185,129,0.1);">
            {{ exercise?.supportedLanguages?.[0] ?? 'JavaScript' }}
          </span>
          <span class="text-[10px] text-slate-600 ml-auto font-mono-stat">UTF-8</span>
        </div>

        <!-- Monaco Editor -->
        <div class="flex-1 overflow-hidden min-h-0">
          <MonacoEditor
            v-model="code"
            :language="'javascript'"
            :highlight-line="highlightLine"
            class="h-full"
          />
        </div>

        <!-- AI Chat panel -->
        <div class="flex flex-col flex-shrink-0" style="height:260px; border-top:1px solid rgba(16,185,129,0.1);">
          <div class="h-9 flex items-center justify-between px-3 flex-shrink-0"
            style="background:#060D16; border-bottom:1px solid rgba(255,255,255,0.05);">
            <div class="flex items-center gap-2">
              <Sparkles class="w-3.5 h-3.5 text-emerald-400" />
              <span class="text-[11px] font-bold text-slate-300 tracking-wide">AI MENTOR</span>
              <span v-if="aiLoading" class="text-[10px] text-emerald-400 animate-pulse">● THINKING</span>
            </div>
            <button @click="aiPanelOpen = !aiPanelOpen" class="text-slate-600 hover:text-slate-300 transition-colors">
              <X class="w-3.5 h-3.5" />
            </button>
          </div>

          <!-- Messages -->
          <div ref="aiChatRef" class="flex-1 overflow-y-auto p-3 space-y-3" style="background:#020408;">
            <div v-for="(msg, idx) in aiMessages" :key="idx"
              :class="['text-[12px] leading-relaxed', msg.role === 'user' ? 'text-right' : '']">
              <span :class="['inline-block px-3 py-2 rounded-xl max-w-[90%] text-left',
                msg.role === 'ai'
                  ? 'text-slate-300'
                  : 'text-emerald-300 font-semibold']"
                :style="msg.role === 'ai'
                  ? 'background:rgba(255,255,255,0.04); border:1px solid rgba(255,255,255,0.06)'
                  : 'background:rgba(16,185,129,0.1); border:1px solid rgba(16,185,129,0.2)'">
                {{ msg.text }}
              </span>
            </div>
            <div v-if="aiLoading" class="flex gap-1 pl-1">
              <span v-for="i in 3" :key="i" class="w-1.5 h-1.5 rounded-full bg-emerald-500 animate-bounce"
                :style="`animation-delay:${(i-1)*0.15}s`"></span>
            </div>
          </div>

          <!-- Input -->
          <div class="flex items-center gap-2 p-2 flex-shrink-0"
            style="border-top:1px solid rgba(255,255,255,0.05); background:#060D16;">
            <input v-model="userAiInput" @keydown="handleAiKeydown"
              placeholder="Hỏi AI Mentor..."
              class="flex-1 text-[12px] bg-transparent text-slate-200 outline-none placeholder-slate-600"
              :disabled="aiLoading" />
            <button @click="sendAiMessage" :disabled="aiLoading"
              class="p-1.5 rounded-lg transition-all disabled:opacity-40"
              style="color:#10B981;"
              @mouseenter="($event.target as HTMLElement).style.background='rgba(16,185,129,0.1)'"
              @mouseleave="($event.target as HTMLElement).style.background='transparent'">
              <Send class="w-3.5 h-3.5" />
            </button>
          </div>
        </div>
      </div>
    </div>
  </div>

  <!-- ── Solution Modal ── -->
  <Teleport to="body">
    <Transition name="modal">
      <div v-if="showSolutionModal" class="fixed inset-0 z-50 flex items-center justify-center p-4"
        style="background:rgba(0,0,0,0.75); backdrop-filter:blur(8px);"
        @click.self="showSolutionModal = false">
        <div class="w-full max-w-2xl rounded-2xl overflow-hidden"
          style="background:#060D16; border:1px solid rgba(99,102,241,0.3); box-shadow:0 25px 60px rgba(0,0,0,0.7);">

          <!-- Modal header -->
          <div class="flex items-center justify-between px-6 py-4"
            style="border-bottom:1px solid rgba(255,255,255,0.05); background:rgba(99,102,241,0.08);">
            <div class="flex items-center gap-3">
              <div class="w-8 h-8 rounded-lg flex items-center justify-center" style="background:rgba(99,102,241,0.2);">
                <BookOpen class="w-4 h-4" style="color:#818CF8;" />
              </div>
              <div>
                <h2 class="font-bold text-slate-100 text-sm">Code Mẫu — {{ exercise?.title }}</h2>
                <p class="text-[11px] text-slate-500">Tham khảo giải pháp chuẩn. Tự làm trước khi xem nhé! 💪</p>
              </div>
            </div>
            <button @click="showSolutionModal = false" class="p-1.5 rounded-lg text-slate-600 hover:text-slate-300 hover:bg-white/5 transition-all">
              <X class="w-4 h-4" />
            </button>
          </div>

          <!-- No solution available -->
          <div v-if="!currentSolution" class="px-6 py-10 text-center">
            <p class="text-slate-500 text-sm">Chưa có code mẫu cho bài này. Hỏi AI Mentor để được gợi ý nhé! 🤖</p>
          </div>

          <template v-else>
            <!-- Complexity badges -->
            <div class="flex items-center gap-3 px-6 pt-4">
              <span class="text-xs font-bold px-2.5 py-1 rounded-full" style="background:rgba(16,185,129,0.1); color:#10B981; border:1px solid rgba(16,185,129,0.2);">⚡ Time: {{ currentSolution.complexity }}</span>
              <span class="text-xs font-bold px-2.5 py-1 rounded-full" style="background:rgba(59,130,246,0.1); color:#60A5FA; border:1px solid rgba(59,130,246,0.2);">🗃 Space: {{ currentSolution.spaceComplexity }}</span>
            </div>

            <!-- Steps breakdown -->
            <div class="px-6 pt-4">
              <p class="text-[11px] font-bold uppercase tracking-widest text-slate-500 mb-2">📋 Giải thích từng bước</p>
              <ul class="space-y-1.5">
                <li v-for="step in currentSolution.steps" :key="step"
                  class="text-xs text-slate-300 py-1.5 px-3 rounded-lg"
                  style="background:rgba(255,255,255,0.03); border:1px solid rgba(255,255,255,0.05);">
                  {{ step }}
                </li>
              </ul>
            </div>

            <!-- Code block -->
            <div class="px-6 pt-4">
              <p class="text-[11px] font-bold uppercase tracking-widest text-slate-500 mb-2">💻 Code giải</p>
              <pre class="text-xs text-emerald-300 p-4 rounded-xl overflow-x-auto"
                style="background:#020408; border:1px solid rgba(16,185,129,0.15); font-family:'JetBrains Mono',monospace; line-height:1.7;">{{ currentSolution.code }}</pre>
            </div>

            <!-- Actions -->
            <div class="flex items-center justify-between px-6 py-4" style="border-top:1px solid rgba(255,255,255,0.05);">
              <p class="text-[11px] text-slate-600">Tự viết lại sẽ giúp bạn hiểu sâu hơn! 🎯</p>
              <div class="flex gap-2">
                <button @click="showSolutionModal = false"
                  class="px-4 py-1.5 rounded-lg text-sm text-slate-400 hover:text-slate-200 transition-all"
                  style="background:rgba(255,255,255,0.05);">
                  Đóng
                </button>
                <button @click="applySolution"
                  class="flex items-center gap-1.5 px-4 py-1.5 rounded-lg text-sm font-bold transition-all"
                  style="background:rgba(99,102,241,0.2); color:#818CF8; border:1px solid rgba(99,102,241,0.3);"
                  @mouseenter="($event.currentTarget as HTMLElement).style.background='rgba(99,102,241,0.35)'"
                  @mouseleave="($event.currentTarget as HTMLElement).style.background='rgba(99,102,241,0.2)'">
                  Áp dụng vào Editor
                </button>
              </div>
            </div>
          </template>
        </div>
      </div>
    </Transition>
  </Teleport>
</template>

<style scoped>
.modal-enter-active, .modal-leave-active { transition: all 0.2s ease; }
.modal-enter-from, .modal-leave-to { opacity: 0; transform: scale(0.96); }
</style>
