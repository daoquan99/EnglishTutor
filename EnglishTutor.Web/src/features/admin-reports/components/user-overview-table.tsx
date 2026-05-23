"use client";

import { Skeleton } from "@/shared/components/ui/skeleton";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/shared/components/ui/table";
import type { UserOverviewCard } from "../types/admin-reports";

function formatDate(utc: string | null) {
  if (!utc) return "Never";
  return new Date(utc).toLocaleDateString(undefined, {
    month: "short",
    day: "numeric",
    year: "numeric",
  });
}

export function UserOverviewTable({ users }: { users?: UserOverviewCard[] }) {
  if (!users) {
    return <Skeleton className="h-60 w-full" />;
  }

  return (
    <Table>
      <TableHeader>
        <TableRow>
          <TableHead>User</TableHead>
          <TableHead>Level</TableHead>
          <TableHead>EXP</TableHead>
          <TableHead>Streak</TableHead>
          <TableHead>Last Active</TableHead>
        </TableRow>
      </TableHeader>
      <TableBody>
        {users.map((u) => (
          <TableRow key={u.userId}>
            <TableCell>
              <div>
                <p className="font-medium">{u.displayName}</p>
                <p className="text-xs text-muted-foreground">{u.email}</p>
              </div>
            </TableCell>
            <TableCell>{u.currentLevel}</TableCell>
            <TableCell>{u.totalExp}</TableCell>
            <TableCell>{u.currentStreakDays}d</TableCell>
            <TableCell className="text-xs">
              {formatDate(u.lastActivityAtUtc)}
            </TableCell>
          </TableRow>
        ))}
      </TableBody>
    </Table>
  );
}
