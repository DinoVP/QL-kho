<script setup>
import { ArchiveBoxIcon, ArrowDownTrayIcon, ArrowUpTrayIcon, CurrencyDollarIcon } from '@heroicons/vue/24/outline'
import VueApexCharts from 'vue3-apexcharts'

// === CẤU HÌNH HIỂN THỊ KHI KHÔNG CÓ DATA ===
const noDataConfig = {
  text: 'Chưa có dữ liệu',
  align: 'center',
  verticalAlign: 'middle',
  style: { color: '#9ca3af', fontSize: '14px', fontFamily: 'Inter, sans-serif' }
}

// === 1. BIỂU ĐỒ ĐƯỜNG/DIỆN TÍCH (Trống) ===
const areaOptions = {
  chart: { type: 'area', toolbar: { show: false }, fontFamily: 'Inter, sans-serif' },
  colors: ['#3b82f6', '#f97316'],
  dataLabels: { enabled: false },
  stroke: { curve: 'smooth', width: 2 },
  xaxis: { categories: ['T2', 'T3', 'T4', 'T5', 'T6', 'T7', 'CN'] },
  noData: noDataConfig, // Bật chế độ không có dữ liệu
  legend: { position: 'top', horizontalAlign: 'right' }
}
const areaSeries = [] // Trả về mảng rỗng

// === 2. BIỂU ĐỒ TRÒN (Trống) ===
const donutOptions = {
  chart: { type: 'donut', fontFamily: 'Inter, sans-serif' },
  labels: [],
  colors: ['#3b82f6', '#10b981', '#f59e0b', '#6366f1'],
  noData: noDataConfig, // Bật chế độ không có dữ liệu
  plotOptions: { pie: { donut: { size: '70%' } } },
}
const donutSeries = [] // Trả về mảng rỗng

// === 3. BIỂU ĐỒ CỘT NGANG (Trống) ===
const barOptions = {
  chart: { type: 'bar', toolbar: { show: false }, fontFamily: 'Inter, sans-serif' },
  plotOptions: { bar: { horizontal: true, borderRadius: 4 } },
  colors: ['#10b981'],
  noData: noDataConfig, // Bật chế độ không có dữ liệu
}
const barSeries = [] // Trả về mảng rỗng

// === 4. BIỂU ĐỒ VÒNG CUNG (Trống - 0%) ===
const radialOptions = {
  chart: { type: 'radialBar', fontFamily: 'Inter, sans-serif' },
  plotOptions: {
    radialBar: {
      hollow: { size: '65%' },
      dataLabels: {
        name: { show: true, color: '#6b7280', fontSize: '13px' },
        value: { show: true, color: '#111827', fontSize: '30px', fontWeight: 'bold' }
      }
    }
  },
  colors: ['#ef4444'],
  labels: ['Đã lấp đầy']
}
const radialSeries = [0] // Công suất 0% vì chưa có hàng
</script>

<template>
  <div class="space-y-6 md:space-y-8 animate-fade-in pb-10 px-0 md:px-1">
    
    <div>
      <h2 class="text-xl md:text-2xl font-bold text-gray-800">Dashboard Phân tích</h2>
      <p class="text-xs md:text-sm text-gray-500 mt-1">Hệ thống đang chờ đồng bộ dữ liệu từ các giao dịch kho hàng</p>
    </div>

    <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4 md:gap-6">
      <div class="bg-white rounded-xl border border-gray-200 p-5 shadow-sm">
        <div class="p-3 bg-blue-50 text-blue-600 rounded-lg w-fit mb-3"><ArchiveBoxIcon class="w-6 h-6" /></div>
        <p class="text-sm font-medium text-gray-500">Tổng mặt hàng (SKU)</p>
        <p class="text-2xl font-bold text-gray-900 mt-1">0</p>
      </div>

      <div class="bg-white rounded-xl border border-gray-200 p-5 shadow-sm">
        <div class="p-3 bg-green-50 text-green-600 rounded-lg w-fit mb-3"><ArrowDownTrayIcon class="w-6 h-6" /></div>
        <p class="text-sm font-medium text-gray-500">Nhập kho (Tháng này)</p>
        <p class="text-2xl font-bold text-gray-900 mt-1">0 <span class="text-sm font-normal text-gray-500">Phiếu</span></p>
      </div>

      <div class="bg-white rounded-xl border border-gray-200 p-5 shadow-sm">
        <div class="p-3 bg-orange-50 text-orange-600 rounded-lg w-fit mb-3"><ArrowUpTrayIcon class="w-6 h-6" /></div>
        <p class="text-sm font-medium text-gray-500">Xuất kho (Tháng này)</p>
        <p class="text-2xl font-bold text-gray-900 mt-1">0 <span class="text-sm font-normal text-gray-500">Phiếu</span></p>
      </div>

      <div class="bg-white rounded-xl border border-gray-200 p-5 shadow-sm">
        <div class="p-3 bg-purple-50 text-purple-600 rounded-lg w-fit mb-3"><CurrencyDollarIcon class="w-6 h-6" /></div>
        <p class="text-sm font-medium text-gray-500">Ước tính Giá trị Tồn</p>
        <p class="text-2xl font-bold text-gray-900 mt-1">0 <span class="text-sm font-normal text-gray-500">VNĐ</span></p>
      </div>
    </div>

    <div class="grid grid-cols-1 lg:grid-cols-3 gap-4 md:gap-6">
      <div class="bg-white rounded-xl border border-gray-200 shadow-sm p-5 lg:col-span-2">
        <h3 class="text-gray-800 font-bold mb-1">Lưu lượng Nhập / Xuất kho</h3>
        <p class="text-xs text-gray-500 mb-4">Chưa có giao dịch trong 7 ngày qua</p>
        <VueApexCharts type="area" height="300" :options="areaOptions" :series="areaSeries" />
      </div>

      <div class="bg-white rounded-xl border border-gray-200 shadow-sm p-5 lg:col-span-1">
        <h3 class="text-gray-800 font-bold mb-1">Cơ cấu Tồn kho</h3>
        <p class="text-xs text-gray-500 mb-4">Kho hiện tại đang trống</p>
        <VueApexCharts type="donut" height="320" :options="donutOptions" :series="donutSeries" />
      </div>
    </div>

    <div class="grid grid-cols-1 lg:grid-cols-3 gap-4 md:gap-6">
      <div class="bg-white rounded-xl border border-gray-200 shadow-sm p-5 lg:col-span-2">
        <h3 class="text-gray-800 font-bold mb-1">Top 5 Sản phẩm Xuất nhiều nhất</h3>
        <p class="text-xs text-gray-500 mb-4">Chưa có thống kê xuất hàng</p>
        <VueApexCharts type="bar" height="300" :options="barOptions" :series="barSeries" />
      </div>

      <div class="bg-white rounded-xl border border-gray-200 shadow-sm p-5 lg:col-span-1 flex flex-col">
        <h3 class="text-gray-800 font-bold mb-1">Công suất chứa (Kho Tổng)</h3>
        <p class="text-xs text-gray-500 mb-4">Kho đang hoàn toàn trống</p>
        <div class="flex-1 flex items-center justify-center">
          <VueApexCharts type="radialBar" height="350" :options="radialOptions" :series="radialSeries" />
        </div>
      </div>
    </div>

  </div>
</template>