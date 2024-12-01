import { Category } from "./category";
import { PromptSeed } from "./promt-seed";

export class Prompt {
    id: number = 0;
    themeId: number = 0;
    categoryId: number = 0;
    text: string = '';
    themeName: string = '';
    seeds: PromptSeed[] = [];
    category : Category = new Category();
}