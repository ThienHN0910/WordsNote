<template>
  <div class="btn-group m-1" :class="inputClass">
    <button type="button" class="btn btn-primary" :class="buttonClass">{{ dropdownTitle }}</button>
    <button
      type="button"
      class="btn btn-primary dropdown-toggle dropdown-toggle-split"
      :class="buttonClass"
      data-bs-toggle="dropdown"
      aria-expanded="false"
    ></button>
    <ul class="dropdown-menu app-base-dropdown" :class="menuClass" role="menu">
      <li v-if="addNew">
        <input
          type="text"
          class="form-control"
          :placeholder="searchTitle"
          v-model="addNewInput"
          @input="onInputChange"
          role="menuitem"
        />
      </li>
      <li v-for="(item, idx) in dropdownList" :key="idx">
        <a class="dropdown-item" href="#" @click.prevent="onSelect(item)" role="menuitem">{{
          displayKey ? (item as any)[displayKey] : item
        }}</a>
      </li>
      <li v-if="addNew">
        <a class="dropdown-item" href="#" @click.prevent="onAddNew" role="menuitem">
          {{ addNew }}
        </a>
      </li>
    </ul>
  </div>
</template>

<script lang="ts" setup>
import { defineProps, defineEmits } from 'vue'
import { ref } from 'vue'
const props = defineProps({
  modelValueInput: {
    type: String,
    default: '',
  },
  dropdownTitle: {
    type: String, 
    default: 'Dropdown' 
  },
  buttonClass: {
    type: [String, Array, Object], default: '' 
  },
  menuClass: { 
    type: [String, Array, Object], default: '' 
  },
  dropdownList: { 
    type: Array, 
    default: [],
  },
  inputClass: { 
    type: String, 
    default: '' 
  },
  addNew: { 
    type: String, 
    default: '' 
  },
  searchTitle: { 
    type: String, 
    default: '' 
  },
  displayKey: { 
    type: String, 
    default: null 
  },
  modelValue: { type: Object, default: null },
})

const addNewInput = ref(props.modelValueInput)

const emit = defineEmits<{
  (e: 'update:modelValue', value: any): void
  (e: 'add-new'): void
  (e: 'search', value: string): void
}>()

function onSelect(item: any) {
  emit('update:modelValue', item)
}

function onAddNew() {
  emit('add-new')
}

function onInputChange() {
  emit('search', addNewInput.value)
}
</script>
