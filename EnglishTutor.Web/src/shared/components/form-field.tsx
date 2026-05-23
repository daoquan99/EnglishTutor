import { Label } from "@/shared/components/ui/label"
import { cn } from "@/shared/lib/utils"

interface FormFieldProps {
  label: string
  htmlFor: string
  error?: string
  description?: string
  children: React.ReactNode
  className?: string
  labelExtra?: React.ReactNode
}

function FormField({
  label,
  htmlFor,
  error,
  description,
  children,
  className,
  labelExtra,
}: FormFieldProps) {
  return (
    <div className={cn("grid gap-2", className)}>
      <div className="flex items-center justify-between">
        <Label htmlFor={htmlFor}>{label}</Label>
        {labelExtra}
      </div>
      {children}
      {error && <p className="text-xs text-destructive">{error}</p>}
      {!error && description && (
        <p className="text-xs text-muted-foreground">{description}</p>
      )}
    </div>
  )
}

export { FormField }
