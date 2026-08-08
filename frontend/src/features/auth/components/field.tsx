import { forwardRef, type InputHTMLAttributes, type ReactNode } from 'react'

interface FieldProps extends InputHTMLAttributes<HTMLInputElement> {
  label: string
  icon: ReactNode
  error?: string
}

/** Labeled input with a leading icon, matching `.field`/`.field-input`/`.input` in auth.css. */
export const Field = forwardRef<HTMLInputElement, FieldProps>(function Field(
  { label, icon, error, id, ...inputProps },
  ref,
) {
  return (
    <div className="field">
      <label htmlFor={id}>{label}</label>
      <div className="field-input">
        {icon}
        <input
          ref={ref}
          id={id}
          className="input"
          aria-invalid={error ? 'true' : undefined}
          aria-describedby={error ? `${id}-error` : undefined}
          {...inputProps}
        />
      </div>
      {error ? (
        <p className="field-error" id={`${id}-error`} role="alert">
          {error}
        </p>
      ) : null}
    </div>
  )
})
