import * as React from 'react'
import { type ToastProps } from '@radix-ui/react-toast'

const TOAST_LIMIT = 1
const TOAST_REMOVE_DELAY = 1000

const actionTypes = {
  ADD_TOAST: 'ADD_TOAST',
  UPDATE_TOAST: 'UPDATE_TOAST',
  DISMISS_TOAST: 'DISMISS_TOAST',
  REMOVE_TOAST: 'REMOVE_TOAST'
} as const

type ActionType = typeof actionTypes

type Toast = ToastProps & {
  id: string
  title?: React.ReactNode
  description?: React.ReactNode
  action?: React.ReactNode
}

type State = {
  toasts: Toast[]
}

const toastTimeouts = new Map<string, ReturnType<typeof setTimeout>>()

const addToRemoveQueue = (toastId: string) => {
  if (toastTimeouts.has(toastId)) {
    return
  }

  const timeout = setTimeout(() => {
    toastTimeouts.delete(toastId)
    dispatch({ type: actionTypes.REMOVE_TOAST, toastId })
  }, TOAST_REMOVE_DELAY)

  toastTimeouts.set(toastId, timeout)
}

const reducer = (state: State, action: { type: ActionType[keyof ActionType]; toast?: Toast; toastId?: string }) => {
  switch (action.type) {
    case actionTypes.ADD_TOAST:
      return {
        ...state,
        toasts: [action.toast!, ...state.toasts].slice(0, TOAST_LIMIT)
      }
    case actionTypes.UPDATE_TOAST:
      return {
        ...state,
        toasts: state.toasts.map((toast) => (toast.id === action.toast?.id ? { ...toast, ...action.toast } : toast))
      }
    case actionTypes.DISMISS_TOAST: {
      const { toastId } = action

      if (toastId) {
        addToRemoveQueue(toastId)
      } else {
        state.toasts.forEach((toast) => {
          addToRemoveQueue(toast.id)
        })
      }

      return {
        ...state,
        toasts: state.toasts.map((toast) => (toast.id === toastId || toastId === undefined ? { ...toast, open: false } : toast))
      }
    }
    case actionTypes.REMOVE_TOAST:
      return {
        ...state,
        toasts: state.toasts.filter((toast) => toast.id !== action.toastId)
      }
  }
}

const listeners: Array<(state: State) => void> = []

let memoryState: State = { toasts: [] }

const dispatch = (action: { type: ActionType[keyof ActionType]; toast?: Toast; toastId?: string }) => {
  memoryState = reducer(memoryState, action)
  listeners.forEach((listener) => {
    listener(memoryState)
  })
}

function toast({ ...props }: Omit<Toast, 'id'>) {
  const id = Math.random().toString(36).slice(2)

  const update = (props: Toast) =>
    dispatch({
      type: actionTypes.UPDATE_TOAST,
      toast: { ...props, id }
    })
  const dismiss = () => dispatch({ type: actionTypes.DISMISS_TOAST, toastId: id })

  dispatch({
    type: actionTypes.ADD_TOAST,
    toast: {
      ...props,
      id
    }
  })

  return {
    id,
    dismiss,
    update
  }
}

function useToast() {
  const [state, setState] = React.useState<State>(memoryState)

  React.useEffect(() => {
    listeners.push(setState)
    return () => {
      const index = listeners.indexOf(setState)
      if (index > -1) {
        listeners.splice(index, 1)
      }
    }
  }, [])

  return {
    ...state,
    toast,
    dismiss: (toastId?: string) => dispatch({ type: actionTypes.DISMISS_TOAST, toastId })
  }
}

export { useToast, toast }
