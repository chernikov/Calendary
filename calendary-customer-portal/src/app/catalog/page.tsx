import Link from 'next/link'

import { Badge } from '@/components/ui/badge'
import { Button } from '@/components/ui/button'
import { Card, CardContent, CardDescription, CardFooter, CardHeader, CardTitle } from '@/components/ui/card'
import { Container } from '@/components/layout/Container'
import { ROUTES } from '@/lib/navigation'
import { templates } from '@/data/mock-templates'

export const metadata = {
  title: 'Каталог шаблонів — Calendary'
}

export default function CatalogPage() {
  return (
    <section className="py-12">
      <Container>
        <div className="flex flex-col gap-4 pb-8">
          <h1 className="text-3xl font-bold">Каталог шаблонів</h1>
          <p className="text-muted-foreground">Оберіть шаблон та переходьте у редактор для персоналізації.</p>
        </div>
        <div className="grid gap-6 md:grid-cols-3">
          {templates.map((template) => (
            <Card key={template.id}>
              <CardHeader>
                <Badge>{template.category}</Badge>
                <CardTitle>{template.name}</CardTitle>
                <CardDescription>{template.description}</CardDescription>
              </CardHeader>
              <CardContent>
                <p className="text-2xl font-semibold">{template.price} ₴</p>
              </CardContent>
              <CardFooter className="justify-between">
                <Button variant="outline" asChild>
                  <Link href={`${ROUTES.EDITOR}/${template.id}`}>Редагувати</Link>
                </Button>
                <Button asChild>
                  <Link href={ROUTES.CART}>У кошик</Link>
                </Button>
              </CardFooter>
            </Card>
          ))}
        </div>
      </Container>
    </section>
  )
}
