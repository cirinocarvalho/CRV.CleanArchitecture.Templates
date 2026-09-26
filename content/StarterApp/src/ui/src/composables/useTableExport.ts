// Copy / CSV / Print export for a data table.
// PDF is handled via the browser print dialog ("Save as PDF").

export interface ExportColumn<T> {
  label: string;
  value: (row: T) => string | number;
}

interface ExportOptions {
  fileName: string;
  title: string;
}

export function useTableExport<T>(
  columns: ExportColumn<T>[],
  getRows: () => T[],
  options: ExportOptions,
) {
  function cellText(row: T, col: ExportColumn<T>): string {
    return String(col.value(row) ?? "");
  }

  async function copyTable(): Promise<void> {
    const header = columns.map((c) => c.label).join("\t");
    const body = getRows()
      .map((r) => columns.map((c) => cellText(r, c)).join("\t"))
      .join("\n");
    await navigator.clipboard.writeText(`${header}\n${body}`);
  }

  function downloadCsv(): void {
    const esc = (v: string) => `"${v.replace(/"/g, '""')}"`;
    const header = columns.map((c) => esc(c.label)).join(",");
    const body = getRows()
      .map((r) => columns.map((c) => esc(cellText(r, c))).join(","))
      .join("\n");
    const blob = new Blob([`${header}\n${body}`], {
      type: "text/csv;charset=utf-8;",
    });
    const url = URL.createObjectURL(blob);
    const a = document.createElement("a");
    a.href = url;
    a.download = options.fileName;
    a.click();
    URL.revokeObjectURL(url);
  }

  // Table data originates from the API (records, user-entered text), so every
  // interpolated value below has to be escaped - document.write builds a real
  // document, and an unescaped cell would execute as markup in it.
  function escapeHtml(value: string): string {
    return value
      .replace(/&/g, "&amp;")
      .replace(/</g, "&lt;")
      .replace(/>/g, "&gt;")
      .replace(/"/g, "&quot;")
      .replace(/'/g, "&#39;");
  }

  function printTable(): void {
    const title = escapeHtml(options.title);
    const header = columns
      .map((c) => `<th>${escapeHtml(c.label)}</th>`)
      .join("");
    const rows = getRows()
      .map(
        (r) =>
          `<tr>${columns.map((c) => `<td>${escapeHtml(cellText(r, c))}</td>`).join("")}</tr>`,
      )
      .join("");
    const win = window.open("", "_blank");
    if (!win) return;
    win.document.write(
      `<html><head><title>${title}</title>` +
        `<style>body{font-family:sans-serif;padding:16px}` +
        `table{border-collapse:collapse;width:100%;font-size:12px}` +
        `th,td{border:1px solid #ccc;padding:4px 8px;text-align:left}` +
        `th{background:#2176d2;color:#fff}</style></head>` +
        `<body><h3>${title}</h3>` +
        `<table><thead><tr>${header}</tr></thead><tbody>${rows}</tbody></table></body></html>`,
    );
    win.document.close();
    win.focus();
    win.print();
  }

  return { copyTable, downloadCsv, printTable };
}
