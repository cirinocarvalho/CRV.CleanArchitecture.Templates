import { onBeforeRouteLeave } from "vue-router";

export function useRouteLeaveConfirm(message: string) {
  onBeforeRouteLeave((_to, _from, next) => {
    if (confirm(message)) {
      next(); // allow navigation
    } else {
      next(false); // cancel navigation
    }
  });
}