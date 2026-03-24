<script setup>
import { ref } from 'vue'
import { MagnifyingGlassIcon, PlusIcon, ExclamationTriangleIcon } from '@heroicons/vue/24/outline'

const defects = ref([]) 
</script>

<template>
  <div class="space-y-5 md:space-y-6 animate-fade-in pb-10 px-0 md:px-1">
    
    <div class="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
      <div>
        <h2 class="text-xl md:text-2xl font-bold text-gray-800">Xử lý Hàng Lỗi</h2>
        <p class="text-xs md:text-sm text-gray-500 mt-1">Ghi nhận và xử lý (trả/hủy/sửa) các mặt hàng không đạt tiêu chuẩn</p>
      </div>
      <button class="bg-red-600 hover:bg-red-700 text-white px-4 py-2.5 rounded-lg flex items-center gap-2 text-sm font-semibold shadow-sm transition-colors active:scale-95 justify-center">
        <PlusIcon class="w-5 h-5" /> Báo cáo Hàng Lỗi
      </button>
    </div>

    <div class="bg-white p-3 md:p-4 rounded-xl border border-gray-200 flex flex-col sm:flex-row items-center gap-3 md:gap-4 shadow-sm">
      <div class="relative w-full sm:flex-1 max-w-none sm:max-w-md">
        <div class="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
          <MagnifyingGlassIcon class="w-5 h-5 text-gray-400" />
        </div>
        <input type="text" class="block w-full pl-10 pr-3 py-2 border border-gray-200 rounded-lg text-sm focus:ring-1 focus:ring-primary-500 outline-none" placeholder="Tìm mã báo cáo, mã SKU lỗi...">
      </div>
      <select class="w-full sm:w-auto border border-gray-200 rounded-lg text-sm px-4 py-2 text-gray-600 outline-none focus:ring-1 focus:ring-primary-500 cursor-pointer">
        <option value="">Tất cả phương án</option>
        <option value="huy">Chờ Hủy bỏ</option>
        <option value="tra_ncc">Trả Nhà cung cấp</option>
      </select>
    </div>

    <div class="bg-white rounded-xl border border-gray-200 shadow-sm overflow-hidden">
      <div class="w-full overflow-x-auto custom-scrollbar">
        <table class="min-w-[900px] w-full divide-y divide-gray-200">
          <thead class="bg-gray-50">
            <tr>
              <th class="px-5 py-3.5 text-left text-xs font-bold text-gray-500 uppercase">Mã Báo cáo</th>
              <th class="px-5 py-3.5 text-left text-xs font-bold text-gray-500 uppercase">Ngày báo cáo</th>
              <th class="px-5 py-3.5 text-left text-xs font-bold text-gray-500 uppercase">Sản phẩm (SKU)</th>
              <th class="px-5 py-3.5 text-left text-xs font-bold text-gray-500 uppercase">Số lượng lỗi</th>
              <th class="px-5 py-3.5 text-left text-xs font-bold text-gray-500 uppercase">Phương án xử lý</th>
              <th class="px-5 py-3.5 text-left text-xs font-bold text-gray-500 uppercase">Trạng thái</th>
              <th class="px-5 py-3.5 text-right text-xs font-bold text-gray-500 uppercase">Thao tác</th>
            </tr>
          </thead>
          <tbody>
            <tr>
              <td colspan="7" class="px-6 py-16 md:py-24 text-center">
                <div class="flex flex-col items-center justify-center">
                  <div class="bg-red-50 p-4 rounded-full mb-4 shadow-inner">
                    <ExclamationTriangleIcon class="w-10 h-10 md:w-12 md:h-12 text-red-500" />
                  </div>
                  <h3 class="text-sm md:text-base font-semibold text-gray-900">Không có hàng lỗi</h3>
                  <p class="text-xs md:text-sm text-gray-500 mt-2 max-w-sm mx-auto">Kho hàng đang hoạt động trơn tru. Không có báo cáo hàng lỗi nào.</p>
                </div>
              </td>
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
.custom-scrollbar:hover::-webkit-scrollbar-thumb { background: #cbd5e1; }
</style>