<template>
  <div class="post-editor-container">
    <h3 class="mb-3">Post Editor</h3>

    <div class="document-editor">
      <!-- Toolbar -->
      <div ref="toolbarContainer" class="document-editor__toolbar"></div>

      <!-- Editor container -->
      <div class="document-editor__editable-container">
        <ckeditor
          :editor="editor"
          v-model="editorData"
          :config="editorConfig"
          @ready="onEditorReady"
          
        ></ckeditor>
      </div>
    </div>

    <div class="actions mt-3">
      <button class="btn btn-primary" @click="savePost" :disabled="isSaving">
        {{ isSaving ? 'Posting...' : 'Post' }}
      </button>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, defineProps, defineEmits } from "vue";
import DecoupledEditor from "@ckeditor/ckeditor5-build-decoupled-document";
import { PostService } from "@/services/CMS/PostService";

const props = defineProps({
  limitImageUpload: {
    type: Number,
    default: 0 // 0 means no limit
  }
});

const emit = defineEmits(["post-created"]);

const editor = DecoupledEditor;

const editorData = ref(
  ""
);

const toolbarContainer = ref<HTMLElement | null>(null);
const isSaving = ref(false);

const editorConfig = PostService.editorConfig;

/* ---------------------------------------
  TOOLBAR HANDLER (Decoupled Editor)
  --------------------------------------- */
const onEditorReady = (editorInstance: any) => {
    const toolbarEl = editorInstance.ui.view.toolbar.element;

    if (toolbarContainer.value) {
      toolbarContainer.value.appendChild(toolbarEl);
    }
  }
/* ---------------------------------------
  Save Post
--------------------------------------- */
const savePost = async () => {
  if (!editorData.value.trim()) {
    alert("Post content cannot be empty.");
    return;
  }

  // Check image upload limit
  if (props.limitImageUpload > 0) {
    const imgCount = (editorData.value.match(/<img[^>]+src="data:image/g) || []).length;
    if (imgCount > props.limitImageUpload) {
      alert(`You can only upload a maximum of ${props.limitImageUpload} images.`);
      return;
    }
  }

  isSaving.value = true;

  try {
    const res = await PostService.createPost(editorData.value);

    if (res.status === 200 || res.status === 201) {
      alert("Post created successfully!");
      editorData.value = "";
      emit("post-created", res.data);
    }
  } catch (err: any) {
    alert(err.response?.data?.message || "Failed to create post.");
  } finally {
    isSaving.value = false;
  }
};
</script>

