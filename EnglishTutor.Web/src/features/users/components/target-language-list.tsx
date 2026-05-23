"use client";

import { Check, Loader2 } from "lucide-react";
import { Button } from "@/shared/components/ui/button";
import { Skeleton } from "@/shared/components/ui/skeleton";
import { useTargetLanguages } from "../hooks/use-target-languages";
import { useActivateTargetLanguage } from "../hooks/use-activate-target-language";
import { COMMON_LANGUAGES } from "../types/users";
import { LevelBadge } from "./level-badge";

function getLanguageName(code: string): string {
  return COMMON_LANGUAGES.find((l) => l.code === code)?.name ?? code;
}

export function TargetLanguageList() {
  const { data: languages, isPending } = useTargetLanguages();
  const activate = useActivateTargetLanguage();

  if (isPending) {
    return (
      <div className="grid gap-2">
        {Array.from({ length: 2 }, (_, i) => (
          <Skeleton key={i} className="h-14 w-full rounded-lg" />
        ))}
      </div>
    );
  }

  if (!languages?.length) {
    return (
      <p className="py-4 text-center text-sm text-muted-foreground">
        No target languages added yet.
      </p>
    );
  }

  return (
    <ul className="grid gap-2">
      {languages.map((lang) => (
        <li
          key={lang.id}
          className="flex items-center gap-3 rounded-lg border p-3"
        >
          <div className="flex-1">
            <div className="flex items-center gap-2">
              <span className="font-medium">{getLanguageName(lang.targetLanguageCode)}</span>
              {lang.isActive && (
                <span className="inline-flex items-center gap-1 rounded-full bg-primary/10 px-2 py-0.5 text-xs font-medium text-primary">
                  <Check className="size-3" />
                  Active
                </span>
              )}
            </div>
            <div className="mt-1 flex items-center gap-2 text-xs text-muted-foreground">
              <LevelBadge level={lang.currentLevel} />
              <span>→</span>
              <LevelBadge level={lang.targetLevel} />
            </div>
          </div>
          {!lang.isActive && (
            <Button
              variant="outline"
              size="sm"
              disabled={activate.isPending}
              onClick={() => activate.mutate(lang.id)}
            >
              {activate.isPending && activate.variables === lang.id && (
                <Loader2 className="animate-spin" />
              )}
              Activate
            </Button>
          )}
        </li>
      ))}
    </ul>
  );
}
