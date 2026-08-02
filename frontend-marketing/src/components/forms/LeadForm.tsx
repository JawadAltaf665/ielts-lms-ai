"use client";

import { useState, type FormEvent } from "react";
import { Button } from "@/components/ui/Button";
import { CheckCircle2 } from "lucide-react";

type Status = "idle" | "submitting" | "success" | "error";

export function LeadForm({ variant = "contact" }: { variant?: "contact" | "demo" }) {
  const [status, setStatus] = useState<Status>("idle");

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setStatus("submitting");

    const form = event.currentTarget;
    const data = Object.fromEntries(new FormData(form).entries());

    try {
      const res = await fetch("/api/contact", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(data),
      });
      if (!res.ok) throw new Error("submission failed");
      setStatus("success");
      form.reset();
    } catch {
      setStatus("error");
    }
  }

  if (status === "success") {
    return (
      <div className="glass-panel flex flex-col items-center gap-3 rounded-lg p-10 text-center">
        <CheckCircle2 className="text-accent-cyan" size={32} />
        <h3 className="text-lg font-medium">Thanks — we&apos;ll be in touch</h3>
        <p className="text-sm text-muted">
          {variant === "demo"
            ? "We typically respond within one business day to schedule your demo."
            : "We typically respond within one business day."}
        </p>
      </div>
    );
  }

  return (
    <form onSubmit={handleSubmit} className="glass-panel space-y-5 rounded-lg p-8">
      <div className="grid gap-5 sm:grid-cols-2">
        <div>
          <label htmlFor="name" className="text-sm font-medium">
            Name
          </label>
          <input
            id="name"
            name="name"
            type="text"
            required
            className="mt-2 w-full rounded-md border border-white/[0.08] bg-white/[0.03] px-3.5 py-2.5 text-sm outline-none transition-colors focus:border-accent-violet"
          />
        </div>
        <div>
          <label htmlFor="email" className="text-sm font-medium">
            Email
          </label>
          <input
            id="email"
            name="email"
            type="email"
            required
            className="mt-2 w-full rounded-md border border-white/[0.08] bg-white/[0.03] px-3.5 py-2.5 text-sm outline-none transition-colors focus:border-accent-violet"
          />
        </div>
      </div>

      <div>
        <label htmlFor="organization" className="text-sm font-medium">
          Organization <span className="text-muted">(optional)</span>
        </label>
        <input
          id="organization"
          name="organization"
          type="text"
          className="mt-2 w-full rounded-md border border-white/[0.08] bg-white/[0.03] px-3.5 py-2.5 text-sm outline-none transition-colors focus:border-accent-violet"
        />
      </div>

      {variant === "demo" && (
        <div>
          <label htmlFor="interest" className="text-sm font-medium">
            I&apos;m interested as a...
          </label>
          <select
            id="interest"
            name="interest"
            defaultValue="Institute"
            className="mt-2 w-full rounded-md border border-white/[0.08] bg-white/[0.03] px-3.5 py-2.5 text-sm outline-none transition-colors focus:border-accent-violet"
          >
            <option>Individual learner</option>
            <option>Teacher</option>
            <option>Institute</option>
            <option>Enterprise</option>
          </select>
        </div>
      )}

      <div>
        <label htmlFor="message" className="text-sm font-medium">
          {variant === "demo" ? "Anything specific you'd like us to cover?" : "Message"}
        </label>
        <textarea
          id="message"
          name="message"
          required
          rows={4}
          className="mt-2 w-full rounded-md border border-white/[0.08] bg-white/[0.03] px-3.5 py-2.5 text-sm outline-none transition-colors focus:border-accent-violet"
        />
      </div>

      {status === "error" && (
        <p className="text-sm text-red-400">Something went wrong. Please try again.</p>
      )}

      <Button type="submit" size="lg" className="w-full" disabled={status === "submitting"}>
        {status === "submitting" ? "Sending..." : variant === "demo" ? "Book a demo" : "Send message"}
      </Button>
    </form>
  );
}
