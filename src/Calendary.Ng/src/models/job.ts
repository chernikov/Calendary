import { JobTask } from "./job-task";

export class Job {
    id: number = 0;
    status: string = '';
    createdAt: Date = new Date();
    completedAt?: Date | null = null
    tasks: JobTask[] = [];
}