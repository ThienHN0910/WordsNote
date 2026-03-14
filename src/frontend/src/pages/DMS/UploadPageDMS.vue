<template>
  <BaseInput label="Title" v-model="title" />
  <BaseInput label="Description" v-model="description" />
  <BaseInput label="Upload File" type="File" @change="onFileChange" />
  <div class="row">
    <div class="col-6"></div>
    <BaseDropdown
      class="col-2"
      :dropdownList="listTag"
      dropdownTitle="Select Tags"
      v-model="selectedTag"
      searchTitle="Search Tag"
      addNew="Add New Tag"
      @addNew="handleAddNewTag"
    ></BaseDropdown>
  </div>
  <div class="row mt-1">
    <div class="col-6"></div>
    <div class="col-2">
      <BaseButton @click="onUpload" :disabled="isLoading">{{ isLoading ? 'Uploading...' : 'Upload' }}</BaseButton>
    </div>
  </div>
</template>

<script lang="ts" setup>
import BaseDropdown from "@/components/bases/BaseDropdown.vue";
import BaseInput from "@/components/bases/BaseInput.vue";
import { ref } from "vue";
import { UploadService } from "@/services/DMS/UploadService";
import { msg } from "@/constants/Msg.error";
import BaseButton from "@/components/bases/BaseButton.vue";

const title = ref("");
const description = ref("");
const selectedTag = ref<any>({});
const file = ref<File | null>(null);
const isLoading = ref(false);
const listTag = ref([]);

function onFileChange(event: Event) {
  const target = event.target as HTMLInputElement;
  if (target.files && target.files.length > 0) {
    file.value = target.files[0];
  }
}

async function onUpload() {
  try {
    isLoading.value = true;
    await UploadService.uploadDocument({
      file: file.value,
      doc: {
        Title: title.value,
        Description: description.value,
        Tags: selectedTag.value || [],
      },
    });
    alert(msg.UPLOAD_SUCCESS);
  } catch (err: any) {
    alert(msg.UPLOAD_FAIL + "..." + err.message);
  } finally {
    isLoading.value = false;
  }
}

async function handleAddNewTag(TagName: String) {
  debugger;
}
</script>
