import { NextResponse } from 'next/server';
import type { NextRequest } from 'next/server';
import { verifyToken } from './lib/auth';

export async function middleware(request: NextRequest) {
  const { pathname } = request.nextUrl;

  // Public paths
  if (
    pathname === '/' ||
    pathname.startsWith('/join') ||
    pathname === '/api/auth/login' ||
    pathname === '/api/attendance/heartbeat' ||
    pathname.startsWith('/_next') ||
    pathname.startsWith('/favicon.ico')
  ) {
    return NextResponse.next();
  }

  // Check if it's a protected route
  const isApi = pathname.startsWith('/api/');
  const isAdminPage = pathname.startsWith('/admin');

  if (isApi || isAdminPage) {
    const token = request.cookies.get('auth_token')?.value;
    const payload = token ? await verifyToken(token) : null;

    if (!payload) {
      if (isApi) {
        return NextResponse.json({ error: 'Unauthorized' }, { status: 401 });
      } else {
        return NextResponse.redirect(new URL('/', request.url));
      }
    }
  }

  return NextResponse.next();
}

export const config = {
  matcher: ['/((?!_next/static|_next/image|favicon.ico).*)'],
};
