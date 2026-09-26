// ─────────────────────────────────────────────
// Imports
// ─────────────────────────────────────────────
import { createRouter, createWebHistory } from "vue-router";
//#if (useAuth)
import { useLoginStore } from "../store/loginStore";
//#endif

// ─────────────────────────────────────────────
// Route Definitions
// ─────────────────────────────────────────────
const routes = [
  //#if (useAuth)
  {
    path: "/login",
    name: "Login",
    component: () => import("../views/Login.vue"),
    children: [],
  },
  //#endif
  //#if (useLocalIdentity)
  {
    path: "/register",
    name: "Register",
    component: () => import("../views/Register.vue"),
    children: [],
  },
  {
    path: "/forgot-password",
    name: "ForgotPassword",
    component: () => import("../views/ForgotPassword.vue"),
    children: [],
  },
  {
    path: "/reset-password",
    name: "ResetPassword",
    component: () => import("../views/ResetPassword.vue"),
    children: [],
  },
  {
    path: "/change-password",
    name: "ChangePassword",
    component: () => import("../views/ChangePassword.vue"),
    meta: { requiresAuth: true },
    children: [],
  },
  //#endif
  {
    path: "/",
    name: "Home",
    component: () => import("../views/Home.vue"),
    //#if (useAuth)
    meta: { requiresAuth: true },
    //#endif
    children: [],
  },
  {
    path: "/attachments",
    name: "Attachments",
    component: () => import("../views/Attachments.vue"),
    //#if (useAuth)
    meta: { requiresAuth: true },
    //#endif
    children: [],
  },
  //#if (useAuth)
  {
    path: "/admin",
    name: "Admin",
    component: () => import("../views/Admin.vue"),
    meta: { requiresAuth: true, requiresAdmin: true },
    children: [],
  },
  //#endif
  // Help center: every topic renders the same Help.vue (the content is chosen
  // by path), so a new topic is one more path here plus its entries in
  // assets/help-content.ts and assets/help-sidebar.ts. The overview keeps the
  // name "Help" because the top nav links to it by name. Help pages are public
  // on purpose — someone who cannot sign in still needs to read them.
  ...[
    "/help",
    //#if (useAuth)
    "/help/getting-started",
    //#if (useLocalIdentity)
    "/help/getting-started/account",
    //#endif
    "/help/getting-started/signing-in",
    //#endif
    "/help/attachments",
    //#if (useAuth)
    "/help/admin",
    //#endif
    "/help/faq",
    "/help/contact",
    "/help/policies",
    "/help/terms-of-use",
    "/help/accessibility",
    "/help/privacy",
  ].map((path) => ({
    path,
    name:
      path === "/help"
        ? "Help"
        : "Help-" + path.slice("/help/".length).replace(/\//g, "-"),
    component: () => import("../views/Help.vue"),
    children: [],
  })),
];

// ─────────────────────────────────────────────
// Router Creation
// ─────────────────────────────────────────────
const router = createRouter({
  history: createWebHistory(import.meta.env.VITE_BASE_URL),
  scrollBehavior(_to, _from, _savedPosition) {
    // always scroll to top
    return { top: 0 };
  },
  routes,
});

// ─────────────────────────────────────────────
// Global Navigation Guards
// ─────────────────────────────────────────────
router.beforeEach((to, _from, next) => {
  const lowerCasePath = to.path.toLowerCase();

  // Step 1: Force lowercase path so routes are effectively case-insensitive.
  if (to.path !== lowerCasePath) {
    return next({ path: lowerCasePath, query: to.query, hash: to.hash });
  }

  //#if (useAuth)
  // Step 2: Check auth-required routes.
  const loginStore = useLoginStore();
  if (to.meta?.requiresAuth && !loginStore.isAuthenticated) {
    return next({ name: "Login" });
  }

  if (to.meta?.requiresAdmin && !loginStore.roles.includes("Admin")) {
    return next({ name: "Home" });
  }

  //#endif
  next(); // allow navigation
});

// ─────────────────────────────────────────────
// Export
// ─────────────────────────────────────────────
export default router;
