"use client";

import Link from "next/link";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { Loader2, Mail, User } from "lucide-react";
import { useMemo } from "react";
import { Button } from "@/shared/components/ui/button";
import { Input } from "@/shared/components/ui/input";
import { FormField } from "@/shared/components/form-field";
import { PasswordInput } from "@/shared/components/password-input";
import { cn } from "@/shared/lib/utils";
import { ApiError } from "@/shared/api";
import { registerSchema, type RegisterFormValues } from "../schemas/register-schema";
import { useRegister } from "../hooks/use-register";

function getPasswordStrength(password: string): { score: number; label: string } {
  if (!password) return { score: 0, label: "" };
  let score = 0;
  if (password.length >= 8) score++;
  if (password.length >= 12) score++;
  if (/[a-z]/.test(password) && /[A-Z]/.test(password)) score++;
  if (/\d/.test(password)) score++;
  if (/[^a-zA-Z0-9]/.test(password)) score++;

  if (score <= 1) return { score: 1, label: "Weak" };
  if (score <= 2) return { score: 2, label: "Fair" };
  if (score <= 3) return { score: 3, label: "Good" };
  return { score: 4, label: "Strong" };
}

const strengthColors = [
  "",
  "bg-destructive",
  "bg-warning",
  "bg-info",
  "bg-success",
];

const strengthTextColors = [
  "",
  "text-destructive",
  "text-warning",
  "text-info",
  "text-success",
];

const formFields = new Set<string>(["email", "displayName", "password", "confirmPassword"]);

export function RegisterForm() {
  const registerMutation = useRegister();
  const {
    register,
    handleSubmit,
    setError,
    watch,
    formState: { errors },
  } = useForm<RegisterFormValues>({
    resolver: zodResolver(registerSchema),
    defaultValues: { email: "", displayName: "", password: "", confirmPassword: "" },
  });

  const passwordValue = watch("password");
  const strength = useMemo(() => getPasswordStrength(passwordValue), [passwordValue]);

  const onSubmit = (data: RegisterFormValues) => {
    registerMutation.mutate(data, {
      onError: (error) => {
        if (error instanceof ApiError && error.isValidation && error.details) {
          for (const [field, messages] of Object.entries(error.details)) {
            const key = field.charAt(0).toLowerCase() + field.slice(1);
            if (formFields.has(key)) {
              setError(key as keyof RegisterFormValues, { message: messages[0] });
            }
          }
        }
      },
    });
  };

  const serverError =
    registerMutation.error instanceof ApiError && !registerMutation.error.isValidation
      ? registerMutation.error.message
      : registerMutation.error && !(registerMutation.error instanceof ApiError)
        ? "An unexpected error occurred. Please try again."
        : null;

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="grid gap-4">
      {serverError && (
        <div className="rounded-lg border border-destructive/50 bg-destructive/10 px-3 py-2.5 text-sm text-destructive">
          {serverError}
        </div>
      )}

      <FormField label="Display name" htmlFor="displayName" error={errors.displayName?.message}>
        <div className="relative">
          <Input
            id="displayName"
            placeholder="Your name"
            autoComplete="name"
            className="pr-10"
            aria-invalid={!!errors.displayName}
            {...register("displayName")}
          />
          <User className="pointer-events-none absolute right-3 top-1/2 size-4 -translate-y-1/2 text-muted-foreground/50" />
        </div>
      </FormField>

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

      <FormField label="Password" htmlFor="password" error={errors.password?.message}>
        <PasswordInput
          id="password"
          placeholder="At least 8 characters"
          autoComplete="new-password"
          aria-invalid={!!errors.password}
          {...register("password")}
        />
        {passwordValue && (
          <div className="space-y-1">
            <div className="flex gap-1">
              {[1, 2, 3, 4].map((level) => (
                <div
                  key={level}
                  className={cn(
                    "h-1 flex-1 rounded-full transition-colors",
                    level <= strength.score ? strengthColors[strength.score] : "bg-muted",
                  )}
                />
              ))}
            </div>
            <p className={cn("text-xs", strengthTextColors[strength.score])}>
              {strength.label}
            </p>
          </div>
        )}
      </FormField>

      <FormField label="Confirm password" htmlFor="confirmPassword" error={errors.confirmPassword?.message}>
        <PasswordInput
          id="confirmPassword"
          placeholder="Repeat your password"
          autoComplete="new-password"
          aria-invalid={!!errors.confirmPassword}
          {...register("confirmPassword")}
        />
      </FormField>

      <Button type="submit" disabled={registerMutation.isPending} className="mt-1 w-full" size="lg">
        {registerMutation.isPending && <Loader2 className="animate-spin" />}
        Create account
      </Button>

      <p className="text-center text-sm text-muted-foreground lg:hidden">
        Already have an account?{" "}
        <Link href="/login" className="font-medium text-primary underline-offset-4 hover:underline">
          Sign in
        </Link>
      </p>
    </form>
  );
}
