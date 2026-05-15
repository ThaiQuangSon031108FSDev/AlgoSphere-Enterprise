<script setup lang="ts">
import Sidebar from './components/Sidebar.vue'
</script>

<template>
  <div class="flex min-h-screen bg-brand-slate text-white antialiased">
    <Sidebar />
    <main class="flex-1 ml-64 min-h-screen overflow-y-auto">
      <router-view />
    </main>
  </div>
</template>

<style>
/* Ẩn thanh cuộn mặc định của main để dùng style riêng nếu cần */
main::-webkit-scrollbar {
  width: 6px;
}
main::-webkit-scrollbar-thumb {
  background: #1e293b;
  border-radius: 10px;
}
</style>
