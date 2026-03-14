import { PostAPI } from '@/apis/CMS/PostAPI'

function dataURLtoFile(dataurl: string, filename: string) {
  const arr = dataurl.split(",");
  const mime = arr[0].match(/:(.*?);/)![1];
  const bstr = atob(arr[1]);
  let n = bstr.length;
  let u8arr = new Uint8Array(n);
  while (n--) u8arr[n] = bstr.charCodeAt(n);
  return new File([u8arr], filename, { type: mime });
}

export const PostService = {
  async getPosts(page: number, limit: number) {
    return await PostAPI.GetPosts(page, limit)
  },

  async createPost(content: string) {
    const formData = new FormData();

    // Regex tách ảnh base64 trong nội dung
    const imgRegex = /data:image\/(.*?);base64,([a-zA-Z0-9+/=]+)/g;

    let match;
    let index = 0;
    let replacedContent = content;

    while ((match = imgRegex.exec(content)) !== null) {
      const ext = match[1];            // png, jpg...
      const base64 = match[2];         // phần base64
      const fullTag = match[0];        // "data:image/...base64,..."

      const file = dataURLtoFile(
        `data:image/${ext};base64,${base64}`,
        `upload_${index}.${ext}`
      );

      formData.append("Files", file);

      // Optional: thay thế base64 trong HTML bằng "placeholder" (nếu backend cần)
      replacedContent = replacedContent.replace(fullTag, `{{image:${index}}}`);

      index++;
    }

    formData.append("Content", replacedContent);

    return await PostAPI.CreatePost(formData);
  },
  editorConfig: {
    extraPlugins: [Base64UploadAdapterPlugin],
  },
}

/* ---------------------------------------
    1) CUSTOM UPLOAD ADAPTER (Base64)
--------------------------------------- */
class Base64UploadAdapter {
  loader: any;
  constructor(loader: any) {
    this.loader = loader;
  }
  upload() {
    return this.loader.file.then(
      (file: File) =>
        new Promise((resolve, reject) => {
          const reader = new FileReader();
          reader.onload = () => resolve({ default: reader.result });
          reader.onerror = (err) => reject(err);
          reader.readAsDataURL(file);
        })
    );
  }
  abort() { }
}

function Base64UploadAdapterPlugin(editor: any) {
  editor.plugins.get("FileRepository").createUploadAdapter = (loader: any) => {
    return new Base64UploadAdapter(loader);
  };
}


