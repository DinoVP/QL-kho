<script setup>
import { ref } from 'vue'
import { MagnifyingGlassIcon, FunnelIcon, ClipboardDocumentListIcon } from '@heroicons/vue/24/outline'

// ĐÃ XÓA SẠCH DATA MẪU CHỜ API
const logs = ref([])

const getActionColor = (action) => {
  switch(action) {
    case 'THÊM MỚI': return 'bg-green-100 text-green-700 border-green-200'
    case 'CẬP NHẬT': return 'bg-blue-100 text-blue-700 border-blue-200'
    case 'XÓA': return 'bg-red-100 text-red-700 border-red-200'
    case 'ĐĂNG NHẬP': return 'bg-purple-100 text-purple-700 border-purple-200'
    default: return 'bg-gray-100 text-gray-700 border-gray-200'
  }
}
</script>

<template>
  <div class="space-y-5 md:space-y-6 animate-fade-in pb-10 px-0 md:px-1">
    
    <div>
      <h2 class="text-xl md:text-2xl font-bold text-gray-800">Nhật ký Hệ thống (Audit Logs)</h2>
      <p class="text-xs md:text-sm text-gray-500 mt-1">Ghi vết toàn bộ thao tác của người dùng để đảm bảo tính minh bạch</p>
    </div>

    <div class="bg-white p-3 md:p-4 rounded-xl border border-gray-200 flex flex-col sm:flex-row items-center gap-3 md:gap-4 shadow-sm">
      <div class="relative w-full sm:flex-1">
        <div class="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
          <MagnifyingGlassIcon class="w-5 h-5 text-gray-400" />
        </div>
        <input type="text" class="block w-full pl-10 pr-3 py-2 border border-gray-200 rounded-lg text-sm focus:ring-1 focus:ring-primary-500 outline-none" placeholder="Tìm theo người dùng, nội dung chi tiết...">
      </div>
      
      <div class="flex gap-2 w-full sm:w-auto">
        <select class="w-full sm:w-auto border border-gray-200 rounded-lg text-sm px-3 py-2 focus:ring-1 focus:ring-primary-500 outline-none cursor-pointer">
          <option value="">Tất cả Hành động</option>
          <option value="ADD">Thêm mới</option>
          <option value="UPDATE">Cập nhật</option>
          <option value="DELETE">Xóa</option>
        </select>
        <button class="bg-gray-50 border border-gray-200 text-gray-600 px-3 py-2 rounded-lg flex items-center gap-2 text-sm hover:bg-gray-100 transition-colors">
          <FunnelIcon class="w-4 h-4" /> Lọc ngày
        </button>
      </div>
    </div>

    <div class="bg-white rounded-xl border border-gray-200 shadow-sm overflow-hidden">
      <div class="w-full overflow-x-auto custom-scrollbar">
        <table class="min-w-[950px] w-full divide-y divide-gray-200">
          <thead class="bg-gray-50">
            <tr>
              <th class="px-5 py-3.5 text-left text-xs font-bold text-gray-500 uppercase tracking-wider">Thời gian</th>
              <th class="px-5 py-3.5 text-left text-xs font-bold text-gray-500 uppercase tracking-wider">Người thực hiện</th>
              <th class="px-5 py-3.5 text-left text-xs font-bold text-gray-500 uppercase tracking-wider">Hành động</th>
              <th class="px-5 py-3.5 text-left text-xs font-bold text-gray-500 uppercase tracking-wider">Đối tượng</th>
              <th class="px-5 py-3.5 text-left text-xs font-bold text-gray-500 uppercase tracking-wider">Chi tiết thao tác</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-gray-100 bg-white">
            <tr v-if="logs.length === 0">
              <td colspan="5" class="px-6 py-16 text-center">
                <ClipboardDocumentListIcon class="w-12 h-12 text-gray-300 mx-auto mb-3" />
                <h3 class="text-base font-semibold text-gray-700">Chưa có nhật ký nào</h3>
                <p class="text-sm text-gray-500 mt-1">Hệ thống đang chờ API ghi vết dữ liệu.</p>
              </td>
            </tr>
            <tr v-for="log in logs" :key="log.id" class="hover:bg-gray-50 transition-colors">
              <td class="px-5 py-3 text-sm text-gray-500 font-medium whitespace-nowrap">{{ log.time }}</td>
              <td class="px-5 py-3 text-sm font-bold text-slate-700">{{ log.user }}</td>
              <td class="px-5 py-3"><span :class="['text-[10px] font-bold px-2 py-1 rounded border tracking-wider', getActionColor(log.action)]">{{ log.action }}</span></td>
              <td class="px-5 py-3 text-sm font-semibold text-gray-600">{{ log.target }}</td>
              <td class="px-5 py-3 text-sm text-gray-600 truncate max-w-xs">{{ log.detail }}</td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

  </div>
</template>

<style scoped>
.custom-scrollbar::-webkit-scrollbar { height: 6px; }
.custom-scrollbar::-webkit-scrollbar-track { background: transparent; }
.custom-scrollbar::-webkit-scrollbar-thumb { background: #e2e8f0; border-radius: 10px; }
</style>