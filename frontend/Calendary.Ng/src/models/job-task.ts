export class JobTask {
    id: number = 0;
    imageUrl: string = "";
    processedImageUrl?: string | null = null;
    status: string = "";
    retryCount: number = 0;
    createdAt: Date = new Date();
    completedAt?: Date | null = null;
}