<script setup lang="ts">
import { ref, onMounted, onBeforeUnmount, watch } from 'vue'
import loader from '@monaco-editor/loader'

interface Props {
  modelValue: string
  language?: string
  highlightLine?: number | null
  readOnly?: boolean
}

const props = withDefaults(defineProps<Props>(), {
  language: 'javascript',
  highlightLine: null,
  readOnly: false,
})

const emit = defineEmits<{
  'update:modelValue': [value: string]
  'deltas': [deltas: any[]]
}>()

const containerRef = ref<HTMLDivElement | null>(null)
let editor: any = null
let decorations: string[] = []
const deltas = ref<any[]>([])

onMounted(async () => {
  if (!containerRef.value) return

  // Configure loader to use CDN
  loader.config({
    paths: { vs: 'https://cdn.jsdelivr.net/npm/monaco-editor@0.52.0/min/vs' },
  })

  const monaco = await loader.init()

  // Define AlgoSphere dark theme
  monaco.editor.defineTheme('algosphere-dark', {
    base: 'vs-dark',
    inherit: true,
    rules: [
      { token: 'keyword', foreground: '10B981', fontStyle: 'bold' },
      { token: 'string', foreground: '34D399' },
      { token: 'number', foreground: '60A5FA' },
      { token: 'comment', foreground: '475569', fontStyle: 'italic' },
      { token: 'type', foreground: 'A78BFA' },
      { token: 'function', foreground: 'FBBF24' },
    ],
    colors: {
      'editor.background': '#020408',
      'editor.foreground': '#E2E8F0',
      'editorLineNumber.foreground': '#2D3748',
      'editorLineNumber.activeForeground': '#10B981',
      'editor.selectionBackground': '#10B98130',
      'editor.inactiveSelectionBackground': '#10B98115',
      'editorCursor.foreground': '#10B981',
      'editor.lineHighlightBackground': '#0A1628',
      'editor.lineHighlightBorder': '#1E293B',
      'editorGutter.background': '#020408',
      'scrollbar.shadow': '#00000000',
      'scrollbarSlider.background': '#10B98130',
      'scrollbarSlider.hoverBackground': '#10B98155',
      'scrollbarSlider.activeBackground': '#10B98180',
    },
  })

  editor = monaco.editor.create(containerRef.value, {
    value: props.modelValue,
    language: props.language,
    theme: 'algosphere-dark',
    readOnly: props.readOnly,
    fontSize: 13,
    fontFamily: "'JetBrains Mono', 'Fira Code', monospace",
    fontLigatures: true,
    lineHeight: 22,
    minimap: { enabled: false },
    scrollBeyondLastLine: false,
    renderLineHighlight: 'all',
    overviewRulerLanes: 0,
    hideCursorInOverviewRuler: true,
    padding: { top: 16, bottom: 16 },
    suggest: { showWords: false },
    automaticLayout: true,
    tabSize: 2,
    wordWrap: 'on',
    smoothScrolling: true,
    cursorBlinking: 'phase',
    cursorSmoothCaretAnimation: 'on',
    bracketPairColorization: { enabled: true },
  })

  // Emit changes & track deltas
  editor.onDidChangeModelContent((e: any) => {
    const val = editor.getValue()
    emit('update:modelValue', val)

    // Track deltas for Anti-Cheat
    const now = Date.now()
    e.changes.forEach((change: any) => {
      const type = change.text.length > 1 ? 'paste' : (change.rangeLength > 0 && change.text.length === 0 ? 'delete' : 'type')
      deltas.value.push({
        t: now,
        a: type,
        l: change.text.length,
        rl: change.rangeLength
      })
    })
    
    // Periodically emit deltas (or could just expose via ref)
    emit('deltas', [...deltas.value])
  })

  // Highlight initial line if set
  if (props.highlightLine !== null) {
    applyHighlight(props.highlightLine)
  }
})

const applyHighlight = (line: number | null) => {
  if (!editor) return

  if (line === null || line <= 0) {
    decorations = editor.deltaDecorations(decorations, [])
    return
  }

  decorations = editor.deltaDecorations(decorations, [
    {
      range: { startLineNumber: line, startColumn: 1, endLineNumber: line, endColumn: 1 },
      options: {
        isWholeLine: true,
        className: 'monaco-highlight-line',
        glyphMarginClassName: 'monaco-highlight-glyph',
        linesDecorationsClassName: 'monaco-highlight-decoration',
      },
    },
  ])

  // Scroll highlighted line into view
  editor.revealLineInCenterIfOutsideViewport(line)
}

watch(() => props.modelValue, (newVal) => {
  if (editor && editor.getValue() !== newVal) {
    editor.setValue(newVal)
  }
})

watch(() => props.highlightLine, (line) => {
  applyHighlight(line ?? null)
})

onBeforeUnmount(() => {
  editor?.dispose()
})

defineExpose({
  getDeltas: () => [...deltas.value],
  resetDeltas: () => { deltas.value = [] }
})
</script>

<template>
  <div ref="containerRef" class="monaco-container" />
</template>

<style scoped>
.monaco-container {
  width: 100%;
  height: 100%;
  overflow: hidden;
}
</style>

<style>
/* Global Monaco highlight styles — not scoped */
.monaco-highlight-line {
  background: rgba(16, 185, 129, 0.12) !important;
  border-left: 2px solid #10B981 !important;
}
.monaco-highlight-glyph {
  background: #10B981 !important;
  border-radius: 50%;
  width: 8px !important;
  height: 8px !important;
  margin-top: 7px;
  margin-left: 4px;
}
</style>
