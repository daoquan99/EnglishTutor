"use client";

import { useState } from "react";
import { Controller, useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { Loader2, Plus } from "lucide-react";
import { Button } from "@/shared/components/ui/button";
import { Label } from "@/shared/components/ui/label";
import {
  Dialog,
  DialogClose,
  DialogContent,
  DialogDescription,
  DialogTitle,
  DialogTrigger,
} from "@/shared/components/ui/dialog";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/shared/components/ui/select";
import { ApiError } from "@/shared/api";
import {
  addTargetLanguageSchema,
  type AddTargetLanguageFormValues,
} from "../schemas/target-language-schema";
import { useAddTargetLanguage } from "../hooks/use-add-target-language";
import { COMMON_LANGUAGES, LANGUAGE_LEVELS } from "../types/users";

export function AddTargetLanguageDialog() {
  const [open, setOpen] = useState(false);
  const addLanguage = useAddTargetLanguage();

  const {
    control,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<AddTargetLanguageFormValues>({
    resolver: zodResolver(addTargetLanguageSchema),
    defaultValues: { targetLanguageCode: "", currentLevel: undefined, targetLevel: undefined },
  });

  const serverError =
    addLanguage.error instanceof ApiError ? addLanguage.error.message : null;

  const onSubmit = (data: AddTargetLanguageFormValues) => {
    addLanguage.mutate(data, {
      onSuccess: () => {
        setOpen(false);
        reset();
      },
    });
  };

  return (
    <Dialog
      open={open}
      onOpenChange={(nextOpen) => {
        setOpen(nextOpen);
        if (!nextOpen) {
          reset();
          addLanguage.reset();
        }
      }}
    >
      <DialogTrigger
        render={
          <Button variant="outline" size="sm">
            <Plus />
            Add language
          </Button>
        }
      />
      <DialogContent>
        <DialogTitle>Add target language</DialogTitle>
        <DialogDescription>Choose a language and your current/target levels.</DialogDescription>

        <form onSubmit={handleSubmit(onSubmit)} className="mt-4 grid gap-4">
          {serverError && (
            <div className="rounded-lg border border-destructive/50 bg-destructive/10 px-3 py-2 text-sm text-destructive">
              {serverError}
            </div>
          )}

          <Controller
            control={control}
            name="targetLanguageCode"
            render={({ field }) => (
              <div className="grid gap-1.5">
                <Label htmlFor="targetLanguageCode">Language</Label>
                <Select value={field.value} onValueChange={(val) => field.onChange(val as string)}>
                  <SelectTrigger id="targetLanguageCode" className="w-full">
                    <SelectValue placeholder="Select language" />
                  </SelectTrigger>
                  <SelectContent>
                    {COMMON_LANGUAGES.map((lang) => (
                      <SelectItem key={lang.code} value={lang.code}>
                        {lang.name}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
                {errors.targetLanguageCode && (
                  <p className="text-xs text-destructive">
                    {errors.targetLanguageCode.message}
                  </p>
                )}
              </div>
            )}
          />

          <div className="grid grid-cols-2 gap-4">
            <Controller
              control={control}
              name="currentLevel"
              render={({ field }) => (
                <div className="grid gap-1.5">
                  <Label htmlFor="currentLevel">Current level</Label>
                  <Select value={field.value ?? ""} onValueChange={(val) => field.onChange(val as string)}>
                    <SelectTrigger id="currentLevel" className="w-full">
                      <SelectValue placeholder="Level" />
                    </SelectTrigger>
                    <SelectContent>
                      {LANGUAGE_LEVELS.map((level) => (
                        <SelectItem key={level} value={level}>
                          {level}
                        </SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                  {errors.currentLevel && (
                    <p className="text-xs text-destructive">{errors.currentLevel.message}</p>
                  )}
                </div>
              )}
            />

            <Controller
              control={control}
              name="targetLevel"
              render={({ field }) => (
                <div className="grid gap-1.5">
                  <Label htmlFor="targetLevel">Target level</Label>
                  <Select value={field.value ?? ""} onValueChange={(val) => field.onChange(val as string)}>
                    <SelectTrigger id="targetLevel" className="w-full">
                      <SelectValue placeholder="Level" />
                    </SelectTrigger>
                    <SelectContent>
                      {LANGUAGE_LEVELS.map((level) => (
                        <SelectItem key={level} value={level}>
                          {level}
                        </SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                  {errors.targetLevel && (
                    <p className="text-xs text-destructive">{errors.targetLevel.message}</p>
                  )}
                </div>
              )}
            />
          </div>

          <div className="flex justify-end gap-2">
            <DialogClose render={<Button variant="outline">Cancel</Button>} />
            <Button type="submit" disabled={addLanguage.isPending}>
              {addLanguage.isPending && <Loader2 className="animate-spin" />}
              Add language
            </Button>
          </div>
        </form>
      </DialogContent>
    </Dialog>
  );
}
