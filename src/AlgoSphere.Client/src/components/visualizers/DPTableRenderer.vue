<template>
  <div class="dp-table-visualizer flex flex-col items-center justify-start p-6 bg-slate-950/80 backdrop-blur border border-slate-800 rounded-xl w-full h-[450px] overflow-hidden">
    <div class="w-full flex items-center justify-between border-b border-slate-800/80 pb-4 mb-4">
      <div class="flex items-center space-x-2">
        <span class="text-xs font-mono text-slate-500 uppercase tracking-widest">DS Mode:</span>
        <span class="text-xs font-mono font-semibold px-2 py-0.5 rounded bg-amber-500/10 text-amber-400 border border-amber-500/25">
          Dynamic Programming Table (2D Matrix)
        </span>
      </div>
      <div class="text-xs font-mono text-slate-400">
        Grid Size: <span class="text-amber-400 font-bold font-mono">{{ numRows }} × {{ numCols }}</span>
      </div>
    </div>

    <!-- Empty State -->
    <div v-if="numRows === 0" class="flex flex-col items-center justify-center space-y-3 text-slate-500 my-auto animate-pulse">
      <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor" class="w-12 h-12 stroke-slate-700">
        <path stroke-linecap="round" stroke-linejoin="round" d="M3.75 6A2.25 2.25 0 016 3.75h2.25A2.25 2.25 0 0110.5 6v2.25a2.25 2.25 0 01-2.25 2.25H6a2.25 2.25 0 01-2.25-2.25V6zM3.75 15.75A2.25 2.25 0 016 13.5h2.25a2.25 2.25 0 012.25 2.25V18a2.25 2.25 0 01-2.25 2.25H6A2.25 2.25 0 013.75 18v-2.25zM13.5 6a2.25 2.25 0 012.25-2.25H18A2.25 2.25 0 0120.25 6v2.25A2.25 2.25 0 0118 10.5h-2.25a2.25 2.25 0 01-2.25-2.25V6zM13.5 15.75a2.25 2.25 0 012.25-2.25H18a2.25 2.25 0 012.25 2.25V18A2.25 2.25 0 0118 20.25h-2.25A2.25 2.25 0 0113.5 18v-2.25z" />
      </svg>
      <p class="font-mono text-sm">DP Table is empty. Set cell values inside loops to visualize.</p>
    </div>

    <!-- DP Matrix Grid -->
    <div v-else class="w-full h-full overflow-auto flex items-start justify-center p-2">
      <div class="relative min-w-max border border-slate-800 bg-slate-900/10 rounded-xl p-4">
        <!-- Grid Header Row (Column indexes) -->
        <div class="flex items-center">
          <div class="w-12 h-10 flex items-center justify-center font-mono text-xs text-slate-600 border-b border-r border-slate-800">
            r\c
          </div>
          <div 
            v-for="col in numCols" 
            :key="col-1" 
            class="w-16 h-10 flex items-center justify-center font-mono text-xs text-slate-500 font-bold border-b border-slate-800"
          >
            {{ col-1 }}
          </div>
        </div>

        <!-- Grid Body Rows -->
        <div 
          v-for="row in numRows" 
          :key="row-1" 
          class="flex items-center"
        >
          <!-- Row label -->
          <div class="w-12 h-12 flex items-center justify-center font-mono text-xs text-slate-500 font-bold border-r border-slate-800">
            {{ row-1 }}
          </div>

          <!-- Cells -->
          <div 
            v-for="col in numCols" 
            :key="col-1"
            class="w-16 h-12 flex items-center justify-center relative border border-slate-800/40 hover:border-amber-500/50 hover:bg-slate-800/20 group transition-all cursor-help"
            :class="{
              'bg-amber-500/5 border-amber-500/60 shadow-amber-950/20 shadow-inner z-10': isLastUpdated(row-1, col-1)
            }"
          >
            <span class="font-mono text-sm text-slate-200 font-bold">
              {{ getCellValue(row-1, col-1) }}
            </span>

            <!-- Cell Tooltip showing Formula / Math details -->
            <div 
              v-if="getCellFormula(row-1, col-1)"
              class="absolute bottom-full left-1/2 -translate-x-1/2 mb-2 w-48 bg-slate-900 border border-slate-800 rounded-lg p-2 shadow-2xl opacity-0 pointer-events-none group-hover:opacity-100 group-hover:pointer-events-auto transition-opacity z-50 flex flex-col space-y-1.5"
            >
              <div class="text-[10px] font-mono text-slate-500 uppercase tracking-wider border-b border-slate-800 pb-1">
                Cell formula [{{ row-1 }},{{ col-1 }}]
              </div>
              <div class="text-xs font-mono text-amber-400 leading-normal break-all font-semibold">
                {{ getCellFormula(row-1, col-1) }}
              </div>
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

const dpTable = computed(() => {
  return props.dsState['dpTable'] || { grid: {}, maxRow: -1, maxCol: -1 };
});

const numRows = computed(() => {
  return dpTable.value.maxRow !== -1 ? dpTable.value.maxRow + 1 : 0;
});

const numCols = computed(() => {
  return dpTable.value.maxCol !== -1 ? dpTable.value.maxCol + 1 : 0;
});

const getCellValue = (r: number, c: number) => {
  const key = `${r},${c}`;
  const cell = dpTable.value.grid[key];
  return cell !== undefined ? cell.val : '-';
};

const getCellFormula = (r: number, c: number) => {
  const key = `${r},${c}`;
  const cell = dpTable.value.grid[key];
  return cell?.formula || '';
};

// Check if this cell was the last one updated in the grid
const isLastUpdated = (r: number, c: number) => {
  // Simple heuristic: cell matches the max bounds of updates
  const key = `${r},${c}`;
  const cell = dpTable.value.grid[key];
  if (!cell) return false;
  // Let's check if the table has this cell
  return Object.keys(dpTable.value.grid).pop() === key;
};
</script>

<style scoped>
.dp-table-visualizer ::-webkit-scrollbar {
  width: 8px;
  height: 8px;
}
.dp-table-visualizer ::-webkit-scrollbar-track {
  background: transparent;
}
.dp-table-visualizer ::-webkit-scrollbar-thumb {
  background: #1e293b;
  border-radius: 4px;
}
.dp-table-visualizer ::-webkit-scrollbar-thumb:hover {
  background: #334155;
}
</style>
