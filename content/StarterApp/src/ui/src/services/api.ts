// ─────────────────────────────────────────────
// Imports
// ─────────────────────────────────────────────
import axios from "axios";
//#if (useAuth)
import { getStoredToken } from "./auth";
import router from "../router/index";
import { useLoginStore } from "../store/loginStore";
//#endif

// ─────────────────────────────────────────────
// API Instance
// ─────────────────────────────────────────────
const api = axios.create({
  baseURL: import.meta.env.VITE_API_URL,
});

//#if (useAuth)
// ─────────────────────────────────────────────
// Interceptors
// ─────────────────────────────────────────────
api.interceptors.request.use((config) => {
  const token = getStoredToken();
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

// On 401 from any authorized call (expired/invalid token), sign out and return
// to Login so the app doesn't hang on a session that can no longer authorize.
// Skipped while already on the Login page so a failed login-flow call (e.g. the
// best-effort profile lookup) can't tear down the session being established.
api.interceptors.response.use(
  (response) => response,
  (error) => {
    const onLoginPage = router.currentRoute.value.name === "Login";
    if (error.response?.status === 401 && !onLoginPage) {
      useLoginStore().signOut();
      router.push({ name: "Login" });
    }
    return Promise.reject(error);
  },
);

//#endif
// ─────────────────────────────────────────────
// Export
// ─────────────────────────────────────────────
export default api;
