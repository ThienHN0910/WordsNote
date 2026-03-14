<script lang="ts" setup>
import { computed, ref, watch, toRefs } from "vue";


type Ext = string;
interface Props {
  extensionAccept?: string | Ext[];
  maxFiles?: number;
  modelValue?: File[];
  validateExtension?: boolean;
}
const props = withDefaults(defineProps<Props>(), {
  extensionAccept: "",
  maxFiles: 10,
  modelValue: () => [],
  validateExtension: true,
});


const emit = defineEmits<{
  (e: "files-selected", files: File[]): void;
  (e: "update:modelValue", files: File[]): void;
  (e: "file-removed", file: File, index: number): void;
}>();


const { extensionAccept, maxFiles } = toRefs(props);
const isDragging = ref(false);
const errors = ref<string[]>([]);
const filesList = ref<File[]>(props.modelValue ?? []);


const normalizedExts = computed<Ext[]>(() => {
  if (!extensionAccept.value) return [];
  if (Array.isArray(extensionAccept.value)) {
    return extensionAccept.value
      .map((e) => e.trim().replace(/^\./, "").toLowerCase())
      .filter(Boolean);
  }
  return extensionAccept.value
    .split(",")
    .map((e) => e.trim().replace(/^\./, "").toLowerCase())
    .filter(Boolean);
});


const inputAcceptAttr = computed(() => {
  if (!normalizedExts.value.length) return "";
  return normalizedExts.value.map((e) => `.${e}`).join(",");
});


function clearErrors() {
  errors.value = [];
}


function addError(msg: string) {
  errors.value.push(msg);
}


function validateExtensions(files: File[]): File[] {
  if (!props.validateExtension || normalizedExts.value.length === 0) {
    return files;
  }
  const allowed = new Set(normalizedExts.value);
  return files.filter((f) => {
    const ext = f.name.split(".").pop()?.toLowerCase() ?? "";
    const ok = allowed.has(ext);
    if (!ok)
      addError(
        `File "${f.name}" invalid (allow: ${normalizedExts.value.join(", ")})`
      );
    return ok;
  });
}


function capByMax(files: File[]): File[] {
  if (!maxFiles.value || maxFiles.value <= 0) return files;
  if (files.length > maxFiles.value) {
    addError(`Chỉ cho phép tối đa ${maxFiles.value} file.`);
    return files.slice(0, maxFiles.value);
  }
  return files;
}


function handleIncomingFiles(fileList: FileList | null) {
  if (!fileList || fileList.length === 0) return;
  clearErrors();
  const incoming = Array.from(fileList);
  const validated = validateExtensions(incoming);
  const merged = capByMax([...filesList.value, ...validated]);
  if (merged.length === 0 && validated.length === 0) {
    emit("update:modelValue", []);
    emit("files-selected", []);
    return;
  }
  filesList.value = merged;
  emit("update:modelValue", filesList.value);
  emit("files-selected", filesList.value);
}


function onInputChange(e: Event) {
  const input = e.target as HTMLInputElement;
  handleIncomingFiles(input.files);
  input.value = "";
}
function onDragEnter(e: DragEvent) {
  e.preventDefault();
  e.stopPropagation();
  isDragging.value = true;
}
function onDragOver(e: DragEvent) {
  e.preventDefault();
  e.stopPropagation();
  isDragging.value = true;
}
function onDragLeave(e: DragEvent) {
  e.preventDefault();
  e.stopPropagation();
  isDragging.value = false;
}
function onDrop(e: DragEvent) {
  e.preventDefault();
  e.stopPropagation();
  isDragging.value = false;
  handleIncomingFiles(e.dataTransfer?.files ?? null);
}
function removeFile(index: number) {
  const removed = filesList.value[index];
  filesList.value = filesList.value.filter((_, i) => i !== index);
  emit("update:modelValue", filesList.value);
  emit("files-selected", filesList.value);
  if (removed) emit("file-removed", removed, index);
}


watch(
  () => props.modelValue,
  (nv) => {
    if (!nv) return;
    errors.value = [];
    filesList.value = capByMax(validateExtensions(nv));
  },
  { deep: true }
);
</script>


<template>
  <div class="du-wrapper">
    <div
      class="du-dropzone"
      :class="{ 'du-dragging': isDragging }"
      @dragenter="onDragEnter"
      @dragover="onDragOver"
      @dragleave="onDragLeave"
      @drop="onDrop"
    >
      <div class="du-instructions">
        <strong>drop file here</strong>
        <span>or</span>
        <label class="du-button" for="du-file-input">Choose file</label>
      </div>
      <input
        id="du-file-input"
        class="du-input"
        type="file"
        multiple
        :accept="inputAcceptAttr"
        @change="onInputChange"
      />
    </div>
    <div v-if="filesList.length" class="du-list">
      <div class="du-list-title">
        ({{ filesList.length }}{{ maxFiles ? " / " + maxFiles : "" }}) files selected
      </div>
      <ul class="du-items">
        <li v-for="(f, idx) in filesList" :key="f.name + f.size" class="du-item">
          <span class="du-file-name">{{ f.name }}</span>
          <span class="du-file-size">({{ (f.size / (1024 * 1024)).toFixed(2) }} MB)</span>
          <button
            type="button"
            class="du-remove"
            title="Xoá file"
            aria-label="Xoá file"
            @click="removeFile(idx)"
          >
            ×
          </button>
        </li>
      </ul>
    </div>
    <div v-if="errors.length" class="du-errors">
      <ul>
        <li v-for="(err, i) in errors" :key="i">{{ err }}</li>
      </ul>
    </div>
  </div>
</template>
