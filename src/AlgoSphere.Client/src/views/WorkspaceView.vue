<script setup lang="ts">
import { ref, computed, onMounted, onBeforeUnmount } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { API_BASE } from '../utils/api'
import { Play, Pause, RotateCcw, ChevronLeft, ChevronRight, Save, Sparkles, Send, X, BookOpen, Eye, Zap, Cpu, Code2 } from 'lucide-vue-next'
import * as d3 from 'd3'
import * as signalR from '@microsoft/signalr'
import { HUB_BASE } from '../utils/api'
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
    code: `function climbStairs(n) {\n  if (n <= 2) return n;\n  let a = 1, b = 2;\n  for (let i = 3; i <= n; i++) {\n    [a, b] = [b, a + b];\n  }\n  return b;\n}`,
    steps: [
      '1️⃣ Base case: n=1 → 1 cách, n=2 → 2 cách.',
      '2️⃣ Nhận ra: f(n) = f(n-1) + f(n-2) (giống Fibonacci).',
      '3️⃣ Dùng 2 biến a, b thay vì mảng để tiết kiệm bộ nhớ O(1).',
      '4️⃣ Mỗi bước: a = b, b = a + b.',
    ]
  },
  'fibonacci': {
    complexity: 'O(n)', spaceComplexity: 'O(1)',
    code: `function fib(n) {\n  if (n <= 1) return n;\n  let prev = 0, curr = 1;\n  for (let i = 2; i <= n; i++) {\n    [prev, curr] = [curr, prev + curr];\n  }\n  return curr;\n}`,
    steps: [
      '1️⃣ Xử lý trường hợp cơ bản n=0, n=1.',
      '2️⃣ Dùng 2 biến để lưu giá trị của F(n-1) và F(n-2).',
      '3️⃣ Duyệt từ 2 đến n, cập nhật giá trị mới bằng tổng 2 số trước.',
      '4️⃣ Trả về kết quả cuối cùng.',
    ]
  },
  'merge sort': {
    complexity: 'O(n log n)', spaceComplexity: 'O(n)',
    code: `function mergeSort(arr) {\n  if (arr.length <= 1) return arr;\n  const mid = Math.floor(arr.length / 2);\n  const left = mergeSort(arr.slice(0, mid));\n  const right = mergeSort(arr.slice(mid));\n  return merge(left, right);\n}\n\nfunction merge(left, right) {\n  let result = [], i = 0, j = 0;\n  while (i < left.length && j < right.length) {\n    if (left[i] < right[j]) result.push(left[i++]);\n    else result.push(right[j++]);\n  }\n  return [...result, ...left.slice(i), ...right.slice(j)];\n}`,
    steps: [
      '1️⃣ Chia mảng thành 2 nửa cho đến khi mỗi mảng chỉ còn 1 phần tử.',
      '2️⃣ Sử dụng hàm merge để gộp 2 mảng đã sắp xếp lại với nhau.',
      '3️⃣ So sánh từng phần tử ở 2 đầu mảng để đưa vào mảng kết quả.',
      '4️⃣ Đệ quy cho đến khi mảng được sắp xếp hoàn toàn.',
    ]
  },
  'quick sort': {
    complexity: 'O(n log n)', spaceComplexity: 'O(log n)',
    code: `function quickSort(arr) {\n  if (arr.length <= 1) return arr;\n  const pivot = arr[arr.length - 1];\n  const left = [], right = [];\n  for (let i = 0; i < arr.length - 1; i++) {\n    if (arr[i] < pivot) left.push(arr[i]);\n    else right.push(arr[i]);\n  }\n  return [...quickSort(left), pivot, ...quickSort(right)];\n}`,
    steps: [
      '1️⃣ Chọn một phần tử làm Pivot (thường là phần tử cuối).',
      '2️⃣ Phân loại: Các số nhỏ hơn Pivot vào mảng left, lớn hơn vào mảng right.',
      '3️⃣ Đệ quy sắp xếp mảng left và mảng right.',
      '4️⃣ Gộp kết quả: [left] + Pivot + [right].',
    ]
  },
  'valid parentheses': {
    complexity: 'O(n)', spaceComplexity: 'O(n)',
    code: `function isValid(s) {\n  const stack = [];\n  const map = { ")": "(", "}": "{", "]": "[" };\n  for (const char of s) {\n    if (map[char]) {\n      if (stack.pop() !== map[char]) return false;\n    } else {\n      stack.push(char);\n    }\n  }\n  return stack.length === 0;\n}`,
    steps: [
      '1️⃣ Khởi tạo một Stack trống.',
      '2️⃣ Duyệt từng ký tự trong chuỗi.',
      '3️⃣ Nếu là dấu mở ngoặc → Push vào Stack.',
      '4️⃣ Nếu là dấu đóng → Pop từ Stack và kiểm tra xem có khớp không.',
      '5️⃣ Cuối cùng, nếu Stack trống → Chuỗi hợp lệ.',
    ]
  },
  'max subarray': {
    complexity: 'O(n)', spaceComplexity: 'O(1)',
    code: `function maxSubArray(nums) {\n  let maxSum = nums[0], currentSum = nums[0];\n  for (let i = 1; i < nums.length; i++) {\n    currentSum = Math.max(nums[i], currentSum + nums[i]);\n    maxSum = Math.max(maxSum, currentSum);\n  }\n  return maxSum;\n}`,
    steps: [
      '1️⃣ Thuật toán Kadane: Duyệt mảng 1 lần.',
      '2️⃣ Tại mỗi vị trí, quyết định: Bắt đầu mảng con mới hay cộng dồn tiếp.',
      '3️⃣ currentSum = max(số hiện tại, tổng cũ + số hiện tại).',
      '4️⃣ Cập nhật maxSum nếu tìm thấy tổng lớn hơn.',
    ]
  },
  'reverse linked list': {
    complexity: 'O(n)', spaceComplexity: 'O(1)',
    code: `function reverseList(head) {\n  let prev = null, curr = head;\n  while (curr) {\n    const next = curr.next;\n    curr.next = prev;\n    prev = curr;\n    curr = next;\n  }\n  return prev;\n}`,
    steps: [
      '1️⃣ Dùng 3 con trỏ: prev, curr, next.',
      '2️⃣ Lưu con trỏ next trước khi thay đổi liên kết.',
      '3️⃣ Đảo ngược liên kết: curr.next = prev.',
      '4️⃣ Di chuyển prev và curr tiến lên một bước.',
    ]
  },
  'best time to buy stock': {
    complexity: 'O(n)', spaceComplexity: 'O(1)',
    code: `function maxProfit(prices) {\n  let minPrice = Infinity, maxProfit = 0;\n  for (const price of prices) {\n    minPrice = Math.min(minPrice, price);\n    maxProfit = Math.max(maxProfit, price - minPrice);\n  }\n  return maxProfit;\n}`,
    steps: [
      '1️⃣ Duyệt mảng giá chứng khoán.',
      '2️⃣ Luôn cập nhật giá thấp nhất đã gặp (minPrice).',
      '3️⃣ Tính lợi nhuận tiềm năng nếu bán ở giá hiện tại.',
      '4️⃣ Lưu lại lợi nhuận cao nhất (maxProfit).',
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
const router = useRouter()
const exercise = ref<any>(null)
const code = ref('// Viết thuật toán Bubble Sort của bạn...\nfunction bubbleSort(arr) {\n  for (let i = 0; i < arr.length; i++) {\n    for (let j = 0; j < arr.length - i - 1; j++) {\n      if (arr[j] > arr[j + 1]) {\n        [arr[j], arr[j + 1]] = [arr[j + 1], arr[j]]\n      }\n    }\n  }\n  return arr\n}')
const loading = ref(false)
const aiLoading = ref(false)
const userAiInput = ref('')
const editorRef = ref<any>(null)
const currentStep = ref<TraceStep | null>(null)
const currentIndex = ref(-1)
const totalSteps = ref(0)
const visualizerData = ref<number[]>([])
const highlightLine = ref<number | null>(null)
// Full TraceLog — passed to AI for rich context
// ── Gamification ──────────────────────────────────────────────────
const showLevelUpModal = ref(false)
const showSuccessOverlay = ref(false)
const gamiData = ref<any>(null)
const suspicionLevel = ref('None')

// ── Arena / Real-time ──────────────────────────────────────────────
const isArenaMatch = computed(() => !!route.query.matchId)
const isSpectator = computed(() => route.query.mode === 'spectator')
const opponentProgress = ref(0)
const myProgress = ref(0) // Used for spectator to track player 1
const matchStatus = ref('Đang thi đấu...')
const dsState = ref<Record<string, any>>({}) // State of secondary structures (Set/Map)
let arenaConnection: signalR.HubConnection | null = null

const setupArenaHub = async () => {
  if (!isArenaMatch.value) return
  const token = localStorage.getItem('token')
  arenaConnection = new signalR.HubConnectionBuilder()
    .withUrl(`${HUB_BASE}/ws/arena`, { accessTokenFactory: () => token! })
    .withAutomaticReconnect()
    .build()

  arenaConnection.on('OpponentProgress', (prog: number) => {
    opponentProgress.value = prog
  })
  
  // If spectator, we might receive progress for 'both' if the Hub is updated
  // For now, we'll assume Hub broadcasts to all in group
  
  arenaConnection.on('MatchEnded', (data: any) => {
    matchStatus.value = data.winner === arenaConnection?.connectionId ? '🏆 BẠN THẮNG!' : '❌ ĐỐI THỦ ĐÃ THẮNG!'
    if (isSpectator.value) matchStatus.value = '🏁 TRẬN ĐẤU KẾT THÚC'
  })

  try {
    await arenaConnection.start()
    if (isSpectator.value) {
      await arenaConnection.invoke('JoinMatchAsSpectator', route.query.matchId)
    }
  } catch (e) { console.error('Arena Hub failed', e) }
}

const renderNodeLinkVisualizer = (data: { nodes: Record<number, any>, links: any[] }) => {
  if (!svgRef.value) return
  const svg = d3.select(svgRef.value)
  svg.selectAll('*').remove()

  const width = svgRef.value.clientWidth || 600
  const height = svgRef.value.clientHeight || 400
  const padding = 40  // keep nodes away from edges

  const nodes = Object.values(data.nodes)
  if (nodes.length === 0) return

  const links = data.links.map(l => ({
    source: l.source,
    target: l.target,
    label: l.prop
  }))

  const simulation = d3.forceSimulation(nodes as any)
    .force('link', d3.forceLink(links).id((d: any) => d.id).distance(90).strength(1))
    .force('charge', d3.forceManyBody().strength(-200))
    .force('center', d3.forceCenter(width / 2, height / 2))
    .force('collision', d3.forceCollide(35))
    // Boundary force: keep nodes inside the SVG
    .force('bound', () => {
      for (const node of (nodes as any[])) {
        node.x = Math.max(padding, Math.min(width - padding, node.x ?? width / 2))
        node.y = Math.max(padding, Math.min(height - padding, node.y ?? height / 2))
      }
    })

  const link = svg.append('g')
    .selectAll('line')
    .data(links)
    .enter().append('line')
    .attr('stroke', '#475569')
    .attr('stroke-width', 2)
    .attr('marker-end', 'url(#arrowhead)')

  svg.append('defs').append('marker')
    .attr('id', 'arrowhead')
    .attr('viewBox', '-0 -5 10 10')
    .attr('refX', 20)
    .attr('refY', 0)
    .attr('orient', 'auto')
    .attr('markerWidth', 6)
    .attr('markerHeight', 6)
    .attr('xoverflow', 'visible')
    .append('svg:path')
    .attr('d', 'M 0,-5 L 10 ,0 L 0,5')
    .attr('fill', '#475569')
    .style('stroke', 'none')

  const node = svg.append('g')
    .selectAll('circle')
    .data(nodes)
    .enter().append('g')

  node.append('circle')
    .attr('r', 18)
    .attr('fill', '#0F172A')
    .attr('stroke', '#10B981')
    .attr('stroke-width', 2)

  node.append('text')
    .attr('text-anchor', 'middle')
    .attr('dy', '.35em')
    .attr('fill', '#FFF')
    .attr('font-size', '12px')
    .attr('font-weight', 'bold')
    .text((d: any) => d.val)

  simulation.on('tick', () => {
    link
      .attr('x1', (d: any) => d.source.x)
      .attr('y1', (d: any) => d.source.y)
      .attr('x2', (d: any) => d.target.x)
      .attr('y2', (d: any) => d.target.y)

    node.attr('transform', (d: any) => `translate(${d.x},${d.y})`)
  })
}

const renderVisualizer = (data: any[], step: TraceStep | null) => {
  if (!svgRef.value) return
  const svg = d3.select(svgRef.value)
  svg.selectAll('*').remove()

  if (totalSteps.value === 0 && !isPlaying.value) return;

  const W = svgRef.value.clientWidth
  const H = svgRef.value.clientHeight

  const x = d3.scaleBand()
    .domain(data.map((_, i) => i.toString()))
    .range([8, W - 8])
    .padding(0.2)

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

const lastTraceLog = ref<TraceLog | null>(null)
const aiMessages = ref<{ role: 'ai' | 'user'; text: string }[]>([
  { role: 'ai', text: 'Chào bạn! Tôi là AI Mentor. Viết thuật toán → Run Code để bắt đầu trực quan hóa. Sau đó hỏi tôi bất cứ điều gì! 💡' },
])

const SPEEDS = [0.25, 0.5, 1, 2, 4]
const speedIdx = ref(2) // default 1x
const currentSpeed = computed(() => SPEEDS[speedIdx.value])
const isPlaying = ref(false)
const isNodeMode = ref(false)  // true when visualizing linked list / tree nodes

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
        deltas: editorRef.value?.getDeltas() ?? []
      }),
    })
    const responseData = await res.json()
    const result = responseData.execution
    gamiData.value = responseData.gamification
    suspicionLevel.value = responseData.suspicionLevel || 'None'

    const log: TraceLog = JSON.parse(result.traceLog)
    lastTraceLog.value = log  // Store full log for AI context

    // Sandbox error: backend returned a trace with error action or empty trace
    if (!log.trace || log.trace.length === 0) {
      aiMessages.value.push({ role: 'ai', text: `⚠️ Sandbox lỗi: ${result.message || 'Code không thể chạy được.'}` })
      loading.value = false
      return
    }

    player = TracePlayer.create(log, (state, step, idx, total, ds, nodes) => {
      visualizerData.value = state
      currentStep.value = step
      currentIndex.value = idx
      totalSteps.value = total
      highlightLine.value = step?.l ?? null
      dsState.value = ds || {}
      
      if (nodes && Object.keys(nodes.nodes).length > 0) {
        visualizerData.value = [] // Clear bars if nodes exist
        renderNodeLinkVisualizer(nodes)
      } else {
        renderVisualizer(state, step)
      }
      
      isPlaying.value = player?.isPlaying ?? false
    })

    player.setSpeed(currentSpeed.value)
    
    // Auto-detect view type: if trace has nodes, clear bars immediately
    const hasNodes = log.trace.some(s => s.a === 'node')
    isNodeMode.value = hasNodes
    if (hasNodes) {
      visualizerData.value = []
      currentIndex.value = -1
    } else {
      visualizerData.value = log.initialState
    }
    
    totalSteps.value = log.trace.length
    
    // Initial render for array mode only
    if (!hasNodes) {
       renderVisualizer(log.initialState, null)
    }

    // Reset deltas after successful capture
    editorRef.value?.resetDeltas()

    // Handle Gamification Success
    if (result.success && gamiData.value) {
      showSuccessOverlay.value = true
      
      // Broadcast progress if in Arena
      if (isArenaMatch.value && arenaConnection) {
        await arenaConnection.invoke('SendProgress', route.query.matchId, 100)
        // CRITICAL: Officially submit result to end match for everyone
        await arenaConnection.invoke('SubmitResult', route.query.matchId, true)
      }

      if (gamiData.value.isLevelUp) {
        setTimeout(() => { showLevelUpModal.value = true }, 2000)
      }
      
      aiMessages.value.push({
        role: 'ai',
        text: `✨ Tuyệt vời! Bạn đã hoàn thành bài tập và nhận được ${gamiData.value.xpEarned + gamiData.value.bonusXp} XP. ${gamiData.value.isLevelUp ? '🎊 BẠN ĐÃ LÊN CẤP ' + gamiData.value.level + '!' : ''}`
      })
    } else {
      aiMessages.value.push({
        role: 'ai',
        text: `✅ Code đã chạy! Tôi đã phân tích ${log.trace.length} bước thực thi. Bạn muốn tôi giải thích bước nào?`
      })
    }
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
  if (isArenaMatch.value) setupArenaHub()
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
      } else if (title.includes('reverse linked list') || title.includes('reverse list') || (title.includes('reverse') && title.includes('link'))) {
        code.value = '// Đảo ngược Linked List\n// ListNode đã được định nghĩa sẵn trong môi trường chạy\nfunction reverseList() {\n  // Tạo linked list mẫu: 1 -> 2 -> 3 -> 4 -> 5\n  let head = new ListNode(1);\n  head.next = new ListNode(2);\n  head.next.next = new ListNode(3);\n  head.next.next.next = new ListNode(4);\n  head.next.next.next.next = new ListNode(5);\n\n  // Đảo ngược\n  let prev = null;\n  let curr = head;\n  while (curr !== null) {\n    let next = curr.next;\n    curr.next = prev;\n    prev = curr;\n    curr = next;\n  }\n  return prev;\n}'
      } else if (title.includes('linked list') || title.includes('danh sách liên kết')) {
        code.value = '// Bài tập Linked List\n// ListNode đã được định nghĩa sẵn trong môi trường chạy\nfunction reverseList() {\n  let head = new ListNode(1);\n  head.next = new ListNode(2);\n  head.next.next = new ListNode(3);\n  // Viết logic xử lý ở đây...\n}'
      } else if (title.includes('inorder') || title.includes('preorder') || title.includes('postorder') || title.includes('tree')) {
        code.value = '// Bài tập Binary Tree\n// TreeNode đã được định nghĩa sẵn trong môi trường chạy\nfunction solve() {\n  let root = new TreeNode(1);\n  root.left = new TreeNode(2);\n  root.right = new TreeNode(3);\n  // Viết logic traversal ở đây...\n}'
      } else if (title.includes('quick sort') || title.includes('quicksort')) {
        code.value = '// Viết thuật toán Quick Sort\nfunction quickSort(arr, low = 0, high = arr.length - 1) {\n  if (low < high) {\n    let pivot = arr[high];\n    // code...\n  }\n  return arr;\n}'
      } else {
        code.value = `// Hoàn thành bài tập: ${exercise.value.title}\nfunction solve() {\n  \n}`
      }
    }
  } catch { /* offline */ }
})

import { gsap } from 'gsap'
const levelUpRef = ref<HTMLElement | null>(null)

const runLevelUpAnim = () => {
  if (!levelUpRef.value) return
  const tl = gsap.timeline()
  tl.fromTo('.level-card', { scale: 0.5, opacity: 0, y: 50 }, { scale: 1, opacity: 1, y: 0, duration: 0.6, ease: 'back.out(1.7)' })
  tl.fromTo('.level-glow', { opacity: 0, scale: 0.5 }, { opacity: 1, scale: 1.5, duration: 1, repeat: -1, yoyo: true }, '-=0.3')
  tl.fromTo('.level-text', { y: 20, opacity: 0 }, { y: 0, opacity: 1, duration: 0.4, stagger: 0.1 }, '-=0.5')
}


const runSuccessAnim = () => {
  const card = document.querySelector('.success-card')
  if (!card) return
  
  const tl = gsap.timeline()
  tl.fromTo(card, { y: 100, opacity: 0 }, { y: 0, opacity: 1, duration: 0.5, ease: 'power3.out' })
  
  const badges = document.querySelectorAll('.badge-stamp')
  if (badges.length > 0) {
    tl.fromTo(badges, { scale: 3, opacity: 0, rotate: -20 }, { scale: 1, opacity: 1, rotate: 0, duration: 0.4, stagger: 0.2, ease: 'bounce.out' }, '-=0.2')
  }
}

import { watch, nextTick } from 'vue'
watch(showLevelUpModal, async (val) => {
  if (val) {
    await nextTick()
    runLevelUpAnim()
  }
})
watch(showSuccessOverlay, async (val) => {
  if (val) {
    await nextTick()
    runSuccessAnim()
  }
})

onBeforeUnmount(() => { 
  player?.pause() 
  arenaConnection?.stop()
})
</script>

<template>
  <div class="h-screen flex flex-col overflow-hidden" style="background:#020408">

    <!-- ── Header ── -->
    <header class="h-13 flex items-center justify-between px-4 flex-shrink-0"
      style="background:#060D16; border-bottom:1px solid rgba(16,185,129,0.12);">
      <div class="flex items-center gap-3">
        <button @click="exercise?.topicId ? router.push('/topic/' + exercise.topicId) : router.back()" 
          class="p-1.5 rounded-lg text-slate-500 hover:text-emerald-400 hover:bg-white/5 transition-all">
          <ChevronLeft class="w-5 h-5" />
        </button>
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

        <button id="run-code-btn" @click="handleRunCode" :disabled="loading"
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

        <div class="flex-1 relative flex flex-col min-h-0" style="background:#020408;">
          <!-- Visualizer Area -->
          <div class="flex-1 relative">
            <svg ref="svgRef" width="100%" height="100%" style="overflow:hidden;"></svg>

            <!-- Empty state / Waiting for code -->
            <div v-if="!isNodeMode && (!visualizerData.length || (currentIndex === -1 && totalSteps === 0))" 
              class="absolute inset-0 flex flex-col items-center justify-center gap-3 text-slate-600">
              <div class="w-16 h-16 rounded-2xl flex items-center justify-center" style="background:rgba(16,185,129,0.05);border:1px solid rgba(16,185,129,0.1);">
                <Code2 class="w-7 h-7 text-emerald-900" />
              </div>
              <p class="text-sm font-medium">Bấm RUN CODE để trực quan hóa thuật toán</p>
              <p class="text-[10px] text-slate-700 max-w-[200px] text-center">Hệ thống sẽ tự động vẽ Mảng hoặc Cấu trúc dữ liệu dựa trên code của bạn.</p>
            </div>

            <!-- Action overlay / Error overlay -->
            <div v-if="currentStep" class="absolute top-3 left-3 flex items-center gap-2">
              <span v-if="currentStep.a === 'err'" 
                class="text-[11px] font-bold px-2.5 py-1 rounded font-mono-stat uppercase tracking-widest bg-red-500/20 text-red-500 border border-red-500/40">
                ❌ ERROR: {{ currentStep.v?.msg || 'Execution failed' }}
              </span>
              <template v-else>
                <span class="text-[11px] font-bold px-2.5 py-1 rounded font-mono-stat uppercase tracking-widest"
                  :style="`background:${ACTION_COLORS[currentStep.a]}20; color:${ACTION_COLORS[currentStep.a]}; border:1px solid ${ACTION_COLORS[currentStep.a]}40`">
                  {{ currentStep.a === 'cmp' ? '⚖ COMPARE' : currentStep.a === 'swp' ? '↔ SWAP' : currentStep.a === 'fnd' ? '✓ FOUND' : currentStep.a.toUpperCase() }}
                </span>
                <span v-if="currentStep.l" class="text-[11px] text-slate-500 font-mono-stat">LINE {{ currentStep.l }}</span>
              </template>

              <!-- Anti-Cheat Status Badge -->
              <div v-if="suspicionLevel !== 'None'" 
                class="flex items-center gap-1.5 px-2.5 py-1 rounded text-[10px] font-black uppercase tracking-tighter shadow-lg"
                :class="suspicionLevel === 'Low' ? 'bg-yellow-500/20 text-yellow-500 border border-yellow-500/30' : 'bg-red-500/20 text-red-500 border border-red-500/40 animate-pulse'">
                <ShieldAlert class="w-3 h-3" />
                {{ suspicionLevel }} SUSPICION
              </div>
            </div>

            <!-- Memory Map (Secondary Structures) -->
            <div v-if="Object.keys(dsState).length > 0" class="absolute top-4 right-48 w-48 flex flex-col gap-3">
              <div v-for="(ds, name) in dsState" :key="name" 
                class="rounded-xl p-3 border border-emerald-500/30 bg-black/60 backdrop-blur-md shadow-2xl">
                <div class="flex items-center gap-2 mb-2">
                  <div class="w-2 h-2 rounded-full bg-emerald-500 animate-pulse"></div>
                  <span class="text-[9px] font-black text-emerald-400 uppercase tracking-widest">{{ name }} ({{ ds.type }})</span>
                </div>
                <div class="flex flex-wrap gap-1.5">
                  <div v-for="item in ds.items" :key="item"
                    class="w-6 h-6 rounded bg-emerald-500/10 border border-emerald-500/20 flex items-center justify-center text-[10px] font-bold text-white transition-all transform scale-110">
                    {{ item }}
                  </div>
                </div>
              </div>
            </div>

            <!-- Variable Watch Panel -->
            <div v-if="currentStep?.v" class="absolute top-4 right-4 w-40 rounded-xl overflow-hidden shadow-2xl border transition-all duration-300"
              style="background:rgba(6,13,22,0.85); border-color:rgba(16,185,129,0.2); backdrop-filter:blur(12px);">
              <div class="px-3 py-2 flex items-center justify-between" style="border-bottom:1px solid rgba(16,185,129,0.1); background:rgba(16,185,129,0.05);">
                <div class="flex items-center gap-2">
                  <Eye class="w-3.5 h-3.5 text-emerald-400" />
                  <span class="text-[10px] font-bold text-slate-300 uppercase tracking-widest">Variable Watch</span>
                </div>
              </div>
              <div class="p-3 space-y-2.5">
                <div v-if="!Object.keys(currentStep.v).filter(k => k !== 'val' && k !== 'ds').length" class="text-[9px] text-slate-600 italic text-center py-2">
                  Scanning locals...
                </div>
                <template v-for="(val, key) in currentStep.v" :key="key">
                  <div v-if="key !== 'val' && key !== 'ds'" class="flex items-center justify-between text-[11px] font-mono-stat">
                    <span class="text-slate-500">{{ key }}</span>
                    <span class="text-emerald-400 font-bold bg-emerald-400/10 px-2 py-0.5 rounded-md min-w-[1.5rem] text-center border border-emerald-400/20 shadow-[0_0_10px_rgba(16,185,129,0.1)]">
                      {{ val }}
                    </span>
                  </div>
                </template>
              </div>
            </div>
          </div>
        </div>

        <!-- Progress bar -->
        <div class="h-1 cursor-pointer" style="background:rgba(255,255,255,0.04);" @click="seekProgress">
          <div class="h-full transition-all duration-200" style="background:linear-gradient(90deg,#10B981,#34D399);"
            :style="`width:${progressPct}%`"></div>
        </div>

        <!-- Arena Progress Overlay -->
        <div v-if="isArenaMatch" class="px-6 py-3 flex items-center gap-6" 
          :style="isSpectator ? 'background:#060D16; border-bottom:1px solid rgba(99,102,241,0.3);' : 'background:#0A1628; border-bottom:1px solid rgba(239,68,68,0.2);'">
          
          <div v-if="isSpectator" class="flex items-center gap-2 px-3 py-1 rounded bg-indigo-500/10 border border-indigo-500/20 mr-2">
            <div class="w-1.5 h-1.5 rounded-full bg-indigo-500 animate-pulse"></div>
            <span class="text-[9px] font-black text-indigo-400 tracking-tighter">OBSERVER MODE</span>
          </div>

          <div class="flex-1">
            <div class="flex justify-between text-[10px] font-black mb-1.5 uppercase tracking-widest">
              <span class="text-blue-400">{{ isSpectator ? 'PLAYER 1' : 'BẠN' }}</span>
              <span class="text-slate-400">{{ isSpectator ? myProgress : 100 }}%</span>
            </div>
            <div class="h-1.5 rounded-full overflow-hidden bg-white/5">
              <div class="h-full transition-all duration-500" :style="`width: ${isSpectator ? myProgress : progressPct}%; background: #3B82F6;`"></div>
            </div>
          </div>
          <div class="text-xs font-black italic text-red-500">VS</div>
          <div class="flex-1">
            <div class="flex justify-between text-[10px] font-black mb-1.5 uppercase tracking-widest">
              <span class="text-red-400">{{ isSpectator ? 'PLAYER 2' : 'ĐỐI THỦ' }}</span>
              <span class="text-slate-400">{{ opponentProgress }}%</span>
            </div>
            <div class="h-1.5 rounded-full overflow-hidden bg-white/5">
              <div class="h-full transition-all duration-500" :style="`width: ${opponentProgress}%; background: #EF4444;`"></div>
            </div>
          </div>
          <div class="px-3 py-1 rounded bg-red-500/10 border border-red-500/20 text-[10px] font-bold text-red-400">
            {{ matchStatus }}
          </div>
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
            ref="editorRef"
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
  <!-- ── Level Up Modal ── -->
  <Teleport to="body">
    <Transition name="fade">
      <div v-if="showLevelUpModal" ref="levelUpRef" class="fixed inset-0 z-[100] flex items-center justify-center p-4"
        style="background:rgba(0,0,0,0.85); backdrop-filter:blur(10px);">
        
        <!-- Animated Background Rays -->
        <div class="level-glow absolute w-96 h-96 rounded-full blur-[100px] opacity-20"
          style="background:radial-gradient(circle, #10B981, #3B82F6);"></div>

        <div class="level-card relative w-full max-w-sm p-10 rounded-[40px] text-center border overflow-hidden"
          style="background:linear-gradient(180deg, #060D16, #020408); border-color:rgba(16,185,129,0.3); box-shadow:0 0 80px rgba(16,185,129,0.2);">
          
          <div class="absolute top-0 left-0 w-full h-1" style="background:linear-gradient(90deg, transparent, #10B981, transparent);"></div>

          <div class="mb-8 relative inline-block">
            <div class="w-24 h-24 rounded-3xl rotate-12 flex items-center justify-center border-2 border-emerald-500/30"
              style="background:rgba(16,185,129,0.1); backdrop-filter:blur(5px);">
              <Sparkles class="w-12 h-12 text-emerald-400 -rotate-12" />
            </div>
            <div class="absolute -top-2 -right-2 w-8 h-8 rounded-full bg-emerald-500 text-black font-black flex items-center justify-center text-xs animate-bounce">
              !
            </div>
          </div>

          <h2 class="level-text text-[10px] font-black tracking-[0.4em] uppercase text-emerald-500 mb-2">CONGRATULATIONS</h2>
          <h1 class="level-text text-5xl font-black text-white mb-6 italic tracking-tighter">LEVEL UP!</h1>
          
          <div class="level-text flex items-center justify-center gap-4 mb-8">
            <div class="text-4xl font-black text-slate-600 line-through">LV.{{ gamiData?.level - 1 }}</div>
            <div class="w-8 h-px bg-slate-800"></div>
            <div class="text-6xl font-black text-emerald-400 drop-shadow-[0_0_15px_rgba(16,185,129,0.5)]">LV.{{ gamiData?.level }}</div>
          </div>

          <p class="level-text text-xs text-slate-400 mb-10 leading-relaxed px-4">
            Bạn đã mở khóa thêm các bài tập mới trong Skill Tree. Tiếp tục phát huy nhé!
          </p>

          <button @click="showLevelUpModal = false"
            class="level-text w-full py-4 rounded-2xl font-black text-sm tracking-widest transition-all active:scale-95"
            style="background:#10B981; color:#000; box-shadow:0 10px 30px rgba(16,185,129,0.3);">
            TIẾP TỤC HÀNH TRÌNH
          </button>
        </div>
      </div>
    </Transition>
  </Teleport>
  <!-- ── Success Performance Overlay ── -->
  <Teleport to="body">
    <Transition name="fade">
      <div v-if="showSuccessOverlay" ref="successOverlayRef" class="fixed inset-0 z-[90] flex items-center justify-center p-4 pointer-events-none">
        <div class="success-card w-full max-w-lg p-8 rounded-3xl pointer-events-auto shadow-[0_30px_100px_rgba(0,0,0,0.8)]"
          style="background:#060D16; border:1px solid rgba(16,185,129,0.2);">
          
          <div class="flex items-start justify-between mb-8">
            <div>
              <div class="text-emerald-500 font-black tracking-widest text-[10px] mb-1">MISSION COMPLETED</div>
              <h2 class="text-3xl font-black text-white italic">ACCEPTED!</h2>
            </div>
            <button @click="showSuccessOverlay = false" class="p-2 rounded-xl hover:bg-white/5 transition-all text-slate-500">
              <X class="w-5 h-5" />
            </button>
          </div>

          <div class="grid grid-cols-2 gap-6 mb-8">
            <div class="p-4 rounded-2xl" style="background:rgba(255,255,255,0.03); border:1px solid rgba(255,255,255,0.05);">
              <div class="text-[10px] text-slate-500 font-bold uppercase mb-1">XP Earned</div>
              <div class="text-2xl font-black text-white">+{{ gamiData?.xpEarned + gamiData?.bonusXp }} <span class="text-emerald-400 text-sm">XP</span></div>
            </div>
            <div class="p-4 rounded-2xl" style="background:rgba(255,255,255,0.03); border:1px solid rgba(255,255,255,0.05);">
              <div class="text-[10px] text-slate-500 font-bold uppercase mb-1">Current Level</div>
              <div class="text-2xl font-black text-white">LV.{{ gamiData?.level }}</div>
            </div>
          </div>

          <!-- Badges Section -->
          <div v-if="gamiData?.IsSpeedDemon || gamiData?.IsMemoryMaster">
            <div class="text-[10px] text-slate-600 font-bold uppercase tracking-widest mb-4 text-center">Performance Badges</div>
            <div class="flex justify-center gap-8">
              <!-- Speed Demon -->
              <div v-if="gamiData?.IsSpeedDemon" class="badge-stamp flex flex-col items-center gap-2">
                <div class="w-16 h-16 rounded-full flex items-center justify-center border-4 border-yellow-500/30 shadow-[0_0_20px_rgba(245,158,11,0.2)]"
                  style="background:rgba(245,158,11,0.1);">
                  <Zap class="w-8 h-8 text-yellow-500 fill-current" />
                </div>
                <span class="text-[9px] font-black text-yellow-500 uppercase">Speed Demon</span>
              </div>
              <!-- Memory Master -->
              <div v-if="gamiData?.IsMemoryMaster" class="badge-stamp flex flex-col items-center gap-2">
                <div class="w-16 h-16 rounded-full flex items-center justify-center border-4 border-blue-500/30 shadow-[0_0_20px_rgba(59,130,246,0.2)]"
                  style="background:rgba(59,130,246,0.1);">
                  <Cpu class="w-8 h-8 text-blue-500" />
                </div>
                <span class="text-[9px] font-black text-blue-500 uppercase">Memory Master</span>
              </div>
            </div>
          </div>

          <button @click="showSuccessOverlay = false"
            class="w-full mt-10 py-4 rounded-2xl font-black text-sm tracking-widest transition-all active:scale-95"
            style="background:#10B981; color:#000;">
            TIẾP TỤC
          </button>
        </div>
      </div>
    </Transition>
  </Teleport>
</template>

<style scoped>
.modal-enter-active, .modal-leave-active { transition: all 0.2s ease; }
.modal-enter-from, .modal-leave-to { opacity: 0; transform: scale(0.96); }

.fade-enter-active, .fade-leave-active { transition: opacity 0.3s ease; }
.fade-enter-from, .fade-leave-to { opacity: 0; }
</style>
