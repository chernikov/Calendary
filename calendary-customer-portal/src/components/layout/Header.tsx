'use client'

import Link from 'next/link'
import { usePathname } from 'next/navigation'
import { Menu, ShoppingCart } from 'lucide-react'
import { useState } from 'react'

import { cn } from '@/lib/utils'
import { ROUTES } from '@/lib/navigation'
import { Button } from '@/components/ui/button'
import { DropdownMenu, DropdownMenuContent, DropdownMenuItem, DropdownMenuTrigger } from '@/components/ui/dropdown-menu'
import { Container } from './Container'
import { ThemeToggle } from './ThemeToggle'

const navigation = [
  { label: 'Каталог', href: ROUTES.CATALOG },
  { label: 'Редактор', href: ROUTES.EDITOR },
  { label: 'Кошик', href: ROUTES.CART }
]

export default function Header() {
  const pathname = usePathname()
  const [open, setOpen] = useState(false)

  return (
    <header className="sticky top-0 z-40 border-b bg-background/80 backdrop-blur">
      <Container className="flex h-16 items-center justify-between">
        <Link href={ROUTES.HOME} className="text-lg font-semibold">
          Calendary
        </Link>
        <nav className="hidden items-center gap-6 md:flex">
          {navigation.map((item) => (
            <Link
              key={item.href}
              href={item.href}
              className={cn('text-sm font-medium transition-colors hover:text-primary', pathname.startsWith(item.href) && 'text-primary')}
            >
              {item.label}
            </Link>
          ))}
        </nav>
        <div className="flex items-center gap-3">
          <ThemeToggle />
          <Button variant="ghost" size="icon" asChild>
            <Link href={ROUTES.CART}>
              <ShoppingCart className="h-5 w-5" />
              <span className="sr-only">Перейти до кошика</span>
            </Link>
          </Button>
          <DropdownMenu>
            <DropdownMenuTrigger asChild>
              <Button variant="secondary">Обліковий запис</Button>
            </DropdownMenuTrigger>
            <DropdownMenuContent align="end">
              <DropdownMenuItem asChild>
                <Link href={ROUTES.LOGIN}>Увійти</Link>
              </DropdownMenuItem>
              <DropdownMenuItem asChild>
                <Link href={ROUTES.REGISTER}>Зареєструватися</Link>
              </DropdownMenuItem>
            </DropdownMenuContent>
          </DropdownMenu>
          <Button variant="ghost" size="icon" className="md:hidden" onClick={() => setOpen((prev) => !prev)}>
            <Menu className="h-5 w-5" />
          </Button>
        </div>
      </Container>
      {open && (
        <div className="border-t bg-background py-4 md:hidden">
          <Container className="flex flex-col gap-4">
            {navigation.map((item) => (
              <Link
                key={item.href}
                href={item.href}
                className={cn('text-base font-medium', pathname.startsWith(item.href) && 'text-primary')}
                onClick={() => setOpen(false)}
              >
                {item.label}
              </Link>
            ))}
            <Button asChild>
              <Link href={ROUTES.LOGIN}>Вхід</Link>
            </Button>
            <Button variant="outline" asChild>
              <Link href={ROUTES.REGISTER}>Реєстрація</Link>
            </Button>
          </Container>
        </div>
      )}
    </header>
  )
}
