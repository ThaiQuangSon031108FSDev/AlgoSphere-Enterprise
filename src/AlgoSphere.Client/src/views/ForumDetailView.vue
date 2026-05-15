<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { API_BASE } from '../utils/api'
import { ChevronLeft, MessageSquare, Plus, Clock, User, Eye } from 'lucide-vue-next'

const route  = useRoute()
const router = useRouter()

const forumId   = Number(route.params.id)
const forum     = ref<any>(null)
const discussions = ref<any[]>([])
const loading   = ref(true)
const page      = ref(1)

// Create discussion modal
const showCreate  = ref(false)
const newTitle    = ref('')
const newContent  = ref('')
const creating    = ref(false)
const createError = ref('')

const load = async () => {
  loading.value = true
  try {
    // Get forum metadata from list (reuse forum list endpoint)
    const fRes = await fetch(`${API_BASE}/forums`)
    if (fRes.ok) {
      const forums: any[] = await fRes.json()
      forum.value = forums.find(f => f.id === forumId) ?? { title: `Forum #${forumId}`, description: '' }
    }

    const dRes = await fetch(`${API_BASE}/forums/${forumId}/discussions?page=${page.value}`)
    if (dRes.ok) discussions.value = await dRes.json()
  } catch (e) {
    console.error(e)
  } finally {
    loading.value = false
  }
}

const createDiscussion = async () => {
  if (!newTitle.value.trim() || !newContent.value.trim()) {
    createError.value = 'Vui lòng điền tiêu đề và nội dung.'
    return
  }
  const token = localStorage.getItem('token')
  if (!token) { router.push('/login'); return }

  creating.value = true
  createError.value = ''
  try {
    const res = await fetch(`${API_BASE}/forums/discussions`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json', Authorization: `Bearer ${token}` },
      body: JSON.stringify({ forumId, title: newTitle.value, content: newContent.value }),
    })
    if (!res.ok) throw new Error('API error')
    const id = await res.json()
    showCreate.value = false
    newTitle.value = ''
    newContent.value = ''
    router.push({ name: 'discussion', params: { id } })
  } catch {
    createError.value = 'Tạo bài đăng thất bại. Thử lại.'
  } finally {
    creating.value = false
  }
}

const formatDate = (d: string) => new Date(d).toLocaleDateString('vi-VN', { day:'2-digit', month:'2-digit', year:'numeric' })

onMounted(load)
</script>

<template>
  <div class="p-8 max-w-5xl mx-auto">

    <!-- Back + header -->
    <div class="flex items-center gap-3 mb-8">
      <router-link to="/forum" class="p-2 rounded-lg text-slate-500 hover:text-emerald-400 hover:bg-white/5 transition-all">
        <ChevronLeft class="w-5 h-5" />
      </router-link>
      <div>
        <p class="text-xs font-bold tracking-widest uppercase mb-0.5" style="color:#10B981;">DIỄN ĐÀN</p>
        <h1 class="text-2xl font-bold text-slate-100">{{ forum?.title ?? '...' }}</h1>
        <p v-if="forum?.description" class="text-slate-500 text-sm mt-0.5">{{ forum.description }}</p>
      </div>
    </div>

    <!-- Toolbar -->
    <div class="flex justify-between items-center mb-6">
      <span class="text-xs text-slate-500 font-mono-stat">{{ discussions.length }} bài thảo luận</span>
      <button @click="showCreate = true"
        class="flex items-center gap-2 px-4 py-2 rounded-xl font-bold text-sm transition-all"
        style="background:rgba(16,185,129,0.1);color:#10B981;border:1px solid rgba(16,185,129,0.2);">
        <Plus class="w-4 h-4" /> Tạo thảo luận
      </button>
    </div>

    <!-- Loading -->
    <div v-if="loading" class="space-y-3">
      <div v-for="n in 5" :key="n" class="rounded-xl h-20 animate-pulse"
        style="background:#0A1628;border:1px solid rgba(255,255,255,0.05);" />
    </div>

    <!-- Discussion list -->
    <div v-else class="space-y-3">
      <router-link v-for="d in discussions" :key="d.id"
        :to="{ name: 'discussion', params: { id: d.id } }"
        class="flex items-start gap-4 p-4 rounded-xl cursor-pointer group transition-all"
        style="background:#0A1628;border:1px solid rgba(255,255,255,0.06);"
        @mouseenter="($event.currentTarget as HTMLElement).style.borderColor='rgba(16,185,129,0.25)'"
        @mouseleave="($event.currentTarget as HTMLElement).style.borderColor='rgba(255,255,255,0.06)'">

        <!-- Icon -->
        <div class="w-10 h-10 rounded-xl flex items-center justify-center flex-shrink-0 mt-0.5"
          style="background:rgba(16,185,129,0.08);border:1px solid rgba(16,185,129,0.15);">
          <MessageSquare class="w-4 h-4" style="color:#10B981;" />
        </div>

        <!-- Content -->
        <div class="flex-1 min-w-0">
          <h3 class="font-bold text-slate-100 group-hover:text-emerald-400 transition-colors truncate">{{ d.title }}</h3>
          <div class="flex items-center gap-4 mt-1.5 text-xs text-slate-500">
            <span class="flex items-center gap-1"><User class="w-3 h-3" /> {{ d.authorUsername }}</span>
            <span class="flex items-center gap-1"><Clock class="w-3 h-3" /> {{ formatDate(d.createdAt) }}</span>
            <span class="flex items-center gap-1"><MessageSquare class="w-3 h-3" /> {{ d.commentCount }}</span>
            <span class="flex items-center gap-1"><Eye class="w-3 h-3" /> {{ d.views }}</span>
          </div>
        </div>
      </router-link>

      <!-- Empty -->
      <div v-if="!discussions.length" class="text-center py-16">
        <MessageSquare class="w-10 h-10 text-slate-700 mx-auto mb-3" />
        <p class="text-slate-500">Chưa có bài thảo luận nào.</p>
        <button @click="showCreate = true" class="mt-4 text-emerald-400 text-sm font-bold hover:underline">
          Tạo bài đầu tiên →
        </button>
      </div>
    </div>

    <!-- Create Discussion Modal -->
    <div v-if="showCreate" class="fixed inset-0 z-50 flex items-center justify-center"
      style="background:rgba(0,0,0,0.7);" @click.self="showCreate = false">
      <div class="w-full max-w-lg rounded-2xl p-6 shadow-2xl"
        style="background:#0A1628;border:1px solid rgba(16,185,129,0.2);">
        <h2 class="text-lg font-bold text-slate-100 mb-4">Tạo thảo luận mới</h2>

        <input v-model="newTitle" placeholder="Tiêu đề bài đăng..."
          class="w-full px-4 py-3 rounded-xl text-sm mb-3 outline-none"
          style="background:#060D16;border:1px solid rgba(255,255,255,0.08);color:#F1F5F9;" />

        <textarea v-model="newContent" placeholder="Nội dung chi tiết..." rows="5"
          class="w-full px-4 py-3 rounded-xl text-sm mb-3 outline-none resize-none"
          style="background:#060D16;border:1px solid rgba(255,255,255,0.08);color:#F1F5F9;font-family:inherit;" />

        <p v-if="createError" class="text-red-400 text-xs mb-3">{{ createError }}</p>

        <div class="flex justify-end gap-3">
          <button @click="showCreate = false" class="px-4 py-2 text-sm text-slate-400 hover:text-slate-200">Hủy</button>
          <button @click="createDiscussion" :disabled="creating"
            class="px-6 py-2 rounded-xl text-sm font-bold transition-all"
            style="background:#10B981;color:#000;">
            {{ creating ? 'Đang đăng...' : 'Đăng bài' }}
          </button>
        </div>
      </div>
    </div>

  </div>
</template>
