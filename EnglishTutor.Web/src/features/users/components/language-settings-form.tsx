"use client";

import { useEffect } from "react";
import { Controller, useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { Loader2 } from "lucide-react";
import { Button } from "@/shared/components/ui/button";
import { Label } from "@/shared/components/ui/label";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/shared/components/ui/select";
import { Skeleton } from "@/shared/components/ui/skeleton";
import {
  languageSettingsSchema,
  type LanguageSettingsFormValues,
} from "../schemas/language-settings-schema";
import { useLanguageSettings } from "../hooks/use-language-settings";
import { useUpdateLanguageSettings } from "../hooks/use-update-language-settings";
import { useTargetLanguages } from "../hooks/use-target-languages";
import { COMMON_LANGUAGES } from "../types/users";

function LanguageSelect({
  value,
  onChange,
  label,
  id,
  options,
  error,
}: {
  value: string;
  onChange: (val: string) => void;
  label: string;
  id: string;
  options: readonly { code: string; name: string }[];
  error?: string;
}) {
  return (
    <div className="grid gap-1.5">
      <Label htmlFor={id}>{label}</Label>
      <Select value={value} onValueChange={(val) => onChange(val as string)}>
        <SelectTrigger id={id} className="w-full" aria-invalid={!!error}>
          <SelectValue placeholder="Select language" />
        </SelectTrigger>
        <SelectContent>
          {options.map((lang) => (
            <SelectItem key={lang.code} value={lang.code}>
              {lang.name}
            </SelectItem>
          ))}
        </SelectContent>
      </Select>
      {error && <p className="text-xs text-destructive">{error}</p>}
    </div>
  );
}

export function LanguageSettingsForm() {
  const { data: settings, isPending: settingsLoading } = useLanguageSettings();
  const { data: targetLanguages } = useTargetLanguages();
  const updateSettings = useUpdateLanguageSettings();

  const {
    control,
    handleSubmit,
    reset,
    formState: { errors, isDirty },
  } = useForm<LanguageSettingsFormValues>({
    resolver: zodResolver(languageSettingsSchema),
    defaultValues: {
      nativeLanguageCode: "",
      uiLanguageCode: "",
      explanationLanguageCode: "",
      activeTargetLanguageCode: "",
    },
  });

  useEffect(() => {
    if (settings) {
      reset({
        nativeLanguageCode: settings.nativeLanguageCode,
        uiLanguageCode: settings.uiLanguageCode,
        explanationLanguageCode: settings.explanationLanguageCode,
        activeTargetLanguageCode: settings.activeTargetLanguageCode,
      });
    }
  }, [settings, reset]);

  if (settingsLoading) {
    return (
      <div className="grid gap-4">
        {Array.from({ length: 4 }, (_, i) => (
          <Skeleton key={i} className="h-8 w-full" />
        ))}
      </div>
    );
  }

  const targetLangOptions = (targetLanguages ?? []).map((tl) => {
    const found = COMMON_LANGUAGES.find((l) => l.code === tl.targetLanguageCode);
    return { code: tl.targetLanguageCode, name: found?.name ?? tl.targetLanguageCode };
  });

  return (
    <form
      onSubmit={handleSubmit((data) => updateSettings.mutate(data))}
      className="grid gap-4"
    >
      <Controller
        control={control}
        name="nativeLanguageCode"
        render={({ field }) => (
          <LanguageSelect
            id="nativeLanguageCode"
            label="Native language"
            value={field.value}
            onChange={field.onChange}
            options={COMMON_LANGUAGES}
            error={errors.nativeLanguageCode?.message}
          />
        )}
      />

      <Controller
        control={control}
        name="uiLanguageCode"
        render={({ field }) => (
          <LanguageSelect
            id="uiLanguageCode"
            label="UI language"
            value={field.value}
            onChange={field.onChange}
            options={COMMON_LANGUAGES}
            error={errors.uiLanguageCode?.message}
          />
        )}
      />

      <Controller
        control={control}
        name="explanationLanguageCode"
        render={({ field }) => (
          <LanguageSelect
            id="explanationLanguageCode"
            label="Explanation language"
            value={field.value}
            onChange={field.onChange}
            options={COMMON_LANGUAGES}
            error={errors.explanationLanguageCode?.message}
          />
        )}
      />

      {targetLangOptions.length > 0 && (
        <Controller
          control={control}
          name="activeTargetLanguageCode"
          render={({ field }) => (
            <LanguageSelect
              id="activeTargetLanguageCode"
              label="Active target language"
              value={field.value}
              onChange={field.onChange}
              options={targetLangOptions}
              error={errors.activeTargetLanguageCode?.message}
            />
          )}
        />
      )}

      <div className="flex justify-end">
        <Button type="submit" disabled={!isDirty || updateSettings.isPending}>
          {updateSettings.isPending && <Loader2 className="animate-spin" />}
          Save settings
        </Button>
      </div>
    </form>
  );
}
