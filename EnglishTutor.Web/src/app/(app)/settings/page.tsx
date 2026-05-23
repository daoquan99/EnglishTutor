"use client";

import { Settings } from "lucide-react";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/shared/components/ui/tabs";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/shared/components/ui/card";
import { PageHeader } from "@/shared/components/page-header";
import { ProfileForm } from "@/features/users/components/profile-form";
import { LanguageSettingsForm } from "@/features/users/components/language-settings-form";
import { TargetLanguageList } from "@/features/users/components/target-language-list";
import { AddTargetLanguageDialog } from "@/features/users/components/add-target-language-dialog";
import { NotificationSettingsForm } from "@/features/notifications/components/notification-settings-form";

export default function SettingsPage() {
  return (
    <div className="page-container page-section">
      <PageHeader
        icon={Settings}
        title="Settings"
        description="Manage your profile, languages, and notifications."
      />

      <Tabs defaultValue="profile" className="mt-6">
        <TabsList>
          <TabsTrigger value="profile">Profile</TabsTrigger>
          <TabsTrigger value="languages">Languages</TabsTrigger>
          <TabsTrigger value="notifications">Notifications</TabsTrigger>
        </TabsList>

        <TabsContent value="profile">
          <Card className="mt-4">
            <CardHeader>
              <CardTitle>Profile</CardTitle>
              <CardDescription>Update your display name and bio.</CardDescription>
            </CardHeader>
            <CardContent>
              <ProfileForm />
            </CardContent>
          </Card>
        </TabsContent>

        <TabsContent value="languages">
          <div className="mt-4 grid gap-4">
            <Card>
              <CardHeader>
                <CardTitle>Language settings</CardTitle>
                <CardDescription>
                  Set your native, UI, and explanation languages.
                </CardDescription>
              </CardHeader>
              <CardContent>
                <LanguageSettingsForm />
              </CardContent>
            </Card>

            <Card>
              <CardHeader className="flex-row items-center justify-between">
                <div>
                  <CardTitle>Target languages</CardTitle>
                  <CardDescription>Languages you are learning.</CardDescription>
                </div>
                <AddTargetLanguageDialog />
              </CardHeader>
              <CardContent>
                <TargetLanguageList />
              </CardContent>
            </Card>
          </div>
        </TabsContent>

        <TabsContent value="notifications">
          <Card className="mt-4">
            <CardHeader>
              <CardTitle>Notification preferences</CardTitle>
              <CardDescription>
                Choose which notifications you want to receive.
              </CardDescription>
            </CardHeader>
            <CardContent>
              <NotificationSettingsForm />
            </CardContent>
          </Card>
        </TabsContent>
      </Tabs>
    </div>
  );
}
