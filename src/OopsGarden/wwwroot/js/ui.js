import { $ } from "./dom.js";
import { t } from "./localization.js";

export function toast(message) {
    const el = $("toast");
    el.textContent = message;
    el.hidden = false;
    clearTimeout(toast.timer);
    toast.timer = setTimeout(() => { el.hidden = true; }, 2600);
}

export function showError(error) {
    toast(error?.message || t("toast.error"));
}

export function loadingMarkup(messageKey = "loading.generic") {
    return `<div class="loading-state" role="status" aria-live="polite">
        <span class="notes-spinner" aria-hidden="true"></span>
        <span>${t(messageKey)}</span>
    </div>`;
}

export function setRegionLoading(elementOrId, isLoading, messageKey = "loading.generic") {
    const element = typeof elementOrId === "string" ? $(elementOrId) : elementOrId;
    if (!element) return;
    element.setAttribute("aria-busy", isLoading ? "true" : "false");
    if (isLoading) {
        element.innerHTML = loadingMarkup(messageKey);
    }
}

export function setBusyOverlay(elementOrId, isLoading, messageKey = "loading.generic") {
    const element = typeof elementOrId === "string" ? $(elementOrId) : elementOrId;
    if (!element) return;
    element.setAttribute("aria-busy", isLoading ? "true" : "false");
    element.classList.toggle("is-busy", isLoading);
    const existing = element.querySelector(":scope > .busy-overlay");
    if (!isLoading) {
        existing?.remove();
        return;
    }

    if (!existing) {
        const overlay = document.createElement("div");
        overlay.className = "busy-overlay";
        overlay.innerHTML = loadingMarkup(messageKey);
        element.append(overlay);
    }
}

export function setButtonLoading(button, isLoading, messageKey = "loading.generic") {
    if (!button) return;
    if (isLoading) {
        if (!button.dataset.idleHtml) {
            button.dataset.idleHtml = button.innerHTML;
        }
        button.disabled = true;
        button.classList.add("is-loading");
        button.innerHTML = `<span class="button-spinner" aria-hidden="true"></span><span>${t(messageKey)}</span>`;
        return;
    }

    button.disabled = false;
    button.classList.remove("is-loading");
    button.innerHTML = button.dataset.idleHtml || button.innerHTML;
    delete button.dataset.idleHtml;
}

export async function withButtonLoading(button, messageKey, action) {
    setButtonLoading(button, true, messageKey);
    try {
        return await action();
    } catch (error) {
        showError(error);
        return null;
    } finally {
        setButtonLoading(button, false);
    }
}
