import { NextRequest, NextResponse } from 'next/server';
import { prisma } from '@/lib/db';
import { requireAdmin } from '@/lib/auth';

export async function GET(
  req: NextRequest,
  { params }: { params: Promise<{ id: string }> }
) {
  try {
    // Public endpoint to validate session code
    const code = (await params).id;
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

export async function DELETE(
  req: NextRequest,
  { params }: { params: Promise<{ id: string }> }
) {
  try {
    await requireAdmin();
    const code = (await params).id;
    
    await prisma.attendanceSession.update({
      where: { code },
      data: { isActive: false }
    });

    return NextResponse.json({ success: true });
  } catch (error) {
    return NextResponse.json({ error: 'Unauthorized or not found' }, { status: 401 });
  }
}
