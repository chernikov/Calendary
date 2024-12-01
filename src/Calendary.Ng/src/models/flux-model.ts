import { Category } from "./category";
import { Job } from "./job";
import { Training } from "./training";

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