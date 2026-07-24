import { NextRequest, NextResponse } from 'next/server';
import { loginSchema } from '@/lib/validation';
import { prisma } from '@/lib/db';
import bcrypt from 'bcrypt';
import { signToken, setAuthCookie } from '@/lib/auth';

export async function POST(req: NextRequest) {
  try {
    const body = await req.json();
    const { username, password } = loginSchema.parse(body);

    const admin = await prisma.admin.findUnique({ where: { username } });
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
