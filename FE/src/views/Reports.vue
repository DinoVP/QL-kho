<script setup>
import { ref } from 'vue'
import { CalendarIcon, ArrowDownTrayIcon, ChartBarIcon, DocumentTextIcon } from '@heroicons/vue/24/outline'

const reports = ref([]) 
</script>

<template>
  <div class="space-y-5 md:space-y-6 animate-fade-in pb-10 px-0 md:px-1">
    
    <div class="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
      <div>
        <h2 class="text-xl md:text-2xl font-bold text-gray-800">Báo cáo Xuất / Nhập</h2>
        <p class="text-xs md:text-sm text-gray-500 mt-1">Trích xuất số liệu thống kê kho hàng theo thời gian cụ thể</p>
      </div>
      <button class="bg-green-600 hover:bg-green-700 text-white px-4 py-2.5 rounded-lg flex items-center gap-2 text-sm font-semibold shadow-sm transition-colors active:scale-95 justify-center">
        <ArrowDownTrayIcon class="w-5 h-5" /> Tải báo cáo Excel
      </button>
    </div>

    <div class="bg-white p-4 md:p-5 rounded-xl border border-gray-200 shadow-sm grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
      
      <div>
        <label class="block text-xs font-bold text-gray-600 mb-1.5 uppercase">Loại báo cáo</label>
        <select class="w-full border border-gray-200 rounded-lg text-sm px-3 py-2 text-gray-700 outline-none focus:ring-1 focus:ring-primary-500 cursor-pointer">
          <option>Báo cáo Nhập kho</option>
          <option>Báo cáo Xuất kho</option>
          <option>Báo cáo Biến động Tồn</option>
        </select>
      </div>

      <div>
        <label class="block text-xs font-bold text-gray-600 mb-1.5 uppercase">Từ ngày</label>
        <div class="relative">
          <div class="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none"><CalendarIcon class="w-4 h-4 text-gray-400"/></div>
          <input type="date" class="block w-full pl-9 pr-3 py-2 border border-gray-200 rounded-lg text-sm text-gray-700 focus:ring-1 focus:ring-primary-500 outline-none cursor-pointer">
        </div>
      </div>

      <div>
        <label class="block text-xs font-bold text-gray-600 mb-1.5 uppercase">Đến ngày</label>
        <div class="relative">
          <div class="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none"><CalendarIcon class="w-4 h-4 text-gray-400"/></div>
          <input type="date" class="block w-full pl-9 pr-3 py-2 border border-gray-200 rounded-lg text-sm text-gray-700 focus:ring-1 focus:ring-primary-500 outline-none cursor-pointer">
        </div>
      </div>

      <div class="flex items-end">
        <button class="w-full bg-primary-50 hover:bg-primary-100 text-primary-700 border border-primary-200 px-4 py-2 rounded-lg text-sm font-semibold transition-colors flex items-center justify-center gap-2">
          <DocumentTextIcon class="w-5 h-5" /> Lập báo cáo
        </button>
      </div>

    </div>

    <div class="bg-white rounded-xl border border-gray-200 shadow-sm overflow-hidden">
      <div class="bg-gray-50 border-b border-gray-200 px-5 py-3">
        <h3 class="text-sm font-bold text-gray-700">Xem trước dữ liệu</h3>
      </div>
      <div class="p-16 md:p-24 text-center">
        <ChartBarIcon class="w-10 h-10 md:w-12 md:h-12 text-gray-300 mx-auto mb-4" />
        <h3 class="text-sm md:text-base font-semibold text-gray-600">Chọn tham số và bấm "Lập báo cáo"</h3>
        <p class="text-xs md:text-sm text-gray-400 mt-1">Dữ liệu xem trước sẽ hiển thị ở đây trước khi bạn xuất Excel.</p>
      </div>
    </div>

  </div>
</template>