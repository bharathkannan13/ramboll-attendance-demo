import { NextRequest, NextResponse } from 'next/server';
import { prisma } from '@/lib/db';
import { requireAdmin } from '@/lib/auth';
import { formatDuration } from '@/lib/utils';
import * as ExcelJS from 'exceljs';

export async function GET(req: NextRequest) {
  try {
    await requireAdmin();
    const { searchParams } = new URL(req.url);
    const dateParam = searchParams.get('date');
    
    const where: any = {};
    if (dateParam) {
      const date = new Date(dateParam);
      date.setHours(0, 0, 0, 0);
      where.date = date;
    }

    const records = await prisma.attendanceRecord.findMany({
      where,
      orderBy: { username: 'asc' },
    });

    const workbook = new ExcelJS.Workbook();
    const worksheet = workbook.addWorksheet('Attendance');

    worksheet.columns = [
      { header: 'Username', key: 'username', width: 20 },
      { header: 'Date', key: 'date', width: 15 },
      { header: 'First Seen', key: 'firstSeen', width: 20 },
      { header: 'Last Seen', key: 'lastSeen', width: 20 },
      { header: 'Total Hours', key: 'totalHours', width: 15 },
      { header: 'IP Address', key: 'ipAddress', width: 20 },
      { header: 'Status', key: 'status', width: 15 },
    ];

    records.forEach(record => {
      worksheet.addRow({
        username: record.username,
        date: record.date.toISOString().split('T')[0],
        firstSeen: record.firstSeen.toLocaleTimeString(),
        lastSeen: record.lastSeen.toLocaleTimeString(),
        totalHours: formatDuration(record.totalMinutes),
        ipAddress: record.ipAddress || 'N/A',
        status: record.status,
      });
    });

    const buffer = await workbook.xlsx.writeBuffer();

    return new NextResponse(buffer, {
      headers: {
        'Content-Type': 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet',
        'Content-Disposition': 'attachment; filename="attendance.xlsx"',
      }
    });
  } catch (error) {
    return NextResponse.json({ error: 'Unauthorized' }, { status: 401 });
  }
}
