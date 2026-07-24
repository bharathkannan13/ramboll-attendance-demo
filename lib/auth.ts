import { jwtVerify, SignJWT } from 'jose';
import { cookies } from 'next/headers';

const SECRET_KEY = new TextEncoder().encode(process.env.JWT_SECRET || 'super-secret-default-key-change-in-prod');

export async function signToken(payload: { username: string; adminId: string }) {
  return await new SignJWT(payload)
    .setProtectedHeader({ alg: 'HS256' })
    .setIssuedAt()
    .setExpirationTime('24h')
    .sign(SECRET_KEY);
}

export async function verifyToken(token: string) {
  try {
    const { payload } = await jwtVerify(token, SECRET_KEY);
    return payload as { username: string; adminId: string };
  } catch (err) {
    return null;
  }
}

export async function getSession() {
  const cookieStore = await cookies();
  const token = cookieStore.get('auth_token')?.value;
  if (!token) return null;
  return await verifyToken(token);
}

export async function requireAdmin() {
  const session = await getSession();
  if (!session) {
    throw new Error('Unauthorized');
  }
  return session;
}

export async function setAuthCookie(token: string) {
  const cookieStore = await cookies();
  cookieStore.set({
    name: 'auth_token',
    value: token,
    httpOnly: true,
    secure: process.env.NODE_ENV === 'production',
    sameSite: 'lax',
    path: '/',
    maxAge: 86400,
  });
}

export async function clearAuthCookie() {
  const cookieStore = await cookies();
  cookieStore.delete('auth_token');
}
