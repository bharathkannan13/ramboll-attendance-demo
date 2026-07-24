import { NextRequest, NextResponse } from 'next/server';
import { prisma } from '@/lib/db';
import { requireAdmin } from '@/lib/auth';

export async function GET(req: NextRequest) {
  try {
    await requireAdmin();
    const { searchParams } = new URL(req.url);
    const dateParam = searchParams.get('date');
    const search = searchParams.get('search') || '';

    const where: any = {};
    
    if (dateParam) {
      const date = new Date(dateParam);
      date.setHours(0, 0, 0, 0);
      where.date = date;
    }

    if (search) {
      where.username = { contains: search, mode: 'insensitive' };
    }

    const records = await prisma.attendanceRecord.findMany({
      where,
      orderBy: { lastSeen: 'desc' },
      include: { session: true }
    });

    // Timeout logic: older than 90 seconds -> OFFLINE
    const now = new Date();
    const updatedRecords = await Promise.all(records.map(async (record) => {
      if (record.status === 'ONLINE' && now.getTime() - record.lastSeen.getTime() > 90000) {
        return await prisma.attendanceRecord.update({
          where: { id: record.id },
          data: { status: 'OFFLINE' },
          include: { session: true }
        });
      }
      return record;
    }));

    return NextResponse.json(updatedRecords);
  } catch (error) {
    return NextResponse.json({ error: 'Unauthorized' }, { status: 401 });
  }
}
