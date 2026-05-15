<script setup lang="ts">
import { ref, computed } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { LogIn, UserPlus, Zap, Eye, EyeOff } from 'lucide-vue-next'

const router = useRouter()
const route  = useRoute()

// Start on register tab if coming from ?tab=register or /register redirect
const activeTab = ref<'login' | 'register'>(
  route.query.tab === 'register' ? 'register' : 'login'
)

// ── Login state ─────────────────────────────────────────
const loginUsername = ref('')
const loginPassword = ref('')
const loginLoading  = ref(false)
const loginError    = ref('')
const showLoginPw   = ref(false)

const handleLogin = async () => {
  loginLoading.value = true
  loginError.value   = ''
  try {
    const res = await fetch('/api/v1/users/login', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({
        username: loginUsername.value,
        password: loginPassword.value,
      }),
    })
    if (res.ok) {
      const data = await res.json()
      localStorage.setItem('token', data.token)
      // Roles are embedded in JWT but also returned for convenience
      if (data.roles?.includes('Admin')) {
        router.push('/admin')
      } else {
        router.push('/dashboard')
      }
    } else {
      loginError.value = 'Tên đăng nhập hoặc mật khẩu không đúng.'
    }
  } catch {
    loginError.value = 'Không kết nối được server. Kiểm tra backend đã chạy chưa.'
  } finally {
    loginLoading.value = false
  }
}

// ── Register state ───────────────────────────────────────
const regUsername  = ref('')
const regEmail     = ref('')
const regPassword  = ref('')
const regConfirm   = ref('')
const regLoading   = ref(false)
const regError     = ref('')
const regSuccess   = ref(false)
const showRegPw    = ref(false)
const showRegPw2   = ref(false)

const passwordStrength = computed(() => {
  const p = regPassword.value
  if (!p) return 0
  let score = 0
  if (p.length >= 8)            score++
  if (/[A-Z]/.test(p))         score++
  if (/[0-9]/.test(p))         score++
  if (/[^A-Za-z0-9]/.test(p))  score++
  return score
})

const strengthLabel = computed(() => {
  const map = ['', 'Yếu', 'Trung bình', 'Khá', 'Mạnh']
  return map[passwordStrength.value] ?? ''
})

const strengthColor = computed(() => {
  const map = ['', '#EF4444', '#F97316', '#FBBF24', '#10B981']
  return map[passwordStrength.value] ?? '#1E293B'
})

const handleRegister = async () => {
  regError.value = ''

  if (regPassword.value !== regConfirm.value) {
    regError.value = 'Mật khẩu xác nhận không khớp.'
    return
  }
  if (passwordStrength.value < 2) {
    regError.value = 'Mật khẩu quá yếu. Cần tối thiểu 8 ký tự và số.'
    return
  }

  regLoading.value = true
  try {
    const res = await fetch('/api/v1/users/register', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({
        username: regUsername.value,
        email:    regEmail.value,
        password: regPassword.value,
      }),
    })
    if (res.ok) {
      regSuccess.value = true
      setTimeout(() => {
        activeTab.value = 'login'
        loginUsername.value = regUsername.value
        regSuccess.value = false
      }, 1800)
    } else {
      const err = await res.json().catch(() => ({}))
      regError.value = err?.message ?? 'Đăng ký thất bại. Username hoặc email đã tồn tại.'
    }
  } catch {
    regError.value = 'Không kết nối được server.'
  } finally {
    regLoading.value = false
  }
}
</script>

<template>
  <div class="min-h-screen flex items-center justify-center p-4 relative overflow-hidden"
    style="background:#020408;">

    <!-- Background grid -->
    <div class="absolute inset-0 pointer-events-none"
      style="background-image:linear-gradient(rgba(16,185,129,0.03) 1px,transparent 1px),linear-gradient(90deg,rgba(16,185,129,0.03) 1px,transparent 1px);background-size:60px 60px;">
    </div>
    <!-- Radial glow -->
    <div class="absolute top-1/3 left-1/2 -translate-x-1/2 -translate-y-1/2 w-[500px] h-[500px] rounded-full pointer-events-none"
      style="background:radial-gradient(circle,rgba(16,185,129,0.07) 0%,transparent 70%);"></div>

    <div class="relative w-full max-w-md">
      <!-- Logo -->
      <div class="text-center mb-6">
        <router-link to="/" class="inline-flex items-center gap-2.5 group">
          <div class="w-9 h-9 rounded-xl flex items-center justify-center"
            style="background:linear-gradient(135deg,#10B981,#059669);">
            <Zap class="w-5 h-5 text-white fill-current" />
          </div>
          <span class="font-black text-xl tracking-tight" style="color:#10B981;">ALGOSPHERE</span>
        </router-link>
      </div>

      <!-- Card -->
      <div class="rounded-2xl p-8"
        style="background:rgba(6,13,22,0.9);border:1px solid rgba(16,185,129,0.15);backdrop-filter:blur(20px);">

        <!-- Tab switcher -->
        <div class="flex rounded-xl p-1 mb-7" style="background:rgba(255,255,255,0.04);">
          <button
            @click="activeTab = 'login'"
            class="flex-1 py-2 text-sm font-bold rounded-lg transition-all duration-200"
            :style="activeTab === 'login'
              ? 'background:rgba(16,185,129,0.15);color:#10B981;'
              : 'color:#475569;'">
            Đăng nhập
          </button>
          <button
            @click="activeTab = 'register'"
            class="flex-1 py-2 text-sm font-bold rounded-lg transition-all duration-200"
            :style="activeTab === 'register'
              ? 'background:rgba(16,185,129,0.15);color:#10B981;'
              : 'color:#475569;'">
            Đăng ký
          </button>
        </div>

        <!-- ── Login Form ── -->
        <form v-if="activeTab === 'login'" @submit.prevent="handleLogin" class="space-y-5">
          <div>
            <label class="block text-xs font-bold text-slate-400 uppercase tracking-wide mb-1.5">
              Username
            </label>
            <input
              v-model="loginUsername"
              type="text"
              required
              autocomplete="username"
              placeholder="Nhập username..."
              class="w-full text-sm px-4 py-3 rounded-xl outline-none transition-all"
              style="background:#0A1628;border:1px solid rgba(255,255,255,0.08);color:#E2E8F0;"
              @focus="($event.target as HTMLElement).style.borderColor='rgba(16,185,129,0.4)'"
              @blur="($event.target as HTMLElement).style.borderColor='rgba(255,255,255,0.08)'"
            />
          </div>

          <div>
            <label class="block text-xs font-bold text-slate-400 uppercase tracking-wide mb-1.5">
              Mật khẩu
            </label>
            <div class="relative">
              <input
                v-model="loginPassword"
                :type="showLoginPw ? 'text' : 'password'"
                required
                autocomplete="current-password"
                placeholder="••••••••"
                class="w-full text-sm px-4 py-3 pr-10 rounded-xl outline-none transition-all"
                style="background:#0A1628;border:1px solid rgba(255,255,255,0.08);color:#E2E8F0;"
                @focus="($event.target as HTMLElement).style.borderColor='rgba(16,185,129,0.4)'"
                @blur="($event.target as HTMLElement).style.borderColor='rgba(255,255,255,0.08)'"
              />
              <button type="button" tabindex="-1"
                @click="showLoginPw = !showLoginPw"
                class="absolute right-3 top-1/2 -translate-y-1/2 text-slate-600 hover:text-slate-300 transition-colors">
                <component :is="showLoginPw ? EyeOff : Eye" class="w-4 h-4" />
              </button>
            </div>
          </div>

          <p v-if="loginError" class="text-xs text-red-400 text-center py-2 px-3 rounded-lg"
            style="background:rgba(239,68,68,0.08);border:1px solid rgba(239,68,68,0.2);">
            {{ loginError }}
          </p>

          <button type="submit" :disabled="loginLoading"
            class="w-full flex items-center justify-center gap-2 py-3 rounded-xl font-bold text-sm transition-all disabled:opacity-50"
            style="background:linear-gradient(135deg,#10B981,#059669);color:#fff;">
            <LogIn v-if="!loginLoading" class="w-4 h-4" />
            {{ loginLoading ? 'Đang xác thực...' : 'Đăng nhập' }}
          </button>

          <p class="text-center text-xs text-slate-600">
            Chưa có tài khoản?
            <button type="button" @click="activeTab = 'register'" class="ml-1 font-bold transition-colors"
              style="color:#10B981;" @mouseenter="($event.target as HTMLElement).style.textDecoration='underline'"
              @mouseleave="($event.target as HTMLElement).style.textDecoration='none'">
              Đăng ký ngay →
            </button>
          </p>
        </form>

        <!-- ── Register Form ── -->
        <form v-else @submit.prevent="handleRegister" class="space-y-4">
          <!-- Success state -->
          <div v-if="regSuccess"
            class="text-center py-6 px-4 rounded-xl"
            style="background:rgba(16,185,129,0.08);border:1px solid rgba(16,185,129,0.2);">
            <div class="text-3xl mb-2">🎉</div>
            <p class="font-bold text-emerald-400 mb-1">Đăng ký thành công!</p>
            <p class="text-xs text-slate-500">Đang chuyển sang trang đăng nhập...</p>
          </div>

          <template v-else>
            <div>
              <label class="block text-xs font-bold text-slate-400 uppercase tracking-wide mb-1.5">
                Username
              </label>
              <input
                v-model="regUsername"
                type="text"
                required
                autocomplete="username"
                placeholder="Ví dụ: algo_master"
                class="w-full text-sm px-4 py-3 rounded-xl outline-none transition-all"
                style="background:#0A1628;border:1px solid rgba(255,255,255,0.08);color:#E2E8F0;"
                @focus="($event.target as HTMLElement).style.borderColor='rgba(16,185,129,0.4)'"
                @blur="($event.target as HTMLElement).style.borderColor='rgba(255,255,255,0.08)'"
              />
            </div>

            <div>
              <label class="block text-xs font-bold text-slate-400 uppercase tracking-wide mb-1.5">
                Email
              </label>
              <input
                v-model="regEmail"
                type="email"
                required
                autocomplete="email"
                placeholder="email@example.com"
                class="w-full text-sm px-4 py-3 rounded-xl outline-none transition-all"
                style="background:#0A1628;border:1px solid rgba(255,255,255,0.08);color:#E2E8F0;"
                @focus="($event.target as HTMLElement).style.borderColor='rgba(16,185,129,0.4)'"
                @blur="($event.target as HTMLElement).style.borderColor='rgba(255,255,255,0.08)'"
              />
            </div>

            <div>
              <label class="block text-xs font-bold text-slate-400 uppercase tracking-wide mb-1.5">
                Mật khẩu
              </label>
              <div class="relative">
                <input
                  v-model="regPassword"
                  :type="showRegPw ? 'text' : 'password'"
                  required
                  autocomplete="new-password"
                  placeholder="Tối thiểu 8 ký tự"
                  class="w-full text-sm px-4 py-3 pr-10 rounded-xl outline-none transition-all"
                  style="background:#0A1628;border:1px solid rgba(255,255,255,0.08);color:#E2E8F0;"
                  @focus="($event.target as HTMLElement).style.borderColor='rgba(16,185,129,0.4)'"
                  @blur="($event.target as HTMLElement).style.borderColor='rgba(255,255,255,0.08)'"
                />
                <button type="button" tabindex="-1"
                  @click="showRegPw = !showRegPw"
                  class="absolute right-3 top-1/2 -translate-y-1/2 text-slate-600 hover:text-slate-300 transition-colors">
                  <component :is="showRegPw ? EyeOff : Eye" class="w-4 h-4" />
                </button>
              </div>
              <!-- Password strength bar -->
              <div v-if="regPassword" class="mt-2 flex items-center gap-2">
                <div class="flex gap-1 flex-1">
                  <div v-for="i in 4" :key="i"
                    class="h-1 flex-1 rounded-full transition-all duration-300"
                    :style="i <= passwordStrength ? `background:${strengthColor}` : 'background:rgba(255,255,255,0.06)'">
                  </div>
                </div>
                <span class="text-[10px] font-bold w-16 text-right" :style="`color:${strengthColor}`">
                  {{ strengthLabel }}
                </span>
              </div>
            </div>

            <div>
              <label class="block text-xs font-bold text-slate-400 uppercase tracking-wide mb-1.5">
                Xác nhận mật khẩu
              </label>
              <div class="relative">
                <input
                  v-model="regConfirm"
                  :type="showRegPw2 ? 'text' : 'password'"
                  required
                  autocomplete="new-password"
                  placeholder="Nhập lại mật khẩu"
                  class="w-full text-sm px-4 py-3 pr-10 rounded-xl outline-none transition-all"
                  :style="`background:#0A1628;color:#E2E8F0;border:1px solid ${regConfirm && regConfirm !== regPassword ? 'rgba(239,68,68,0.4)' : 'rgba(255,255,255,0.08)'}`"
                  @focus="($event.target as HTMLElement).style.borderColor='rgba(16,185,129,0.4)'"
                  @blur="($event.target as HTMLElement).style.borderColor='rgba(255,255,255,0.08)'"
                />
                <button type="button" tabindex="-1"
                  @click="showRegPw2 = !showRegPw2"
                  class="absolute right-3 top-1/2 -translate-y-1/2 text-slate-600 hover:text-slate-300 transition-colors">
                  <component :is="showRegPw2 ? EyeOff : Eye" class="w-4 h-4" />
                </button>
              </div>
              <p v-if="regConfirm && regConfirm !== regPassword"
                class="text-[11px] text-red-400 mt-1">
                Mật khẩu không khớp
              </p>
            </div>

            <p v-if="regError" class="text-xs text-red-400 text-center py-2 px-3 rounded-lg"
              style="background:rgba(239,68,68,0.08);border:1px solid rgba(239,68,68,0.2);">
              {{ regError }}
            </p>

            <button type="submit" :disabled="regLoading"
              class="w-full flex items-center justify-center gap-2 py-3 rounded-xl font-bold text-sm transition-all disabled:opacity-50"
              style="background:linear-gradient(135deg,#10B981,#059669);color:#fff;">
              <UserPlus v-if="!regLoading" class="w-4 h-4" />
              {{ regLoading ? 'Đang tạo tài khoản...' : 'Tạo tài khoản' }}
            </button>

            <p class="text-center text-xs text-slate-600">
              Đã có tài khoản?
              <button type="button" @click="activeTab = 'login'" class="ml-1 font-bold transition-colors"
                style="color:#10B981;">
                Đăng nhập →
              </button>
            </p>
          </template>
        </form>
      </div>

      <!-- Back to landing -->
      <p class="text-center mt-5 text-xs text-slate-600">
        <router-link to="/" class="hover:text-slate-400 transition-colors">← Về trang chủ</router-link>
      </p>
    </div>
  </div>
</template>
