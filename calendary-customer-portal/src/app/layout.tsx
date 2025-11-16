import type { Metadata } from 'next'
import { Inter, Montserrat } from 'next/font/google'
import '@/styles/globals.css'

import Header from '@/components/layout/Header'
import Footer from '@/components/layout/Footer'
import { ThemeProvider } from '@/components/providers/theme-provider'
import { Toaster } from '@/components/ui/toaster'

const inter = Inter({ subsets: ['latin', 'cyrillic'], variable: '--font-inter' })
const montserrat = Montserrat({ subsets: ['latin', 'cyrillic'], variable: '--font-montserrat' })

export const metadata: Metadata = {
  title: 'Calendary — Cтворіть свій персоналізований календар',
  description: 'Customer Portal Calendary для замовлення унікальних календарів',
  metadataBase: new URL(process.env.NEXT_PUBLIC_SITE_URL || 'http://localhost:3000')
}

export default function RootLayout({ children }: { children: React.ReactNode }) {
  return (
    <html lang="uk" suppressHydrationWarning>
      <body className={`${inter.variable} ${montserrat.variable} font-sans`}>
        <ThemeProvider attribute="class" defaultTheme="system" enableSystem>
          <Header />
          <main className="min-h-[calc(100vh-200px)] bg-background">{children}</main>
          <Footer />
          <Toaster />
        </ThemeProvider>
      </body>
    </html>
  )
}
