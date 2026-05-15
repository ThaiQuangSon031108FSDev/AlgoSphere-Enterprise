import { createRouter, createWebHistory } from 'vue-router'

// ── Public pages ─────────────────────────────────────────────────
import LandingView from '../views/LandingView.vue'
import LoginView from '../views/LoginView.vue'
import LeaderboardView from '../views/LeaderboardView.vue'

// ── Student App pages ────────────────────────────────────────────
import HomeView from '../views/HomeView.vue'         // Student Dashboard
import SkillTreeView from '../views/SkillTreeView.vue'
import ProfileView from '../views/ProfileView.vue'
import ExerciseListView from '../views/ExerciseListView.vue'
import WorkspaceView from '../views/WorkspaceView.vue'
import ArenaView from '../views/ArenaView.vue'
import ForumListView from '../views/ForumListView.vue'
import ForumDetailView from '../views/ForumDetailView.vue'
import DiscussionDetailView from '../views/DiscussionDetailView.vue'
import TournamentListView from '../views/TournamentListView.vue'
import TournamentBracketView from '../views/TournamentBracketView.vue'

// ── Admin CMS pages ───────────────────────────────────────────────
import AdminView from '../views/AdminView.vue'

const router = createRouter({
  history: createWebHistory(),
  scrollBehavior: () => ({ top: 0 }),
  routes: [
    // ── Public Zone ──────────────────────────────────────────
    {
      path: '/',
      name: 'landing',
      component: LandingView,
      meta: { public: true, title: 'AlgoSphere — Học Thuật Toán Trực Quan' },
    },
    {
      path: '/login',
      name: 'login',
      component: LoginView,
      meta: { public: true, title: 'Đăng nhập — AlgoSphere' },
    },
    {
      path: '/register',
      name: 'register',
      redirect: { path: '/login', query: { tab: 'register' } },
      meta: { public: true },
    },
    {
      path: '/leaderboard',
      name: 'leaderboard',
      component: LeaderboardView,
      meta: { public: true, title: 'Bảng Xếp Hạng — AlgoSphere' },
    },

    // ── Student App ──────────────────────────────────────────
    {
      path: '/dashboard',
      name: 'dashboard',
      component: HomeView,
      meta: { title: 'Dashboard — AlgoSphere' },
    },
    {
      path: '/skill-tree',
      name: 'skill-tree',
      component: SkillTreeView,
      meta: { title: 'Skill Tree — AlgoSphere' },
    },
    {
      path: '/profile',
      name: 'profile',
      component: ProfileView,
      meta: { title: 'Hồ Sơ — AlgoSphere' },
    },
    {
      path: '/topic/:id',
      name: 'exercises',
      component: ExerciseListView,
      meta: { title: 'Bài Tập — AlgoSphere' },
    },
    {
      path: '/workspace/:id',
      name: 'workspace',
      component: WorkspaceView,
      meta: { title: 'Workspace — AlgoSphere', fullscreen: true },
    },
    {
      path: '/arena',
      name: 'arena',
      component: ArenaView,
      meta: { title: 'Code Arena — AlgoSphere' },
    },
    {
      path: '/forum',
      name: 'forum',
      component: ForumListView,
      meta: { title: 'Diễn Đàn — AlgoSphere' },
    },
    {
      path: '/forum/:id',
      name: 'forum-detail',
      component: ForumDetailView,
      meta: { title: 'Chủ đề — AlgoSphere' },
    },
    {
      path: '/forum/discussions/:id',
      name: 'discussion',
      component: DiscussionDetailView,
      meta: { title: 'Thảo luận — AlgoSphere' },
    },
    {
      path: '/tournaments',
      name: 'tournaments',
      component: TournamentListView,
      meta: { title: 'Giải Đấu — AlgoSphere' },
    },
    {
      path: '/tournaments/:id/brackets',
      name: 'brackets',
      component: TournamentBracketView,
      meta: { title: 'Brackets — AlgoSphere' },
    },

    // ── Admin CMS ────────────────────────────────────────────
    {
      path: '/admin',
      name: 'admin',
      component: AdminView,
      meta: { title: 'Admin CMS — AlgoSphere', requiresAdmin: true },
    },

    // ── Settings ────────────────────────────────────────────
    {
      path: '/settings',
      name: 'settings',
      component: ProfileView, // Map Settings to Profile for now
      meta: { title: 'Cài Đặt — AlgoSphere' },
    },

    // ── Redirects ────────────────────────────────────────────
    { path: '/home', redirect: '/dashboard' },
    { path: '/b2b', redirect: '/admin' }, // legacy redirect
    { path: '/:pathMatch(.*)*', redirect: '/' },
  ],
})

// Decode JWT payload without verifying signature (client-side only)
const getJwtRoles = (): string[] => {
  try {
    const token = localStorage.getItem('token')
    if (!token) return []
    const payload = JSON.parse(atob(token.split('.')[1]))
    // ClaimTypes.Role maps to 'http://schemas.microsoft.com/ws/2008/06/identity/claims/role'
    const roleClaim = payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role']
    if (!roleClaim) return []
    return Array.isArray(roleClaim) ? roleClaim : [roleClaim]
  } catch {
    return []
  }
}

// Auth Guard & Title Updater
router.beforeEach((to, _from, next) => {
  if (to.meta?.title) {
    document.title = to.meta.title as string
  }

  const isAuthenticated = !!localStorage.getItem('token')

  // Not logged in → redirect to /login
  if (!to.meta?.public && !isAuthenticated) {
    next('/login')
    return
  }

  // Admin-only routes: check role
  if (to.meta?.requiresAdmin) {
    const roles = getJwtRoles()
    if (!roles.includes('Admin')) {
      next('/dashboard') // silently kick to dashboard
      return
    }
  }

  // Already logged in → skip login page
  if (to.meta?.public && isAuthenticated && to.path === '/login') {
    next('/dashboard')
    return
  }

  next()
})

export default router
