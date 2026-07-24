import { NextRequest, NextResponse } from 'next/server';
import { prisma } from '@/lib/db';
import { heartbeatSchema } from '@/lib/validation';
import { calculateTotalMinutes } from '@/lib/utils';

export async function POST(req: NextRequest) {
  try {
    const body = await req.json();
    const { code, username } = heartbeatSchema.parse(body);

    const session = await prisma.attendanceSession.findUnique({
      where: { code }
    });

    if (!session || !session.isActive) {
      return NextResponse.json({ error: 'Invalid or inactive session' }, { status: 404 });
    }

    const ipAddress = req.headers.get('x-forwarded-for') || req.headers.get('x-real-ip') || null;
    const userAgent = req.headers.get('user-agent') || null;
    const now = new Date();
    const date = new Date();
    date.setHours(0, 0, 0, 0);

    let record = await prisma.attendanceRecord.findFirst({
      where: { sessionId: session.id, username, date }
    });

    if (record) {
      const totalMinutes = calculateTotalMinutes(record.firstSeen, now);
      record = await prisma.attendanceRecord.update({
        where: { id: record.id },
        data: {
          lastSeen: now,
          totalMinutes,
          ipAddress,
          userAgent,
          status: 'ONLINE',
        }
      });
    } else {
      record = await prisma.attendanceRecord.create({
        data: {
          sessionId: session.id,
          username,
          date,
          firstSeen: now,
          lastSeen: now,
          ipAddress,
          userAgent,
          status: 'ONLINE',
        }
      });
    }

    return NextResponse.json({ success: true, record }, { status: 200 });
  } catch (error) {
    return NextResponse.json({ error: 'Invalid request' }, { status: 400 });
  }
}
