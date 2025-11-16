import Link from 'next/link'

import { Button } from '@/components/ui/button'
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'
import { Container } from '@/components/layout/Container'
import { ROUTES } from '@/lib/navigation'

export default function ProfilePage() {
  return (
    <section className="py-12">
      <Container>
        <Card>
          <CardHeader>
            <CardTitle>Особистий кабінет</CardTitle>
            <CardDescription>Аутентифікація буде реалізована в Task 21. Наразі це демо-сторінка.</CardDescription>
          </CardHeader>
          <CardContent className="flex flex-col gap-4">
            <p className="text-sm text-muted-foreground">Вітаємо, Олена! Тут буде відображатися ваша інформація та активні замовлення.</p>
            <Button variant="outline" asChild>
              <Link href={ROUTES.ORDERS}>Переглянути історію замовлень</Link>
            </Button>
          </CardContent>
        </Card>
      </Container>
    </section>
  )
}
