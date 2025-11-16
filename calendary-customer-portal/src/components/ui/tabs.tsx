import * as TabsPrimitive from '@radix-ui/react-tabs'

import { cn } from '@/lib/utils'

const Tabs = TabsPrimitive.Root

const TabsList = TabsPrimitive.List

const TabsTrigger = TabsPrimitive.Trigger

const TabsContent = TabsPrimitive.Content

const StyledTabsList = ({ className, ...props }: React.ComponentPropsWithoutRef<typeof TabsPrimitive.List>) => (
  <TabsList className={cn('inline-flex h-10 items-center justify-center rounded-md bg-muted p-1 text-muted-foreground', className)} {...props} />
)

const StyledTabsTrigger = ({ className, ...props }: React.ComponentPropsWithoutRef<typeof TabsPrimitive.Trigger>) => (
  <TabsTrigger
    className={cn(
      'inline-flex min-w-[100px] items-center justify-center whitespace-nowrap rounded-md px-3 py-1.5 text-sm font-medium ring-offset-background transition-all focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 disabled:pointer-events-none disabled:opacity-50 data-[state=active]:bg-background data-[state=active]:text-foreground data-[state=active]:shadow',
      className,
    )}
    {...props}
  />
)

const StyledTabsContent = ({ className, ...props }: React.ComponentPropsWithoutRef<typeof TabsPrimitive.Content>) => (
  <TabsContent className={cn('mt-2 ring-offset-background focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2', className)} {...props} />
)

export { Tabs, StyledTabsList as TabsList, StyledTabsTrigger as TabsTrigger, StyledTabsContent as TabsContent }
