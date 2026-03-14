export type DocUploadDTO = {
  Title: string;
  Description: string;
  Tags: string[];
}

export interface CreateUpdateUploadDto {
  doc: DocUploadDTO | null
  file: File | null
}

export interface DocumentDto {
  documentId: string;
  docTitle: string;
  docDescription: string;
  createdAt: string;
  updatedAt?: string | null;
  uploadStatus: string;
  tags: Tag[];
  versions: DocVersion[];
}

export interface Tag {
  tagId: string;
  tagName: string;
}

export interface DocVersion {
  versionId: string;
  versionNumber: number;
  documentId: string;
  createdAt?: string;
  updatedAt?: string;
  filePath: string;
  document?: DocumentDto;
}
