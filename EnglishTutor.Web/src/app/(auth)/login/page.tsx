"use client";

import { LoginForm } from "@/features/auth/components/login-form";

export default function LoginPage() {
  return (
    <div className="space-y-6">
      <div className="space-y-1.5">
        <h1 className="font-heading text-2xl font-bold tracking-tight">Welcome back</h1>
        <p className="text-sm text-muted-foreground">
          Enter your credentials to continue learning
        </p>
      </div>
      <LoginForm />
    </div>
  );
}
