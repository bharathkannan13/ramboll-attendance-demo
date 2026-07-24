"use client";

import { useState, useEffect } from 'react';
import { Card } from '@/components/ui/Card';
import { Button } from '@/components/ui/Button';
import { Input } from '@/components/ui/Input';
import { Badge } from '@/components/ui/Badge';

interface AttendanceRecord {
  id: string;
  username: string;
  date: string;
  firstSeen: string;
  lastSeen: string;
  totalHours: string;
  ipAddress: string;
  status: 'online' | 'offline';
}

export default function AdminDashboard() {
  const [records, setRecords] = useState<AttendanceRecord[]>([]);
  const [search, setSearch] = useState('');
  const [dateFilter, setDateFilter] = useState('');
  const [generatedLink, setGeneratedLink] = useState('');
  const [isGenerating, setIsGenerating] = useState(false);

  // Auto-refresh data
  useEffect(() => {
    const fetchData = async () => {
      try {
        const res = await fetch('/api/attendance');
        if (res.ok) {
          const data = await res.json();
          setRecords(data);
        }
      } catch (e) {
        console.error("Failed to fetch attendance data");
      }
    };

    fetchData(); // Initial fetch
    const interval = setInterval(fetchData, 3000); // Poll every 3s
    return () => clearInterval(interval);
  }, []);

  const handleGenerateLink = async () => {
    setIsGenerating(true);
    try {
      const res = await fetch('/api/sessions', { method: 'POST' });
      if (res.ok) {
        const data = await res.json();
        const origin = window.location.origin;
        setGeneratedLink(`${origin}/join/${data.code}`);
      }
    } catch (e) {
      console.error("Failed to generate link");
    } finally {
      setIsGenerating(false);
    }
  };

  const handleCopyLink = () => {
    navigator.clipboard.writeText(generatedLink);
  };

  const handleExport = () => {
    window.open('/api/attendance/export', '_blank');
  };

  const totalOnline = records.filter(r => r.status === 'online').length;
  const totalToday = records.length;
  const avgHours = records.length > 0 ? "4.2h" : "0h"; // Mock computation

  const filteredRecords = records.filter(record => 
    record.username.toLowerCase().includes(search.toLowerCase()) &&
    (dateFilter === '' || record.date === dateFilter)
  );

  return (
    <div className="space-y-6">
      <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
        <Card className="flex flex-col gap-2">
          <span className="text-gray-400 text-sm">Total Online</span>
          <span className="text-3xl font-bold text-emerald-400">{totalOnline}</span>
        </Card>
        <Card className="flex flex-col gap-2">
          <span className="text-gray-400 text-sm">Total Today</span>
          <span className="text-3xl font-bold text-white">{totalToday}</span>
        </Card>
        <Card className="flex flex-col gap-2">
          <span className="text-gray-400 text-sm">Avg Hours Today</span>
          <span className="text-3xl font-bold text-[#00f0ff]">{avgHours}</span>
        </Card>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        <Card className="lg:col-span-1 space-y-6">
          <div>
            <h3 className="text-lg font-medium mb-4">Session Management</h3>
            <Button onClick={handleGenerateLink} isLoading={isGenerating} className="w-full">
              Generate Attendance Link
            </Button>
            
            {generatedLink && (
              <div className="mt-4 p-3 bg-black/30 border border-white/10 rounded-lg flex items-center gap-2">
                <input 
                  type="text" 
                  readOnly 
                  value={generatedLink} 
                  className="bg-transparent text-sm text-gray-300 w-full outline-none"
                />
                <Button variant="secondary" onClick={handleCopyLink} className="!p-2">
                  Copy
                </Button>
              </div>
            )}
          </div>

          <hr className="border-white/10" />

          <div>
            <h3 className="text-lg font-medium mb-4">Filters</h3>
            <div className="space-y-4">
              <Input
                placeholder="Search username..."
                value={search}
                onChange={(e) => setSearch(e.target.value)}
              />
              <Input
                type="date"
                value={dateFilter}
                onChange={(e) => setDateFilter(e.target.value)}
              />
            </div>
          </div>

          <hr className="border-white/10" />

          <div className="space-y-3">
            <Button onClick={handleExport} variant="secondary" className="w-full">
              Export to Excel
            </Button>
            <Button variant="danger" className="w-full">
              Clear Demo Data
            </Button>
          </div>
        </Card>

        <Card className="lg:col-span-2 flex flex-col h-[600px]">
          <div className="flex justify-between items-center mb-4">
            <h3 className="text-lg font-medium">Live Attendance</h3>
            <span className="flex items-center gap-2 text-sm text-gray-400">
              <span className="w-2 h-2 rounded-full bg-emerald-500 animate-pulse-green" />
              Live Updates
            </span>
          </div>

          <div className="flex-1 overflow-auto rounded-lg border border-white/5 bg-black/20">
            <table className="w-full text-left text-sm whitespace-nowrap">
              <thead className="bg-white/5 sticky top-0 backdrop-blur-md">
                <tr>
                  <th className="p-4 font-medium text-gray-300">Username</th>
                  <th className="p-4 font-medium text-gray-300">Date</th>
                  <th className="p-4 font-medium text-gray-300">First Seen</th>
                  <th className="p-4 font-medium text-gray-300">Last Seen</th>
                  <th className="p-4 font-medium text-gray-300">Total Hours</th>
                  <th className="p-4 font-medium text-gray-300">IP Address</th>
                  <th className="p-4 font-medium text-gray-300">Status</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-white/5">
                {filteredRecords.length > 0 ? (
                  filteredRecords.map((record) => (
                    <tr key={record.id} className="hover:bg-white/5 transition-colors">
                      <td className="p-4 text-white">{record.username}</td>
                      <td className="p-4 text-gray-400">{record.date}</td>
                      <td className="p-4 text-gray-400">{record.firstSeen}</td>
                      <td className="p-4 text-gray-400">{record.lastSeen}</td>
                      <td className="p-4 text-gray-400">{record.totalHours}</td>
                      <td className="p-4 text-gray-500 font-mono text-xs">{record.ipAddress}</td>
                      <td className="p-4">
                        <Badge status={record.status}>
                          {record.status === 'online' ? 'Online' : 'Offline'}
                        </Badge>
                      </td>
                    </tr>
                  ))
                ) : (
                  <tr>
                    <td colSpan={7} className="p-8 text-center text-gray-500">
                      No attendance records found.
                    </td>
                  </tr>
                )}
              </tbody>
            </table>
          </div>
        </Card>
      </div>
    </div>
  );
}
