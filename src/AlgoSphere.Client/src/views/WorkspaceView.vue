<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRoute } from 'vue-router'
import { API_BASE } from '../utils/api'
import { Play, RotateCcw, ChevronLeft, ChevronRight, Save, Sparkles } from 'lucide-vue-next'
import * as d3 from 'd3'
import { TracePlayer, type TraceLog, type TraceStep } from '../utils/TracePlayer'

const route = useRoute()
const exercise = ref<any>(null)
const code = ref('')
const loading = ref(false)
const aiLoading = ref(false)
const visualizerData = ref<number[]>([])
const currentStepInfo = ref<TraceStep | null>(null)
const aiMessages = ref<{ role: 'ai' | 'user', text: string }[]>([
  { role: 'ai', text: 'Chào bạn! Tôi là trợ lý AI. Hãy viết thuật toán và bấm Run Code, nếu gặp lỗi tôi sẽ gợi ý ngay tại đây.' }
])
let player: TracePlayer | null = null

// D3.js Logic
const svgRef = ref<SVGSVGElement | null>(null)

const renderVisualizer = (data: number[], step: TraceStep | null) => {
  if (!svgRef.value) return
  const svg = d3.select(svgRef.value)
  svg.selectAll('*').remove()

  const width = 600
  const height = 300

  const x = d3.scaleBand()
    .domain(data.map((_, i) => i.toString()))
    .range([0, width])
    .padding(0.1)

  const y = d3.scaleLinear()
    .domain([0, d3.max(data) || 100])
    .range([height, 0])

  svg.selectAll('rect')
    .data(data)
    .enter()
    .append('rect')
    .attr('x', (_, i) => x(i.toString())!)
    .attr('y', d => y(d))
    .attr('width', x.bandwidth())
    .attr('height', d => height - y(d))
    .attr('fill', (_, i) => {
      if (step?.t.includes(i)) {
        return step.a === 'swp' ? '#6366F1' : '#10B981' // Indigo for swap, Emerald for compare
      }
      return '#1e293b'
    })
    .attr('stroke', (_, i) => step?.t.includes(i) ? '#fff' : 'none')
    .attr('rx', 4)

  svg.selectAll('text')
    .data(data)
    .enter()
    .append('text')
    .attr('x', (_, i) => x(i.toString())! + x.bandwidth() / 2)
    .attr('y', d => y(d) - 10)
    .attr('text-anchor', 'middle')
    .attr('fill', '#fff')
    .attr('font-size', '12px')
    .attr('font-weight', 'bold')
    .text(d => d)
}

const handleRunCode = async () => {
  loading.value = true
  try {
    const res = await fetch(`${API_BASE}/exercises/execute`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ exerciseId: exercise.value.id, code: code.value, language: 'javascript' })
    })
    const result = await res.json()
    const log: TraceLog = JSON.parse(result.traceLog)
    
    player = new TracePlayer(log, (state, step) => {
      visualizerData.value = state
      currentStepInfo.value = step
      renderVisualizer(state, step)
    })
    
    visualizerData.value = log.initialState
    renderVisualizer(log.initialState, null)
  } catch (err) {
    console.error(err)
  } finally {
    loading.value = false
  }
}

const askAI = async () => {
  aiLoading.value = true
  aiMessages.value.push({ role: 'user', text: 'Hãy cho tôi một gợi ý cho bài tập này.' })
  
  try {
    const res = await fetch(`${API_BASE}/ai/hint`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ currentCode: code.value, stateJson: JSON.stringify(visualizerData.value), errorMessage: "" })
    })
    const data = await res.json()
    aiMessages.value.push({ role: 'ai', text: data.explanation })
  } catch (err) {
    aiMessages.value.push({ role: 'ai', text: 'Xin lỗi, tôi đang gặp lỗi kết nối.' })
  } finally {
    aiLoading.value = false
  }
}

const nextStep = () => player?.stepForward()

onMounted(async () => {
  const id = route.params.id
  try {
    const res = await fetch(`${API_BASE}/exercises/${id}`)
    exercise.value = await res.json()
    code.value = "// Viết thuật toán Bubble Sort của bạn...\nfunction bubbleSort(arr) {\n  // logic here\n}"
  } catch (err) {
    console.error(err)
  }
})
</script>

<template>
  <div class="h-screen flex flex-col bg-slate-950 overflow-hidden">
    <!-- Workspace Header -->
    <header class="h-14 border-b border-slate-800 flex items-center justify-between px-4 bg-slate-900 z-10">
      <div class="flex items-center gap-4">
        <router-link to="/" class="p-2 hover:bg-slate-800 rounded-lg text-slate-400">
          <ChevronLeft class="w-5 h-5" />
        </router-link>
        <h2 class="font-bold text-slate-200">{{ exercise?.title || 'Đang tải...' }}</h2>
      </div>
      
      <div class="flex items-center gap-2">
        <button @click="handleRunCode" :disabled="loading" class="flex items-center gap-2 bg-brand-emerald hover:bg-emerald-600 text-white px-4 py-1.5 rounded-lg font-bold text-sm transition-all disabled:opacity-50">
          <Play class="w-4 h-4 fill-current" /> {{ loading ? 'EXECUTING...' : 'RUN CODE' }}
        </button>
        <button class="bg-slate-800 hover:bg-slate-700 text-slate-300 p-2 rounded-lg transition-all">
          <Save class="w-4 h-4" />
        </button>
      </div>
    </header>

    <!-- Workspace Body -->
    <div class="flex-1 flex overflow-hidden">
      <!-- Visualizer (Left) -->
      <div class="flex-1 border-r border-slate-800 flex flex-col">
        <div class="flex-1 bg-brand-slate flex items-center justify-center relative overflow-hidden">
           <svg ref="svgRef" width="600" height="300" class="drop-shadow-2xl"></svg>
           
           <!-- Overlay Info -->
           <div v-if="currentStepInfo" class="absolute top-4 left-4 bg-slate-900/80 p-3 rounded-lg border border-slate-700 text-xs font-mono">
              <div class="text-brand-emerald">ACTION: {{ currentStepInfo.a.toUpperCase() }}</div>
              <div class="text-slate-400">TARGETS: {{ currentStepInfo.t.join(', ') }}</div>
              <div v-if="currentStepInfo.l" class="text-brand-indigo">LINE: {{ currentStepInfo.l }}</div>
           </div>
        </div>
        
        <!-- Playback Controls -->
        <div class="h-16 border-t border-slate-800 bg-slate-900 flex items-center justify-center gap-6">
          <button class="text-slate-400 hover:text-brand-emerald transition-colors"><RotateCcw class="w-5 h-5" /></button>
          <button class="text-slate-400 hover:text-brand-emerald transition-colors"><ChevronLeft class="w-6 h-6" /></button>
          <button @click="nextStep" class="w-10 h-10 bg-brand-emerald/10 text-brand-emerald rounded-full flex items-center justify-center hover:bg-brand-emerald/20 transition-all active:scale-95">
            <Play class="w-5 h-5 fill-current" />
          </button>
          <button @click="nextStep" class="text-slate-400 hover:text-brand-emerald transition-colors"><ChevronRight class="w-6 h-6" /></button>
          <div class="flex items-center gap-2 text-xs font-mono text-slate-500 bg-slate-800 px-3 py-1 rounded-full">
            SPEED: 1.0x
          </div>
        </div>
      </div>

      <!-- Editor (Right) -->
      <div class="w-[450px] flex flex-col bg-slate-900">
        <div class="h-10 border-b border-slate-800 flex items-center justify-between px-4">
           <span class="text-xs font-bold text-slate-500 tracking-widest uppercase">Editor</span>
           <span class="text-[10px] text-brand-emerald font-bold px-2 py-0.5 bg-brand-emerald/10 rounded">JavaScript</span>
        </div>
        <textarea v-model="code" class="flex-1 bg-slate-950 text-emerald-400 font-mono p-6 outline-none resize-none text-sm leading-relaxed" spellcheck="false"></textarea>
        
        <!-- AI Chat (Bottom Right) -->
        <div class="h-64 border-t border-slate-800 bg-slate-900 flex flex-col">
          <div class="p-3 border-b border-slate-800 flex items-center justify-between">
            <div class="flex items-center gap-2">
              <Sparkles class="w-4 h-4 text-brand-emerald" />
              <span class="text-xs font-bold text-slate-300">AI TUTOR MENTOR</span>
            </div>
            <span v-if="aiLoading" class="text-[10px] text-brand-emerald animate-pulse">THINKING...</span>
          </div>
          <div class="flex-1 p-4 text-sm space-y-4 overflow-y-auto bg-slate-950/50">
             <div v-for="(msg, idx) in aiMessages" :key="idx" 
                  :class="msg.role === 'ai' ? 'text-slate-300 italic' : 'text-brand-emerald font-bold'">
                {{ msg.role === 'ai' ? '🤖' : '👤' }} {{ msg.text }}
             </div>
          </div>
          <div class="p-3">
             <button @click="askAI" :disabled="aiLoading" class="w-full py-2 bg-slate-800 hover:bg-slate-700 text-brand-emerald text-xs font-bold rounded-lg transition-all border border-brand-emerald/20 disabled:opacity-50">
               {{ aiLoading ? 'PLEASE WAIT...' : 'ASK AI FOR HINT' }}
             </button>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
textarea {
  tab-size: 2;
}
</style>
