import { ref } from "vue";
import { defineStore } from "pinia";

export interface SidebarItem {
  href?: string;
  text: string;
  children?: SidebarItem[];
}

export interface Sidebar {
  slug: string;
  items: SidebarItem[];
}

export const useSidebarStore = defineStore("sidebar", () => {
  const sidebars = ref<Sidebar[]>([]);

  function setSidebars(data: Sidebar[]) {
    sidebars.value = data;
  }

  function addSidebar(sidebar: Sidebar) {
    const existing = sidebars.value.findIndex((sb) => sb.slug === sidebar.slug);
    if (existing !== -1) {
      sidebars.value[existing] = sidebar;
    } else {
      sidebars.value.push(sidebar);
    }
  }

  function getSidebarBySlug(slug: string): Sidebar | undefined {
    return sidebars.value.find((sb) => sb.slug === slug);
  }

  return {
    sidebars,
    setSidebars,
    addSidebar,
    getSidebarBySlug,
  };
});
