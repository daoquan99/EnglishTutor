"use client";

import Link from "next/link";
import {
  Users,
  Brain,
  Activity,
  AlertTriangle,
  BookCheck,
  FileText,
  Inbox,
  Shield,
  Key,
  Server,
  Route,
} from "lucide-react";

const sections = [
  {
    label: "Users & Access",
    items: [
      { title: "Users", href: "/admin/users", icon: Users, desc: "Manage users and view stats", color: "bg-info/10 text-info" },
      { title: "Roles", href: "/admin/roles", icon: Shield, desc: "Role management", color: "bg-study/10 text-study" },
      { title: "Permissions", href: "/admin/permissions", icon: Key, desc: "Permission management", color: "bg-warning/10 text-warning" },
    ],
  },
  {
    label: "AI",
    items: [
      { title: "AI Usage", href: "/admin/ai-usage", icon: Brain, desc: "Token usage, latency & costs", color: "bg-speaking/10 text-speaking" },
      { title: "Providers", href: "/admin/ai-providers", icon: Server, desc: "AI provider configuration", color: "bg-assessment/10 text-assessment" },
      { title: "Routes", href: "/admin/ai-routes", icon: Route, desc: "AI route configuration", color: "bg-exercise/10 text-exercise" },
    ],
  },
  {
    label: "Reports",
    items: [
      { title: "Activity", href: "/admin/activity", icon: Activity, desc: "Learning activity reports", color: "bg-study/10 text-study" },
      { title: "Mistakes", href: "/admin/mistakes", icon: AlertTriangle, desc: "Common mistake patterns", color: "bg-destructive/10 text-destructive" },
      { title: "Assessments", href: "/admin/assessments", icon: BookCheck, desc: "Assessment pass rates", color: "bg-vocabulary/10 text-vocabulary" },
    ],
  },
  {
    label: "System",
    items: [
      { title: "Audit Logs", href: "/admin/audit-logs", icon: FileText, desc: "Admin action logs", color: "bg-muted text-muted-foreground" },
      { title: "Dead Letters", href: "/admin/dead-letters", icon: Inbox, desc: "Failed integration events", color: "bg-destructive/10 text-destructive" },
    ],
  },
];

export default function AdminDashboardPage() {
  return (
    <div className="page-container page-section">
      <div>
        <h1 className="font-heading text-2xl font-bold tracking-tight">Admin Dashboard</h1>
        <p className="mt-0.5 text-sm text-muted-foreground">
          System overview and management.
        </p>
      </div>

      <div className="mt-8 space-y-8">
        {sections.map((section) => (
          <div key={section.label}>
            <h2 className="mb-3 text-xs font-semibold uppercase tracking-wider text-muted-foreground">
              {section.label}
            </h2>
            <div className="grid gap-3 sm:grid-cols-2 lg:grid-cols-3">
              {section.items.map((item) => (
                <Link
                  key={item.href}
                  href={item.href}
                  className="group flex items-start gap-3 rounded-xl border bg-card/50 p-4 transition-colors hover:bg-card hover:shadow-soft"
                >
                  <div className={`flex size-9 shrink-0 items-center justify-center rounded-lg ${item.color}`}>
                    <item.icon className="size-4.5" />
                  </div>
                  <div>
                    <h3 className="text-sm font-semibold">{item.title}</h3>
                    <p className="mt-0.5 text-xs text-muted-foreground">{item.desc}</p>
                  </div>
                </Link>
              ))}
            </div>
          </div>
        ))}
      </div>
    </div>
  );
}
