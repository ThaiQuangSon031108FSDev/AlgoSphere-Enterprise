<template>
  <div class="recursion-tree-visualizer flex flex-col items-center justify-start p-6 bg-slate-950/80 backdrop-blur border border-slate-800 rounded-xl w-full h-[450px] overflow-hidden relative">
    <div class="w-full flex items-center justify-between border-b border-slate-800/80 pb-4 mb-4 z-10">
      <div class="flex items-center space-x-2">
        <span class="text-xs font-mono text-slate-500 uppercase tracking-widest">DS Mode:</span>
        <span class="text-xs font-mono font-semibold px-2 py-0.5 rounded bg-sky-500/10 text-sky-400 border border-sky-500/25">
          Recursion Tree
        </span>
      </div>
      <div class="flex items-center space-x-3 text-[10px] font-mono">
        <div class="flex items-center space-x-1">
          <span class="w-2 h-2 rounded-full bg-slate-800 border border-slate-700"></span>
          <span class="text-slate-400">Executing</span>
        </div>
        <div class="flex items-center space-x-1">
          <span class="w-2 h-2 rounded-full bg-emerald-500/20 border border-emerald-500"></span>
          <span class="text-slate-400">Returned</span>
        </div>
      </div>
    </div>

    <!-- Empty State -->
    <div v-if="!calls || calls.length === 0" class="flex flex-col items-center justify-center space-y-3 text-slate-500 my-auto animate-pulse">
      <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor" class="w-12 h-12 stroke-slate-700">
        <path stroke-linecap="round" stroke-linejoin="round" d="M18 18.72a9.094 9.094 0 003.741-.479 3 3 0 00-4.682-2.72m.94 3.198l.001.031c0 .225-.012.447-.037.666A11.944 11.944 0 0112 21c-2.17 0-4.207-.576-5.963-1.584A6.062 6.062 0 016 18.719m12 0a5.971 5.971 0 00-.941-3.197m0 0A5.995 5.995 0 0012 12.75a5.995 5.995 0 00-5.058 2.772m0 0a3 3 0 00-4.681 2.72 8.986 8.986 0 003.74.477m.94-3.197a5.971 5.971 0 00-.94 3.197M15 6.75a3 3 0 11-6 0 3 3 0 016 0zm6 3a2.25 2.25 0 11-4.5 0 2.25 2.25 0 014.5 0zm-13.5 0a2.25 2.25 0 11-4.5 0 2.25 2.25 0 014.5 0z" />
      </svg>
      <p class="font-mono text-sm">No recursion trace data found. Add recursion tree calls to visualize.</p>
    </div>

    <!-- D3 Canvas Container -->
    <div v-else class="w-full h-full relative">
      <svg ref="svgRef" class="w-full h-full min-h-[300px]"></svg>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, watch } from 'vue';
import * as d3 from 'd3';

const props = defineProps<{
  dsState: Record<string, any>;
}>();

const svgRef = ref<SVGSVGElement | null>(null);

const recursionData = computed(() => {
  return props.dsState['recursion'] || { calls: [] };
});

const calls = computed(() => {
  return recursionData.value.calls;
});

// Reconstruct standard parent-child tree hierarchy from flat list of calls using call depth
const buildTreeHierarchy = (flatCalls: any[]) => {
  if (flatCalls.length === 0) return null;

  const root = {
    id: flatCalls[0].id,
    fn: flatCalls[0].fn,
    args: flatCalls[0].args,
    returned: flatCalls[0].returned,
    retVal: flatCalls[0].retVal,
    children: [] as any[]
  };

  const stack = [root];

  for (let i = 1; i < flatCalls.length; i++) {
    const call = flatCalls[i];
    const node = {
      id: call.id,
      fn: call.fn,
      args: call.args,
      returned: call.returned,
      retVal: call.retVal,
      children: []
    };

    // Pop from stack until the top is at depth = call.depth - 1
    while (stack.length > call.depth + 1) {
      stack.pop();
    }

    if (stack.length > 0) {
      stack[stack.length - 1].children.push(node);
      stack.push(node);
    }
  }

  return root;
};

const drawTree = () => {
  if (!svgRef.value || !calls.value || calls.value.length === 0) return;

  const svg = d3.select(svgRef.value);
  svg.selectAll('*').remove();

  const width = svgRef.value.clientWidth || 500;
  const height = svgRef.value.clientHeight || 300;

  const treeRoot = buildTreeHierarchy(calls.value);
  if (!treeRoot) return;

  const rootNode = d3.hierarchy(treeRoot) as any;

  const treeLayout = d3.tree()
    .size([width - 80, height - 100]);

  treeLayout(rootNode);

  // Translate root layout for perfect centering
  const g = svg.append('g').attr('transform', 'translate(40, 40)');

  // Render parent-child paths (curved diagonal lines)
  g.append('g')
    .selectAll('path')
    .data(rootNode.links())
    .enter().append('path')
    .attr('fill', 'none')
    .attr('stroke', '#334155')
    .attr('stroke-width', 2)
    .attr('d', d3.linkVertical()
      .x((d: any) => d.x)
      .y((d: any) => d.y) as any
    );

  // Render tree node groups
  const node = g.append('g')
    .selectAll('g.node')
    .data(rootNode.descendants())
    .enter().append('g')
    .attr('class', 'node')
    .attr('transform', (d: any) => `translate(${d.x},${d.y})`);

  // Node boundary circles
  node.append('circle')
    .attr('r', 16)
    .attr('fill', (d: any) => d.data.returned ? '#10B981' : '#1E293B')
    .attr('fill-opacity', 0.15)
    .attr('stroke', (d: any) => d.data.returned ? '#10B981' : '#475569')
    .attr('stroke-width', 2.5);

  // Label showing arguments, e.g. "f(5)" or "f(4)"
  node.append('text')
    .attr('text-anchor', 'middle')
    .attr('dy', '-1.6em')
    .attr('fill', '#E2E8F0')
    .attr('font-size', '10px')
    .attr('font-family', 'monospace')
    .attr('font-weight', 'semibold')
    .text((d: any) => `${d.data.fn}(${d.data.args.join(', ')})`);

  // Label showing computed return value
  node.append('text')
    .attr('text-anchor', 'middle')
    .attr('dy', '0.35em')
    .attr('fill', (d: any) => d.data.returned ? '#34D399' : '#94A3B8')
    .attr('font-size', '11px')
    .attr('font-family', 'monospace')
    .attr('font-weight', 'bold')
    .text((d: any) => d.data.returned ? d.data.retVal : '?');
};

onMounted(() => {
  drawTree();
});

watch(calls, () => {
  drawTree();
}, { deep: true });
</script>

<style scoped>
.node circle {
  transition: fill 0.3s ease, stroke 0.3s ease;
}
</style>
