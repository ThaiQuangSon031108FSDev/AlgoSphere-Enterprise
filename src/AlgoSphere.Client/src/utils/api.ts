/**
 * Centralized API base URL.
 * - In development: Vite proxy forwards /api → localhost:5000
 * - In production: same origin (nginx handles routing)
 */
export const API_BASE = '/api/v1'

/**
 * Centralized SignalR hub URL.
 * Uses relative path so Vite proxy handles it in dev,
 * and nginx handles it in production.
 */
export const HUB_BASE = ''  // empty = same origin, e.g. /ws/arena
