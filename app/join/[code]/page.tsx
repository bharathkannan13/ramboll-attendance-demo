"use client";

import { useState, useEffect, use } from 'react';
import { Card } from '@/components/ui/Card';
import { Button } from '@/components/ui/Button';
import { Input } from '@/components/ui/Input';

type PortalState = 'LOADING' | 'USERNAME_FORM' | 'WELCOME' | 'ACCESS_DENIED' | 'INVALID';

export default function EmployeePortal({ params }: { params: Promise<{ code: string }> }) {
  const { code } = use(params);
  const [state, setState] = useState<PortalState>('LOADING');
  const [username, setUsername] = useState('');
  const [isSubmitting, setIsSubmitting] = useState(false);

  useEffect(() => {
    const validateCode = async () => {
      try {
        const res = await fetch(`/api/sessions/validate?code=${code}`);
        if (res.ok) {
          setState('USERNAME_FORM');
        } else {
          setState('INVALID');
        }
      } catch {
        setState('INVALID');
      }
    };
    validateCode();
  }, [code]);

  useEffect(() => {
    if (state !== 'WELCOME') return;

    const sendHeartbeat = async () => {
      try {
        const res = await fetch('/api/attendance/heartbeat', {
          method: 'POST',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify({ code, username }),
        });
        
        if (!res.ok) {
          setState('ACCESS_DENIED');
        }
      } catch {
        setState('ACCESS_DENIED');
      }
    };

    const interval = setInterval(sendHeartbeat, 30000); // 30 seconds
    return () => clearInterval(interval);
  }, [state, code, username]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!username.trim()) return;
    
    setIsSubmitting(true);
    try {
      const res = await fetch('/api/attendance/heartbeat', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ code, username }),
      });
      
      if (res.ok) {
        setState('WELCOME');
      } else {
        setState('ACCESS_DENIED');
      }
    } catch {
      setState('ACCESS_DENIED');
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <div className="min-h-screen flex items-center justify-center p-4 bg-[#0a0e1a]">
      {state === 'LOADING' && (
        <div className="text-center space-y-4">
          <div className="w-12 h-12 border-4 border-[#0087b3] border-t-transparent rounded-full animate-spin mx-auto"></div>
          <p className="text-gray-400">Validating session...</p>
        </div>
      )}

      {state === 'USERNAME_FORM' && (
        <Card className="w-full max-w-md bg-black/40 backdrop-blur-xl border-white/5 shadow-2xl">
          <div className="text-center mb-8">
            <h1 className="text-2xl font-bold tracking-wider text-transparent bg-clip-text bg-gradient-to-r from-white to-gray-400">
              RAMBoll Attendance
            </h1>
          </div>
          <form onSubmit={handleSubmit} className="space-y-6">
            <Input
              label="Enter Your Username"
              value={username}
              onChange={(e) => setUsername(e.target.value)}
              placeholder="e.g. jdoe"
              required
              autoFocus
            />
            <Button type="submit" className="w-full" isLoading={isSubmitting}>
              Enter
            </Button>
          </form>
        </Card>
      )}

      {state === 'WELCOME' && (
        <Card className="w-full max-w-lg text-center bg-black/40 border-emerald-500/20 shadow-[0_0_50px_rgba(16,185,129,0.1)]">
          <div className="w-20 h-20 bg-emerald-500/20 rounded-full flex items-center justify-center mx-auto mb-6">
            <div className="w-4 h-4 bg-emerald-500 rounded-full animate-pulse-green"></div>
          </div>
          <h1 className="text-2xl font-bold text-white mb-2">Welcome to RAMBoll Attendance Portal</h1>
          <h2 className="text-xl text-emerald-400 mb-6">Welcome, {username}</h2>
          
          <div className="space-y-4 text-gray-300 bg-white/5 p-6 rounded-lg border border-white/5">
            <p className="flex items-center justify-center gap-2 font-medium text-white">
              <span>✅</span> Attendance Started Successfully
            </p>
            <p>You are connected to the authorized network.</p>
            <p className="text-sm text-emerald-400 bg-emerald-500/10 py-2 px-4 rounded-md inline-block">
              Please keep this page open while working.
            </p>
          </div>
        </Card>
      )}

      {state === 'ACCESS_DENIED' && (
        <Card className="w-full max-w-md text-center bg-black/40 border-red-500/20 shadow-[0_0_50px_rgba(239,68,68,0.1)]">
          <div className="text-5xl mb-6">⚠️</div>
          <h1 className="text-2xl font-bold text-red-400 mb-4">Access Denied</h1>
          <p className="text-gray-300 mb-4">Please connect to the authorized RAMBoll network.</p>
          <div className="bg-white/5 p-4 rounded-lg border border-white/10 mt-6">
            <p className="text-sm text-gray-400">Current authorized SSID:</p>
            <p className="font-mono text-white mt-1">Samsung Galaxy S25 Ultra</p>
          </div>
        </Card>
      )}

      {state === 'INVALID' && (
        <Card className="w-full max-w-md text-center bg-black/40">
          <div className="text-5xl mb-6 opacity-50">🔗</div>
          <h1 className="text-xl font-bold text-white mb-2">Invalid Link</h1>
          <p className="text-gray-400">This attendance link is invalid or has expired.</p>
        </Card>
      )}
    </div>
  );
}
