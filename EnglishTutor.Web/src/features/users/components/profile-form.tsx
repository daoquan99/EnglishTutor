"use client";

import { useEffect } from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { Loader2 } from "lucide-react";
import { Button } from "@/shared/components/ui/button";
import { Input } from "@/shared/components/ui/input";
import { Label } from "@/shared/components/ui/label";
import { Textarea } from "@/shared/components/ui/textarea";
import { Skeleton } from "@/shared/components/ui/skeleton";
import { profileSchema, type ProfileFormValues } from "../schemas/profile-schema";
import { useProfile } from "../hooks/use-profile";
import { useUpdateProfile } from "../hooks/use-update-profile";

export function ProfileForm() {
  const { data: profile, isPending } = useProfile();
  const updateProfile = useUpdateProfile();

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors, isDirty },
  } = useForm<ProfileFormValues>({
    resolver: zodResolver(profileSchema),
    defaultValues: { displayName: "", bio: "" },
  });

  useEffect(() => {
    if (profile) {
      reset({
        displayName: profile.displayName,
        avatarUrl: profile.avatarUrl,
        bio: profile.bio,
      });
    }
  }, [profile, reset]);

  if (isPending) {
    return (
      <div className="grid gap-4">
        <Skeleton className="h-8 w-full" />
        <Skeleton className="h-20 w-full" />
      </div>
    );
  }

  return (
    <form
      onSubmit={handleSubmit((data) => updateProfile.mutate(data))}
      className="grid gap-4"
    >
      <div className="grid gap-1.5">
        <Label htmlFor="displayName">Display name</Label>
        <Input
          id="displayName"
          aria-invalid={!!errors.displayName}
          {...register("displayName")}
        />
        {errors.displayName && (
          <p className="text-xs text-destructive">{errors.displayName.message}</p>
        )}
      </div>

      <div className="grid gap-1.5">
        <Label htmlFor="bio">Bio</Label>
        <Textarea
          id="bio"
          rows={3}
          placeholder="Tell us about yourself"
          aria-invalid={!!errors.bio}
          {...register("bio")}
        />
        {errors.bio && <p className="text-xs text-destructive">{errors.bio.message}</p>}
      </div>

      <div className="flex justify-end">
        <Button type="submit" disabled={!isDirty || updateProfile.isPending}>
          {updateProfile.isPending && <Loader2 className="animate-spin" />}
          Save changes
        </Button>
      </div>
    </form>
  );
}
