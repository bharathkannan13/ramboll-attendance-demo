import { NextRequest, NextResponse } from 'next/server';
import { prisma } from '@/lib/db';
import { requireAdmin } from '@/lib/auth';
import { generateSessionCode } from '@/lib/utils';
import { createSessionSchema } from '@/lib/validation';

export async function GET() {
  try {
    await requireAdmin();
    const sessions = await prisma.attendanceSession.findMany({
      orderBy: { createdAt: 'desc' }
    });
    return NextResponse.json(sessions);
  } catch (error) {
    return NextResponse.json({ error: 'Unauthorized' }, { status: 401 });
  }
}

export async function POST(req: NextRequest) {
  try {
    await requireAdmin();
    const body = await req.json().catch(() => ({}));
    const { expiresAt } = createSessionSchema.parse(body);

    const session = await prisma.attendanceSession.create({
      data: {
        code: generateSessionCode(),
        expiresAt: expiresAt ? new Date(expiresAt) : null,
      }
    });
    return NextResponse.json(session, { status: 201 });
  } catch (error) {
    return NextResponse.json({ error: 'Invalid request' }, { status: 400 });
  }
}
