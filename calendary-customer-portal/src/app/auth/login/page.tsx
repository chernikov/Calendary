import Link from 'next/link'

import { useForm } from 'react-hook-form'

import { Button } from '@/components/ui/button'
import { Card, CardContent, CardDescription, CardFooter, CardHeader, CardTitle } from '@/components/ui/card'
import { Form, FormControl, FormField, FormItem, FormLabel, FormMessage } from '@/components/ui/form'
import { Input } from '@/components/ui/input'
import { Container } from '@/components/layout/Container'
import { ROUTES } from '@/lib/navigation'

export default function LoginPage() {
  const form = useForm({
    defaultValues: {
      email: '',
      password: ''
    }
  })

  return (
    <section className="py-12">
      <Container className="max-w-lg">
        <Card>
          <CardHeader>
            <CardTitle>Вхід</CardTitle>
            <CardDescription>Аутентифікація з&apos;явиться після Task 21.</CardDescription>
          </CardHeader>
          <CardContent>
            <Form {...form}>
              <form id="login-form" className="space-y-4" onSubmit={form.handleSubmit(() => undefined)}>
                <FormField
                  control={form.control}
                  name="email"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel>Email</FormLabel>
                      <FormControl>
                        <Input type="email" placeholder="you@example.com" {...field} />
                      </FormControl>
                      <FormMessage />
                    </FormItem>
                  )}
                />
                <FormField
                  control={form.control}
                  name="password"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel>Пароль</FormLabel>
                      <FormControl>
                        <Input type="password" {...field} />
                      </FormControl>
                      <FormMessage />
                    </FormItem>
                  )}
                />
              </form>
            </Form>
          </CardContent>
          <CardFooter className="flex flex-col gap-3">
            <Button className="w-full" type="submit" form="login-form" formNoValidate>
              Увійти
            </Button>
            <p className="text-sm text-muted-foreground">
              Немає акаунта? <Link href={ROUTES.REGISTER} className="text-primary">Зареєструватися</Link>
            </p>
          </CardFooter>
        </Card>
      </Container>
    </section>
  )
}
