import { useForm } from 'react-hook-form'

import { Button } from '@/components/ui/button'
import { Card, CardContent, CardDescription, CardFooter, CardHeader, CardTitle } from '@/components/ui/card'
import { Form, FormControl, FormDescription, FormField, FormItem, FormLabel, FormMessage } from '@/components/ui/form'
import { Input } from '@/components/ui/input'
import { Container } from '@/components/layout/Container'

export default function CheckoutPage() {
  const form = useForm({
    defaultValues: {
      name: '',
      phone: '',
      address: ''
    }
  })

  return (
    <section className="py-12">
      <Container>
        <Card>
          <CardHeader>
            <CardTitle>Оформлення замовлення</CardTitle>
            <CardDescription>Введіть контактні дані, щоб завершити покупку.</CardDescription>
          </CardHeader>
          <CardContent>
            <Form {...form}>
              <form id="checkout-form" className="grid gap-6 md:grid-cols-2" onSubmit={form.handleSubmit(() => undefined)}>
                <FormField
                  control={form.control}
                  name="name"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel>Ім&apos;я</FormLabel>
                      <FormControl>
                        <Input placeholder="Олена" {...field} />
                      </FormControl>
                      <FormDescription>Як звертатися кур&apos;єру.</FormDescription>
                      <FormMessage />
                    </FormItem>
                  )}
                />
                <FormField
                  control={form.control}
                  name="phone"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel>Телефон</FormLabel>
                      <FormControl>
                        <Input placeholder="+380" {...field} />
                      </FormControl>
                      <FormMessage />
                    </FormItem>
                  )}
                />
                <FormField
                  control={form.control}
                  name="address"
                  render={({ field }) => (
                    <FormItem className="md:col-span-2">
                      <FormLabel>Адреса доставки</FormLabel>
                      <FormControl>
                        <Input placeholder="Київ, вул. Прикладна 10" {...field} />
                      </FormControl>
                      <FormMessage />
                    </FormItem>
                  )}
                />
              </form>
            </Form>
          </CardContent>
          <CardFooter className="justify-end">
            <Button type="submit" form="checkout-form">
              Перейти до оплати
            </Button>
          </CardFooter>
        </Card>
      </Container>
    </section>
  )
}
