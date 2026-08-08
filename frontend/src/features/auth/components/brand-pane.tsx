import type { ReactNode } from 'react'

interface BrandPaneProps {
  heading: string
  subtitle: string
  features: { icon: ReactNode; text: string }[]
  footer: string
}

/** Left-hand marketing panel on the split-screen login/register design; hidden below 880px. */
export function BrandPane({ heading, subtitle, features, footer }: BrandPaneProps) {
  return (
    <div className="brandpane">
      <div className="brandmark">IELTS LMS AI</div>
      <div>
        <h1 className="brandhead">{heading}</h1>
        <p className="brandsub">{subtitle}</p>
        <ul className="brandfeats">
          {features.map((f) => (
            <li className="brandfeat" key={f.text}>
              {f.icon}
              {f.text}
            </li>
          ))}
        </ul>
      </div>
      <div className="brandfoot">{footer}</div>
    </div>
  )
}
