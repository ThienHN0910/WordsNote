import apiClient from '@/apis/apiClient';

export const PostAPI = {
  async GetPosts(page: number, limit: number) {
    return await apiClient.get('/api/Post', {
      params: { page, limit },
    });
  },
  async CreatePost(formData: FormData) {
    return await apiClient.post('/api/Post', formData, {
      headers: {
        "Content-Type": "multipart/form-data",
      },
    });
  },
};