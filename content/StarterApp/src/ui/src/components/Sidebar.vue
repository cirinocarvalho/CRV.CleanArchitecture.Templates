<script setup lang="ts">
import { ref, onMounted } from "vue";
import { useSidebarStore, type SidebarItem } from "../store/sidebarStore";

interface Props {
  sidebar?: string;
}

const props = withDefaults(defineProps<Props>(), {
  sidebar: "",
});

const sidebarStore = useSidebarStore();
const sidebarItems = ref<SidebarItem[] | null>(null);

function getCurrentSidebar(sidebar: string): SidebarItem[] {
  const found = sidebarStore.sidebars.find((sb) => sb.slug === sidebar);
  if (found) {
    return found.items;
  }
  return [];
}

onMounted(() => {
  sidebarItems.value = getCurrentSidebar(props.sidebar);
});
</script>
<template>
  <div id="sidebar" ref="sidebar">
    <template v-for="(link, _index) in sidebarItems" :key="`link-${_index}`">
      <div class="sidebar-links" :class="link.children ? 'has-sublinks' : ''">
        <ul>
          <li>
            <template v-if="link.href">
              <router-link :to="link.href">
                {{ link.text }}
              </router-link>
            </template>
            <ul v-if="link.children" class="sidebar-sublinks show">
              <li
                v-for="(subLink, index2) in link.children"
                :key="`sublink-${subLink.href}-${index2}`"
              >
                <router-link v-if="subLink.href" :to="subLink.href">
                  {{ subLink.text }}
                </router-link>
                <span v-else>{{ subLink.text }}</span>
              </li>
            </ul>
          </li>
        </ul>
      </div>
    </template>
  </div>
</template>
<style lang="scss" scoped>
#sidebar {
  width: 100%;
  .sidebar-links {
    .router-link-exact-active {
      background-color: $grey-dark;
      color: $white;
    }
    &:last-child {
      ul li {
        border-bottom: 0;
      }
    }
  }

  ul {
    margin: 0;
    list-style-type: none;
    li {
      margin: 0;
      padding: 0;
      border-bottom: 1px solid $grey-light;
      a {
        color: #0f4d90;
        font-weight: 600;
        font-size: 14px;
        padding: 0.5rem;
        display: block;
      }
      ul.sidebar-sublinks {
        display: none;
        li {
          margin-left: 0.5rem;
          border: 0;
        }
        &.show {
          display: block;
        }
      }
    }
  }
}
</style>
