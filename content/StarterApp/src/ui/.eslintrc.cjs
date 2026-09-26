// ─────────────────────────────────────────────
// ESLint configuration (ESLint 8, legacy .eslintrc format)
// ─────────────────────────────────────────────
// This project is installed with pnpm, whose strict node_modules layout does
// not hoist transitive packages to the project root. ESLint (and Babel) can't
// find those parsers by bare name, so we resolve them explicitly below.
//
// We intentionally avoid a full @typescript-eslint toolchain: `vue-tsc`
// (npm run typecheck) already does type-aware checking. Here we only need
// ESLint to *parse* our TypeScript so lint rules can run. @babel/eslint-parser
// with the TypeScript syntax plugin does exactly that, using packages already
// present in the pnpm store.

const fs = require("fs");
const path = require("path");
const { createRequire } = require("module");

// vue-eslint-parser is a dependency of eslint-plugin-vue; resolve it from that
// package's context rather than the (un-hoisted) project root.
const vueEslintParser = createRequire(
  require.resolve("eslint-plugin-vue"),
).resolve("vue-eslint-parser");

// @babel/plugin-syntax-typescript ships transitively (via vite-plugin-vue-
// devtools) and stays in the store across installs, but isn't hoisted. Locate
// it in node_modules/.pnpm by name so the path survives version bumps.
function resolveSyntaxTypescript() {
  const pnpmDir = path.join(__dirname, "node_modules", ".pnpm");
  const dir = fs
    .readdirSync(pnpmDir)
    .find((d) => d.startsWith("@babel+plugin-syntax-typescript@"));
  if (!dir) {
    throw new Error(
      "@babel/plugin-syntax-typescript not found in node_modules/.pnpm — run `pnpm install`.",
    );
  }
  return path.join(
    pnpmDir,
    dir,
    "node_modules",
    "@babel",
    "plugin-syntax-typescript",
  );
}

const syntaxTypescript = resolveSyntaxTypescript();

// Babel parser options shared by .ts files and <script lang="ts"> blocks.
const babelOptions = {
  requireConfigFile: false,
  babelOptions: {
    plugins: [[syntaxTypescript, { dts: false }]],
  },
};

module.exports = {
  root: true,
  env: { browser: true, es2022: true, node: true },
  extends: ["eslint:recommended"],
  parserOptions: { ecmaVersion: 2022, sourceType: "module" },
  // Declaration files use ambient syntax we don't lint; dist/node_modules are
  // build output / vendored code.
  ignorePatterns: ["dist/", "node_modules/", "*.d.ts"],
  rules: {
    // vue-tsc reports undefined names and unused locals with full type
    // awareness. The Babel parser here can't see types, so leaving these on
    // would flag type-only imports/usages as false positives.
    "no-unused-vars": "off",
    "no-undef": "off",
  },
  overrides: [
    {
      files: ["*.ts", "*.tsx"],
      parser: "@babel/eslint-parser",
      parserOptions: babelOptions,
    },
    {
      files: ["*.vue"],
      // NOTE: We use plugin:vue/base (parser + template parsing) rather than
      // vue3-essential. eslint-plugin-vue's <script setup> macro analysis
      // (defineProps<...> / defineEmits<...>) requires the @typescript-eslint
      // parser AST; with the Babel parser it throws. @typescript-eslint isn't
      // installable here (offline). Below we re-enable the high-value
      // template-body rules, which don't touch that script-setup analysis.
      extends: ["plugin:vue/base"],
      parser: vueEslintParser,
      parserOptions: {
        parser: "@babel/eslint-parser",
        ...babelOptions,
      },
      rules: {
        "vue/require-v-for-key": "error",
        "vue/valid-v-for": "error",
        "vue/no-use-v-if-with-v-for": "error",
        "vue/no-dupe-v-else-if": "error",
        "vue/no-template-key": "error",
        "vue/no-textarea-mustache": "error",
        "vue/valid-template-root": "error",
        "vue/valid-v-if": "error",
        "vue/valid-v-else": "error",
        "vue/valid-v-else-if": "error",
        "vue/valid-v-bind": "error",
        "vue/valid-v-on": "error",
        "vue/valid-v-model": "error",
        "vue/valid-v-slot": "error",
        "vue/no-parsing-error": "error",
      },
    },
  ],
};
