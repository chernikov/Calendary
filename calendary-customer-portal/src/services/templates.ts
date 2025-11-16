import axios from 'axios'
import { type ApiResponse, type TemplateDetail, type TemplateSummary } from '@/types/api'

const api = axios.create({
  baseURL: process.env.NEXT_PUBLIC_API_URL || 'http://localhost:5196/api'
})

export async function fetchTemplates(): Promise<TemplateSummary[]> {
  const { data } = await api.get<ApiResponse<TemplateSummary[]>>('/templates')
  return data.data
}

export async function fetchTemplateById(id: string): Promise<TemplateDetail> {
  const { data } = await api.get<ApiResponse<TemplateDetail>>(`/templates/${id}`)
  return data.data
}
