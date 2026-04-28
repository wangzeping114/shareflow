<template>
  <div class="signature-pad-wrapper">
    <canvas
      ref="canvasRef"
      :width="width"
      :height="height"
      class="signature-canvas"
      @mousedown="startDraw"
      @mousemove="draw"
      @mouseup="stopDraw"
      @mouseleave="stopDraw"
      @touchstart.prevent="startDrawTouch"
      @touchmove.prevent="drawTouch"
      @touchend="stopDraw"
    />
    <div class="signature-pad-actions">
      <n-button size="small" @click="clear">{{ $t('common.clear') }}</n-button>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { NButton } from 'naive-ui'

interface Props {
  width?: number
  height?: number
}

const props = withDefaults(defineProps<Props>(), {
  width: 500,
  height: 200,
})

const emit = defineEmits<{
  'update:modelValue': [value: string]
}>()

const canvasRef = ref<HTMLCanvasElement | null>(null)
let ctx: CanvasRenderingContext2D | null = null
let isDrawing = false

onMounted(() => {
  if (canvasRef.value) {
    ctx = canvasRef.value.getContext('2d')
    if (ctx) {
      ctx.lineWidth = 2
      ctx.lineCap = 'round'
      ctx.strokeStyle = '#000'
    }
  }
})

function getPos(e: MouseEvent) {
  const rect = canvasRef.value!.getBoundingClientRect()
  return { x: e.clientX - rect.left, y: e.clientY - rect.top }
}

function getPosTouch(e: TouchEvent) {
  const rect = canvasRef.value!.getBoundingClientRect()
  const t = e.touches[0]
  return { x: t.clientX - rect.left, y: t.clientY - rect.top }
}

function startDraw(e: MouseEvent) {
  isDrawing = true
  const { x, y } = getPos(e)
  ctx?.beginPath()
  ctx?.moveTo(x, y)
}

function draw(e: MouseEvent) {
  if (!isDrawing || !ctx) return
  const { x, y } = getPos(e)
  ctx.lineTo(x, y)
  ctx.stroke()
}

function startDrawTouch(e: TouchEvent) {
  isDrawing = true
  const { x, y } = getPosTouch(e)
  ctx?.beginPath()
  ctx?.moveTo(x, y)
}

function drawTouch(e: TouchEvent) {
  if (!isDrawing || !ctx) return
  const { x, y } = getPosTouch(e)
  ctx?.lineTo(x, y)
  ctx?.stroke()
}

function stopDraw() {
  if (isDrawing) {
    isDrawing = false
    emitValue()
  }
}

function emitValue() {
  if (canvasRef.value) {
    emit('update:modelValue', canvasRef.value.toDataURL('image/png'))
  }
}

function clear() {
  if (ctx && canvasRef.value) {
    ctx.clearRect(0, 0, props.width, props.height)
    emit('update:modelValue', '')
  }
}

defineExpose({ clear })
</script>

<style scoped>
.signature-pad-wrapper {
  display: inline-flex;
  flex-direction: column;
  gap: 8px;
}

.signature-canvas {
  border: 1px solid #d9d9d9;
  border-radius: 4px;
  cursor: crosshair;
  background: #fff;
  touch-action: none;
}

.signature-pad-actions {
  display: flex;
  justify-content: flex-end;
}
</style>
