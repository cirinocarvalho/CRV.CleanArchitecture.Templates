import { ref } from "vue";
import { defineStore } from "pinia";

function generateUrlSafeToken(length = 150): string {
  const chars =
    "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789-_.~";
  let result = "";
  for (let i = 0; i < length; i++) {
    result += chars.charAt(Math.floor(Math.random() * chars.length));
  }
  return result;
}

const STORAGE_KEY = "myLegalEntityStore";

function loadFromStorage() {
  try {
    const raw = sessionStorage.getItem(STORAGE_KEY);
    if (raw)
      return JSON.parse(raw) as Partial<{
        sessionToken: string;
      }>;
  } catch {
    /* ignore */
  }
  return null;
}

export const useAppStore = defineStore("app", () => {
  const saved = loadFromStorage();
  const sessionToken = ref<string>(saved?.sessionToken ?? "");

  function _persist() {
    try {
      sessionStorage.setItem(
        STORAGE_KEY,
        JSON.stringify({
          sessionToken: sessionToken.value,
        }),
      );
    } catch {
      /* ignore */
    }
  }

  function generateAndSetSessionToken() {
    sessionToken.value = generateUrlSafeToken();
    _persist();
  }

  return {
    sessionToken,
    generateAndSetSessionToken,
  };
});
