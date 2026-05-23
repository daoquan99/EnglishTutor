import Link from "next/link";
import {
  ArrowRight,
  BookOpen,
  Brain,
  GraduationCap,
  MessageSquare,
  Mic,
  Sparkles,
  Target,
  TrendingUp,
  Zap,
} from "lucide-react";

const features = [
  {
    icon: Mic,
    title: "Speaking Practice",
    desc: "Practice real conversations with AI feedback on grammar, pronunciation, and fluency.",
    color: "bg-speaking/10 text-speaking",
  },
  {
    icon: BookOpen,
    title: "Interactive Exercises",
    desc: "Build skills with exercises tailored to your level — grammar drills, fill-in-the-blanks, and more.",
    color: "bg-exercise/10 text-exercise",
  },
  {
    icon: Brain,
    title: "Smart Mistake Tracking",
    desc: "Learn from your errors with personalized review and intelligent spaced repetition.",
    color: "bg-assessment/10 text-assessment",
  },
  {
    icon: TrendingUp,
    title: "Progress Dashboard",
    desc: "Track your journey with detailed stats, daily streaks, and skill breakdowns.",
    color: "bg-success/10 text-success",
  },
  {
    icon: Target,
    title: "Vocabulary Builder",
    desc: "Expand your word bank with contextual learning, flashcards, and pronunciation practice.",
    color: "bg-vocabulary/10 text-vocabulary",
  },
  {
    icon: GraduationCap,
    title: "Level Assessments",
    desc: "Test your skills with comprehensive assessments and get placed at the right CEFR level.",
    color: "bg-info/10 text-info",
  },
];

const stats = [
  { value: "A1–C2", label: "CEFR Levels" },
  { value: "10K+", label: "Vocabulary Items" },
  { value: "50+", label: "Exercise Types" },
  { value: "24/7", label: "AI Availability" },
];

export default function LandingPage() {
  return (
    <div>
      {/* Hero */}
      <section className="relative overflow-hidden">
        <div className="pointer-events-none absolute inset-0 -z-10 bg-[radial-gradient(ellipse_80%_50%_at_50%_-20%,color-mix(in_oklch,var(--primary)_12%,transparent),transparent)]" />

        <div className="page-container py-20 md:py-28 lg:py-32">
          <div className="mx-auto max-w-3xl text-center">
            <div className="mb-6 inline-flex items-center gap-1.5 rounded-full border bg-muted/50 px-3 py-1 text-xs font-medium text-muted-foreground">
              <Sparkles className="size-3 text-primary" />
              Powered by advanced AI
            </div>
            <h1 className="text-balance font-heading text-4xl font-bold tracking-tight sm:text-5xl lg:text-6xl">
              Master English with{" "}
              <span className="bg-gradient-to-r from-primary to-orange-500 bg-clip-text text-transparent">
                intelligent learning
              </span>
            </h1>
            <p className="mx-auto mt-6 max-w-xl text-pretty text-base text-muted-foreground sm:text-lg">
              An AI-powered language platform that adapts to your level, tracks your progress, and
              helps you speak with confidence — anytime, anywhere.
            </p>
            <div className="mt-8 flex flex-col items-center justify-center gap-3 sm:flex-row">
              <Link
                href="/register"
                className="inline-flex h-10 items-center gap-2 rounded-full bg-primary px-6 font-medium text-primary-foreground shadow-soft transition-colors hover:bg-primary/90"
              >
                Start learning free
                <ArrowRight className="size-4" />
              </Link>
              <Link
                href="/login"
                className="inline-flex h-10 items-center gap-2 rounded-full border px-6 font-medium transition-colors hover:bg-muted"
              >
                Sign in
              </Link>
            </div>
          </div>
        </div>
      </section>

      {/* Stats */}
      <section className="border-y bg-muted/30">
        <div className="page-container grid grid-cols-2 gap-6 py-10 sm:grid-cols-4 sm:gap-8">
          {stats.map((s) => (
            <div key={s.label} className="text-center">
              <div className="text-2xl font-bold tracking-tight sm:text-3xl">{s.value}</div>
              <div className="mt-1 text-sm text-muted-foreground">{s.label}</div>
            </div>
          ))}
        </div>
      </section>

      {/* Features */}
      <section className="py-16 md:py-24">
        <div className="page-container">
          <div className="mx-auto max-w-2xl text-center">
            <div className="mb-3 inline-flex items-center gap-1.5 rounded-full border bg-muted/50 px-3 py-1 text-xs font-medium text-muted-foreground">
              <Zap className="size-3 text-primary" />
              Features
            </div>
            <h2 className="font-heading text-3xl font-bold tracking-tight sm:text-4xl">
              Everything you need to learn English
            </h2>
            <p className="mt-3 text-muted-foreground">
              From vocabulary to speaking, our platform covers all aspects of English learning with
              AI-powered personalization.
            </p>
          </div>

          <div className="mt-12 grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
            {features.map((f) => (
              <div
                key={f.title}
                className="group rounded-xl border bg-card/50 p-5 transition-colors hover:bg-card hover:shadow-soft"
              >
                <div className={`flex h-10 w-10 items-center justify-center rounded-lg ${f.color}`}>
                  <f.icon className="size-5" />
                </div>
                <h3 className="mt-4 font-semibold">{f.title}</h3>
                <p className="mt-1.5 text-sm leading-relaxed text-muted-foreground">{f.desc}</p>
              </div>
            ))}
          </div>
        </div>
      </section>

      {/* How it works */}
      <section className="border-t bg-muted/30 py-16 md:py-24">
        <div className="page-container">
          <div className="mx-auto max-w-2xl text-center">
            <h2 className="font-heading text-3xl font-bold tracking-tight sm:text-4xl">How it works</h2>
            <p className="mt-3 text-muted-foreground">
              Get started in minutes with a simple three-step process.
            </p>
          </div>

          <div className="mx-auto mt-12 grid max-w-3xl gap-8 sm:grid-cols-3">
            {[
              {
                step: "1",
                title: "Create account",
                desc: "Sign up for free and tell us about your learning goals and current level.",
              },
              {
                step: "2",
                title: "Take assessment",
                desc: "Our AI evaluates your skills to create a personalized study plan.",
              },
              {
                step: "3",
                title: "Start learning",
                desc: "Practice speaking, build vocabulary, and track your progress daily.",
              },
            ].map((item) => (
              <div key={item.step} className="text-center">
                <div className="mx-auto flex h-10 w-10 items-center justify-center rounded-full bg-primary text-sm font-bold text-primary-foreground">
                  {item.step}
                </div>
                <h3 className="mt-4 font-semibold">{item.title}</h3>
                <p className="mt-1.5 text-sm text-muted-foreground">{item.desc}</p>
              </div>
            ))}
          </div>
        </div>
      </section>

      {/* CTA */}
      <section className="py-16 md:py-24">
        <div className="page-container">
          <div className="mx-auto max-w-2xl rounded-2xl bg-gradient-brand p-8 text-center text-white shadow-card sm:p-12">
            <MessageSquare className="mx-auto size-10 opacity-80" />
            <h2 className="font-heading mt-4 text-2xl font-bold tracking-tight sm:text-3xl">
              Ready to start your journey?
            </h2>
            <p className="mx-auto mt-3 max-w-md text-sm text-white/75">
              Join thousands of learners already improving their English with AI-powered practice and
              personalized feedback.
            </p>
            <Link
              href="/register"
              className="mt-6 inline-flex h-10 items-center gap-2 rounded-full bg-white px-6 font-medium text-foreground shadow-soft transition-colors hover:bg-white/90"
            >
              Get started for free
              <ArrowRight className="size-4" />
            </Link>
          </div>
        </div>
      </section>

      {/* Footer */}
      <footer className="border-t py-8">
        <div className="page-container flex flex-col items-center justify-between gap-4 sm:flex-row">
          <div className="flex items-center gap-2">
            <div className="flex h-6 w-6 items-center justify-center rounded-md bg-primary text-[10px] font-bold text-primary-foreground">
              ET
            </div>
            <span className="text-sm font-medium">EnglishTutor</span>
          </div>
          <p className="text-sm text-muted-foreground">
            &copy; {new Date().getFullYear()} EnglishTutor. All rights reserved.
          </p>
        </div>
      </footer>
    </div>
  );
}
