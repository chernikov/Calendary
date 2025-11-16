'use client'

import { useMemo } from 'react'

import { Button } from '@/components/ui/button'
import { Card, CardContent, CardDescription, CardFooter, CardHeader, CardTitle } from '@/components/ui/card'
import { Container } from '@/components/layout/Container'
import { useCartStore } from '@/store/cart'

export default function CartPage() {
  const { items, removeItem, clear } = useCartStore()
  const total = useMemo(() => items.reduce((sum, item) => sum + item.price * item.quantity, 0), [items])

  return (
    <section className="py-12">
      <Container>
        <Card>
          <CardHeader>
            <CardTitle>Ваш кошик</CardTitle>
            <CardDescription>Додайте шаблони у редакторі, щоб побачити їх тут.</CardDescription>
          </CardHeader>
          <CardContent className="space-y-4">
            {items.length === 0 && <p className="text-sm text-muted-foreground">Кошик порожній.</p>}
            {items.map((item) => (
              <div key={item.id} className="flex items-center justify-between rounded-lg border p-4">
                <div>
                  <p className="font-medium">{item.title}</p>
                  <p className="text-sm text-muted-foreground">Кількість: {item.quantity}</p>
                </div>
                <div className="flex items-center gap-4">
                  <p className="font-semibold">{item.price * item.quantity} ₴</p>
                  <Button variant="ghost" onClick={() => removeItem(item.id)}>
                    Видалити
                  </Button>
                </div>
              </div>
            ))}
          </CardContent>
          <CardFooter className="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
            <p className="text-lg font-semibold">Разом: {total} ₴</p>
            <div className="flex gap-2">
              <Button variant="outline" onClick={clear} disabled={!items.length}>
                Очистити
              </Button>
              <Button disabled={!items.length}>Оформити</Button>
            </div>
          </CardFooter>
        </Card>
      </Container>
    </section>
  )
}
