import { onMounted, onBeforeUnmount } from "vue";

export function useLeavePageConfirm(message: string) {
  const handler = (event: BeforeUnloadEvent) => {
    event.preventDefault();
    event.returnValue = message; // Required for Chrome
    return message;
  };

  onMounted(() => {
      window.addEventListener("beforeunload", handler);
  });

  onBeforeUnmount(() => {
      window.removeEventListener("beforeunload", handler);
  });
}