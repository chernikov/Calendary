import Link from 'next/link'

import { Button } from '@/components/ui/button'
import { Container } from '@/components/layout/Container'
import { ROUTES } from '@/lib/utils'

export default function NotFoundPage() {
  return (
    <section className="py-24">
      <Container className="text-center">
        <p className="text-sm font-semibold text-primary">404</p>
        <h1 className="mt-4 text-4xl font-bold">Сторінку не знайдено</h1>
        <p className="mt-2 text-muted-foreground">Можливо, вона була переміщена або видалена.</p>
        <Button className="mt-6" asChild>
          <Link href={ROUTES.HOME}>На головну</Link>
        </Button>
      </Container>
    </section>
  )
}
