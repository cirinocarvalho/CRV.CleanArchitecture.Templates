// ─────────────────────────────────────────────
// Imports
// ─────────────────────────────────────────────
import { defineStore } from "pinia";
import api from "../services/api";
import { getErrorMessage } from "../utils/errorMessage";
import {
  storeAuth,
  clearAuth,
  isAuthenticated,
  getStoredUser,
  storeProfile,
  getStoredProfile,
  type UserProfile,
} from "../services/auth";

// ─────────────────────────────────────────────
// Login Store
// ─────────────────────────────────────────────
export const useLoginStore = defineStore("login", {
  state: () => {
    const stored = getStoredUser();
    return {
      isAuthenticated: isAuthenticated(),
      email: stored?.email ?? (null as string | null),
      roles: stored?.roles ?? ([] as string[]),
      profile: getStoredProfile() as UserProfile | null,
      error: null as string | null,
    };
  },

  actions: {
    async signIn(credentials: {
      email: string;
      password: string;
    }): Promise<boolean> {
      try {
        const { data } = await api.post<{
          token: string;
          email: string;
          roles: string[];
        }>("/v1/Auth/login", credentials);
        storeAuth(data);
        this.isAuthenticated = true;
        this.email = data.email;
        this.roles = data.roles;
        this.error = null;
        await this.loadProfile();
        return true;
      } catch (err: unknown) {
        this.error = getErrorMessage(err, "Invalid email or password.");
        return false;
      }
    },

    signOut() {
      clearAuth();
      this.isAuthenticated = false;
      this.email = null;
      this.roles = [];
      this.profile = null;
      this.error = null;
    },

    async loadProfile(): Promise<void> {
      if (!this.email) return;
      try {
        const { data } = await api.get<UserProfile>("/v1/Profile/details");
        const profile: UserProfile = {
          identityUserId: data.identityUserId ?? "",
          email: data.email ?? "",
          fullName: data.fullName ?? "",
          company: data.company ?? "",
          roles: data.roles ?? [],
        };
        this.profile = profile;
        storeProfile(profile);
      } catch {
        // Profile lookup is non-fatal; leave profile unset.
      }
    },

    async register(payload: {
      fullName: string;
      email: string;
      password: string;
    }): Promise<boolean> {
      try {
        await api.post("/v1/Auth/register", payload);
        return true;
      } catch (err: unknown) {
        this.error = getErrorMessage(err, "Registration failed.");
        return false;
      }
    },

    async forgotPassword(payload: { email: string }): Promise<void> {
      try {
        await api.post("/v1/Auth/forgot-password", payload);
      } catch {
        // Silently ignore — backend always returns 200 (anti-enumeration)
      }
    },

    async resetPassword(payload: {
      email: string;
      token: string;
      newPassword: string;
    }): Promise<boolean> {
      try {
        await api.post("/v1/Auth/reset-password", payload);
        this.error = null;
        return true;
      } catch (err: unknown) {
        this.error = getErrorMessage(
          err,
          "Password reset failed. The link may be invalid or expired.",
        );
        return false;
      }
    },

    async changePassword(payload: {
      currentPassword: string;
      newPassword: string;
    }): Promise<boolean> {
      try {
        await api.post("/v1/Auth/change-password", payload);
        this.error = null;
        return true;
      } catch (err: unknown) {
        this.error = getErrorMessage(err, "Unable to change password.");
        return false;
      }
    },
  },
});
