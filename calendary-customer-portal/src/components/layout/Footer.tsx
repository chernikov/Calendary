import Link from 'next/link'

import { Container } from './Container'

const footerLinks = [
  { label: 'Про нас', href: '/about' },
  { label: 'Контакти', href: '/contacts' },
  { label: 'Політика конфіденційності', href: '/privacy' }
]

export default function Footer() {
  return (
    <footer className="border-t bg-background py-10">
      <Container className="flex flex-col gap-6 md:flex-row md:items-center md:justify-between">
        <div>
          <p className="text-lg font-semibold">Calendary</p>
          <p className="text-sm text-muted-foreground">© {new Date().getFullYear()} Усі права захищено.</p>
        </div>
        <div className="flex flex-wrap gap-4 text-sm">
          {footerLinks.map((link) => (
            <Link key={link.href} href={link.href} className="text-muted-foreground hover:text-primary">
              {link.label}
            </Link>
          ))}
        </div>
      </Container>
    </footer>
  )
}
