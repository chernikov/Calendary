import * as React from 'react'
import { Slot } from '@radix-ui/react-slot'
import { Controller, type ControllerProps, type FieldPath, type FieldValues, FormProvider, useFormContext } from 'react-hook-form'

import { cn } from '@/lib/utils'
import { Label } from '@/components/ui/label'

const Form = FormProvider

type FormFieldContextValue = {
  name: string
}

const FormFieldContext = React.createContext<FormFieldContextValue | undefined>(undefined)

const FormField = <TFieldValues extends FieldValues = FieldValues, TName extends FieldPath<TFieldValues> = FieldPath<TFieldValues>>(
  props: ControllerProps<TFieldValues, TName>,
) => {
  return (
    <FormFieldContext.Provider value={{ name: props.name as string }}>
      <Controller {...props} />
    </FormFieldContext.Provider>
  )
}

const useFormField = () => {
  const fieldContext = React.useContext(FormFieldContext)
  const itemContext = React.useContext(FormItemContext)
  const { getFieldState, formState } = useFormContext()

  if (!fieldContext || !itemContext) {
    throw new Error('Form components must be used within <FormField> and <FormItem>')
  }

  const fieldState = getFieldState(fieldContext.name, formState)

  return {
    name: fieldContext.name,
    formItemId: itemContext.id,
    ...fieldState
  }
}

type FormItemContextValue = {
  id: string
}

const FormItemContext = React.createContext<FormItemContextValue | undefined>(undefined)

const FormItem = React.forwardRef<HTMLDivElement, React.HTMLAttributes<HTMLDivElement>>(({ className, ...props }, ref) => {
  const id = React.useId()

  return (
    <FormItemContext.Provider value={{ id }}>
      <div ref={ref} className={cn('space-y-2', className)} {...props} />
    </FormItemContext.Provider>
  )
})
FormItem.displayName = 'FormItem'

const FormLabel = React.forwardRef<React.ElementRef<typeof Label>, React.ComponentPropsWithoutRef<typeof Label>>(
  ({ className, ...props }, ref) => {
    const { formItemId } = useFormField()
    return <Label ref={ref} className={cn('text-sm font-medium leading-none', className)} htmlFor={formItemId} {...props} />
  },
)
FormLabel.displayName = 'FormLabel'

const FormControl = React.forwardRef<React.ElementRef<typeof Slot>, React.ComponentPropsWithoutRef<typeof Slot>>((props, ref) => {
  const { formItemId } = useFormField()
  return <Slot ref={ref} id={formItemId} aria-describedby={formItemId ? `${formItemId}-description` : undefined} {...props} />
})
FormControl.displayName = 'FormControl'

const FormDescription = React.forwardRef<HTMLParagraphElement, React.HTMLAttributes<HTMLParagraphElement>>(
  ({ className, ...props }, ref) => {
    const { formItemId } = useFormField()
    return <p ref={ref} id={formItemId ? `${formItemId}-description` : undefined} className={cn('text-sm text-muted-foreground', className)} {...props} />
  },
)
FormDescription.displayName = 'FormDescription'

const FormMessage = React.forwardRef<HTMLParagraphElement, React.HTMLAttributes<HTMLParagraphElement>>(({ className, children, ...props }, ref) => {
  const { formItemId, error } = useFormField()
  const body = error ? String(error.message ?? 'Поле обов\'язкове') : children

  if (!body) {
    return null
  }

  return (
    <p ref={ref} id={formItemId ? `${formItemId}-message` : undefined} className={cn('text-sm font-medium text-destructive', className)} {...props}>
      {body}
    </p>
  )
})
FormMessage.displayName = 'FormMessage'

export { Form, FormField, FormItem, FormLabel, FormControl, FormDescription, FormMessage }
