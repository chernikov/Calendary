export interface UserPhoto {
  id: number;
  userId: number;
  imageUrl: string;
  caption?: string;
  originalFileName?: string;
  fileSize: number;
  createdAt: Date;
  updatedAt?: Date;
  isDeleted: boolean;
}

export interface UserPhotoUpload {
  file: File;
  caption?: string;
}
