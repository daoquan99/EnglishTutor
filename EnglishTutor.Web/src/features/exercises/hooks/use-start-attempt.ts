import { useMutation } from "@tanstack/react-query";
import { useRouter } from "next/navigation";
import { exercisesApi } from "../api/exercises-api";

export function useStartAttempt() {
  const router = useRouter();

  return useMutation({
    mutationFn: (exerciseId: string) => exercisesApi.startAttempt(exerciseId),
    onSuccess: (data) => {
      router.push(`/exercises/${data.exerciseSetId}?attemptId=${data.attemptId}`);
    },
  });
}
