// models/admin-flux-model.ts
import { Job } from './job';
import { Training } from './training';
import { Photo } from './photo';
import { UserInfo } from './user';
import { Category } from './category';

export class AdminFluxModel {
  id: number = 0;
  name: string = '';
  version: string = '';
  description: string = '';
  status: string = ''; 
  categoryId: number = 0;
  archiveUrl?: string;
  createdAt: Date = new Date();
  completedAt?: Date;
  isPaid: boolean = false;
  user: UserInfo = new UserInfo();
  photos: Photo[] = [];
  trainings: Training[] = [];
  jobs: Job[] = [];
  category: Category = new Category();
}
