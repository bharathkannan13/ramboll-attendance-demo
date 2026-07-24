import { NextRequest, NextResponse } from 'next/server';
import { requireAdmin } from '@/lib/auth';
import { prisma } from '@/lib/db';

export const dynamic = 'force-dynamic';

export async function GET(req: NextRequest) {
  try {
    await requireAdmin();
    
    const stream = new ReadableStream({
      async start(controller) {
        let active = true;

        req.signal.addEventListener('abort', () => {
          active = false;
        });

        while (active) {
          const now = new Date();
          const date = new Date();
          date.setHours(0, 0, 0, 0);

          // Update statuses before fetching
          await prisma.attendanceRecord.updateMany({
            where: {
              status: 'ONLINE',
              lastSeen: { lt: new Date(now.getTime() - 90000) }
            },
            data: { status: 'OFFLINE' }
          });

          const records = await prisma.attendanceRecord.findMany({
            where: { date },
            orderBy: { lastSeen: 'desc' }
          });

          const data = `data: ${JSON.stringify(records)}\n\n`;
          controller.enqueue(new TextEncoder().encode(data));

          await new Promise(resolve => setTimeout(resolve, 3000));
        }
      }
    });

    return new NextResponse(stream, {
      headers: {
        'Content-Type': 'text/event-stream',
        'Cache-Control': 'no-cache, no-transform',
        'Connection': 'keep-alive',
      },
    });
  } catch (error) {
    return NextResponse.json({ error: 'Unauthorized' }, { status: 401 });
  }
}
