import { notFound } from 'next/navigation'

import { Button } from '@/components/ui/button'
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'
import { Container } from '@/components/layout/Container'
import { templates } from '@/data/mock-templates'

interface EditorPageProps {
  params: { id: string }
}

export function generateStaticParams() {
  return templates.map((template) => ({ id: template.id }))
}

export default function EditorPage({ params }: EditorPageProps) {
  const template = templates.find((item) => item.id === params.id)

  if (!template) {
    return notFound()
  }

  return (
    <section className="py-12">
      <Container>
        <Card>
          <CardHeader>
            <CardTitle>Редактор: {template.name}</CardTitle>
            <CardDescription>Додайте свої фото, текст та кольори. Canvas-редактор буде підключений у Task 12-16.</CardDescription>
          </CardHeader>
          <CardContent>
            <div className="grid gap-4 md:grid-cols-2">
              <div className="h-64 rounded-lg border bg-muted" />
              <div className="space-y-4">
                <p className="text-sm text-muted-foreground">
                  Тут з&apos;явиться інтерактивний редактор. Поки що ми залишаємо placeholder, щоб дизайнери могли протестувати layout.
                </p>
                <Button>Зберегти зміни</Button>
              </div>
            </div>
          </CardContent>
        </Card>
      </Container>
    </section>
  )
}
