export class AdminTestPrompt
{
    id: number = 0;
    promptId: number = 0;
    trainingId: number = 0;
    text: string = '';
    seed : number | null = null;
    outputSeed : number | null = null;
    status: string = '';
    processedImageUrl?: string = '';
    imageUrl?: string = '';
  }