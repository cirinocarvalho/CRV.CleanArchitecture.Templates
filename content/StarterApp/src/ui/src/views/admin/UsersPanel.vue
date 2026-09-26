<script setup lang="ts">
import { ref, computed, watch, onMounted } from "vue";
import { useLoginStore } from "../../store/loginStore";
import { useTableExport } from "../../composables/useTableExport";
import { useDebouncedRef } from "../../composables/useDebouncedRef";
import ModalSpinner from "../../components/modal-spinner.vue";
import { getErrorMessage } from "../../utils/errorMessage";
import {
  listUsers,
  grantAdmin,
  revokeAdmin,
  setUserCompany,
  setUserActive,
  resetPassword,
  listCompanies,
  type AdminUser,
  type AdminCompany,
} from "../../services/admin";

const loginStore = useLoginStore();
const users = ref<AdminUser[]>([]);
const companies = ref<AdminCompany[]>([]);

// Modal feedback (errors)
const showModal = ref(false);
const modalTitle = ref("");
const modalMessage = ref("");

function showError(message: string) {
  modalTitle.value = "Error";
  modalMessage.value = message;
  showModal.value = true;
}

// Reset-password confirmation modal
const showResetConfirm = ref(false);
const resetTarget = ref<AdminUser | null>(null);

function confirmResetPassword(u: AdminUser) {
  resetTarget.value = u;
  showResetConfirm.value = true;
}

async function doResetPassword() {
  const u = resetTarget.value;
  showResetConfirm.value = false;
  resetTarget.value = null;
  if (!u) return;
  try {
    await resetPassword(u.id);
    modalTitle.value = "Password Reset";
    modalMessage.value = `A password reset email has been sent to ${u.email}.`;
    showModal.value = true;
  } catch (e) {
    showError(messageFrom(e));
  }
}

// ── DataTable state ──────────────────────────────────
type UserSort = "email" | "fullName" | "companyName" | "isActive" | "admin";
const tableSearch = ref("");
const pageSize = ref(25);
const pageSizeOptions = [10, 25, 50, 100];
const currentPage = ref(1);
const sortKey = ref<UserSort>("email");
const sortDir = ref<"asc" | "desc">("asc");

function adminLabel(u: AdminUser): string {
  return u.roles.includes("Admin") ? "Yes" : "No";
}
function companyLabel(u: AdminUser): string {
  return u.companyName ?? "";
}

// ── Computed: Table ──────────────────────────────────
// Driven off the debounced query rather than tableSearch, so filter + sort +
// paginate don't re-run (and re-render the table) on every keystroke.
const debouncedTableSearch = useDebouncedRef(tableSearch);

// A narrowed result set can leave the current page past the end of the list.
watch(debouncedTableSearch, () => {
  currentPage.value = 1;
});

const filteredUsers = computed(() => {
  const q = debouncedTableSearch.value.toLowerCase();
  if (!q) return users.value;
  return users.value.filter((u) =>
    [u.email, u.fullName, companyLabel(u)].some((v) =>
      v.toLowerCase().includes(q),
    ),
  );
});

const sortedUsers = computed(() => {
  const list = [...filteredUsers.value];
  const val = (u: AdminUser): string => {
    switch (sortKey.value) {
      case "companyName":
        return companyLabel(u).toLowerCase();
      case "isActive":
        return u.isActive ? "1" : "0";
      case "admin":
        return u.roles.includes("Admin") ? "1" : "0";
      default:
        return String(u[sortKey.value]).toLowerCase();
    }
  };
  list.sort((a, b) => {
    const av = val(a);
    const bv = val(b);
    return sortDir.value === "asc"
      ? av.localeCompare(bv)
      : bv.localeCompare(av);
  });
  return list;
});

const totalPages = computed(() =>
  Math.max(1, Math.ceil(sortedUsers.value.length / pageSize.value)),
);

const pagedUsers = computed(() => {
  const start = (currentPage.value - 1) * pageSize.value;
  return sortedUsers.value.slice(start, start + pageSize.value);
});

const showingFrom = computed(() =>
  sortedUsers.value.length === 0
    ? 0
    : (currentPage.value - 1) * pageSize.value + 1,
);
const showingTo = computed(() =>
  Math.min(currentPage.value * pageSize.value, sortedUsers.value.length),
);

function setSort(key: UserSort) {
  if (sortKey.value === key) {
    sortDir.value = sortDir.value === "asc" ? "desc" : "asc";
  } else {
    sortKey.value = key;
    sortDir.value = "asc";
  }
  currentPage.value = 1;
}

function sortIcon(key: UserSort): string {
  if (sortKey.value !== key) return "⇅";
  return sortDir.value === "asc" ? "▲" : "▼";
}

// ── Export (Copy / CSV / Print / PDF) ────────────────
const { copyTable, downloadCsv, printTable } = useTableExport<AdminUser>(
  [
    { label: "Email", value: (u) => u.email },
    { label: "Name", value: (u) => u.fullName },
    { label: "Company", value: (u) => companyLabel(u) },
    { label: "Active", value: (u) => (u.isActive ? "Yes" : "No") },
    { label: "Admin", value: (u) => adminLabel(u) },
  ],
  () => sortedUsers.value,
  { fileName: "users.csv", title: "Users" },
);

// ── Data / actions ───────────────────────────────────
// Initial load: users and the company list together.
async function load() {
  [users.value, companies.value] = await Promise.all([
    listUsers(),
    listCompanies(),
  ]);
}

// Refresh after an admin action. Only the user list can change - the company
// list is unaffected by role/company/active toggles, so re-fetching it on every
// action was a wasted round trip.
async function reloadUsers() {
  users.value = await listUsers();
}

function messageFrom(e: unknown): string {
  return getErrorMessage(e);
}

function isSelf(u: AdminUser): boolean {
  return u.id === loginStore.profile?.identityUserId;
}

async function toggleAdmin(u: AdminUser) {
  try {
    if (u.roles.includes("Admin")) {
      await revokeAdmin(u.id);
    } else {
      await grantAdmin(u.id);
    }
    await reloadUsers();
  } catch (e) {
    showError(messageFrom(e));
  }
}

async function changeCompany(u: AdminUser, companyId: number) {
  try {
    await setUserCompany(u.id, companyId);
    await reloadUsers();
  } catch (e) {
    showError(messageFrom(e));
  }
}

async function toggleActive(u: AdminUser) {
  try {
    await setUserActive(u.id, !u.isActive);
    await reloadUsers();
  } catch (e) {
    showError(messageFrom(e));
  }
}

onMounted(load);
</script>

<template>
  <div>
    <!-- Export buttons + Show entries + Search -->
    <div class="datatable-controls">
      <div class="export-buttons">
        <button class="button is-primary is-small mr-1" @click="copyTable">
          Copy
        </button>
        <button class="button is-primary is-small mr-1" @click="downloadCsv">
          CSV
        </button>
        <button class="button is-primary is-small mr-1" @click="printTable">
          PDF
        </button>
        <button class="button is-primary is-small" @click="printTable">
          Print
        </button>
      </div>
      <div class="show-search">
        <div class="show-entries">
          <span>Show</span>
          <div class="select is-small mx-2">
            <select
              v-model="pageSize"
              aria-label="Show entries per page"
              @change="currentPage = 1"
            >
              <option v-for="n in pageSizeOptions" :key="n" :value="n">
                {{ n }}
              </option>
            </select>
          </div>
          <span>entries</span>
        </div>
        <div class="search-box ml-4">
          <span class="mr-2">Search:</span>
          <input
            v-model="tableSearch"
            type="text"
            class="input is-small table-search"
            aria-label="Search users"
            @input="currentPage = 1"
          />
        </div>
      </div>
    </div>

    <p class="showing-text mt-2">
      Showing {{ showingFrom }} to {{ showingTo }} of
      {{ sortedUsers.length }} entries
    </p>

    <div class="table-container">
      <table class="table users-table is-fullwidth">
        <thead>
          <tr>
            <th
              v-for="col in [
                { key: 'email', label: 'Email' },
                { key: 'fullName', label: 'Name' },
                { key: 'companyName', label: 'Company' },
              ]"
              :key="col.key"
              class="sortable"
              @click="setSort(col.key as UserSort)"
            >
              {{ col.label }}
              <span
                class="sort-icon"
                :class="{ active: sortKey === col.key }"
                >{{ sortIcon(col.key as UserSort) }}</span
              >
            </th>
            <th class="sortable has-text-centered" @click="setSort('isActive')">
              Active
              <span
                class="sort-icon"
                :class="{ active: sortKey === 'isActive' }"
                >{{ sortIcon("isActive") }}</span
              >
            </th>
            <th class="sortable has-text-centered" @click="setSort('admin')">
              Admin
              <span
                class="sort-icon"
                :class="{ active: sortKey === 'admin' }"
                >{{ sortIcon("admin") }}</span
              >
            </th>
            <th class="has-text-centered">Actions</th>
          </tr>
        </thead>
        <tbody>
          <tr v-if="pagedUsers.length === 0">
            <td colspan="6" class="has-text-centered no-data">
              No data available in table
            </td>
          </tr>
          <tr v-for="u in pagedUsers" :key="u.id">
            <td>{{ u.email }}</td>
            <td>{{ u.fullName }}</td>
            <td>
              <div class="select is-small">
                <select
                  :value="u.companyId ?? ''"
                  :aria-label="`Company for ${u.email}`"
                  @change="
                    changeCompany(
                      u,
                      Number(($event.target as HTMLSelectElement).value),
                    )
                  "
                >
                  <option value="" disabled>- Select -</option>
                  <option
                    v-for="c in companies"
                    :key="c.companyId"
                    :value="c.companyId"
                  >
                    {{ c.name }}
                  </option>
                </select>
              </div>
            </td>
            <td class="has-text-centered">
              <input
                type="checkbox"
                :checked="u.isActive"
                :disabled="isSelf(u)"
                :aria-label="`Active — ${u.email}`"
                :title="
                  isSelf(u)
                    ? 'You cannot deactivate your own account'
                    : undefined
                "
                @change="toggleActive(u)"
              />
            </td>
            <td class="has-text-centered">
              <input
                type="checkbox"
                :checked="u.roles.includes('Admin')"
                :disabled="isSelf(u)"
                :aria-label="`Admin role — ${u.email}`"
                :title="
                  isSelf(u)
                    ? 'You cannot change your own Admin role'
                    : undefined
                "
                @change="toggleAdmin(u)"
              />
            </td>
            <td class="has-text-centered">
              <button
                class="button is-primary is-small reset-btn"
                :aria-label="`Reset password — ${u.email}`"
                @click="confirmResetPassword(u)"
              >
                Reset Password
              </button>
            </td>
          </tr>
        </tbody>
      </table>
    </div>

    <!-- Pagination -->
    <div v-if="totalPages > 1" class="pagination-controls">
      <button
        class="button is-small"
        :disabled="currentPage === 1"
        @click="currentPage = 1"
      >
        «
      </button>
      <button
        class="button is-small"
        :disabled="currentPage === 1"
        @click="currentPage--"
      >
        ‹
      </button>
      <span class="px-3">Page {{ currentPage }} of {{ totalPages }}</span>
      <button
        class="button is-small"
        :disabled="currentPage === totalPages"
        @click="currentPage++"
      >
        ›
      </button>
      <button
        class="button is-small"
        :disabled="currentPage === totalPages"
        @click="currentPage = totalPages"
      >
        »
      </button>
    </div>

    <ModalSpinner
      v-model="showModal"
      :title="modalTitle"
      :message="modalMessage"
      :is-loading="false"
    />

    <ModalSpinner
      v-model="showResetConfirm"
      title="Reset Password"
      :message="`Reset the password for ${resetTarget?.email ?? ''}?`"
      :is-loading="false"
      :is-proceed="true"
      @proceed="doResetPassword"
    />
  </div>
</template>

<style scoped>
/* ── DataTable controls ──────────────────────────────── */
.datatable-controls {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  flex-wrap: wrap;
  gap: 8px;
}
.export-buttons {
  display: flex;
  align-items: center;
}
/* Compact buttons for the export toolbar. */
.export-buttons .button {
  font-size: 0.75rem !important;
  height: auto;
  padding: 3px 8px;
}
.show-search {
  display: flex;
  align-items: center;
  flex-wrap: wrap;
  gap: 8px;
}
.show-entries {
  display: flex;
  align-items: center;
  font-size: 0.9rem;
}
.search-box {
  display: flex;
  align-items: center;
  font-size: 0.9rem;
}
.table-search {
  width: 200px;
}
.showing-text {
  font-size: 0.85rem;
  color: #555;
}

/* ── Table ───────────────────────────────────────────── */
.users-table thead tr {
  background-color: #2176d2;
}
.users-table thead th {
  color: #fff !important;
  border: none;
  user-select: none;
}
.users-table thead th.sortable {
  cursor: pointer;
  white-space: nowrap;
}
.users-table tbody td {
  vertical-align: middle;
}
.sort-icon {
  font-size: 0.7rem;
  vertical-align: middle;
  margin-left: 4px;
  opacity: 1;
}
.sort-icon.active {
  color: #fff;
}
.no-data {
  color: #888;
  padding: 16px;
}

/* ── Pagination ──────────────────────────────────────── */
.pagination-controls {
  display: flex;
  align-items: center;
  justify-content: flex-end;
  gap: 4px;
  margin-top: 8px;
}
.pagination-controls .button {
  color: #fff;
}
.reset-btn {
  background-color: #2176d2;
  border-color: #2176d2;
  color: #fff;
  font-weight: 600;
  letter-spacing: 0.04em;
  white-space: nowrap;
  font-size: 0.75rem !important;
}
.reset-btn:hover {
  background-color: #1a5fa8;
  border-color: #1a5fa8;
}
</style>
