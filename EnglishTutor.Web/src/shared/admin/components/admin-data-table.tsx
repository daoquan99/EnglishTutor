"use client";

import type { ReactNode } from "react";
import {
  ChevronLeft,
  ChevronRight,
  ChevronsLeft,
  ChevronsRight,
  Loader2,
  Search,
} from "lucide-react";
import { Button } from "@/shared/components/ui/button";
import { Input } from "@/shared/components/ui/input";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/shared/components/ui/select";
import { Skeleton } from "@/shared/components/ui/skeleton";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/shared/components/ui/table";
import { cn } from "@/shared/lib/utils";

export type AdminDataTableColumn<TRow> = {
  /** Stable id for React keys / column targeting. */
  id: string;
  header: ReactNode;
  /** How to render the cell from a row. */
  cell: (row: TRow) => ReactNode;
  className?: string;
  headerClassName?: string;
  /** Right-align numeric / action columns. */
  align?: "left" | "right" | "center";
};

export type AdminDataTablePagination = {
  /** 1-indexed page number. */
  page: number;
  pageSize: number;
  total: number;
  onPageChange: (page: number) => void;
  /** Optional — show page-size selector when provided. */
  onPageSizeChange?: (pageSize: number) => void;
  /** Defaults to [10, 20, 50, 100]. */
  pageSizeOptions?: readonly number[];
};

export type AdminDataTableSearch = {
  value: string;
  onChange: (value: string) => void;
  placeholder?: string;
};

type AdminDataTableProps<TRow> = {
  columns: AdminDataTableColumn<TRow>[];
  data: TRow[] | undefined;
  /** Initial / hard load — shows skeleton rows. */
  isLoading?: boolean;
  /** Background refetch — shows spinner near pagination info. */
  isFetching?: boolean;
  emptyMessage?: ReactNode;
  getRowId: (row: TRow) => string;
  pagination?: AdminDataTablePagination;
  search?: AdminDataTableSearch;
  /** Extra controls shown next to the search box. */
  toolbar?: ReactNode;
  rowActions?: (row: TRow) => ReactNode;
  /** Per-row class override (e.g. dim inactive rows). */
  rowClassName?: (row: TRow) => string | undefined;
};

const DEFAULT_PAGE_SIZE_OPTIONS = [10, 20, 50, 100] as const;

export function AdminDataTable<TRow>({
  columns,
  data,
  isLoading,
  isFetching,
  emptyMessage = "Không có dữ liệu.",
  getRowId,
  pagination,
  search,
  toolbar,
  rowActions,
  rowClassName,
}: AdminDataTableProps<TRow>) {
  const totalColumns = columns.length + (rowActions ? 1 : 0);
  const totalPages = pagination
    ? Math.max(1, Math.ceil(pagination.total / pagination.pageSize))
    : 1;

  return (
    <div className="space-y-3">
      {(search || toolbar) && (
        <div className="flex flex-col gap-2 sm:flex-row sm:items-center sm:justify-between">
          {search ? (
            <div className="relative w-full sm:max-w-sm">
              <Search className="absolute top-1/2 left-3 size-4 -translate-y-1/2 text-muted-foreground" />
              <Input
                value={search.value}
                onChange={(event) => search.onChange(event.target.value)}
                placeholder={search.placeholder ?? "Tìm kiếm..."}
                className="pl-9"
              />
            </div>
          ) : (
            <span />
          )}
          {toolbar ? (
            <div className="flex flex-wrap items-center gap-2">{toolbar}</div>
          ) : null}
        </div>
      )}

      <div className="overflow-hidden rounded-md border">
        <Table>
          <TableHeader className="bg-muted/40">
            <TableRow>
              {columns.map((column) => (
                <TableHead
                  key={column.id}
                  className={cn(
                    "text-xs font-semibold uppercase tracking-wide text-muted-foreground",
                    column.align === "right" && "text-right",
                    column.align === "center" && "text-center",
                    column.headerClassName,
                  )}
                >
                  {column.header}
                </TableHead>
              ))}
              {rowActions ? (
                <TableHead className="w-12 text-right text-xs font-semibold uppercase tracking-wide text-muted-foreground">
                  Thao tác
                </TableHead>
              ) : null}
            </TableRow>
          </TableHeader>
          <TableBody>
            {isLoading ? (
              Array.from({ length: pagination?.pageSize ?? 8 }).map((_, rowIndex) => (
                <TableRow key={`skeleton-${rowIndex}`}>
                  {Array.from({ length: totalColumns }).map((__, cellIndex) => (
                    <TableCell key={`skeleton-${rowIndex}-${cellIndex}`}>
                      <Skeleton className="h-4 w-full" />
                    </TableCell>
                  ))}
                </TableRow>
              ))
            ) : !data || data.length === 0 ? (
              <TableRow>
                <TableCell
                  colSpan={totalColumns}
                  className="h-32 text-center text-sm text-muted-foreground"
                >
                  {emptyMessage}
                </TableCell>
              </TableRow>
            ) : (
              data.map((row) => (
                <TableRow
                  key={getRowId(row)}
                  className={cn("hover:bg-muted/30", rowClassName?.(row))}
                >
                  {columns.map((column) => (
                    <TableCell
                      key={column.id}
                      className={cn(
                        column.align === "right" && "text-right",
                        column.align === "center" && "text-center",
                        column.className,
                      )}
                    >
                      {column.cell(row)}
                    </TableCell>
                  ))}
                  {rowActions ? (
                    <TableCell className="w-12 text-right">{rowActions(row)}</TableCell>
                  ) : null}
                </TableRow>
              ))
            )}
          </TableBody>
        </Table>
      </div>

      {pagination ? (
        <AdminTablePaginationBar
          pagination={pagination}
          totalPages={totalPages}
          isFetching={isFetching}
        />
      ) : null}
    </div>
  );
}

function AdminTablePaginationBar({
  pagination,
  totalPages,
  isFetching,
}: {
  pagination: AdminDataTablePagination;
  totalPages: number;
  isFetching?: boolean;
}) {
  const { page, pageSize, total, onPageChange, onPageSizeChange, pageSizeOptions } =
    pagination;

  const safePage = Math.min(Math.max(1, page), totalPages);
  const firstRow = total === 0 ? 0 : (safePage - 1) * pageSize + 1;
  const lastRow = Math.min(safePage * pageSize, total);
  const sizeOptions = pageSizeOptions ?? DEFAULT_PAGE_SIZE_OPTIONS;

  return (
    <div className="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
      <div className="flex items-center gap-3 text-xs text-muted-foreground">
        <span>
          {total === 0
            ? "0 dòng"
            : `Hiển thị ${firstRow.toLocaleString()}–${lastRow.toLocaleString()} / ${total.toLocaleString()}`}
        </span>
        {isFetching ? (
          <span className="flex items-center gap-1">
            <Loader2 className="size-3 animate-spin" />
            đang cập nhật…
          </span>
        ) : null}
      </div>

      <div className="flex flex-wrap items-center gap-4">
        {onPageSizeChange ? (
          <div className="flex items-center gap-2 text-xs text-muted-foreground">
            <span>Dòng/trang</span>
            <Select
              value={String(pageSize)}
              onValueChange={(value) => {
                if (!value) return;
                const parsed = Number.parseInt(value, 10);
                if (Number.isFinite(parsed)) {
                  onPageSizeChange(parsed);
                }
              }}
            >
              <SelectTrigger className="h-8 w-[5.25rem]" aria-label="Rows per page">
                <SelectValue />
              </SelectTrigger>
              <SelectContent>
                {sizeOptions.map((size) => (
                  <SelectItem key={size} value={String(size)}>
                    {size}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          </div>
        ) : null}

        <div className="flex items-center gap-1">
          <Button
            type="button"
            variant="outline"
            size="icon"
            className="size-8"
            onClick={() => onPageChange(1)}
            disabled={safePage <= 1}
            aria-label="Trang đầu"
          >
            <ChevronsLeft className="size-4" />
          </Button>
          <Button
            type="button"
            variant="outline"
            size="icon"
            className="size-8"
            onClick={() => onPageChange(safePage - 1)}
            disabled={safePage <= 1}
            aria-label="Trang trước"
          >
            <ChevronLeft className="size-4" />
          </Button>

          <PageNumberButtons
            page={safePage}
            totalPages={totalPages}
            onPageChange={onPageChange}
          />

          <Button
            type="button"
            variant="outline"
            size="icon"
            className="size-8"
            onClick={() => onPageChange(safePage + 1)}
            disabled={safePage >= totalPages}
            aria-label="Trang sau"
          >
            <ChevronRight className="size-4" />
          </Button>
          <Button
            type="button"
            variant="outline"
            size="icon"
            className="size-8"
            onClick={() => onPageChange(totalPages)}
            disabled={safePage >= totalPages}
            aria-label="Trang cuối"
          >
            <ChevronsRight className="size-4" />
          </Button>
        </div>
      </div>
    </div>
  );
}

function PageNumberButtons({
  page,
  totalPages,
  onPageChange,
}: {
  page: number;
  totalPages: number;
  onPageChange: (page: number) => void;
}) {
  const pages = buildPageWindow(page, totalPages);

  return (
    <div className="flex items-center gap-1">
      {pages.map((entry, index) =>
        entry === "…" ? (
          <span
            key={`ellipsis-${index}`}
            className="px-2 text-xs text-muted-foreground"
          >
            …
          </span>
        ) : (
          <Button
            key={entry}
            type="button"
            variant={entry === page ? "default" : "outline"}
            size="icon"
            className="size-8 text-xs"
            onClick={() => onPageChange(entry)}
            aria-current={entry === page ? "page" : undefined}
            aria-label={`Trang ${entry}`}
          >
            {entry}
          </Button>
        ),
      )}
    </div>
  );
}

/**
 * Returns a paginator window like [1, '…', 4, 5, 6, '…', 20].
 * Always shows first + last; shows current ± 1 in the middle.
 */
function buildPageWindow(page: number, totalPages: number): (number | "…")[] {
  if (totalPages <= 7) {
    return Array.from({ length: totalPages }, (_, i) => i + 1);
  }

  const result: (number | "…")[] = [1];
  const start = Math.max(2, page - 1);
  const end = Math.min(totalPages - 1, page + 1);

  if (start > 2) {
    result.push("…");
  }
  for (let p = start; p <= end; p++) {
    result.push(p);
  }
  if (end < totalPages - 1) {
    result.push("…");
  }
  result.push(totalPages);
  return result;
}
