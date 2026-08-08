import { PASSWORD_MIN_LENGTH } from '../auth.constants'
import { CheckIcon } from './icons'

const rules = [
  { test: (pw: string) => pw.length >= PASSWORD_MIN_LENGTH, label: `At least ${PASSWORD_MIN_LENGTH} characters` },
  {
    test: (pw: string) => /[A-Z]/.test(pw) && /[a-z]/.test(pw) && /[0-9]/.test(pw) && /[^A-Za-z0-9]/.test(pw),
    label: 'Uppercase, lowercase, a digit and a symbol',
  },
]

/** Live-updating checklist mirroring ConfigurablePasswordPolicy, matching the design's `.pw-reqs`. */
export function PasswordRequirements({ password }: { password: string }) {
  return (
    <ul className="pw-reqs">
      {rules.map((rule) => {
        const met = rule.test(password)
        return (
          <li key={rule.label} className={met ? 'met' : undefined}>
            <CheckIcon />
            {rule.label}
          </li>
        )
      })}
    </ul>
  )
}
