import { Category } from "./category";
import { Job } from "./job";
import { Photo } from "./photo";
import { Training } from "./training";
import { UserInfo } from "./user";

export class FluxModel {
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
    trainings : Training[] = [];
    jobs: Job[] = [];
    category : Category = new Category();
  }

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
  

  export class CreateFluxModel {
    name: string = '';
    categoryId?: number | null = null;
  }