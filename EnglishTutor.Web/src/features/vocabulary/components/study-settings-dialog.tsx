"use client";

import { useState, useEffect } from "react";
import { Settings } from "lucide-react";
import { Button } from "@/shared/components/ui/button";
import { Input } from "@/shared/components/ui/input";
import { Label } from "@/shared/components/ui/label";
import { Switch } from "@/shared/components/ui/switch";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
  DialogTrigger,
} from "@/shared/components/ui/dialog";
import { useStudySettings } from "../hooks/use-study-settings";
import { useUpdateStudySettings } from "../hooks/use-update-study-settings";

interface StudySettingsDialogProps {
  targetLanguageCode: string | null;
}

export function StudySettingsDialog({ targetLanguageCode }: StudySettingsDialogProps) {
  const [open, setOpen] = useState(false);
  const { data: settings } = useStudySettings(targetLanguageCode);
  const updateSettings = useUpdateStudySettings(targetLanguageCode);

  const [newWordsStr, setNewWordsStr] = useState("5");
  const [reviewWordsStr, setReviewWordsStr] = useState("20");
  const [includeMasteredInReview, setIncludeMasteredInReview] = useState(false);

  useEffect(() => {
    if (settings) {
      setNewWordsStr(String(settings.newWordsPerDay));
      setReviewWordsStr(String(settings.reviewWordsPerDay));
      setIncludeMasteredInReview(settings.includeMasteredInReview);
    }
  }, [settings]);

  const clampNumericStr = (value: string, min: number, max: number) => {
    const n = parseInt(value, 10);
    if (isNaN(n) || n < min) return String(min);
    if (n > max) return String(max);
    return String(n);
  };

  const handleSave = () => {
    const newWordsPerDay = Math.max(1, Math.min(50, parseInt(newWordsStr, 10) || 1));
    const reviewWordsPerDay = Math.max(1, Math.min(100, parseInt(reviewWordsStr, 10) || 1));
    updateSettings.mutate(
      { newWordsPerDay, reviewWordsPerDay, includeMasteredInReview },
      { onSuccess: () => setOpen(false) },
    );
  };

  return (
    <Dialog open={open} onOpenChange={setOpen}>
      <DialogTrigger
        render={<Button variant="ghost" size="icon-sm" />}
      >
        <Settings className="size-4" />
      </DialogTrigger>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>Study Settings</DialogTitle>
          <DialogDescription>
            Configure your daily vocabulary learning goals.
          </DialogDescription>
        </DialogHeader>

        <div className="grid gap-4 py-2">
          <div className="grid gap-1.5">
            <Label htmlFor="newWordsPerDay">New words per day (1-50)</Label>
            <Input
              id="newWordsPerDay"
              type="text"
              inputMode="numeric"
              value={newWordsStr}
              onChange={(e) => setNewWordsStr(e.target.value.replace(/\D/g, ""))}
              onBlur={() => setNewWordsStr(clampNumericStr(newWordsStr, 1, 50))}
            />
          </div>
          <div className="grid gap-1.5">
            <Label htmlFor="reviewWordsPerDay">Review words per day (1-100)</Label>
            <Input
              id="reviewWordsPerDay"
              type="text"
              inputMode="numeric"
              value={reviewWordsStr}
              onChange={(e) => setReviewWordsStr(e.target.value.replace(/\D/g, ""))}
              onBlur={() => setReviewWordsStr(clampNumericStr(reviewWordsStr, 1, 100))}
            />
          </div>
          <div className="flex items-center justify-between">
            <Label htmlFor="includeMastered">Include mastered in review</Label>
            <Switch
              id="includeMastered"
              checked={includeMasteredInReview}
              onCheckedChange={setIncludeMasteredInReview}
            />
          </div>
        </div>

        <DialogFooter>
          <Button onClick={handleSave} disabled={updateSettings.isPending}>
            Save
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
