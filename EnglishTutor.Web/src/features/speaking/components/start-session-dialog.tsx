"use client";

import { useState } from "react";
import { Controller, useForm } from "react-hook-form";
import { Loader2, Plus } from "lucide-react";
import { Button } from "@/shared/components/ui/button";
import { Input } from "@/shared/components/ui/input";
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
import { useStartSession } from "../hooks/use-start-session";
import { SESSION_TYPES } from "../types/speaking";

interface FormValues {
  sessionType: string;
  topic: string;
}

export function StartSessionDialog() {
  const [open, setOpen] = useState(false);
  const startSession = useStartSession();

  const { control, register, handleSubmit, reset, watch } = useForm<FormValues>({
    defaultValues: { sessionType: "FreeTalk", topic: "" },
  });

  const sessionType = watch("sessionType");

  const onSubmit = (data: FormValues) => {
    startSession.mutate(
      {
        sessionType: data.sessionType,
        topic: data.topic || null,
      },
      { onSuccess: () => { setOpen(false); reset(); } },
    );
  };

  return (
    <Dialog open={open} onOpenChange={setOpen}>
      <DialogTrigger render={<Button><Plus />New session</Button>} />
      <DialogContent>
        <DialogTitle>Start speaking session</DialogTitle>
        <DialogDescription>Choose a session type and optional topic.</DialogDescription>

        <form onSubmit={handleSubmit(onSubmit)} className="mt-4 grid gap-4">
          <Controller
            control={control}
            name="sessionType"
            render={({ field }) => (
              <div className="grid gap-1.5">
                <Label>Session type</Label>
                <Select value={field.value} onValueChange={(val) => field.onChange(val as string)}>
                  <SelectTrigger className="w-full">
                    <SelectValue />
                  </SelectTrigger>
                  <SelectContent>
                    {SESSION_TYPES.map((t) => (
                      <SelectItem key={t} value={t}>
                        {t.replace(/([A-Z])/g, " $1").trim()}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              </div>
            )}
          />

          {sessionType !== "ConversationPractice" && (
            <div className="grid gap-1.5">
              <Label htmlFor="topic">Topic (optional)</Label>
              <Input id="topic" placeholder="e.g. Travel, Food, Technology" {...register("topic")} />
            </div>
          )}

          <div className="flex justify-end gap-2">
            <DialogClose render={<Button variant="outline">Cancel</Button>} />
            <Button type="submit" disabled={startSession.isPending}>
              {startSession.isPending && <Loader2 className="animate-spin" />}
              Start
            </Button>
          </div>
        </form>
      </DialogContent>
    </Dialog>
  );
}
