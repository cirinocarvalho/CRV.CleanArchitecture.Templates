import axios from "axios";

// ─────────────────────────────────────────────
// User-friendly error messages
// ─────────────────────────────────────────────
// Single source of truth for turning any thrown error (Axios response, network
// failure, timeout, or an unexpected JS error) into a clear message we can show
// the user. Use this everywhere instead of a bare "Something went wrong."
//
// Resolution order:
//   1. A specific message the API returned in the response body — the server
//      knows best, so a real validation/business message always wins.
//   2. A friendly message derived from the situation (offline, timeout, or the
//      HTTP status code).
//   3. The caller's fallback, for the rare case nothing else applies.

/** Last-resort message when we can't say anything more specific. */
export const GENERIC_ERROR_MESSAGE =
  "Something unexpected happened. Please try again in a few moments, and contact support if the problem continues.";

/** Pulls a non-empty message string out of a response body, if the API sent one. */
function serverMessage(error: unknown): string | undefined {
  const data = (error as { response?: { data?: unknown } })?.response?.data;

  if (typeof data === "string") {
    const trimmed = data.trim();
    return trimmed.length > 0 && !trimmed.startsWith("<") ? trimmed : undefined;
  }

  const message = (data as { message?: unknown })?.message;
  if (typeof message === "string" && message.trim().length > 0) {
    return message.trim();
  }

  // ASP.NET ProblemDetails / ModelState style responses.
  const title = (data as { title?: unknown })?.title;
  if (typeof title === "string" && title.trim().length > 0) {
    return title.trim();
  }

  return undefined;
}

/** Friendly text for a given HTTP status code, or undefined if we have none. */
function messageForStatus(status: number): string | undefined {
  if (status >= 500) {
    return "The server ran into a problem completing your request. Please try again in a few moments.";
  }
  switch (status) {
    case 400:
      return "Some of the information provided wasn't valid. Please review your entries and try again.";
    case 401:
      return "Your session has expired. Please sign in again to continue.";
    case 403:
      return "You don't have permission to perform this action.";
    case 404:
      return "We couldn't find what you were looking for. It may have been moved or removed.";
    case 408:
      return "The request took too long. Please try again.";
    case 409:
      return "This action conflicts with the current data. Please refresh the page and try again.";
    case 413:
      return "The file or data you're sending is too large. Please try a smaller one.";
    case 422:
      return "Some of the information provided wasn't valid. Please review your entries and try again.";
    case 429:
      return "You've made too many requests in a short time. Please wait a moment and try again.";
    default:
      return undefined;
  }
}

/**
 * Returns a user-friendly message for any error.
 *
 * @param error    The caught error (Axios error, network error, or anything).
 * @param fallback Optional context-specific fallback used only when we can't
 *                 derive anything more specific. Defaults to a generic message.
 */
export function getErrorMessage(
  error: unknown,
  fallback: string = GENERIC_ERROR_MESSAGE,
): string {
  // 1. Prefer a real message from the API.
  const fromServer = serverMessage(error);
  if (fromServer) return fromServer;

  if (axios.isAxiosError(error)) {
    // 2a. No response at all — the request never reached the server.
    if (!error.response) {
      if (error.code === "ECONNABORTED" || /timeout/i.test(error.message)) {
        return "The request took too long to complete. Please check your connection and try again.";
      }
      return "We couldn't reach the server. Please check your internet connection and try again.";
    }

    // 2b. Map the status code to something a person can understand.
    const fromStatus = messageForStatus(error.response.status);
    if (fromStatus) return fromStatus;
  }

  // 3. Nothing more specific — use the caller's fallback.
  return fallback;
}
