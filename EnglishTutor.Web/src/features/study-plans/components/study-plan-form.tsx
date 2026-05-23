"use client";

import { useEffect } from "react";
import { useForm } from "react-hook-form";
import { Loader2 } from "lucide-react";
import { Button } from "@/shared/components/ui/button";
import { Input } from "@/shared/components/ui/input";
import { Label } from "@/shared/components/ui/label";
import type { StudyPlan, UpdateStudyPlanRequest } from "../types/study-plans";
import { useUpdateStudyPlan } from "../hooks/use-update-study-plan";

interface FormValues {
  preferredStudyTime: string;
  reminderBeforeMinutes: number;
  dailyTargetMinutes: number;
  weeklyTargetMinutes: number;
  monthlyTargetMinutes: number;
  monthlyTargetStudyDays: number;
}

interface StudyPlanFormProps {
  plan: StudyPlan;
}

export function StudyPlanForm({ plan }: StudyPlanFormProps) {
  const updatePlan = useUpdateStudyPlan(plan.targetLanguageCode);

  const { register, handleSubmit, reset, formState } = useForm<FormValues>({
    defaultValues: {
      preferredStudyTime: plan.preferredStudyTime,
      reminderBeforeMinutes: plan.reminderBeforeMinutes,
      dailyTargetMinutes: plan.dailyTargetMinutes,
      weeklyTargetMinutes: plan.weeklyTargetMinutes,
      monthlyTargetMinutes: plan.monthlyTargetMinutes,
      monthlyTargetStudyDays: plan.monthlyTargetStudyDays,
    },
  });

  useEffect(() => {
    reset({
      preferredStudyTime: plan.preferredStudyTime,
      reminderBeforeMinutes: plan.reminderBeforeMinutes,
      dailyTargetMinutes: plan.dailyTargetMinutes,
      weeklyTargetMinutes: plan.weeklyTargetMinutes,
      monthlyTargetMinutes: plan.monthlyTargetMinutes,
      monthlyTargetStudyDays: plan.monthlyTargetStudyDays,
    });
  }, [plan, reset]);

  const onSubmit = (data: FormValues) => {
    const req: UpdateStudyPlanRequest = {
      preferredStudyTime: data.preferredStudyTime,
      reminderBeforeMinutes: data.reminderBeforeMinutes,
      dailyTargetMinutes: data.dailyTargetMinutes,
      weeklyTargetMinutes: data.weeklyTargetMinutes,
      monthlyTargetMinutes: data.monthlyTargetMinutes,
      monthlyTargetStudyDays: data.monthlyTargetStudyDays,
    };
    updatePlan.mutate(req);
  };

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="grid gap-4">
      <div className="grid gap-1.5">
        <Label htmlFor="preferredStudyTime">Preferred study time</Label>
        <Input
          id="preferredStudyTime"
          type="time"
          {...register("preferredStudyTime")}
        />
      </div>

      <div className="grid grid-cols-2 gap-4">
        <div className="grid gap-1.5">
          <Label htmlFor="dailyTargetMinutes">Daily target (min)</Label>
          <Input
            id="dailyTargetMinutes"
            type="number"
            min={5}
            {...register("dailyTargetMinutes", { valueAsNumber: true })}
          />
        </div>
        <div className="grid gap-1.5">
          <Label htmlFor="weeklyTargetMinutes">Weekly target (min)</Label>
          <Input
            id="weeklyTargetMinutes"
            type="number"
            min={5}
            {...register("weeklyTargetMinutes", { valueAsNumber: true })}
          />
        </div>
      </div>

      <div className="grid grid-cols-2 gap-4">
        <div className="grid gap-1.5">
          <Label htmlFor="monthlyTargetMinutes">Monthly target (min)</Label>
          <Input
            id="monthlyTargetMinutes"
            type="number"
            min={5}
            {...register("monthlyTargetMinutes", { valueAsNumber: true })}
          />
        </div>
        <div className="grid gap-1.5">
          <Label htmlFor="monthlyTargetStudyDays">Monthly study days</Label>
          <Input
            id="monthlyTargetStudyDays"
            type="number"
            min={1}
            max={31}
            {...register("monthlyTargetStudyDays", { valueAsNumber: true })}
          />
        </div>
      </div>

      <div className="grid gap-1.5">
        <Label htmlFor="reminderBeforeMinutes">Reminder before (min)</Label>
        <Input
          id="reminderBeforeMinutes"
          type="number"
          min={0}
          {...register("reminderBeforeMinutes", { valueAsNumber: true })}
        />
      </div>

      <Button
        type="submit"
        disabled={!formState.isDirty || updatePlan.isPending}
        className="w-fit"
      >
        {updatePlan.isPending && <Loader2 className="animate-spin" />}
        Save changes
      </Button>
    </form>
  );
}
