export class CreateSynthesis {
    promptId: number = 0;
    fluxModelId: number = 0;
    text: string = '';
    seed: number | null = null;
}

export class Synthesis {
  id: number = 0;
  promptId: number= 0;
  trainingId: number = 0;
  text: string = '';
  seed: number | null = null;
  outputSeed: number | null = null;
  imageUrl: string= '';
  processedImageUrl?: string | null = null;
  status: string = '';
  retryCount: number = 0; 
  createdAt: Date = new Date();
  completedAt?: Date | null = null;
}
