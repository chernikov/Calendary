import { create } from 'zustand'

type CartItem = {
  id: string
  title: string
  price: number
  quantity: number
}

type CartState = {
  items: CartItem[]
  addItem: (item: CartItem) => void
  removeItem: (id: string) => void
  clear: () => void
}

export const useCartStore = create<CartState>((set) => ({
  items: [],
  addItem: (item) =>
    set((state) => {
      const existing = state.items.find((cartItem) => cartItem.id === item.id)
      if (existing) {
        return {
          items: state.items.map((cartItem) =>
            cartItem.id === item.id
              ? { ...cartItem, quantity: cartItem.quantity + item.quantity }
              : cartItem,
          )
        }
      }
      return { items: [...state.items, item] }
    }),
  removeItem: (id) => set((state) => ({ items: state.items.filter((item) => item.id !== id) })),
  clear: () => set({ items: [] })
}))
