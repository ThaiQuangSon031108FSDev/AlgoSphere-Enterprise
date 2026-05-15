<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { API_BASE } from '../utils/api'
import { ChevronLeft, MessageSquare, User, Clock, Send, ThumbsUp } from 'lucide-vue-next'

const route  = useRoute()
const router = useRouter()

const discussionId = Number(route.params.id)
const discussion   = ref<any>(null)
const loading      = ref(true)
const commenting   = ref(false)
const newComment   = ref('')
const commentError = ref('')

const load = async () => {
  loading.value = true
  try {
    const res = await fetch(`${API_BASE}/forums/discussions/${discussionId}`)
    if (res.ok) discussion.value = await res.json()
  } catch (e) {
    console.error(e)
  } finally {
    loading.value = false
  }
}

const postComment = async () => {
  if (!newComment.value.trim()) { commentError.value = 'Nội dung không được rỗng.'; return }
  const token = localStorage.getItem('token')
  if (!token) { router.push('/login'); return }

  commenting.value = true
  commentError.value = ''
  try {
    const res = await fetch(`${API_BASE}/forums/comments`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json', Authorization: `Bearer ${token}` },
      body: JSON.stringify({
        discussionId,
        userId: 0, // backend extracts from JWT — we send 0 as placeholder
        content: newComment.value,
      }),
    })
    if (!res.ok) throw new Error()
    newComment.value = ''
    await load() // reload to show new comment
  } catch {
    commentError.value = 'Gửi bình luận thất bại. Thử lại.'
  } finally {
    commenting.value = false
  }
}

const formatDate = (d: string) =>
  new Date(d).toLocaleString('vi-VN', { day:'2-digit', month:'2-digit', year:'numeric', hour:'2-digit', minute:'2-digit' })

onMounted(load)
</script>

<template>
  <div class="p-8 max-w-4xl mx-auto">

    <!-- Loading -->
    <div v-if="loading" class="space-y-6 animate-pulse">
      <div class="h-8 rounded-xl w-2/3" style="background:#0A1628;" />
      <div class="h-40 rounded-2xl" style="background:#0A1628;" />
    </div>

    <template v-else-if="discussion">
      <!-- Back -->
      <button @click="router.back()" class="flex items-center gap-2 text-slate-500 hover:text-emerald-400 text-sm mb-6 transition-colors">
        <ChevronLeft class="w-4 h-4" /> Quay lại
      </button>

      <!-- Post -->
      <article class="rounded-2xl p-6 mb-8" style="background:#0A1628;border:1px solid rgba(16,185,129,0.15);">
        <div class="flex items-center gap-2 text-xs text-slate-500 mb-4">
          <User class="w-3.5 h-3.5" />
          <span class="font-bold text-slate-300">{{ discussion.author }}</span>
          <span>•</span>
          <Clock class="w-3.5 h-3.5" />
          <span>{{ formatDate(discussion.createdAt) }}</span>
        </div>

        <h1 class="text-2xl font-bold text-slate-100 mb-4">{{ discussion.title }}</h1>

        <!-- Render content with line breaks -->
        <div class="text-slate-300 text-sm leading-relaxed whitespace-pre-wrap">{{ discussion.content }}</div>

        <div class="flex items-center gap-4 mt-6 pt-4" style="border-top:1px solid rgba(255,255,255,0.06);">
          <button class="flex items-center gap-1.5 text-xs text-slate-500 hover:text-emerald-400 transition-colors">
            <ThumbsUp class="w-3.5 h-3.5" /> Hữu ích
          </button>
          <span class="text-xs text-slate-600">{{ discussion.comments?.length ?? 0 }} bình luận</span>
        </div>
      </article>

      <!-- Comments -->
      <section>
        <h2 class="text-sm font-bold text-slate-400 uppercase tracking-widest mb-4 flex items-center gap-2">
          <MessageSquare class="w-4 h-4" />
          Bình luận ({{ discussion.comments?.length ?? 0 }})
        </h2>

        <div class="space-y-3 mb-8">
          <div v-for="c in discussion.comments" :key="c.id"
            class="rounded-xl p-4" style="background:#060D16;border:1px solid rgba(255,255,255,0.05);">
            <div class="flex items-center gap-2 text-xs text-slate-500 mb-2">
              <User class="w-3 h-3" />
              <span class="font-bold text-slate-400">{{ c.author }}</span>
              <span>•</span>
              <span>{{ formatDate(c.createdAt) }}</span>
            </div>
            <p class="text-slate-300 text-sm leading-relaxed whitespace-pre-wrap">{{ c.content }}</p>
          </div>

          <div v-if="!discussion.comments?.length" class="text-center py-8 text-slate-600 text-sm">
            Chưa có bình luận. Hãy là người đầu tiên! 👇
          </div>
        </div>

        <!-- Reply box -->
        <div class="rounded-2xl p-5" style="background:#0A1628;border:1px solid rgba(255,255,255,0.06);">
          <h3 class="text-sm font-bold text-slate-300 mb-3">Viết bình luận</h3>
          <textarea v-model="newComment" placeholder="Chia sẻ suy nghĩ của bạn..." rows="4"
            class="w-full px-4 py-3 rounded-xl text-sm mb-3 outline-none resize-none"
            style="background:#060D16;border:1px solid rgba(255,255,255,0.08);color:#F1F5F9;font-family:inherit;" />
          <p v-if="commentError" class="text-red-400 text-xs mb-3">{{ commentError }}</p>
          <div class="flex justify-end">
            <button @click="postComment" :disabled="commenting"
              class="flex items-center gap-2 px-5 py-2.5 rounded-xl font-bold text-sm transition-all"
              style="background:#10B981;color:#000;">
              <Send class="w-3.5 h-3.5" />
              {{ commenting ? 'Đang gửi...' : 'Gửi bình luận' }}
            </button>
          </div>
        </div>
      </section>
    </template>

    <!-- Not found -->
    <div v-else class="text-center py-24 text-slate-500">
      <MessageSquare class="w-10 h-10 mx-auto mb-3 text-slate-700" />
      <p>Không tìm thấy bài thảo luận này.</p>
    </div>

  </div>
</template>
