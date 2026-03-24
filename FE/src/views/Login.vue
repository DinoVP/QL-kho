<script setup>
import { CubeIcon, LockClosedIcon, UserIcon } from "@heroicons/vue/24/outline";
</script>

<template>
  <div class="min-h-screen bg-gray-50 flex items-center justify-center p-4">
    <div
      class="max-w-md w-full bg-white rounded-2xl shadow-lg border border-gray-100 p-8 space-y-8"
    >
      <div class="text-center">
        <div class="flex justify-center mb-4">
          <div class="bg-primary-50 p-3 rounded-xl inline-block">
            <CubeIcon class="w-10 h-10 text-primary-600" />
          </div>
        </div>
        <h2 class="text-2xl font-bold text-gray-900 tracking-tight">
          Hệ Thống QL Kho
        </h2>
        <p class="text-sm text-gray-500 mt-2">Vui lòng đăng nhập để tiếp tục</p>
      </div>

      <form class="space-y-6" @submit.prevent>
        <div>
          <label class="block text-sm font-medium text-gray-700 mb-1"
            >Tên đăng nhập</label
          >
          <div class="relative">
            <div
              class="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none"
            >
              <UserIcon class="w-5 h-5 text-gray-400" />
            </div>
            <input
              type="text"
              class="block w-full pl-10 pr-3 py-2.5 border border-gray-200 rounded-lg text-sm focus:ring-1 focus:ring-primary-600 focus:border-primary-600 outline-none transition-colors bg-gray-50 focus:bg-white"
              placeholder="Nhập tài khoản..."
            />
          </div>
        </div>

        <div>
          <label class="block text-sm font-medium text-gray-700 mb-1"
            >Mật khẩu</label
          >
          <div class="relative">
            <div
              class="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none"
            >
              <LockClosedIcon class="w-5 h-5 text-gray-400" />
            </div>
            <input
              type="password"
              class="block w-full pl-10 pr-3 py-2.5 border border-gray-200 rounded-lg text-sm focus:ring-1 focus:ring-primary-600 focus:border-primary-600 outline-none transition-colors bg-gray-50 focus:bg-white"
              placeholder="••••••••"
            />
          </div>
        </div>

        <div class="flex items-center justify-between text-sm">
          <label class="flex items-center gap-2 cursor-pointer">
            <input
              type="checkbox"
              class="rounded border-gray-300 text-primary-600 focus:ring-primary-600"
            />
            <span class="text-gray-600">Ghi nhớ tôi</span>
          </label>
          <a
            href="#"
            class="text-primary-600 hover:text-primary-700 font-medium"
            >Quên mật khẩu?</a
          >
        </div>

        <button
          type="button"
          @click="$router.push('/Trang-Chu')"
          class="w-full bg-primary-600 hover:bg-primary-700 text-white py-2.5 rounded-lg text-sm font-bold tracking-wide transition-colors shadow-sm"
        >
          ĐĂNG NHẬP
        </button>
      </form>

      <div
        class="text-center text-xs text-gray-400 mt-4 border-t border-gray-100 pt-6"
      >
        &copy; 2026
      </div>
    </div>
  </div>
</template>
