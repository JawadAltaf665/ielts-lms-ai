import type { ReactNode } from 'react'

import { AlertCircleIcon } from './icons'

export function FormAlert({ children, icon }: { children: ReactNode; icon?: ReactNode }) {
  return (
    <div className="alert" role="alert">
      {icon ?? <AlertCircleIcon />}
      <span>{children}</span>
    </div>
  )
}
