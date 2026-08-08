import { forwardRef, type InputHTMLAttributes, useId, useState } from 'react'

import { EyeIcon, EyeOffIcon, LockIcon } from './icons'

interface PasswordFieldProps extends InputHTMLAttributes<HTMLInputElement> {
  label: string
  error?: string
}

/** Password input with the show/hide toggle from the design (`.field-input .trail`). */
export const PasswordField = forwardRef<HTMLInputElement, PasswordFieldProps>(function PasswordField(
  { label, error, id: providedId, ...inputProps },
  ref,
) {
  const [visible, setVisible] = useState(false)
  const generatedId = useId()
  const id = providedId ?? generatedId

  return (
    <div className="field">
      <label htmlFor={id}>{label}</label>
      <div className="field-input">
        <LockIcon />
        <input
          ref={ref}
          id={id}
          type={visible ? 'text' : 'password'}
          className="input"
          aria-invalid={error ? 'true' : undefined}
          aria-describedby={error ? `${id}-error` : undefined}
          {...inputProps}
        />
        <button
          type="button"
          className="trail"
          onClick={() => setVisible((v) => !v)}
          aria-label={visible ? 'Hide password' : 'Show password'}
        >
          {visible ? <EyeOffIcon /> : <EyeIcon />}
        </button>
      </div>
      {error ? (
        <p className="field-error" id={`${id}-error`} role="alert">
          {error}
        </p>
      ) : null}
    </div>
  )
})
