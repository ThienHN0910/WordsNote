<template>
  <div>
    <div class="row mt-2">
      <div class="col-7"></div>
      <div class="col-1">
        <span>{{ currentPage }} / {{ totalPages }}</span>
      </div>
      <div class="col-1">
        <BaseButton @click="goToPrevPage" :disabled="isFirstPage">prev</BaseButton>
      </div>
      <div class="col-1">
        <BaseButton @click="goToNextPage" :disabled="isLastPage">next</BaseButton>
      </div>
    </div>
    <div class="row">
      <Sidebar />
      <canvas ref="pdfCanvas" style="display: block; position: absolute; z-index: 1" />
      <canvas
        ref="overlayCanvas"
        style="position: absolute; z-index: 2 !important; pointer-events: none"
      />
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, onBeforeUnmount, computed, nextTick } from 'vue'
import * as pdfjsLib from 'pdfjs-dist'
import { PDFDocument, rgb, StandardFonts } from 'pdf-lib'
import type { PDFDocumentProxy } from 'pdfjs-dist/types/src/display/api'
import BaseButton from '@/components/bases/BaseButton.vue'
import Sidebar from '@/components/SideBar.vue'

pdfjsLib.GlobalWorkerOptions.workerSrc = '../../pdfjs-dist/build/pdf.worker.min.mjs'
const pdfCanvas = ref<HTMLCanvasElement | null>(null)
const overlayCanvas = ref<HTMLCanvasElement | null>(null)
let pdfDocument: PDFDocumentProxy | null = null
let pdfLibDocument: PDFDocument | null = null
const pdfUrl = '/edited2.pdf'
const currentPage = ref(1)
const totalPages = ref(1)
const isFirstPage = computed(() => currentPage.value === 1)
const isLastPage = computed(() => currentPage.value === totalPages.value)
// --- Drag state ---
const isDragging = ref(false)
const startX = ref(0)
const startY = ref(0)
const currentX = ref(0)
const currentY = ref(0)
const loadPdf = async () => {
  const loadingTask = pdfjsLib.getDocument(pdfUrl)
  pdfDocument = await loadingTask.promise
  const response = await fetch(pdfUrl)
  const pdfBytes = await response.arrayBuffer()
  pdfLibDocument = await PDFDocument.load(new Uint8Array(pdfBytes))
  totalPages.value = pdfDocument.numPages
  await renderPdf()
}
const renderPdf = async (pdfData?: Uint8Array) => {
  if (!pdfCanvas.value) return
  let documentToRender: PDFDocumentProxy | null = pdfDocument
  if (pdfData) {
    const loadingTask = pdfjsLib.getDocument(pdfData)
    documentToRender = await loadingTask.promise
  }
  if (!documentToRender) return
  const page = await documentToRender.getPage(currentPage.value)
  const scale = 0.75
  const viewport = page.getViewport({ scale })
  const outputScale = window.devicePixelRatio || 1
  const canvas = pdfCanvas.value
  const context = canvas.getContext('2d')!
  canvas.width = Math.floor(viewport.width * outputScale)
  canvas.height = Math.floor(viewport.height * outputScale)
  canvas.style.width = Math.floor(viewport.width) + 'px'
  canvas.style.height = Math.floor(viewport.height) + 'px'
  const transform = outputScale !== 1 ? [outputScale, 0, 0, outputScale, 0, 0] : null
  const renderContext = { canvas: canvas, canvasContext: context, transform: transform as any, viewport: viewport }
  await page.render(renderContext).promise
  await nextTick()
  if (overlayCanvas.value) {
    overlayCanvas.value.width = canvas.width
    overlayCanvas.value.height = canvas.height
    overlayCanvas.value.style.width = canvas.style.width
    overlayCanvas.value.style.height = canvas.style.height
    clearOverlay()
  }
}
const addDrawingToPdf = async (
  x: number,
  y: number,
  width: number,
  height: number,
  text: string,
) => {
  if (!pdfLibDocument) return
  const page = pdfLibDocument.getPage(currentPage.value - 1)
  const { height: pageHeight } = page.getSize()
  const pdfY = pageHeight - y - height
  page.drawRectangle({ x, y: pdfY, width, height, borderColor: rgb(1, 0, 0), borderWidth: 2 })
  const helveticaFont = await pdfLibDocument.embedFont(StandardFonts.Helvetica)
  page.drawText(text, { x: x + 5, y: pdfY + 5, size: 12, font: helveticaFont, color: rgb(0, 0, 0) })
  const modifiedPdfBytes = await pdfLibDocument.save()
  pdfLibDocument = await PDFDocument.load(modifiedPdfBytes)
  await renderPdf(modifiedPdfBytes)
}
function clearOverlay() {
  const overlay = overlayCanvas.value
  if (!overlay) return
  const ctx = overlay.getContext('2d')
  if (!ctx) return
  ctx.clearRect(0, 0, overlay.width, overlay.height)
}
function drawOverlayRect() {
  const overlay = overlayCanvas.value
  if (!overlay) return
  const ctx = overlay.getContext('2d')
  if (!ctx) return
  ctx.clearRect(0, 0, overlay.width, overlay.height)
  ctx.save()
  ctx.strokeStyle = 'red'
  ctx.lineWidth = 2
  ctx.setLineDash([6, 4])
  const x = Math.min(startX.value, currentX.value)
  const y = Math.min(startY.value, currentY.value)
  const width = Math.abs(currentX.value - startX.value)
  const height = Math.abs(currentY.value - startY.value)
  ctx.strokeRect(x, y, width, height)
  ctx.restore()
}
function getCanvasCoords(e: MouseEvent, canvasEl: HTMLCanvasElement) {
  const rect = canvasEl.getBoundingClientRect()
  const scaleX = canvasEl.width / rect.width
  const scaleY = canvasEl.height / rect.height
  const x = (e.clientX - rect.left) * scaleX
  const y = (e.clientY - rect.top) * scaleY
  return { x, y }
}
function onMouseDown(e: MouseEvent) {
  if (!pdfCanvas.value) return
  isDragging.value = true
  const { x, y } = getCanvasCoords(e, pdfCanvas.value)
  startX.value = x
  startY.value = y
  currentX.value = x
  currentY.value = y
  document.addEventListener('mousemove', onMouseMove)
  document.addEventListener('mouseup', onMouseUp)
}
function onMouseMove(e: MouseEvent) {
  if (!isDragging.value || !pdfCanvas.value) return
  const { x, y } = getCanvasCoords(e, pdfCanvas.value)
  currentX.value = x
  currentY.value = y
  drawOverlayRect()
}
function onMouseUp() {
  if (!isDragging.value || !pdfCanvas.value) return
  isDragging.value = false
  document.removeEventListener('mousemove', onMouseMove)
  document.removeEventListener('mouseup', onMouseUp)
  const x = Math.min(startX.value, currentX.value)
  const y = Math.min(startY.value, currentY.value)
  const width = Math.abs(currentX.value - startX.value)
  const height = Math.abs(currentY.value - startY.value)
  if (width > 5 && height > 5) {
    addDrawingToPdf(x, y, width, height, '')
  }
  clearOverlay()
}
const goToPrevPage = async () => {
  if (currentPage.value > 1) {
    currentPage.value--
    await renderPdf()
  }
}
const goToNextPage = async () => {
  if (currentPage.value < totalPages.value) {
    currentPage.value++
    await renderPdf()
  }
}
onMounted(async () => {
  await loadPdf()
  if (pdfCanvas.value) {
    pdfCanvas.value.addEventListener('mousedown', onMouseDown)
  }
})
onBeforeUnmount(() => {
  if (pdfCanvas.value) {
    pdfCanvas.value.removeEventListener('mousedown', onMouseDown)
  }
  document.removeEventListener('mousemove', onMouseMove)
  document.removeEventListener('mouseup', onMouseUp)
})
</script>
