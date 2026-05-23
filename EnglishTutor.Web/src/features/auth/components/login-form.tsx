"use client";

import Link from "next/link";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { Loader2, Mail } from "lucide-react";
import { Button } from "@/shared/components/ui/button";
import { Input } from "@/shared/components/ui/input";
import { FormField } from "@/shared/components/form-field";
import { PasswordInput } from "@/shared/components/password-input";
import { ApiError } from "@/shared/api";
import { loginSchema, type LoginFormValues } from "../schemas/login-schema";
import { useLogin } from "../hooks/use-login";

export function LoginForm() {
  const login = useLogin();

  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<LoginFormValues>({
    resolver: zodResolver(loginSchema),
    defaultValues: { email: "", password: "" },
  });

  const serverError =
    login.error instanceof ApiError ? login.error.message : login.error ? "Something went wrong" : null;

  return (
    <form onSubmit={handleSubmit((data) => login.mutate(data))} className="grid gap-4">
      {serverError && (
        <div className="rounded-lg border border-destructive/50 bg-destructive/10 px-3 py-2.5 text-sm text-destructive">
          {serverError}
        </div>
      )}

      <FormField label="Email" htmlFor="email" error={errors.email?.message}>
        <div className="relative">
          <Input
            id="email"
            type="email"
            placeholder="you@example.com"
            autoComplete="email"
            className="pr-10"
            aria-invalid={!!errors.email}
            {...register("email")}
          />
          <Mail className="pointer-events-none absolute right-3 top-1/2 size-4 -translate-y-1/2 text-muted-foreground/50" />
        </div>
      </FormField>

      <FormField
        label="Password"
        htmlFor="password"
        error={errors.password?.message}
        labelExtra={
          <Link
            href="/login"
            tabIndex={-1}
            className="text-xs text-muted-foreground hover:text-primary"
          >
            Forgot password?
          </Link>
        }
      >
        <PasswordInput
          id="password"
          placeholder="Enter your password"
          autoComplete="current-password"
          aria-invalid={!!errors.password}
          {...register("password")}
        />
      </FormField>

      <Button type="submit" disabled={login.isPending} className="mt-1 w-full" size="lg">
        {login.isPending && <Loader2 className="animate-spin" />}
        Sign in
      </Button>

      <p className="text-center text-sm text-muted-foreground lg:hidden">
        Don&apos;t have an account?{" "}
        <Link href="/register" className="font-medium text-primary underline-offset-4 hover:underline">
          Sign up
        </Link>
      </p>
    </form>
  );
}
