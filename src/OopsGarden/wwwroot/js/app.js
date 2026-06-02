import { adminApi } from "./admin-api.js";
import { createAdminController } from "./admin-controller.js";
import { authApi } from "./auth-api.js";
import { $, qs, qsa, escapeHtml } from "./dom.js";
import { createEventWiring } from "./event-wiring.js";
import { gardenApi } from "./garden-api.js";
import { createGardenRenderer } from "./garden-rendering.js";
import { fileToDataUrl } from "./image-upload.js";
import { loadLanguage, t } from "./localization.js";
import { createLocationDialogController } from "./location-dialog-controller.js";
import {
    reminderMeta,
    reminderStateClass,
    toDateInputValue,
    toMonthKey
} from "./plant-date-utils.js";
import { createPlantDialogController } from "./plant-dialog-controller.js";
import { createPlantHistoryController } from "./plant-history-controller.js";
import { createPlantNotesController } from "./plant-notes-controller.js";
import { createPhotoPreviewController } from "./photo-preview.js";
import { plantsApi } from "./plants-api.js";
import {
    defaultAvatarUrl,
    defaultPlantPhotoUrl,
    loadingState,
    maxUploadImageSide,
    state
} from "./state.js";
import { applyTheme } from "./theme.js";
import {
    setBusyOverlay,
    setRegionLoading,
    showError,
    toast,
    withButtonLoading
} from "./ui.js";

const photoPreview = createPhotoPreviewController({ state, defaultPlantPhotoUrl, t, $ });
const gardenRenderer = createGardenRenderer({
    $,
    defaultPlantPhotoUrl,
    escapeHtml,
    gardenApi,
    loadingState,
    setRegionLoading,
    showError,
    state,
    t
});
const { loadGarden, renderLocationSelect, renderPlantGroups } = gardenRenderer;
const plantNotesController = createPlantNotesController({
    $,
    escapeHtml,
    plantsApi,
    reminderMeta,
    reminderStateClass,
    state,
    t
});
const {
    loadPlantNotes,
    setReminderDateFieldVisibility
} = plantNotesController;
let plantHistoryController;
const plantDialogController = createPlantDialogController({
    $,
    qsa,
    defaultPlantPhotoUrl,
    gardenApi,
    loadPlantNotes,
    renderLocationSelect,
    setBusyOverlay,
    setReminderDateFieldVisibility,
    state,
    t,
    toDateInputValue,
    toMonthKey,
    getPlantHistory: () => plantHistoryController
});
const {
    closePlantDialog,
    closePublicPlantDialog,
    confirmDiscardPlantChanges,
    openCreatePlantDialog,
    openPlantDialog,
    openPublicPlantDialog,
    renderPlantTimelineWarning,
    requestClosePlantDialog,
    setPlantDialogBaseline,
    setPlantEditMode
} = plantDialogController;

plantHistoryController = createPlantHistoryController({
    $,
    closePlantDialog,
    confirmDiscardPlantChanges,
    defaultPlantPhotoUrl,
    escapeHtml,
    plantsApi,
    reminderMeta,
    reminderStateClass,
    renderPlantTimelineWarning,
    renderShell,
    state,
    t,
    toDateInputValue,
    toMonthKey
});
const {
    isWateringCalendarEditable,
    loadPlantHistory,
    openPlantHistoryPage,
    renderPlantHistory,
    renderWateringCalendar,
    shiftWateringCalendarMonth
} = plantHistoryController;

function confirmDelete(key) {
    return window.confirm(t(key));
}

const adminController = createAdminController({
    $,
    adminApi,
    confirmDelete,
    escapeHtml,
    loadingState,
    setRegionLoading,
    showError,
    state,
    t,
    withButtonLoading
});

const locationDialogController = createLocationDialogController({
    $,
    qsa,
    confirmDelete,
    gardenApi,
    loadGarden,
    showError,
    state,
    t,
    toast,
    withButtonLoading
});
const {
    openLocationDialog,
    wireEvents: wireLocationDialogEvents
} = locationDialogController;

function formData(form) {
    return Object.fromEntries(new FormData(form).entries());
}

async function refreshMe() {
    try {
        state.me = await authApi.getMe();
    } catch (error) {
        if (error.status !== 401) {
            throw error;
        }

        state.me = { authenticated: false };
    }

    if (state.me.authenticated && state.me.language && state.me.language !== state.lang) {
        await loadLanguage(state.me.language);
    }
    renderShell();
}

function renderShell() {
    if (state.publicGarden) {
        renderPublicGarden();
        return;
    }

    const authed = Boolean(state.me?.authenticated);
    const isAdmin = state.me?.role === "Admin";
    const isAdminRoute = state.route === "/admin";
    const isUser = authed && !isAdmin;
    const userViews = ["garden", "settings", "plantHistory"];
    const activeView = isAdmin
        ? "admin"
        : isUser && userViews.includes(state.view)
            ? state.view
            : "garden";

    qsa("[data-auth]").forEach((el) => { el.hidden = !authed; });
    qsa("[data-user]").forEach((el) => { el.hidden = !isUser; });
    qsa("[data-admin]").forEach((el) => { el.hidden = !isAdmin; });
    $("authView").hidden = authed || isAdminRoute;
    $("adminAuthView").hidden = authed || !isAdminRoute;
    $("gardenView").hidden = !isUser || activeView !== "garden";
    $("plantHistoryView").hidden = !isUser || activeView !== "plantHistory";
    $("settingsView").hidden = !isUser || activeView !== "settings";
    $("publicGardenView").hidden = true;
    $("adminView").hidden = !isAdmin;
    if (isUser) {
        renderUserIdentity();
        qs("#settingsForm [name=displayName]").value = state.me.name || "";
        qs("#settingsForm [name=isGardenPublic]").checked = Boolean(state.me.isGardenPublic);
        $("sharePublicGardenLink").hidden = !state.me.isGardenPublic;
    }
    if (isUser && activeView === "garden") loadGarden();
    if (isUser && activeView === "plantHistory") renderPlantHistory();
    if (isAdmin && activeView === "admin") adminController.loadAdmin();
}

function renderPublicGarden() {
    qsa("[data-auth]").forEach((el) => { el.hidden = true; });
    qsa("[data-user]").forEach((el) => { el.hidden = true; });
    qsa("[data-admin]").forEach((el) => { el.hidden = true; });
    $("authView").hidden = true;
    $("adminAuthView").hidden = true;
    $("gardenView").hidden = true;
    $("plantHistoryView").hidden = state.view !== "plantHistory";
    $("settingsView").hidden = true;
    $("adminView").hidden = true;
    $("publicGardenView").hidden = state.view === "plantHistory";

    const ownerName = state.publicGarden.name || t("user.defaultName");
    const avatarUrl = state.publicGarden.avatar || defaultAvatarUrl;
    $("publicGardenTitle").textContent = t("public.title").replace("{name}", ownerName);
    document.title = $("publicGardenTitle").textContent || t("garden.title");
    $("publicGardenOwnerName").textContent = ownerName;
    $("publicGardenAvatar").src = avatarUrl;
    $("publicGardenAvatar").alt = ownerName;
    $("publicGardenPlantTotal").textContent = t("plants.total").replace("{count}", (state.publicGarden.plants || []).length);
    if (state.view === "plantHistory") {
        renderPlantHistory();
    } else {
        renderPlantGroups($("publicPlantList"), state.publicGarden.plants || [], { isPublic: true });
    }
}

function setView(view) {
    const isAdmin = state.me?.role === "Admin";
    if (state.publicGarden && view === "garden") {
        state.view = "garden";
        renderShell();
        return;
    }

    if ((isAdmin && view !== "admin") || (!isAdmin && view === "admin")) {
        return;
    }

    state.view = view;
    renderShell();
}

function renderUserIdentity() {
    const avatarUrl = state.me.avatar || defaultAvatarUrl;
    const name = state.me.name || t("user.defaultName");
    const headerAvatar = $("userAvatar");
    const gardenAvatar = $("gardenOwnerAvatar");
    const currentAvatarPreview = $("currentAvatarPreview");
    headerAvatar.src = avatarUrl;
    headerAvatar.alt = name;
    headerAvatar.title = name;
    gardenAvatar.src = avatarUrl;
    gardenAvatar.alt = name;
    currentAvatarPreview.src = avatarUrl;
    currentAvatarPreview.alt = name;
    $("gardenOwnerName").textContent = name;
}

function renderPublicGardenLink() {
    return `${location.origin}${location.pathname}?publicGarden=${state.me.id}`;
}

function resetAvatarPreview() {
    const form = $("settingsForm");
    form.avatar.value = "";
    delete form.dataset.avatarPreview;
    $("newAvatarPreviewSlot").hidden = true;
    $("cancelAvatarChange").hidden = true;
}

async function initPublicGardenFromUrl() {
    const publicGardenId = new URLSearchParams(location.search).get("publicGarden");
    if (!publicGardenId) {
        return false;
    }

    setRegionLoading("publicPlantList", true, "loading.publicGarden");
    state.publicGarden = await gardenApi.getPublicGarden(publicGardenId);
    renderShell();
    return true;
}

const { wireEvents } = createEventWiring({
    $,
    adminController,
    applyTheme,
    authApi,
    closeDeletePlantDialog,
    closePlantDialog,
    closePublicPlantDialog,
    closeWaterPlantDialog,
    confirmDelete,
    defaultPlantPhotoUrl,
    fileToDataUrl,
    formData,
    gardenApi,
    isWateringCalendarEditable,
    loadGarden,
    loadLanguage,
    loadPlantHistory,
    loadPlantNotes,
    maxUploadImageSide,
    openCreatePlantDialog,
    openDeletePlantDialog,
    openLocationDialog,
    openPlantDialog,
    openPlantHistoryPage,
    openPublicPlantDialog,
    openWaterPlantDialog,
    photoPreview,
    plantsApi,
    qs,
    qsa,
    refreshMe,
    renderPublicGardenLink,
    renderShell,
    renderPlantTimelineWarning,
    requestClosePlantDialog,
    resetAvatarPreview,
    setPlantDialogBaseline,
    setPlantEditMode,
    setReminderDateFieldVisibility,
    setView,
    shiftWateringCalendarMonth,
    showError,
    state,
    t,
    toDateInputValue,
    toast,
    updateDeletePlantConfirmationState,
    wireLocationDialogEvents,
    withButtonLoading
});
async function initInviteFromUrl() {
    const invite = new URLSearchParams(location.search).get("invite");
    if (invite) {
        qs("#registerForm [name=inviteCode]").value = invite;
    }
}

wireEvents();
applyTheme(state.theme);
await loadLanguage(state.lang);
await initInviteFromUrl();
if (await initPublicGardenFromUrl()) {
    // Public gardens do not need an authenticated session.
} else {
await refreshMe();
}
