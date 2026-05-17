<template>
  <div class="two-pointer-visualizer flex flex-col items-center justify-start p-6 bg-slate-950/80 backdrop-blur border border-slate-800 rounded-xl w-full h-[450px] overflow-hidden">
    <div class="w-full flex items-center justify-between border-b border-slate-800/80 pb-4 mb-6">
      <div class="flex items-center space-x-2">
        <span class="text-xs font-mono text-slate-500 uppercase tracking-widest">DS Mode:</span>
        <span class="text-xs font-mono font-semibold px-2 py-0.5 rounded bg-sky-500/10 text-sky-400 border border-sky-500/25">
          Two-Pointer / Sliding Window
        </span>
      </div>
      <div class="flex items-center space-x-4">
        <div v-for="(_, name) in activePointers" :key="name" class="flex items-center space-x-1.5 text-xs font-mono">
          <span 
            class="w-2.5 h-2.5 rounded-full"
            :style="{ backgroundColor: getPointerColor(String(name)) }"
          ></span>
          <span class="text-slate-300 font-bold uppercase">{{ name }}:</span>
          <span class="text-slate-400">Index {{ activePointers[name].pos }} (val: {{ activePointers[name].val }})</span>
        </div>
      </div>
    </div>

    <!-- Empty State -->
    <div v-if="array.length === 0" class="flex flex-col items-center justify-center space-y-3 text-slate-500 my-auto animate-pulse">
      <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor" class="w-12 h-12 stroke-slate-700">
        <path stroke-linecap="round" stroke-linejoin="round" d="M3 7.5L7.5 3m0 0L12 7.5M7.5 3v13.5m13.5 0L16.5 21m0 0L12 16.5m4.5 4.5V7.5" />
      </svg>
      <p class="font-mono text-sm">No array elements found. Run a pointer algorithm to visualize.</p>
    </div>

    <!-- Pointers Overlay Array Visualizer -->
    <div v-else class="flex flex-col items-center justify-center w-full my-auto space-y-6">
      <div class="flex items-center space-x-3 justify-center flex-wrap gap-y-4">
        <div 
          v-for="(item, idx) in array" 
          :key="idx" 
          class="relative flex flex-col items-center"
        >
          <!-- Pointer Arrows (Top) -->
          <div class="h-16 flex flex-col justify-end items-center space-y-1 relative w-16 mb-2">
            <div 
              v-for="(_, name) in activePointersAtIdx(idx)" 
              :key="name"
              class="flex flex-col items-center animate-bounce-slow"
            >
              <span 
                class="text-[10px] font-mono font-bold px-1.5 py-0.5 rounded shadow border uppercase text-slate-950"
                :style="{ 
                  backgroundColor: getPointerColor(String(name)),
                  borderColor: getPointerColor(String(name))
                }"
              >
                {{ name }}
              </span>
              <svg class="w-3 h-3 -mt-0.5" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
                <path d="M12 21L12 3M12 21L5 14M12 21L19 14" stroke-width="3" stroke-linecap="round" stroke-linejoin="round" :stroke="getPointerColor(String(name))"/>
              </svg>
            </div>
          </div>

          <!-- Array Square element -->
          <div 
            class="w-16 h-16 bg-slate-900 border-2 rounded-xl flex items-center justify-center shadow-lg hover:scale-105 transition-all"
            :class="[
              isIdxActive(idx) ? 'border-sky-500 bg-sky-500/5' : 'border-slate-800 hover:border-slate-700'
            ]"
          >
            <span class="font-mono text-slate-100 font-bold text-xl">{{ item }}</span>
          </div>

          <!-- Bottom index tag -->
          <span class="text-xs font-mono text-slate-600 mt-2">idx: {{ idx }}</span>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue';

const props = defineProps<{
  array: any[];
  dsState: Record<string, any>;
}>();

// Map pointers to a color palette (Amber, Sky Blue, Orange, Pink, Lime, Teal, Emerald)
const pointerColors: Record<string, string> = {
  left: '#F97316',   // Warm Orange
  right: '#0EA5E9',  // Sky Blue
  i: '#F59E0B',      // Amber
  j: '#10B981',      // Emerald
  ptr: '#EC4899',    // Pink
  slow: '#84CC16',   // Lime
  fast: '#06B6D4'    // Teal
};

const getPointerColor = (name: string): string => {
  const norm = name.toLowerCase();
  return pointerColors[norm] || pointerColors.ptr;
};

// Retrieve active pointers
const activePointers = computed(() => {
  return props.dsState['pointers'] || {};
});

// Find all pointers sitting at a specific index
const activePointersAtIdx = (idx: number) => {
  const matches: Record<string, any> = {};
  Object.entries(activePointers.value).forEach(([name, data]: [string, any]) => {
    if (data.pos === idx) {
      matches[name] = data;
    }
  });
  return matches;
};

const isIdxActive = (idx: number) => {
  return Object.values(activePointers.value).some((data: any) => data.pos === idx);
};
</script>

<style scoped>
.animate-bounce-slow {
  animation: bounce 2s infinite;
}

@keyframes bounce {
  0%, 100% {
    transform: translateY(0);
    animation-timing-function: cubic-bezier(0.8,0,1,1);
  }
  50% {
    transform: translateY(-4px);
    animation-timing-function: cubic-bezier(0,0,0.2,1);
  }
}
</style>
