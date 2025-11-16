import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'
import { Container } from '@/components/layout/Container'

const orders = [
  {
    id: 'ORD-1001',
    status: 'Очікує на оплату',
    date: '12 листопада 2025',
    total: '749 ₴'
  }
]

export default function OrdersPage() {
  return (
    <section className="py-12">
      <Container>
        <Card>
          <CardHeader>
            <CardTitle>Історія замовлень</CardTitle>
            <CardDescription>Повноцінна інтеграція з API буде у наступних етапах.</CardDescription>
          </CardHeader>
          <CardContent className="space-y-4">
            {orders.map((order) => (
              <div key={order.id} className="rounded-lg border p-4">
                <p className="font-semibold">#{order.id}</p>
                <p className="text-sm text-muted-foreground">{order.date}</p>
                <p className="text-sm">{order.status}</p>
                <p className="text-sm font-medium">{order.total}</p>
              </div>
            ))}
          </CardContent>
        </Card>
      </Container>
    </section>
  )
}
