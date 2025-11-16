'use client'

import * as React from 'react'
import * as ToastPrimitives from '@radix-ui/react-toast'
import { X } from 'lucide-react'

import { cn } from '@/lib/utils'
import { useToast } from './use-toast'

const ToastProvider = ToastPrimitives.Provider
const ToastViewport = React.forwardRef<React.ElementRef<typeof ToastPrimitives.Viewport>, React.ComponentPropsWithoutRef<typeof ToastPrimitives.Viewport>>(
  ({ className, ...props }, ref) => (
    <ToastPrimitives.Viewport
      ref={ref}
      className={cn('fixed top-0 right-0 z-[100] flex max-h-screen w-full flex-col-reverse p-4 sm:bottom-auto sm:left-auto sm:top-4 sm:right-4', className)}
      {...props}
    />
  ),
)
ToastViewport.displayName = ToastPrimitives.Viewport.displayName

const Toast = React.forwardRef<React.ElementRef<typeof ToastPrimitives.Root>, React.ComponentPropsWithoutRef<typeof ToastPrimitives.Root>>(
  ({ className, ...props }, ref) => (
    <ToastPrimitives.Root
      ref={ref}
      className={cn(
        'pointer-events-auto relative flex w-full max-w-sm items-center justify-between space-x-4 overflow-hidden rounded-md border bg-background p-4 text-foreground shadow-lg',
        className,
      )}
      {...props}
    />
  ),
)
Toast.displayName = ToastPrimitives.Root.displayName

const ToastTitle = React.forwardRef<React.ElementRef<typeof ToastPrimitives.Title>, React.ComponentPropsWithoutRef<typeof ToastPrimitives.Title>>(
  ({ className, ...props }, ref) => (
    <ToastPrimitives.Title ref={ref} className={cn('text-sm font-semibold', className)} {...props} />
  ),
)
ToastTitle.displayName = ToastPrimitives.Title.displayName

const ToastDescription = React.forwardRef<React.ElementRef<typeof ToastPrimitives.Description>, React.ComponentPropsWithoutRef<typeof ToastPrimitives.Description>>(
  ({ className, ...props }, ref) => (
    <ToastPrimitives.Description ref={ref} className={cn('text-sm text-muted-foreground', className)} {...props} />
  ),
)
ToastDescription.displayName = ToastPrimitives.Description.displayName

const ToastClose = React.forwardRef<React.ElementRef<typeof ToastPrimitives.Close>, React.ComponentPropsWithoutRef<typeof ToastPrimitives.Close>>(
  ({ className, ...props }, ref) => (
    <ToastPrimitives.Close ref={ref} className={cn('absolute right-2 top-2 rounded-md p-1 text-muted-foreground hover:text-foreground', className)} toast-close="" {...props}>
      <X className="h-4 w-4" />
    </ToastPrimitives.Close>
  ),
)
ToastClose.displayName = ToastPrimitives.Close.displayName

const Toaster = () => {
  const { toasts } = useToast()

  return (
    <ToastProvider>
      {toasts.map(({ id, title, description, action, ...props }) => (
        <Toast key={id} {...props}>
          <div className="grid gap-1">
            {title && <ToastTitle>{title}</ToastTitle>}
            {description && <ToastDescription>{description}</ToastDescription>}
          </div>
          {action}
          <ToastClose />
        </Toast>
      ))}
      <ToastViewport />
    </ToastProvider>
  )
}

export { Toaster, ToastProvider, ToastViewport, Toast, ToastTitle, ToastDescription, ToastClose }
