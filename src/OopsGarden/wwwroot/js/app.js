import { adminApi } from "./admin-api.js";
import { createAdminController } from "./admin-controller.js";
import { authApi } from "./auth-api.js";
import { $, qs, qsa, escapeHtml } from "./dom.js";
import { createEventWiring } from "./event-wiring.js";
import { gardenApi } from "./garden-api.js";
import { createGardenRenderer } from "./garden-rendering.js";
import { fileToDataUrl } from "./image-upload.js";
import { loadLanguage, t } from "./localization.js";
import {
    reminderMeta,
    reminderStateClass,
    toDateInputValue,
    toMonthKey
} from "./plant-date-utils.js";
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
const plantHistoryController = createPlantHistoryController({
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

function confirmDiscardPlantChanges() {
    return !hasUnsavedPlantDialogChanges() || window.confirm(t("confirm.discardPlantChanges"));
}

function getPlantDialogSnapshot() {
    const form = $("plantDialogForm");
    return {
        id: form.elements.id.value || "",
        name: form.elements.name.value || "",
        description: form.elements.description.value || "",
        soil: form.elements.soil.value || "",
        locationId: form.elements.locationId.value || "",
        plantedOn: form.elements.plantedOn.value || "",
        photoDataUrl: form.dataset.photoPreview || form.dataset.photo || ""
    };
}

function setPlantDialogBaseline() {
    state.plantDialogBaseline = getPlantDialogSnapshot();
}

function hasUnsavedPlantDialogChanges() {
    if ($("plantEditDialog").hidden || $("plantDialogForm").dataset.public === "true" || !state.plantDialogBaseline) {
        return false;
    }

    return JSON.stringify(getPlantDialogSnapshot()) !== JSON.stringify(state.plantDialogBaseline);
}

function getEarliestPlantHistoryDate() {
    const dates = state.plantHistory.items
        .map((item) => toDateInputValue(item.occurredAt))
        .filter(Boolean)
        .sort();
    return dates[0] || "";
}

function renderPlantTimelineWarning() {
    const form = $("plantDialogForm");
    const warning = $("plantTimelineWarning");
    const plantedOn = form.elements.plantedOn.value;
    const earliestHistoryDate = getEarliestPlantHistoryDate();
    const isInvalidTimeline = form.dataset.public !== "true"
        && Boolean(form.elements.id.value)
        && Boolean(plantedOn)
        && Boolean(earliestHistoryDate)
        && plantedOn > earliestHistoryDate;

    warning.hidden = !isInvalidTimeline;
    form.elements.plantedOn.classList.toggle("timeline-warning-field", isInvalidTimeline);
}

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

function openPublicPlantDialog(id) {
    const plant = state.publicGarden?.plants?.find((item) => item.id === id);
    if (!plant) return;

    const form = $("plantDialogForm");
    form.reset();
    form.elements.id.value = plant.id;
    form.elements.name.value = plant.name;
    form.elements.description.value = plant.description || "";
    form.elements.soil.value = plant.soil || "";
    form.elements.plantedOn.value = plant.plantedOn || "";
    form.elements.lastWateredOn.value = toDateInputValue(plant.lastWateredAt);
    form.dataset.photo = plant.photoDataUrl || "";
    delete form.dataset.photoPreview;
    $("plantPhotoPreview").src = plant.photoDataUrl || defaultPlantPhotoUrl;
    $("plantPhotoPreview").alt = plant.name;
    $("plantPhotoPreviewLabel").textContent = t("plants.photo");
    form.elements.locationId.innerHTML = "";
    form.elements.locationId.append(new Option(plant.location?.name || t("common.none"), plant.location?.id || ""));
    $("plantDialogTitle").textContent = plant.name;
    $("lastWateredField").hidden = false;
    $("plantNotesPanel").hidden = false;
    $("plantNoteText").value = "";
    $("plantNoteIsReminder").checked = false;
    $("plantNoteReminderDate").value = "";
    setReminderDateFieldVisibility();
    setPlantDialogPublicMode(true);
    $("plantEditDialog").hidden = false;
    state.wateringCalendarMonth = toMonthKey(new Date());
    renderWateringCalendar([]);
    Promise.all([
        loadPlantNotes(plant.id, 1, { isPublic: true, publicGardenId: state.publicGarden.id, mode: "all" }),
        loadPlantHistory(plant.id, {
            plantName: plant.name,
            isPublic: true,
            publicGardenId: state.publicGarden.id,
            renderPage: false,
            renderCalendar: true
        })
    ]);
}

function closePublicPlantDialog() {
    closePlantDialog();
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

async function openPlantDialog(id) {
    resetPlantDialogMode();
    $("plantEditDialog").hidden = false;
    setBusyOverlay("plantDialogForm", true, "loading.plant");
    try {
        if (!state.locations.length) {
            state.locations = await gardenApi.getLocations();
        }

        const plant = state.plants.find((item) => item.id === id);
        if (!plant) {
            closePlantDialog();
            return;
        }

        const form = $("plantDialogForm");
        form.elements.id.value = plant.id;
        form.elements.name.value = plant.name;
        form.elements.description.value = plant.description || "";
        form.elements.soil.value = plant.soil || "";
        form.elements.plantedOn.value = plant.plantedOn || "";
        form.elements.lastWateredOn.value = toDateInputValue(plant.lastWateredAt);
        form.dataset.photo = plant.photoDataUrl || "";
        delete form.dataset.photoPreview;
        $("plantPhotoPreview").src = plant.photoDataUrl || defaultPlantPhotoUrl;
        $("plantPhotoPreview").alt = plant.name;
        $("plantPhotoPreviewLabel").textContent = t("plants.photo");
        renderLocationSelect(form.elements.locationId, plant.location?.id || plant.locationId || "");
        $("plantDialogTitle").textContent = plant.name;
        $("lastWateredField").hidden = false;
        $("deletePlantFromDialog").hidden = false;
        $("plantNotesPanel").hidden = false;
        $("plantNoteText").value = "";
        $("plantNoteIsReminder").checked = false;
        $("plantNoteReminderDate").value = "";
        setReminderDateFieldVisibility();
        setPlantEditMode(false);
        setPlantDialogBaseline();
        state.wateringCalendarMonth = toMonthKey(new Date());
        state.plantHistory = {
            ...state.plantHistory,
            plantId: plant.id,
            plantName: plant.name,
            isPublic: false,
            publicGardenId: null,
            items: []
        };
        renderWateringCalendar([]);
        renderPlantTimelineWarning();
        await Promise.all([
            loadPlantNotes(plant.id, 1, { mode: "all" }),
            loadPlantHistory(plant.id, { plantName: plant.name, isPublic: false, renderPage: false, renderCalendar: true })
        ]);
    } finally {
        setBusyOverlay("plantDialogForm", false);
    }
}

async function openCreatePlantDialog() {
    resetPlantDialogMode();
    $("plantEditDialog").hidden = false;
    setBusyOverlay("plantDialogForm", true, "loading.plant");
    try {
        if (!state.locations.length) {
            state.locations = await gardenApi.getLocations();
        }

        const form = $("plantDialogForm");
        form.reset();
        form.elements.id.value = "";
        form.dataset.photo = "";
        delete form.dataset.photoPreview;
        form.elements.soil.value = "";
        form.elements.lastWateredOn.value = "";
        $("plantPhotoPreview").src = defaultPlantPhotoUrl;
        $("plantPhotoPreview").alt = t("plants.add");
        $("plantPhotoPreviewLabel").textContent = t("plants.photo");
        renderLocationSelect(form.elements.locationId, "");
        $("plantDialogTitle").textContent = t("plants.add");
        $("lastWateredField").hidden = true;
        $("wateringHeatmap").innerHTML = "";
        renderPlantTimelineWarning();
        $("deletePlantFromDialog").hidden = true;
        $("plantNotesPanel").hidden = true;
        setPlantEditMode(true);
        setPlantDialogBaseline();
    } finally {
        setBusyOverlay("plantDialogForm", false);
    }
}

function requestClosePlantDialog() {
    if (!confirmDiscardPlantChanges()) {
        return false;
    }

    closePlantDialog();
    return true;
}

function closePlantDialog() {
    const form = $("plantDialogForm");
    state.plantNotesRequestId += 1;
    state.plantDialogBaseline = null;
    resetPlantDialogMode();
    form.reset();
    form.dataset.photo = "";
    delete form.dataset.photoPreview;
    $("plantPhotoPreview").src = defaultPlantPhotoUrl;
    $("plantPhotoPreviewLabel").textContent = t("plants.photo");
    renderPlantTimelineWarning();
    $("lastWateredField").hidden = true;
    $("deletePlantFromDialog").hidden = true;
    $("plantNotesPanel").hidden = true;
    $("plantNoteText").value = "";
    $("plantNoteIsReminder").checked = false;
    $("plantNoteReminderDate").value = "";
    setReminderDateFieldVisibility();
    setPlantEditMode(true);
    state.plantNotes = {
        plantId: null,
        publicGardenId: null,
        isPublic: false,
        isLoading: false,
        mode: "all",
        page: 1,
        pageSize: 5,
        total: 0,
        items: [],
        hasPrevious: false,
        hasNext: false
    };
    $("plantEditDialog").hidden = true;
}

function openDeletePlantDialog(id, name) {
    if (!id || !name) return;

    const form = $("deletePlantForm");
    form.reset();
    form.elements.id.value = id;
    form.elements.name.value = name;
    $("deletePlantWarning").textContent = t("plants.deleteWarning").replace("{name}", name);
    $("confirmDeletePlant").disabled = true;
    $("deletePlantDialog").hidden = false;
    form.elements.confirmationName.focus();
}

function closeDeletePlantDialog() {
    const form = $("deletePlantForm");
    form.reset();
    $("confirmDeletePlant").disabled = true;
    $("deletePlantDialog").hidden = true;
}

function updateDeletePlantConfirmationState() {
    const form = $("deletePlantForm");
    $("confirmDeletePlant").disabled = form.elements.confirmationName.value.trim() !== form.elements.name.value;
}

function openWaterPlantDialog(id, name, date = "") {
    if (!id || !name) return;

    const form = $("waterPlantForm");
    form.reset();
    form.elements.id.value = id;
    form.elements.date.value = date;
    $("waterPlantPrompt").textContent = t("plants.waterPrompt")
        .replace("{name}", name)
        .replace("{date}", date || t("common.today"));
    $("waterPlantDialog").hidden = false;
    $("confirmWaterPlant").focus();
}

function closeWaterPlantDialog() {
    const form = $("waterPlantForm");
    form.reset();
    $("waterPlantDialog").hidden = true;
}

function setPlantEditMode(enabled) {
    const form = $("plantDialogForm");
    const isExistingPlant = Boolean(form.elements.id.value);
    const effectiveEnabled = enabled || !isExistingPlant;
    $("plantEditMode").checked = effectiveEnabled;

    form.elements.name.readOnly = !effectiveEnabled;
    form.elements.description.readOnly = !effectiveEnabled;
    form.elements.soil.readOnly = !effectiveEnabled;
    form.elements.locationId.disabled = !effectiveEnabled;
    form.elements.plantedOn.readOnly = !effectiveEnabled;
    form.elements.lastWateredOn.readOnly = true;
    form.elements.photo.disabled = !effectiveEnabled;

    qsa(".plant-edit-command").forEach((element) => {
        element.hidden = !effectiveEnabled;
    });
    $("deletePlantFromDialog").hidden = !effectiveEnabled || !isExistingPlant;
    qsa("[data-plant-photo-field]").forEach((element) => {
        element.hidden = !effectiveEnabled;
    });
    $("wateringCalendarHint").hidden = !isWateringCalendarEditable();
    if (isExistingPlant && state.plantHistory.plantId === form.elements.id.value) {
        renderWateringCalendar(state.plantHistory.items);
    }
}

function setPlantDialogPublicMode(enabled) {
    const form = $("plantDialogForm");
    form.dataset.public = enabled ? "true" : "false";
    $("plantEditMode").closest("label").hidden = enabled;
    $("plantEditMode").checked = false;
    form.elements.name.readOnly = true;
    form.elements.description.readOnly = true;
    form.elements.soil.readOnly = true;
    form.elements.locationId.disabled = true;
    form.elements.plantedOn.readOnly = true;
    form.elements.lastWateredOn.readOnly = true;
    form.elements.photo.disabled = true;
    qsa(".plant-edit-command").forEach((element) => {
        element.hidden = true;
    });
    qsa("[data-plant-photo-field]").forEach((element) => {
        element.hidden = true;
    });
    $("addPlantNote").hidden = true;
    $("plantNoteText").hidden = true;
    $("plantNoteComposer").hidden = true;
    $("plantNoteComposerActions").hidden = true;
    $("plantNotesTabs").hidden = true;
    $("wateringCalendarHint").hidden = true;
    $("openPlantHistory").hidden = false;
    document.querySelector('label[for="plantNoteText"]').hidden = true;
}

function resetPlantDialogMode() {
    const form = $("plantDialogForm");
    form.dataset.public = "false";
    $("plantEditMode").closest("label").hidden = false;
    $("addPlantNote").hidden = false;
    $("plantNoteText").hidden = false;
    $("plantNoteComposer").hidden = state.plantNotes.mode === "overdue";
    $("plantNoteComposerActions").hidden = state.plantNotes.mode === "overdue";
    $("plantNotesTabs").hidden = false;
    $("openPlantHistory").hidden = false;
    document.querySelector('label[for="plantNoteText"]').hidden = false;
}

function openLocationDialog(id = "") {
    const form = $("locationDialogForm");
    const location = id ? state.locations.find((item) => item.id === id) : null;
    form.reset();
    form.elements.id.value = location?.id || "";
    form.elements.name.value = location?.name || "";
    $("locationDialogTitle").textContent = location ? t("locations.edit") : t("locations.add");
    $("deleteLocationFromDialog").hidden = !location;
    $("locationDialog").hidden = false;
    form.elements.name.focus();
}

function closeLocationDialog() {
    $("locationDialogForm").reset();
    $("locationDialog").hidden = true;
}

const { wireEvents } = createEventWiring({
    $,
    adminController,
    applyTheme,
    authApi,
    closeDeletePlantDialog,
    closeLocationDialog,
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
