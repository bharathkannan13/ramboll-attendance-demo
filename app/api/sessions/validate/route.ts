import { NextRequest, NextResponse } from 'next/server';
import { prisma } from '@/lib/db';

export async function GET(req: NextRequest) {
  try {
    const code = req.nextUrl.searchParams.get('code');
    if (!code) {
      return NextResponse.json({ error: 'Missing code parameter' }, { status: 400 });
    }

    const session = await prisma.attendanceSession.findUnique({
      where: { code }
    });

    if (!session || !session.isActive) {
      return NextResponse.json({ error: 'Session not found or inactive' }, { status: 404 });
    }

    return NextResponse.json({ id: session.id, code: session.code, isActive: session.isActive });
  } catch (error) {
    return NextResponse.json({ error: 'Internal server error' }, { status: 500 });
  }
}
