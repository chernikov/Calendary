
export class FluxModel {
    id: number = 0;
    name: string = '';
    description: string = '';
    status: string = '';
    gender: number = 0;
    archiveUrl?: string;
    createdAt: Date = new Date();
    completedAt?: Date;
    isPaid: boolean = false;
  }