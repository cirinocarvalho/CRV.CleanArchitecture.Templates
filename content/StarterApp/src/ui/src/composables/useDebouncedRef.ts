import { ref, watch, onUnmounted, type Ref } from "vue";

/**
 * Returns a ref that mirrors `source` but only updates after `delayMs` of quiet.
 *
 * Use this for the query behind a table filter. Binding v-model straight to the
 * filter's computed re-runs filter + sort + paginate on every keystroke, which
 * on a few hundred rows means a full re-render per character typed. Keep
 * v-model on the raw ref so the input still echoes instantly, and drive the
 * expensive computed off the debounced copy.
 */
export function useDebouncedRef<T>(source: Ref<T>, delayMs = 200): Ref<T> {
  const debounced = ref(source.value) as Ref<T>;
  let timer: ReturnType<typeof setTimeout> | undefined;

  watch(source, (value) => {
    if (timer !== undefined) clearTimeout(timer);
    timer = setTimeout(() => {
      debounced.value = value;
    }, delayMs);
  });

  onUnmounted(() => {
    if (timer !== undefined) clearTimeout(timer);
  });

  return debounced;
}
