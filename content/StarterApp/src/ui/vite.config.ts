import { defineConfig, loadEnv } from "vite";
import vue from "@vitejs/plugin-vue";
import checker from "vite-plugin-checker";
import fs from "fs";
import path from "path";
import vueDevTools from "vite-plugin-vue-devtools";

// Local HTTPS is opt-in: drop a key/cert pair in src/ui/ssl and the dev server
// picks them up. Without them it serves over plain HTTP, so a freshly generated
// project runs with no extra setup.
const sslDir = path.resolve(__dirname, "ssl");
const keyPath = path.join(sslDir, "key.pem");
const certPath = path.join(sslDir, "cert.pem");
const hasLocalCerts = fs.existsSync(keyPath) && fs.existsSync(certPath);

// https://vitejs.dev/config/
export default defineConfig(({ mode }) => {
  const isDevOrProd = mode === "dev" || mode === "prod";
  const env = loadEnv(mode, process.cwd());

  return {
    plugins: [
      vue(),
      vueDevTools(),
      checker({
        vueTsc: true,
      }),
    ],
    base: env.VITE_BASE_URL || "",
    server: !isDevOrProd
      ? {
          port: 3000,
          https: hasLocalCerts
            ? {
                key: fs.readFileSync(keyPath),
                cert: fs.readFileSync(certPath),
              }
            : undefined,
          // Forward API calls to the ASP.NET host so the browser sees a single
          // origin and no CORS preflight is needed in local development.
          proxy: {
            "/api": {
              target: env.VITE_DEV_API_PROXY || "https://localhost:7170",
              changeOrigin: true,
              secure: false,
            },
          },
        }
      : undefined,
    css: {
      preprocessorOptions: {
        scss: {
          silenceDeprecations: [
            "import",
            "if-function",
            "global-builtin",
            "color-functions",
          ],
          // Vite 7 dropped the legacy Sass API, so this is the modern-API
          // spelling of what used to be `includePaths`.
          loadPaths: [
            path.resolve(__dirname, "node_modules"),
            path.resolve(__dirname, "node_modules/.pnpm/node_modules"),
            path.resolve(__dirname, "node_modules/bulma/sass"),
          ],
          additionalData: `
            @import "bulma/sass/utilities/_all.sass";
          `,
        },
      },
    },
  };
});
