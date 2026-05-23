import Link from "next/link";
import { Button } from "@/shared/components/ui/button";

export default function NotFound() {
  return (
    <div className="flex min-h-dvh flex-col items-center justify-center gap-4">
      <h1 className="text-6xl font-bold text-muted-foreground">404</h1>
      <p className="text-lg text-muted-foreground">Page not found</p>
      <Button render={<Link href="/" />}>Go home</Button>
    </div>
  );
}
