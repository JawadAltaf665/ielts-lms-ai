import Link from "next/link";

const columns = [
  {
    title: "Product",
    links: [
      { href: "/features", label: "Features" },
      { href: "/ai", label: "AI Capabilities" },
      { href: "/pricing", label: "Pricing" },
      { href: "/security", label: "Security" },
    ],
  },
  {
    title: "Solutions",
    links: [
      { href: "/solutions#students", label: "Students" },
      { href: "/solutions#teachers", label: "Teachers" },
      { href: "/solutions#institutes", label: "Institutes" },
      { href: "/solutions#enterprise", label: "Enterprise" },
    ],
  },
  {
    title: "Company",
    links: [
      { href: "/about", label: "About" },
      { href: "/blog", label: "Blog" },
      { href: "/careers", label: "Careers" },
      { href: "/contact", label: "Contact" },
    ],
  },
  {
    title: "Legal",
    links: [
      { href: "/privacy", label: "Privacy" },
      { href: "/terms", label: "Terms" },
    ],
  },
];

export function Footer() {
  return (
    <footer className="relative border-t border-white/[0.08] py-16">
      <div className="mx-auto max-w-7xl px-6 lg:px-8">
        <div className="grid grid-cols-2 gap-8 md:grid-cols-6">
          <div className="col-span-2">
            <Link href="/" className="flex items-center gap-2 text-sm font-semibold tracking-tight">
              <span className="inline-block h-6 w-6 rounded-md bg-gradient-to-br from-accent-indigo to-accent-cyan" />
              IELTS LMS AI
            </Link>
            <p className="mt-4 max-w-xs text-sm text-muted">
              The AI-native platform for IELTS preparation — for individual learners, institutes, and enterprises.
            </p>
          </div>

          {columns.map((col) => (
            <div key={col.title}>
              <h3 className="text-sm font-medium text-foreground">{col.title}</h3>
              <ul className="mt-4 space-y-3">
                {col.links.map((link) => (
                  <li key={link.href}>
                    <Link href={link.href} className="text-sm text-muted transition-colors hover:text-foreground">
                      {link.label}
                    </Link>
                  </li>
                ))}
              </ul>
            </div>
          ))}
        </div>

        <div className="mt-12 border-t border-white/[0.08] pt-8 text-xs text-muted">
          <p>&copy; {new Date().getFullYear()} IELTS LMS AI. All rights reserved.</p>
          <p className="mt-2 max-w-3xl">
            IELTS is a trademark of its respective owners. IELTS LMS AI is an independent preparation platform and
            does not claim official affiliation with or endorsement by the IELTS test owners. AI-generated band
            scores and feedback are estimates for practice purposes only and are not official IELTS results.
          </p>
        </div>
      </div>
    </footer>
  );
}
