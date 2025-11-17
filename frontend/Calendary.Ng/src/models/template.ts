import { PagedResult } from './paged-result';

export interface TemplateSummary {
  id: number;
  name: string;
  description: string;
  category: string;
  price: number;
  previewImageUrl: string;
}

export interface TemplateDetail extends TemplateSummary {
  templateData: string;
  sortOrder: number;
  createdAt: string;
  galleryImages?: string[];
  features?: string[];
  recommendedUses?: string[];
  tags?: string[];
}

export interface TemplateQueryParams {
  category?: string | null;
  search?: string | null;
  minPrice?: number | null;
  maxPrice?: number | null;
  page?: number;
  pageSize?: number;
  sortBy?: string | null;
}

export type TemplatePagedResult = PagedResult<TemplateSummary>;
