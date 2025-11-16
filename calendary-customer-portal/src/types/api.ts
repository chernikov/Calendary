export type TemplateCategory = 'wall' | 'desk' | 'poster'

export interface TemplateSummary {
  id: string
  name: string
  description: string
  thumbnailUrl: string
  category: TemplateCategory
  price: number
}

export interface TemplateDetail extends TemplateSummary {
  availableSizes: string[]
  tags: string[]
  pages: number
}

export interface ApiResponse<T> {
  data: T
  message?: string
}
