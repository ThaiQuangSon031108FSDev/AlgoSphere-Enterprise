<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { LogIn } from 'lucide-vue-next'

const router = useRouter()
const username = ref('')
const password = ref('')
const loading = ref(false)
const error = ref('')

const handleLogin = async () => {
  loading.value = true
  error.value = ''
  try {
    const res = await fetch('/api/v1/users/login', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ username: username.value, password: password.value })
    })
    
    if (res.ok) {
      const data = await res.json()
      localStorage.setItem('token', data.token)
      router.push('/')
    } else {
      error.value = 'Tên đăng nhập hoặc mật khẩu không đúng.'
    }
  } catch (err) {
    error.value = 'Lỗi kết nối server.'
  } finally {
    loading.value = false
  }
}
</script>

<template>
  <div class="min-h-screen flex items-center justify-center bg-brand-slate p-4">
    <div class="max-w-md w-full bg-slate-900/50 backdrop-blur-xl p-8 rounded-2xl border border-slate-800 shadow-2xl">
      <div class="text-center mb-8">
        <h2 class="text-3xl font-black text-brand-emerald italic mb-2">ALGOSPHERE</h2>
        <p class="text-slate-400">Đăng nhập để tiếp tục học tập</p>
      </div>

      <form @submit.prevent="handleLogin" class="space-y-6">
        <div>
          <label class="block text-sm font-medium text-slate-300 mb-2">Username</label>
          <input v-model="username" type="text" required class="w-full bg-slate-800 border border-slate-700 rounded-lg px-4 py-3 focus:ring-2 focus:ring-brand-emerald focus:border-transparent outline-none transition-all" />
        </div>
        <div>
          <label class="block text-sm font-medium text-slate-300 mb-2">Password</label>
          <input v-model="password" type="password" required class="w-full bg-slate-800 border border-slate-700 rounded-lg px-4 py-3 focus:ring-2 focus:ring-brand-emerald focus:border-transparent outline-none transition-all" />
        </div>

        <button :disabled="loading" type="submit" class="w-full bg-brand-emerald hover:bg-emerald-600 text-white font-bold py-3 rounded-lg flex items-center justify-center gap-2 transition-all disabled:opacity-50">
          <LogIn v-if="!loading" class="w-5 h-5" />
          {{ loading ? 'Đang xác thực...' : 'Đăng nhập' }}
        </button>

        <p v-if="error" class="text-center text-red-400 text-sm">{{ error }}</p>
      </form>

      <div class="mt-8 text-center text-sm text-slate-500">
        Chưa có tài khoản? <router-link to="/register" class="text-brand-emerald hover:underline">Đăng ký ngay</router-link>
      </div>
    </div>
  </div>
</template>
