<template>
  <div class="hashmap-visualizer flex flex-col items-center justify-start p-6 bg-slate-950/80 backdrop-blur border border-slate-800 rounded-xl w-full h-[450px] overflow-hidden">
    <div class="w-full flex items-center justify-between border-b border-slate-800/80 pb-4 mb-4">
      <div class="flex items-center space-x-2">
        <span class="text-xs font-mono text-slate-500 uppercase tracking-widest">DS Mode:</span>
        <span class="text-xs font-mono font-semibold px-2 py-0.5 rounded bg-emerald-500/10 text-emerald-400 border border-emerald-500/25">
          HashMap (Buckets)
        </span>
      </div>
      <div class="text-xs font-mono text-slate-400">
        Total Size: <span class="text-emerald-400 font-bold font-mono">{{ Object.keys(items).length }}</span> keys
      </div>
    </div>

    <!-- Empty State -->
    <div v-if="Object.keys(items).length === 0" class="flex flex-col items-center justify-center space-y-3 text-slate-500 my-auto animate-pulse">
      <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor" class="w-12 h-12 stroke-slate-700">
        <path stroke-linecap="round" stroke-linejoin="round" d="M19.5 12c0-1.232-.046-2.453-.138-3.662a4.006 4.006 0 00-3.7-3.7 48.656 48.656 0 00-7.324 0 4.006 4.006 0 00-3.7 3.7C4.547 9.547 4.5 10.768 4.5 12s.047 2.453.138 3.662a4.006 4.006 0 003.7 3.7 48.656 48.656 0 007.324 0 4.006 4.006 0 003.7-3.7C19.453 14.453 19.5 13.232 19.5 12z" />
      </svg>
      <p class="font-mono text-sm">HashMap is empty. Set key-value pairs to view mapping.</p>
    </div>

    <!-- Bucket View -->
    <div v-else class="w-full h-full overflow-y-auto px-2 space-y-3">
      <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4 py-2">
        <div 
          v-for="bucket in buckets" 
          :key="bucket.index" 
          class="bucket-card flex flex-col p-4 bg-slate-900/60 border border-slate-800 rounded-xl hover:border-slate-700 transition-all shadow-md relative"
          :class="{
            'border-amber-500/30 bg-amber-500/5 shadow-amber-950/10': bucket.active
          }"
        >
          <div class="flex justify-between items-center mb-2">
            <span class="text-xs font-mono text-slate-500 uppercase">Bucket {{ bucket.index }}</span>
            <span v-if="bucket.active" class="w-1.5 h-1.5 rounded-full bg-amber-400 animate-pulse"></span>
          </div>

          <div class="flex flex-col space-y-2">
            <div 
              v-for="pair in bucket.pairs" 
              :key="pair.key"
              class="pair-badge flex items-center justify-between px-3 py-2 bg-slate-950 border border-slate-800 rounded-lg group hover:border-emerald-500/30 transition-colors shadow-inner"
            >
              <div class="flex items-center space-x-2">
                <span class="text-xs font-mono text-emerald-400 font-bold bg-emerald-500/10 px-1.5 py-0.5 rounded">{{ pair.key }}</span>
                <span class="text-slate-400 font-mono">→</span>
                <span class="text-sm font-mono text-slate-200 font-bold">{{ pair.value }}</span>
              </div>
            </div>
            <div v-if="bucket.pairs.length === 0" class="text-xs font-mono text-slate-600 italic py-2">
              Empty
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue';

const props = defineProps<{
  dsState: Record<string, any>;
}>();

// Simple Hash function to distribute keys into 8 buckets (0-7)
const hashString = (str: string, bucketCount: number = 8) => {
  let hash = 0;
  for (let i = 0; i < str.length; i++) {
    hash = str.charCodeAt(i) + ((hash << 5) - hash);
  }
  return Math.abs(hash) % bucketCount;
};

// Retrieve active map container
const activeMap = computed(() => {
  const keys = Object.keys(props.dsState).filter(k => props.dsState[k]?.type === 'Map');
  if (keys.length === 0) return null;
  return props.dsState[keys[0]];
});

const items = computed(() => {
  return activeMap.value?.items || {};
});

// Group elements into buckets
const buckets = computed(() => {
  const bucketCount = 8;
  const list = Array.from({ length: bucketCount }, (_, idx) => ({
    index: idx,
    pairs: [] as Array<{ key: string; value: any }>,
    active: false
  }));

  Object.entries(items.value).forEach(([key, val]) => {
    const bucketIdx = hashString(key, bucketCount);
    list[bucketIdx].pairs.push({ key, value: val });
  });

  return list;
});
</script>

<style scoped>
.bucket-card {
  transition: transform 0.2s cubic-bezier(0.16, 1, 0.3, 1), border-color 0.2s;
}
.bucket-card:hover {
  transform: translateY(-2px);
}
</style>
