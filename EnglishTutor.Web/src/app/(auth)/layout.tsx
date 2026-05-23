"use client";

import Link from "next/link";
import { usePathname } from "next/navigation";
import { BookOpen, BrainCircuit, GraduationCap, Mic, Sparkles } from "lucide-react";
import { ThemeToggle } from "@/shared/components/theme-toggle";

const features = [
  { icon: BookOpen, label: "Smart vocabulary building with spaced repetition" },
  { icon: BrainCircuit, label: "AI-powered exercises tailored to your level" },
  { icon: Mic, label: "Real-time speaking practice with feedback" },
  { icon: GraduationCap, label: "Track progress and level up with assessments" },
];

export default function AuthLayout({ children }: { children: React.ReactNode }) {
  const pathname = usePathname();
  const isLogin = pathname === "/login";

  return (
    <div className="flex min-h-dvh">
      {/* Left branding panel — hidden on mobile */}
      <div className="relative hidden w-[45%] max-w-[560px] flex-col justify-between overflow-hidden bg-gradient-brand p-8 text-white lg:flex xl:w-[42%]">
        {/* Decorative circles */}
        <div className="pointer-events-none absolute -left-20 -top-20 h-72 w-72 rounded-full bg-white/[0.07]" />
        <div className="pointer-events-none absolute -bottom-16 -right-16 h-56 w-56 rounded-full bg-white/[0.05]" />
        <div className="pointer-events-none absolute right-12 top-1/3 h-32 w-32 rounded-full bg-white/[0.04]" />

        {/* Logo */}
        <div className="relative">
          <Link href="/" className="flex items-center gap-2.5">
            <div className="flex h-9 w-9 items-center justify-center rounded-xl bg-white/20 text-sm font-bold backdrop-blur-sm">
              ET
            </div>
            <span className="text-lg font-semibold tracking-tight">EnglishTutor</span>
          </Link>
        </div>

        {/* Center content */}
        <div className="relative space-y-8">
          <div className="space-y-3">
            <div className="inline-flex items-center gap-1.5 rounded-full bg-white/15 px-3 py-1 text-xs font-medium backdrop-blur-sm">
              <Sparkles className="size-3" />
              AI-Powered Learning
            </div>
            <h2 className="font-heading text-3xl font-bold leading-tight tracking-tight xl:text-4xl">
              Master English
              <br />
              with confidence
            </h2>
            <p className="max-w-sm text-sm leading-relaxed text-white/75">
              Personalized lessons, real-time feedback, and intelligent practice designed to accelerate your fluency.
            </p>
          </div>

          <div className="space-y-3">
            {features.map(({ icon: Icon, label }) => (
              <div key={label} className="flex items-start gap-3">
                <div className="mt-0.5 flex h-7 w-7 shrink-0 items-center justify-center rounded-lg bg-white/15 backdrop-blur-sm">
                  <Icon className="size-3.5" />
                </div>
                <span className="text-sm leading-relaxed text-white/85">{label}</span>
              </div>
            ))}
          </div>
        </div>

        {/* Bottom */}
        <p className="relative text-xs text-white/40">
          &copy; {new Date().getFullYear()} EnglishTutor. All rights reserved.
        </p>
      </div>

      {/* Right form panel */}
      <div className="flex flex-1 flex-col">
        {/* Top bar */}
        <div className="flex items-center justify-between px-4 py-3 sm:px-8">
          <Link href="/" className="flex items-center gap-2 lg:invisible">
            <div className="flex h-7 w-7 items-center justify-center rounded-lg bg-primary text-xs font-bold text-primary-foreground">
              ET
            </div>
            <span className="text-sm font-semibold">EnglishTutor</span>
          </Link>
          <div className="flex items-center gap-3">
            <ThemeToggle />
            <span className="text-sm text-muted-foreground">
              {isLogin ? "Don’t have an account?" : "Already have an account?"}{" "}
              <Link
                href={isLogin ? "/register" : "/login"}
                className="font-medium text-primary underline-offset-4 hover:underline"
              >
                {isLogin ? "Sign up" : "Sign in"}
              </Link>
            </span>
          </div>
        </div>

        {/* Form area */}
        <div className="flex flex-1 items-center justify-center px-4 pb-8 sm:px-8">
          <div className="w-full max-w-[420px]">{children}</div>
        </div>
      </div>
    </div>
  );
}
