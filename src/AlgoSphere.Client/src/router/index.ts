import { createRouter, createWebHistory } from 'vue-router'
import HomeView from '../views/HomeView.vue'
import LoginView from '../views/LoginView.vue'
import ExerciseListView from '../views/ExerciseListView.vue'
import WorkspaceView from '../views/WorkspaceView.vue'
import ArenaView from '../views/ArenaView.vue'
import LeaderboardView from '../views/LeaderboardView.vue'
import ForumListView from '../views/ForumListView.vue'
import TournamentListView from '../views/TournamentListView.vue'
import TournamentBracketView from '../views/TournamentBracketView.vue'
import B2BDashboardView from '../views/B2BDashboardView.vue'

const router = createRouter({
  history: createWebHistory(),
  routes: [
    { path: '/', component: HomeView, name: 'home' },
    { path: '/login', component: LoginView, name: 'login' },
    { path: '/topic/:id', component: ExerciseListView, name: 'exercises' },
    { path: '/workspace/:id', component: WorkspaceView, name: 'workspace' },
    { path: '/arena', component: ArenaView, name: 'arena' },
    { path: '/leaderboard', component: LeaderboardView, name: 'leaderboard' },
    { path: '/forum', component: ForumListView, name: 'forum' },
    { path: '/tournaments', component: TournamentListView, name: 'tournaments' },
    { path: '/tournaments/:id/brackets', component: TournamentBracketView, name: 'brackets' },
    { path: '/b2b', component: B2BDashboardView, name: 'b2b' },
  ]
})

export default router
