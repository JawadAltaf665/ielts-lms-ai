import type { Metadata, Viewport } from "next";
import { Geist, Geist_Mono } from "next/font/google";
import { Nav } from "@/components/layout/Nav";
import { Footer } from "@/components/layout/Footer";
import "./globals.css";

const geistSans = Geist({
  variable: "--font-geist-sans",
  subsets: ["latin"],
});

const geistMono = Geist_Mono({
  variable: "--font-geist-mono",
  subsets: ["latin"],
});

const siteUrl = "https://ieltslms.ai";

export const metadata: Metadata = {
  metadataBase: new URL(siteUrl),
  title: {
    default: "IELTS LMS AI — AI-Powered IELTS Preparation Platform",
    template: "%s — IELTS LMS AI",
  },
  description:
    "The AI-native IELTS preparation platform. AI Tutor, AI Writing & Speaking evaluation, band prediction, and a full computer-based IELTS simulator — for individual learners and institutions alike.",
  keywords: [
    "IELTS preparation",
    "AI IELTS",
    "IELTS writing evaluation",
    "IELTS speaking evaluation",
    "computer based IELTS",
    "band score predictor",
  ],
  openGraph: {
    type: "website",
    url: siteUrl,
    siteName: "IELTS LMS AI",
    title: "IELTS LMS AI — AI-Powered IELTS Preparation Platform",
    description:
      "AI Tutor, AI-evaluated Writing & Speaking, band prediction, and a full computer-based IELTS simulator.",
  },
  twitter: {
    card: "summary_large_image",
    title: "IELTS LMS AI — AI-Powered IELTS Preparation Platform",
    description:
      "AI Tutor, AI-evaluated Writing & Speaking, band prediction, and a full computer-based IELTS simulator.",
  },
  robots: {
    index: true,
    follow: true,
  },
};

export const viewport: Viewport = {
  themeColor: "#05060a",
  colorScheme: "dark",
};

export default function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <html
      lang="en"
      className={`${geistSans.variable} ${geistMono.variable} h-full antialiased`}
    >
      <body className="min-h-full flex flex-col bg-background text-foreground">
        <Nav />
        <main className="flex-1">{children}</main>
        <Footer />
      </body>
    </html>
  );
}
