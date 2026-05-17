<template>
  <div class="stack-queue-visualizer flex flex-col items-center justify-center p-6 bg-slate-950/80 backdrop-blur border border-slate-800 rounded-xl w-full h-[450px] relative overflow-hidden">
    <div class="absolute top-4 left-4 flex items-center space-x-2">
      <span class="text-xs font-mono text-slate-500 uppercase tracking-widest">DS Mode:</span>
      <span class="text-xs font-mono font-semibold px-2 py-0.5 rounded bg-amber-500/10 text-amber-400 border border-amber-500/25">
        {{ dsType }}
      </span>
    </div>

    <!-- Empty State -->
    <div v-if="items.length === 0" class="flex flex-col items-center justify-center space-y-3 text-slate-500 animate-pulse">
      <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor" class="w-12 h-12 stroke-slate-700">
        <path stroke-linecap="round" stroke-linejoin="round" d="M20.25 7.5l-.625 10.632a2.25 2.25 0 01-2.247 2.118H6.622a2.25 2.25 0 01-2.247-2.118L3.75 7.5M10 11.25h4M3.375 7.5h17.25c.621 0 1.125-.504 1.125-1.125v-1.5c0-.621-.504-1.125-1.125-1.125H3.375c-.621 0-1.125.504-1.125 1.125v1.5c0 .621.504 1.125 1.125 1.125z" />
      </svg>
      <p class="font-mono text-sm">Container is empty. Run operations to push/enqueue items.</p>
    </div>

    <!-- Stack Rendering (LIFO - Vertical) -->
    <div v-else-if="dsType === 'Stack'" class="stack-container flex flex-col-reverse justify-start items-center w-full max-w-[200px] h-[340px] border-b-4 border-x-4 border-slate-700 rounded-b-xl px-4 py-2 relative overflow-y-auto mt-6">
      <TransitionGroup name="stack-node" tag="div" class="flex flex-col-reverse space-y-2 space-y-reverse w-full">
        <div 
          v-for="(item, idx) in items" 
          :key="idx" 
          class="stack-item flex items-center justify-between px-4 py-3 bg-slate-900 border border-slate-800 rounded-lg shadow-lg group hover:border-amber-500/50 transition-colors w-full"
          :class="{
            'border-amber-500 bg-amber-500/5 shadow-amber-950/20': idx === items.length - 1,
            'border-emerald-500/40': idx === 0
          }"
        >
          <div class="flex items-center space-x-3">
            <span class="text-xs font-mono text-slate-500">[{{ idx }}]</span>
            <span class="font-mono text-slate-200 font-bold text-lg">{{ item }}</span>
          </div>
          <span 
            v-if="idx === items.length - 1" 
            class="text-[10px] font-mono px-1.5 py-0.5 rounded bg-amber-500/20 text-amber-400 font-medium uppercase tracking-wider"
          >
            Top
          </span>
          <span 
            v-else-if="idx === 0" 
            class="text-[10px] font-mono px-1.5 py-0.5 rounded bg-emerald-500/20 text-emerald-400 font-medium uppercase tracking-wider"
          >
            Base
          </span>
        </div>
      </TransitionGroup>
    </div>

    <!-- Queue Rendering (FIFO - Horizontal Conveyor Belt) -->
    <div v-else-if="dsType === 'Queue'" class="queue-container flex items-center justify-start w-full max-w-[800px] h-[200px] border-y-2 border-slate-800 relative bg-slate-900/20 rounded-xl px-6 overflow-x-auto">
      <!-- Front Indicator Label -->
      <div v-if="items.length > 0" class="absolute left-2 top-2 text-[10px] font-mono text-emerald-400 uppercase tracking-widest flex items-center space-x-1">
        <span class="w-1.5 h-1.5 rounded-full bg-emerald-400 animate-pulse"></span>
        <span>Front (Dequeue)</span>
      </div>

      <TransitionGroup name="queue-node" tag="div" class="flex space-x-4 items-center min-w-max py-4">
        <div 
          v-for="(item, idx) in items" 
          :key="idx" 
          class="queue-item flex flex-col items-center justify-center w-24 h-24 bg-slate-900 border border-slate-800 rounded-xl shadow-lg relative group hover:border-amber-500/50 transition-colors"
          :class="{
            'border-emerald-500 bg-emerald-500/5': idx === 0,
            'border-amber-500 bg-amber-500/5': idx === items.length - 1
          }"
        >
          <span class="text-[10px] font-mono text-slate-500 absolute top-2 left-2">[{{ idx }}]</span>
          <span class="font-mono text-slate-200 font-bold text-2xl mt-1">{{ item }}</span>
          
          <div class="absolute bottom-2 flex space-x-1">
            <span 
              v-if="idx === 0" 
              class="text-[9px] font-mono px-1 py-0.2 rounded bg-emerald-500/20 text-emerald-400 font-medium uppercase"
            >
              Head
            </span>
            <span 
              v-if="idx === items.length - 1" 
              class="text-[9px] font-mono px-1 py-0.2 rounded bg-amber-500/20 text-amber-400 font-medium uppercase"
            >
              Tail
            </span>
          </div>
        </div>
      </TransitionGroup>

      <!-- Rear Indicator Label -->
      <div v-if="items.length > 0" class="absolute right-2 bottom-2 text-[10px] font-mono text-amber-400 uppercase tracking-widest flex items-center space-x-1">
        <span>Rear (Enqueue)</span>
        <span class="w-1.5 h-1.5 rounded-full bg-amber-400 animate-pulse"></span>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue';

const props = defineProps<{
  dsState: Record<string, any>;
}>();

// Auto-detect whether it is a Stack or Queue, or select the first container
const activeContainer = computed(() => {
  const keys = Object.keys(props.dsState).filter(k => k !== 'pointers' && k !== 'dpTable' && k !== 'graph' && k !== 'recursion');
  if (keys.length === 0) return null;
  return props.dsState[keys[0]];
});

const dsType = computed(() => {
  return activeContainer.value?.type || 'Stack';
});

const items = computed(() => {
  return activeContainer.value?.items || [];
});
</script>

<style scoped>
/* Stack Transition Animations */
.stack-node-enter-active,
.stack-node-leave-active {
  transition: all 0.4s cubic-bezier(0.16, 1, 0.3, 1);
}

.stack-node-enter-from {
  opacity: 0;
  transform: translateY(-30px) scale(0.9);
}

.stack-node-leave-to {
  opacity: 0;
  transform: translateY(30px) scale(0.9);
}

/* Queue Transition Animations */
.queue-node-enter-active,
.queue-node-leave-active {
  transition: all 0.4s cubic-bezier(0.16, 1, 0.3, 1);
}

.queue-node-enter-from {
  opacity: 0;
  transform: translateX(40px) scale(0.8);
}

.queue-node-leave-to {
  opacity: 0;
  transform: translateX(-40px) scale(0.8);
}
</style>
