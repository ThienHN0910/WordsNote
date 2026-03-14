<template>
  <div>
    <ul class="row" v-for="doc in docs" :key="doc.documentId">
      <li class="col-8">
        <h5>{{ doc.docTitle }}</h5>
        <p>{{ doc.docDescription }}</p>
        <small>Created at: {{ doc.createdAt }}</small>
      </li>
      <li class="col-1 align-content-center">
        <b class="text-success">{{ doc.uploadStatus }}</b>
      </li>
      <li class="col-2 align-content-center">
        <BaseDropdown
          :dropdownTitle="`Version: ${selectedVersionMap[doc.documentId]?.versionNumber ?? ''}`"
          :dropdownList="versionsMap[doc.documentId] || []"
          v-model="(selectedVersionMap[doc.documentId] as any)"
          displayKey="versionNumber"
        />
        <BaseButton @click="onOpenDoc(doc, selectedVersionMap[doc.documentId])">Open</BaseButton>
      </li>
    </ul>
    <div v-if="isLoadFail"><span class="text-danger">Loading Fail...</span></div>
  </div>
</template>

<script lang="ts" setup>
import { ref, onMounted, reactive } from "vue";
import { DocService } from "@/services/DMS/DocService";
import { VersionService } from "@/services/DMS/VersionService";
import { type DocumentDto, type DocVersion } from "@/dtos/DocType";
import BaseDropdown from "@/components/bases/BaseDropdown.vue";
import BaseButton from "@/components/bases/BaseButton.vue";

const isLoading = ref(false);
const isLoadFail = ref(false);
const docs = ref<DocumentDto[]>([]);
const versionsMap = reactive<{ [key: string]: DocVersion[] }>({});
const selectedVersionMap = reactive<{ [key: string]: DocVersion | null }>({});

async function getDocList() {
  try {
    isLoading.value = true;
    docs.value = await DocService.getDocList();
    if (docs.value.length === 0) {
      isLoading.value = false;
      return;
    }
    for (const doc of docs.value) {
      await loadVersionsForDoc(doc.documentId);
    }
    isLoading.value = false;
  } catch (err: any) {
    alert("get docs fail" + err);
    isLoading.value = false;
    isLoadFail.value = true;
  }
}

async function loadVersionsForDoc(documentId: string) {
  const res = await VersionService.getVersionByDocumentId(documentId);
  versionsMap[documentId] = res;
  selectedVersionMap[documentId] ??= res?.[0] ?? null;
}

async function onOpenDoc(doc: DocumentDto, selectedVersion: DocVersion | null) {
  if (!selectedVersion) {
    alert("Please select a version");
    return;
  }
}

onMounted(async () => {
  await getDocList();
});
</script>
