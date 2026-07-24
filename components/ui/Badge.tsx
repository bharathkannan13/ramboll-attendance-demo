import React from 'react';

interface BadgeProps {
  status: 'online' | 'offline' | 'warning';
  children: React.ReactNode;
}

export function Badge({ status, children }: BadgeProps) {
  const styles = {
    online: "bg-emerald-500/10 text-emerald-400 border-emerald-500/20",
    offline: "bg-red-500/10 text-red-400 border-red-500/20",
    warning: "bg-amber-500/10 text-amber-400 border-amber-500/20"
  };

  const dots = {
    online: "bg-emerald-500 animate-pulse-green",
    offline: "bg-red-500",
    warning: "bg-amber-500"
  };

  return (
    <span className={`inline-flex items-center gap-1.5 px-2.5 py-1 rounded-full text-xs font-medium border ${styles[status]}`}>
      <span className={`w-1.5 h-1.5 rounded-full ${dots[status]}`} />
      {children}
    </span>
  );
}
