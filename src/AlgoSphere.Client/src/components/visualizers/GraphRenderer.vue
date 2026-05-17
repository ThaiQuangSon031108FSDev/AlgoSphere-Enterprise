<template>
  <div class="graph-visualizer flex flex-col items-center justify-start p-6 bg-slate-950/80 backdrop-blur border border-slate-800 rounded-xl w-full h-[450px] overflow-hidden relative">
    <div class="w-full flex items-center justify-between border-b border-slate-800/80 pb-4 mb-4 z-10">
      <div class="flex items-center space-x-2">
        <span class="text-xs font-mono text-slate-500 uppercase tracking-widest">DS Mode:</span>
        <span class="text-xs font-mono font-semibold px-2 py-0.5 rounded bg-emerald-500/10 text-emerald-400 border border-emerald-500/25">
          Graph (BFS/DFS Traversal)
        </span>
      </div>
      <div class="flex items-center space-x-3 text-[10px] font-mono">
        <div class="flex items-center space-x-1">
          <span class="w-2 h-2 rounded-full bg-slate-800 border border-slate-700"></span>
          <span class="text-slate-400">Unvisited</span>
        </div>
        <div class="flex items-center space-x-1">
          <span class="w-2 h-2 rounded-full bg-amber-500/20 border border-amber-500 animate-pulse"></span>
          <span class="text-slate-400">Visiting</span>
        </div>
        <div class="flex items-center space-x-1">
          <span class="w-2 h-2 rounded-full bg-emerald-500/20 border border-emerald-500"></span>
          <span class="text-slate-400">Visited</span>
        </div>
      </div>
    </div>

    <!-- Empty State -->
    <div v-if="!graphData || (graphData.edges.length === 0 && Object.keys(graphData.nodeStates).length === 0)" class="flex flex-col items-center justify-center space-y-3 text-slate-500 my-auto animate-pulse">
      <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor" class="w-12 h-12 stroke-slate-700">
        <path stroke-linecap="round" stroke-linejoin="round" d="M18 18.72a9.094 9.094 0 003.741-.479 3 3 0 00-4.682-2.72m.94 3.198l.001.031c0 .225-.012.447-.037.666A11.944 11.944 0 0112 21c-2.17 0-4.207-.576-5.963-1.584A6.062 6.062 0 016 18.719m12 0a5.971 5.971 0 00-.941-3.197m0 0A5.995 5.995 0 0012 12.75a5.995 5.995 0 00-5.058 2.772m0 0a3 3 0 00-4.681 2.72 8.986 8.986 0 003.74.477m.94-3.197a5.971 5.971 0 00-.94 3.197M15 6.75a3 3 0 11-6 0 3 3 0 016 0zm6 3a2.25 2.25 0 11-4.5 0 2.25 2.25 0 014.5 0zm-13.5 0a2.25 2.25 0 11-4.5 0 2.25 2.25 0 014.5 0z" />
      </svg>
      <p class="font-mono text-sm">No graph node or edge traces found. Add node visits to visualize.</p>
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

const graphData = computed(() => {
  return props.dsState['graph'];
});

const getVisitedStateColor = (state: string): string => {
  if (state === 'visiting') return '#F59E0B'; // Amber / Orange
  if (state === 'visited') return '#10B981';  // Emerald Green
  return '#1E293B'; // Unvisited Slate
};

const getVisitedStateStroke = (state: string): string => {
  if (state === 'visiting') return '#F59E0B';
  if (state === 'visited') return '#10B981';
  return '#475569';
};

const drawGraph = () => {
  if (!svgRef.value || !graphData.value) return;

  const svg = d3.select(svgRef.value);
  svg.selectAll('*').remove();

  const width = svgRef.value.clientWidth || 500;
  const height = svgRef.value.clientHeight || 300;
  const padding = 30;

  // Derive unique nodes from edges and nodeStates
  const nodeIds = new Set<string>();
  graphData.value.edges.forEach((e: any) => {
    nodeIds.add(e.u.toString());
    nodeIds.add(e.v.toString());
  });
  Object.keys(graphData.value.nodeStates).forEach(id => nodeIds.add(id.toString()));

  const nodes = Array.from(nodeIds).map(id => ({
    id,
    state: graphData.value.nodeStates[id] || 'unvisited'
  }));

  const links = graphData.value.edges.map((e: any) => ({
    source: e.u.toString(),
    target: e.v.toString(),
    weight: e.weight,
    directed: e.directed
  }));

  if (nodes.length === 0) return;

  const simulation = d3.forceSimulation(nodes as any)
    .force('link', d3.forceLink(links).id((d: any) => d.id).distance(120).strength(0.8))
    .force('charge', d3.forceManyBody().strength(-220))
    .force('center', d3.forceCenter(width / 2, height / 2))
    .force('collision', d3.forceCollide(30))
    .force('bound', () => {
      nodes.forEach((node: any) => {
        node.x = Math.max(padding, Math.min(width - padding, node.x ?? width / 2));
        node.y = Math.max(padding, Math.min(height - padding, node.y ?? height / 2));
      });
    });

  // Render edge arrowhead marker definitions
  svg.append('defs').append('marker')
    .attr('id', 'graph-arrowhead')
    .attr('viewBox', '-0 -5 10 10')
    .attr('refX', 22)
    .attr('refY', 0)
    .attr('orient', 'auto')
    .attr('markerWidth', 5)
    .attr('markerHeight', 5)
    .attr('xoverflow', 'visible')
    .append('path')
    .attr('d', 'M 0,-4 L 8 ,0 L 0,4')
    .attr('fill', '#475569');

  // Render edges
  const linkGroup = svg.append('g')
    .selectAll('line')
    .data(links)
    .enter().append('g');

  const linkLine = linkGroup.append('line')
    .attr('stroke', '#334155')
    .attr('stroke-width', 2)
    .attr('marker-end', (d: any) => d.directed ? 'url(#graph-arrowhead)' : null);

  // Render weight labels if present
  const linkText = linkGroup.filter((d: any) => d.weight !== undefined)
    .append('text')
    .attr('text-anchor', 'middle')
    .attr('fill', '#94A3B8')
    .attr('font-size', '10px')
    .attr('font-family', 'monospace')
    .text((d: any) => d.weight);

  // Render nodes
  const node = svg.append('g')
    .selectAll('g.node')
    .data(nodes)
    .enter().append('g')
    .attr('class', 'node')
    .call(d3.drag()
      .on('start', dragstarted)
      .on('drag', dragged)
      .on('end', dragended) as any
    );

  node.append('circle')
    .attr('r', 18)
    .attr('fill', (d: any) => getVisitedStateColor(d.state))
    .attr('fill-opacity', 0.15)
    .attr('stroke', (d: any) => getVisitedStateStroke(d.state))
    .attr('stroke-width', 2.5);

  node.append('text')
    .attr('text-anchor', 'middle')
    .attr('dy', '.35em')
    .attr('fill', '#FFF')
    .attr('font-size', '12px')
    .attr('font-weight', 'bold')
    .attr('font-family', 'monospace')
    .text((d: any) => d.id);

  simulation.on('tick', () => {
    linkLine
      .attr('x1', (d: any) => d.source.x)
      .attr('y1', (d: any) => d.source.y)
      .attr('x2', (d: any) => d.target.x)
      .attr('y2', (d: any) => d.target.y);

    linkText
      .attr('x', (d: any) => (d.source.x + d.target.x) / 2)
      .attr('y', (d: any) => (d.source.y + d.target.y) / 2 - 5);

    node.attr('transform', (d: any) => `translate(${d.x},${d.y})`);
  });

  function dragstarted(event: any, d: any) {
    if (!event.active) simulation.alphaTarget(0.3).restart();
    d.fx = d.x;
    d.fy = d.y;
  }

  function dragged(event: any, d: any) {
    d.fx = event.x;
    d.fy = event.y;
  }

  function dragended(event: any, d: any) {
    if (!event.active) simulation.alphaTarget(0);
    d.fx = null;
    d.fy = null;
  }
};

onMounted(() => {
  drawGraph();
});

watch(graphData, () => {
  drawGraph();
}, { deep: true });
</script>

<style scoped>
.node circle {
  transition: fill 0.3s ease, stroke 0.3s ease;
}
</style>
