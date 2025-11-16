import Link from 'next/link'
import Image from 'next/image'

import { Button } from '@/components/ui/button'
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'
import { Container } from '@/components/layout/Container'
import { ROUTES } from '@/lib/navigation'

const highlights = [
  {
    title: '200+ шаблонів',
    description: 'Велика бібліотека сучасних шаблонів для настінних та настільних календарів.'
  },
  {
    title: 'Редактор у браузері',
    description: 'Миттєво налаштовуйте макет, фото та кольори без додаткових програм.'
  },
  {
    title: 'Доставка по всій Україні',
    description: 'Партнерство з Новою Поштою гарантує швидке та надійне отримання.'
  }
]

export default function HomePage() {
  return (
    <>
      <section className="bg-gradient-to-b from-primary/10 to-background py-16">
        <Container className="grid gap-10 md:grid-cols-2 md:items-center">
          <div className="space-y-6">
            <p className="inline-flex items-center rounded-full border px-3 py-1 text-sm text-primary">Новий Customer Portal</p>
            <h1 className="text-4xl font-bold leading-tight text-foreground md:text-5xl">
              Створіть персональний календар за кілька хвилин
            </h1>
            <p className="text-lg text-muted-foreground">
              Обирайте шаблон, додавайте фотографії та замовляйте друк з доставкою додому. Увесь процес відбувається онлайн, без зайвих турбот.
            </p>
            <div className="flex flex-col gap-3 sm:flex-row">
              <Button size="lg" asChild>
                <Link href={ROUTES.CATALOG}>Перейти до каталогу</Link>
              </Button>
              <Button size="lg" variant="outline" asChild>
                <Link href={ROUTES.EDITOR}>Запустити редактор</Link>
              </Button>
            </div>
          </div>
          <div className="relative h-72 w-full overflow-hidden rounded-2xl border bg-card shadow-lg md:h-96">
            <Image src="https://images.unsplash.com/photo-1489515217757-5fd1be406fef" alt="Приклад календаря" fill className="object-cover" priority />
          </div>
        </Container>
      </section>
      <section className="py-16">
        <Container className="grid gap-6 md:grid-cols-3">
          {highlights.map((item) => (
            <Card key={item.title}>
              <CardHeader>
                <CardTitle>{item.title}</CardTitle>
                <CardDescription>{item.description}</CardDescription>
              </CardHeader>
              <CardContent>
                <Button variant="link" asChild>
                  <Link href={ROUTES.CATALOG}>Дізнатися більше</Link>
                </Button>
              </CardContent>
            </Card>
          ))}
        </Container>
      </section>
    </>
  )
}
