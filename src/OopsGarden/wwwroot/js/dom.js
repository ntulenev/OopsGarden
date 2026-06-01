export const $ = (id) => document.getElementById(id);
export const qs = (selector) => document.querySelector(selector);
export const qsa = (selector) => [...document.querySelectorAll(selector)];

export function escapeHtml(value) {
    const div = document.createElement("div");
    div.textContent = value ?? "";
    return div.innerHTML;
}
