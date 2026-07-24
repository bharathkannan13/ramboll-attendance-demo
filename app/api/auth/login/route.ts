import { NextRequest, NextResponse } from 'next/server';
import { loginSchema } from '@/lib/validation';
import { prisma } from '@/lib/db';
import bcrypt from 'bcrypt';
import { signToken, setAuthCookie } from '@/lib/auth';

export async function POST(req: NextRequest) {
  try {
    const body = await req.json();
    const { username, password } = loginSchema.parse(body);

    let admin = await prisma.admin.findUnique({ where: { username } });

    const envUser = process.env.ADMIN_USERNAME || 'admin';
    const envPass = process.env.ADMIN_PASSWORD || 'RambollAdmin2026';

    // Self-healing fallback: auto-create Admin record if database is empty on first deployment
    if (!admin && username === envUser && password === envPass) {
      const hash = await bcrypt.hash(password, 10);
      admin = await prisma.admin.create({
        data: { username, passwordHash: hash }
      });
    }

    if (!admin || !(await bcrypt.compare(password, admin.passwordHash))) {
      return NextResponse.json({ error: 'Invalid credentials' }, { status: 401 });
    }

    const token = await signToken({ username: admin.username, adminId: admin.id });
    await setAuthCookie(token);

    return NextResponse.json({ success: true }, { status: 200 });
  } catch (error) {
    return NextResponse.json({ error: 'Invalid request' }, { status: 400 });
  }
}
