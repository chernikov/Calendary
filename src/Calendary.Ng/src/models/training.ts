// Тип для TrainingDto
export class Training {
    id: number = 0;
    replicateId: string = "";
    version: string = '';
    status: string = "";
    createdAt: Date = new Date();
    completedAt?: Date | null  = null;
}