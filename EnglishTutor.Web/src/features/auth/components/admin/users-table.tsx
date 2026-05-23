"use client";

import { MoreHorizontal } from "lucide-react";
import { usePathname, useRouter, useSearchParams } from "next/navigation";
import { useCallback, useEffect, useMemo, useState } from "react";
import { Badge } from "@/shared/components/ui/badge";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from "@/shared/components/ui/dropdown-menu";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/shared/components/ui/select";
import {
  Tooltip,
  TooltipContent,
  TooltipTrigger,
} from "@/shared/components/ui/tooltip";
import { useDebouncedValue } from "@/shared/hooks/use-debounced-value";
import { formatRelativeTime } from "@/shared/lib/format-relative-time";
import { cn } from "@/shared/lib/utils";
import {
  AdminDataTable,
  type AdminDataTableColumn,
} from "@/shared/admin";
import { useAdminUsers } from "../../hooks/use-admin-users";
import type {
  AuthRole,
  AuthUserListItem,
  UserSortOrder,
  UserStatusFilter,
} from "../../types/admin-auth";

const PAGE_SIZE_OPTIONS = [10, 20, 50, 100] as const;
const DEFAULT_PAGE_SIZE = 20;

const SORT_OPTIONS: { value: UserSortOrder; label: string }[] = [
  { value: "NewestFirst", label: "Newest first" },
  { value: "OldestFirst", label: "Oldest first" },
  { value: "RecentlyUpdated", label: "Recently updated" },
  { value: "DisplayNameAsc", label: "Name (A → Z)" },
];

const STATUSES: UserStatusFilter[] = ["All", "Active", "Suspended"];

type UserAction = "view" | "edit" | "roles" | "suspend" | "restore";

interface UsersTableProps {
  roles: AuthRole[] | undefined;
  onAction: (action: UserAction, user: AuthUserListItem) => void;
}

const absoluteDateFormatter = new Intl.DateTimeFormat(undefined, {
  dateStyle: "medium",
  timeStyle: "short",
});

function readStatus(value: string | null): UserStatusFilter {
  return value === "Active" || value === "Suspended" ? value : "All";
}

function readSort(value: string | null): UserSortOrder {
  if (
    value === "OldestFirst" ||
    value === "RecentlyUpdated" ||
    value === "DisplayNameAsc"
  ) {
    return value;
  }
  return "NewestFirst";
}

function readPageSize(value: string | null): number {
  const parsed = value ? Number.parseInt(value, 10) : NaN;
  return (PAGE_SIZE_OPTIONS as readonly number[]).includes(parsed)
    ? parsed
    : DEFAULT_PAGE_SIZE;
}

export function UsersTable({ roles, onAction }: UsersTableProps) {
  const router = useRouter();
  const pathname = usePathname();
  const searchParams = useSearchParams();

  const search = searchParams.get("q") ?? "";
  const status = readStatus(searchParams.get("status"));
  const roleId = searchParams.get("roleId") ?? "all";
  const sort = readSort(searchParams.get("sort"));
  const pageSize = readPageSize(searchParams.get("pageSize"));
  const page = Math.max(1, Number.parseInt(searchParams.get("page") ?? "1", 10) || 1);

  const [searchInput, setSearchInput] = useState(search);
  const debouncedSearch = useDebouncedValue(searchInput, 300);

  const updateParams = useCallback(
    (mutations: Record<string, string | null | undefined>) => {
      const params = new URLSearchParams(searchParams.toString());
      for (const [key, value] of Object.entries(mutations)) {
        if (value === undefined || value === null || value === "") {
          params.delete(key);
        } else {
          params.set(key, value);
        }
      }
      const query = params.toString();
      router.replace(query ? `${pathname}?${query}` : pathname, { scroll: false });
    },
    [pathname, router, searchParams],
  );

  useEffect(() => {
    if (debouncedSearch === search) return;
    updateParams({ q: debouncedSearch || undefined, page: undefined });
  }, [debouncedSearch, search, updateParams]);

  useEffect(() => {
    setSearchInput(search);
  }, [search]);

  const queryParams = useMemo(
    () => ({
      search: search.trim() || undefined,
      status,
      roleId: roleId === "all" ? undefined : roleId,
      page,
      pageSize,
      sort,
    }),
    [search, status, roleId, page, pageSize, sort],
  );

  const { data, isFetching } = useAdminUsers(queryParams);

  const roleNameById = useMemo(
    () => new Map<string, string>(roles?.map((role) => [role.id, role.name]) ?? []),
    [roles],
  );

  const columns: AdminDataTableColumn<AuthUserListItem>[] = useMemo(
    () => [
      {
        id: "displayName",
        header: "Display name",
        cell: (user) => (
          <span
            className={cn(
              "font-medium",
              !user.isActive && "text-foreground/70",
            )}
          >
            {user.displayName}
          </span>
        ),
      },
      {
        id: "email",
        header: "Email",
        cell: (user) => (
          <span className="text-sm text-muted-foreground">{user.email}</span>
        ),
      },
      {
        id: "roles",
        header: "Roles",
        cell: (user) => (
          <div className="flex flex-wrap gap-1">
            {user.roleIds.length === 0 ? (
              <span className="text-xs text-muted-foreground">—</span>
            ) : (
              user.roleIds.map((id) => (
                <Badge key={id} variant="secondary">
                  {roleNameById.get(id) ?? id.slice(0, 8)}
                </Badge>
              ))
            )}
          </div>
        ),
      },
      {
        id: "status",
        header: "Status",
        cell: (user) => (
          <Badge variant={user.isActive ? "default" : "destructive"}>
            {user.isActive ? "Active" : "Suspended"}
          </Badge>
        ),
      },
      {
        id: "created",
        header: "Created",
        cell: (user) => (
          <Tooltip>
            <TooltipTrigger className="cursor-default text-sm text-muted-foreground">
              {formatRelativeTime(user.createdAtUtc)}
            </TooltipTrigger>
            <TooltipContent>
              {absoluteDateFormatter.format(new Date(user.createdAtUtc))}
            </TooltipContent>
          </Tooltip>
        ),
      },
    ],
    [roleNameById],
  );

  return (
    <AdminDataTable<AuthUserListItem>
      data={data?.items}
      isLoading={!data}
      isFetching={isFetching}
      getRowId={(user) => user.id}
      emptyMessage="Không có user nào khớp filter."
      rowClassName={(user) =>
        !user.isActive ? "text-muted-foreground" : undefined
      }
      search={{
        value: searchInput,
        onChange: setSearchInput,
        placeholder: "Tìm theo email hoặc display name…",
      }}
      toolbar={
        <>
          <Select
            value={status}
            onValueChange={(value) =>
              updateParams({
                status: value === "All" ? undefined : value,
                page: undefined,
              })
            }
          >
            <SelectTrigger className="h-9 w-36" aria-label="Filter by status">
              <SelectValue />
            </SelectTrigger>
            <SelectContent>
              {STATUSES.map((value) => (
                <SelectItem key={value} value={value}>
                  {value === "All" ? "All statuses" : value}
                </SelectItem>
              ))}
            </SelectContent>
          </Select>
          <Select
            value={roleId}
            onValueChange={(value) =>
              updateParams({
                roleId: value === "all" ? undefined : value,
                page: undefined,
              })
            }
          >
            <SelectTrigger className="h-9 w-44" aria-label="Filter by role">
              <SelectValue placeholder="Filter by role" />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="all">All roles</SelectItem>
              {roles?.map((role) => (
                <SelectItem key={role.id} value={role.id}>
                  {role.name}
                </SelectItem>
              ))}
            </SelectContent>
          </Select>
          <Select
            value={sort}
            onValueChange={(value) =>
              updateParams({
                sort: value === "NewestFirst" ? undefined : value,
                page: undefined,
              })
            }
          >
            <SelectTrigger className="h-9 w-44" aria-label="Sort users">
              <SelectValue />
            </SelectTrigger>
            <SelectContent>
              {SORT_OPTIONS.map((option) => (
                <SelectItem key={option.value} value={option.value}>
                  {option.label}
                </SelectItem>
              ))}
            </SelectContent>
          </Select>
        </>
      }
      columns={columns}
      rowActions={(user) => (
        <DropdownMenu>
          <DropdownMenuTrigger
            aria-label={`Actions for ${user.displayName}`}
            className="inline-flex size-8 cursor-pointer items-center justify-center rounded-md outline-none transition-colors hover:bg-muted focus-visible:ring-2 focus-visible:ring-ring"
          >
            <MoreHorizontal className="size-4" />
          </DropdownMenuTrigger>
          <DropdownMenuContent align="end">
            <DropdownMenuItem onClick={() => onAction("view", user)}>
              View details
            </DropdownMenuItem>
            <DropdownMenuItem onClick={() => onAction("edit", user)}>
              Edit display name
            </DropdownMenuItem>
            <DropdownMenuItem onClick={() => onAction("roles", user)}>
              Set roles
            </DropdownMenuItem>
            <DropdownMenuSeparator />
            {user.isActive ? (
              <DropdownMenuItem
                onClick={() => onAction("suspend", user)}
                className="text-destructive"
              >
                Suspend user
              </DropdownMenuItem>
            ) : (
              <DropdownMenuItem onClick={() => onAction("restore", user)}>
                Restore user
              </DropdownMenuItem>
            )}
          </DropdownMenuContent>
        </DropdownMenu>
      )}
      pagination={{
        page,
        pageSize,
        total: data?.total ?? 0,
        pageSizeOptions: PAGE_SIZE_OPTIONS,
        onPageChange: (next) => {
          updateParams({ page: next === 1 ? undefined : String(next) });
        },
        onPageSizeChange: (next) => {
          updateParams({
            pageSize: next === DEFAULT_PAGE_SIZE ? undefined : String(next),
            page: undefined,
          });
        },
      }}
    />
  );
}
