import { z } from 'zod';

export const loginSchema = z.object({
  username: z.string().min(1, "Username is required"),
  password: z.string().min(1, "Password is required"),
});

export const createSessionSchema = z.object({
  expiresAt: z.string().datetime().optional().nullable(),
});

export const heartbeatSchema = z.object({
  code: z.string().length(8, "Session code must be 8 characters"),
  username: z.string().min(2, "Username must be at least 2 characters"),
});

export const attendanceQuerySchema = z.object({
  date: z.string().optional(),
  search: z.string().optional(),
});
