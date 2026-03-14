import { FileAPI } from '@/apis/DMS/FileAPI'
import { type CreateUpdateUploadDto, type DocUploadDTO } from '@/dtos/DocType'
export const UploadService = {
  async uploadDocument(params: CreateUpdateUploadDto) {
    if (!params.file) throw new Error('File is required')
    return await FileAPI.upload(params)
  },
}
