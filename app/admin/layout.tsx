"use client";

import { useEffect, useState } from 'react';
import { useRouter } from 'next/navigation';
import { Button } from '@/components/ui/Button';

export default function AdminLayout({ children }: { children: React.ReactNode }) {
  const router = useRouter();
  const [isChecking, setIsChecking] = useState(true);

  useEffect(() => {
    // Simple client-side auth check
    const checkAuth = async () => {
      try {
        const res = await fetch('/api/auth/login');
        if (!res.ok) {
          router.push('/');
        } else {
          setIsChecking(false);
        }
      } catch {
        router.push('/');
      }
    };
    checkAuth();
  }, [router]);

  if (isChecking) {
    return (
      <div className="min-h-screen flex items-center justify-center">
        <div className="w-8 h-8 border-4 border-[#0087b3] border-t-transparent rounded-full animate-spin"></div>
      </div>
    );
  }

  return (
    <div className="min-h-screen flex flex-col">
      <header className="glass-card sticky top-0 z-50 rounded-none border-t-0 border-l-0 border-r-0 px-6 py-4">
        <div className="max-w-7xl mx-auto flex items-center justify-between">
          <div className="flex items-center gap-4">
            <h1 className="text-xl font-bold tracking-wider text-transparent bg-clip-text bg-gradient-to-r from-white to-gray-400">
              RAMBOLL ATTENDANCE
            </h1>
            <span className="px-2 py-1 rounded text-xs font-medium bg-[#0087b3]/20 text-[#00f0ff] border border-[#0087b3]/30">
              ADMIN PORTAL
            </span>
          </div>
          <Button variant="ghost" onClick={() => router.push('/')} className="text-sm">
            Sign Out
          </Button>
        </div>
      </header>
      <main className="flex-1 w-full max-w-7xl mx-auto p-6">
        {children}
      </main>
    </div>
  );
}
