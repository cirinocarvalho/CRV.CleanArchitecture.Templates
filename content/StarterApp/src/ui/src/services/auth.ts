// ─────────────────────────────────────────────
// Types
// ─────────────────────────────────────────────
export interface AuthResponse {
  token: string;
  email: string;
  roles: string[];
}

export interface UserProfile {
  identityUserId: string;
  email: string;
  fullName: string;
  company: string;
  roles: string[];
}

// ─────────────────────────────────────────────
// Session Storage Keys
// ─────────────────────────────────────────────
const TOKEN_KEY = 'starterapp_auth_token';
const USER_KEY  = 'starterapp_auth_user';
const PROFILE_KEY = 'starterapp_auth_profile';

// ─────────────────────────────────────────────
// Token Accessors
// ─────────────────────────────────────────────
export function getStoredToken(): string | null {
  return sessionStorage.getItem(TOKEN_KEY);
}

export function storeAuth(auth: AuthResponse): void {
  sessionStorage.setItem(TOKEN_KEY, auth.token);
  sessionStorage.setItem(USER_KEY, JSON.stringify({ email: auth.email, roles: auth.roles }));
}

export function clearAuth(): void {
  sessionStorage.removeItem(TOKEN_KEY);
  sessionStorage.removeItem(USER_KEY);
  sessionStorage.removeItem(PROFILE_KEY);
}

export function getStoredUser(): { email: string; roles: string[] } | null {
  const raw = sessionStorage.getItem(USER_KEY);
  return raw ? JSON.parse(raw) : null;
}

export function storeProfile(profile: UserProfile): void {
  sessionStorage.setItem(PROFILE_KEY, JSON.stringify(profile));
}

export function getStoredProfile(): UserProfile | null {
  const raw = sessionStorage.getItem(PROFILE_KEY);
  return raw ? JSON.parse(raw) : null;
}

export function isAuthenticated(): boolean {
  return !!getStoredToken();
}

// Reads the JWT `exp` claim (seconds) and returns it as epoch milliseconds.
function decodeTokenExpiry(token: string): number | null {
  try {
    const payload = token.split(".")[1];
    if (!payload) return null;
    const base64 = payload.replace(/-/g, "+").replace(/_/g, "/");
    const claims = JSON.parse(atob(base64));
    return typeof claims.exp === "number" ? claims.exp * 1000 : null;
  } catch {
    return null;
  }
}

// True when a token is stored and its expiry has passed.
export function isTokenExpired(): boolean {
  const token = getStoredToken();
  if (!token) return false;
  const expiry = decodeTokenExpiry(token);
  return expiry !== null && Date.now() >= expiry;
}
