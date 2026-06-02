import { adminApi } from "./admin-api.js";
import { createAdminController } from "./admin-controller.js";
import { authApi } from "./auth-api.js";
import { $, qs, qsa, escapeHtml } from "./dom.js";
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

function wireEvents() {
    $("themeSelect").addEventListener("change", (event) => {
        applyTheme(event.target.value);
    });

    $("languageSelect").addEventListener("change", async (event) => {
        await loadLanguage(event.target.value);
        if (state.me?.authenticated && state.me.role !== "Admin") {
            await authApi.updateSettings({
                displayName: state.me.name || t("user.defaultName"),
                language: state.lang,
                avatarDataUrl: state.me.avatar || null,
                isGardenPublic: Boolean(state.me.isGardenPublic)
            });
        }
        renderShell();
    });

    $("loginForm").addEventListener("submit", async (event) => {
        event.preventDefault();
        await withButtonLoading(event.submitter, "loading.login", async () => {
            await authApi.login(formData(event.currentTarget));
            await refreshMe();
        });
    });

    $("adminLoginForm").addEventListener("submit", async (event) => {
        event.preventDefault();
        await withButtonLoading(event.submitter, "loading.login", async () => {
            await authApi.adminLogin(formData(event.currentTarget));
            await refreshMe();
        });
    });

    $("registerForm").addEventListener("submit", async (event) => {
        event.preventDefault();
        await withButtonLoading(event.submitter, "loading.saving", async () => {
            const data = formData(event.currentTarget);
            data.language = state.lang;
            await authApi.register(data);
            await refreshMe();
        });
    });

    $("logoutBtn").addEventListener("click", async (event) => {
        await withButtonLoading(event.currentTarget, "loading.generic", async () => {
            await authApi.logout();
            await refreshMe();
        });
    });

    $("settingsForm").addEventListener("submit", async (event) => {
        event.preventDefault();
        await withButtonLoading(event.submitter, "loading.saving", async () => {
            const form = event.currentTarget;
            const avatarDataUrl = form.dataset.avatarPreview || state.me.avatar || null;
            await authApi.updateSettings({
                displayName: form.displayName.value,
                language: state.lang,
                avatarDataUrl,
                isGardenPublic: form.elements.isGardenPublic.checked
            });
            await refreshMe();
            resetAvatarPreview();
            toast(t("toast.saved"));
        });
    });

    qs("#settingsForm [name=avatar]").addEventListener("change", async (event) => {
        const form = $("settingsForm");
        const avatarDataUrl = await fileToDataUrl(event.target.files[0], maxUploadImageSide);
        if (!avatarDataUrl) {
            resetAvatarPreview();
            return;
        }

        form.dataset.avatarPreview = avatarDataUrl;
        $("newAvatarPreview").src = avatarDataUrl;
        $("newAvatarPreview").alt = t("settings.newAvatar");
        $("newAvatarPreviewSlot").hidden = false;
        $("cancelAvatarChange").hidden = false;
    });

    $("cancelAvatarChange").addEventListener("click", resetAvatarPreview);

    $("sharePublicGardenLink").addEventListener("click", async () => {
        await navigator.clipboard.writeText(renderPublicGardenLink());
        toast(t("toast.done"));
    });

    $("settingsShortcut").addEventListener("click", () => setView("settings"));
    $("settingsShortcut").addEventListener("keydown", (event) => {
        if (event.key === "Enter" || event.key === " ") {
            event.preventDefault();
            setView("settings");
        }
    });

    $("createLocationBtn").addEventListener("click", () => openLocationDialog());
    $("createPlantBtn").addEventListener("click", async (event) => {
        await withButtonLoading(event.currentTarget, "loading.plant", openCreatePlantDialog);
    });

    $("locationDialogForm").addEventListener("submit", async (event) => {
        event.preventDefault();
        try {
            await withButtonLoading(event.submitter, "loading.saving", async () => {
                const form = event.currentTarget;
                const id = form.elements.id.value;
                await gardenApi.saveLocation(id, { name: form.elements.name.value });
                closeLocationDialog();
                await loadGarden();
                toast(t("toast.saved"));
            });
        } catch (error) {
            showError(error);
        }
    });

    $("closeLocationDialog").addEventListener("click", closeLocationDialog);
    qsa("[data-close-location-dialog]").forEach((button) => button.addEventListener("click", closeLocationDialog));
    $("locationDialog").addEventListener("click", (event) => {
        if (event.target.id === "locationDialog") {
            closeLocationDialog();
        }
    });

    $("deleteLocationFromDialog").addEventListener("click", async (event) => {
        const id = $("locationDialogForm").elements.id.value;
        if (!id) return;
        if (!confirmDelete("confirm.deleteLocation")) return;
        await withButtonLoading(event.currentTarget, "loading.deleting", async () => {
            await gardenApi.deleteLocation(id);
            closeLocationDialog();
            await loadGarden();
            toast(t("toast.done"));
        });
    });

    qs("#plantDialogForm [name=photo]").addEventListener("change", async (event) => {
        const form = $("plantDialogForm");
        const photoDataUrl = await fileToDataUrl(event.target.files[0], maxUploadImageSide);
        if (!photoDataUrl) {
            delete form.dataset.photoPreview;
            $("plantPhotoPreview").src = form.dataset.photo || defaultPlantPhotoUrl;
            $("plantPhotoPreviewLabel").textContent = t("plants.photo");
            return;
        }

        form.dataset.photoPreview = photoDataUrl;
        $("plantPhotoPreview").src = photoDataUrl;
        $("plantPhotoPreview").alt = t("plants.newPhoto");
        $("plantPhotoPreviewLabel").textContent = t("plants.newPhoto");
    });

    $("plantDialogForm").addEventListener("submit", async (event) => {
        event.preventDefault();
        try {
            await withButtonLoading(event.submitter, "loading.saving", async () => {
                const form = event.currentTarget;
                const photoDataUrl = await fileToDataUrl(form.photo.files[0], maxUploadImageSide);
                const id = form.elements.id.value;
                const nextLocationId = form.elements.locationId.value || "";
                const payload = {
                    name: form.elements.name.value,
                    description: form.elements.description.value,
                    soil: form.elements.soil.value,
                    locationId: nextLocationId || null,
                    plantedOn: form.elements.plantedOn.value || null,
                    lastWateredOn: null,
                    photoDataUrl: form.dataset.photoPreview || photoDataUrl || form.dataset.photo || null
                };
                await plantsApi.savePlant(id, payload);
                closePlantDialog();
                await loadGarden();
                toast(t("toast.saved"));
            });
        } catch (error) {
            showError(error);
        }
    });

    qs("#plantDialogForm [name=plantedOn]").addEventListener("input", renderPlantTimelineWarning);

    $("deletePlantFromDialog").addEventListener("click", async (event) => {
        const id = $("plantDialogForm").elements.id.value;
        if (!id) return;
        const plantName = state.plants.find((plant) => plant.id === id)?.name || $("plantDialogForm").elements.name.value;
        openDeletePlantDialog(id, plantName);
    });

    $("deletePlantNameConfirm").addEventListener("input", updateDeletePlantConfirmationState);
    $("deletePlantForm").addEventListener("submit", async (event) => {
        event.preventDefault();
        updateDeletePlantConfirmationState();
        if ($("confirmDeletePlant").disabled) {
            return;
        }

        const id = event.currentTarget.elements.id.value;
        await withButtonLoading(event.submitter, "loading.deleting", async () => {
            await plantsApi.deletePlant(id);
            closeDeletePlantDialog();
            if ($("plantDialogForm").elements.id.value === id) {
                closePlantDialog();
            }

            await loadGarden();
            toast(t("toast.done"));
        });
    });

    $("waterPlantForm").addEventListener("submit", async (event) => {
        event.preventDefault();
        const form = event.currentTarget;
        const plantId = form.elements.id.value;
        const wateredOn = form.elements.date.value;
        if (!plantId) return;

        await withButtonLoading(event.submitter, "loading.saving", async () => {
            if (wateredOn) {
                await plantsApi.createWatering(plantId, { wateredOn });
            } else {
                await plantsApi.waterPlant(plantId);
            }

            closeWaterPlantDialog();
            await loadGarden();
            if ($("plantDialogForm").elements.id.value === plantId) {
                const plant = state.plants.find((item) => item.id === plantId);
                $("plantDialogForm").elements.lastWateredOn.value = toDateInputValue(plant?.lastWateredAt);
                await loadPlantHistory(plantId, { renderPage: false, renderCalendar: true });
            }

            toast(t("toast.saved"));
        });
    });

    $("plantEditMode").addEventListener("change", (event) => {
        setPlantEditMode(event.currentTarget.checked);
        if (event.currentTarget.checked) {
            setPlantDialogBaseline();
        }
    });

    $("plantPhotoPreviewButton").addEventListener("click", () => {
        const plantId = $("plantDialogForm").elements.id.value;
        if (plantId) {
            photoPreview.openPlantPhotoPreview(
                plantId,
                $("plantPhotoPreview").src,
                $("plantDialogTitle").textContent || t("plants.photo"),
                $("plantPhotoPreview").alt);
            return;
        }

        photoPreview.openPhotoPreview($("plantPhotoPreview").src, $("plantDialogTitle").textContent || t("plants.photo"), $("plantPhotoPreview").alt);
    });

    $("historyPlantPhotoButton").addEventListener("click", () => {
        photoPreview.openPlantPhotoPreview(
            state.plantHistory.plantId,
            $("historyPlantPhoto").src,
            $("historyPlantName").textContent || t("plants.photo"),
            $("historyPlantPhoto").alt);
    });

    $("openPlantHistory").addEventListener("click", openPlantHistoryPage);
    $("plantNoteIsReminder").addEventListener("change", setReminderDateFieldVisibility);
    qsa("[data-notes-mode]").forEach((button) => button.addEventListener("click", async () => {
        if (!state.plantNotes.plantId || state.plantNotes.mode === button.dataset.notesMode) return;
        await loadPlantNotes(state.plantNotes.plantId, 1, { mode: button.dataset.notesMode });
    }));
    $("backToGardenFromHistory").addEventListener("click", () => setView("garden"));
    $("backToPlantFromHistory").addEventListener("click", async () => {
        const plantId = state.plantHistory.plantId;
        const isPublic = state.plantHistory.isPublic;
        setView("garden");
        if (plantId) {
            if (isPublic) {
                openPublicPlantDialog(plantId);
            } else {
                await openPlantDialog(plantId);
            }
        }
    });

    $("addPlantNote").addEventListener("click", async (event) => {
        const plantId = state.plantNotes.plantId;
        const text = $("plantNoteText").value.trim();
        const isReminder = $("plantNoteIsReminder").checked;
        const reminderDate = $("plantNoteReminderDate").value;
        if (!plantId || !text) return;
        if (isReminder && !reminderDate) {
            $("plantNoteReminderDate").reportValidity();
            return;
        }

        await withButtonLoading(event.currentTarget, "loading.saving", async () => {
            await plantsApi.createNote(plantId, { text, isAutomatic: false, isReminder, reminderDate: isReminder ? reminderDate : null });
            $("plantNoteText").value = "";
            $("plantNoteIsReminder").checked = false;
            $("plantNoteReminderDate").value = "";
            setReminderDateFieldVisibility();
            await loadPlantNotes(plantId, 1);
            await loadGarden();
            toast(t("toast.saved"));
        });
    });

    $("previousPlantNotesPage").addEventListener("click", async () => {
        if (!state.plantNotes.plantId || !state.plantNotes.hasPrevious) return;
        await loadPlantNotes(state.plantNotes.plantId, state.plantNotes.page - 1);
    });

    $("nextPlantNotesPage").addEventListener("click", async () => {
        if (!state.plantNotes.plantId || !state.plantNotes.hasNext) return;
        await loadPlantNotes(state.plantNotes.plantId, state.plantNotes.page + 1);
    });

    $("closePlantDialog").addEventListener("click", requestClosePlantDialog);
    qsa("[data-close-dialog]").forEach((button) => button.addEventListener("click", requestClosePlantDialog));
    $("plantEditDialog").addEventListener("click", (event) => {
        if (event.target.id === "plantEditDialog") {
            requestClosePlantDialog();
        }
    });
    $("closeDeletePlantDialog").addEventListener("click", closeDeletePlantDialog);
    qsa("[data-close-delete-plant-dialog]").forEach((button) => button.addEventListener("click", closeDeletePlantDialog));
    $("deletePlantDialog").addEventListener("click", (event) => {
        if (event.target.id === "deletePlantDialog") {
            closeDeletePlantDialog();
        }
    });
    $("closeWaterPlantDialog").addEventListener("click", closeWaterPlantDialog);
    qsa("[data-close-water-plant-dialog]").forEach((button) => button.addEventListener("click", closeWaterPlantDialog));
    $("waterPlantDialog").addEventListener("click", (event) => {
        if (event.target.id === "waterPlantDialog") {
            closeWaterPlantDialog();
        }
    });
    $("closePublicPlantDialog").addEventListener("click", closePublicPlantDialog);
    $("publicPlantDialog").addEventListener("click", (event) => {
        if (event.target.id === "publicPlantDialog") {
            closePublicPlantDialog();
        }
    });
    $("closePhotoPreviewDialog").addEventListener("click", photoPreview.closePhotoPreviewDialog);
    $("previousPhotoPreview").addEventListener("click", () => photoPreview.shiftPhotoPreview(-1));
    $("nextPhotoPreview").addEventListener("click", () => photoPreview.shiftPhotoPreview(1));
    $("photoPreviewDialog").addEventListener("click", (event) => {
        if (event.target.id === "photoPreviewDialog") {
            photoPreview.closePhotoPreviewDialog();
        }
    });

    adminController.wireEvents();

    document.body.addEventListener("click", async (event) => {
        const target = event.target.closest("button");
        if (!target) return;
        if (await adminController.handleButtonClick(target)) return;
        if (target.dataset.water) {
            const plant = state.plants.find((item) => item.id === target.dataset.water);
            openWaterPlantDialog(target.dataset.water, plant?.name || "");
        }
        if (target.dataset.calendarShift) {
            shiftWateringCalendarMonth(Number(target.dataset.calendarShift));
        }
        if (target.dataset.calendarWaterDate) {
            const plantId = $("plantDialogForm").elements.id.value;
            if (!plantId || !isWateringCalendarEditable()) return;

            const plantName = state.plants.find((item) => item.id === plantId)?.name || $("plantDialogForm").elements.name.value;
            openWaterPlantDialog(plantId, plantName, target.dataset.calendarWaterDate);
        }
        if (target.dataset.historyPhotoPreview) {
            const item = state.plantHistory.items.find((historyItem) => historyItem.id === target.dataset.historyPhotoPreview);
            if (!item?.photoDataUrl) return;

            photoPreview.openPlantPhotoPreview(
                state.plantHistory.plantId,
                item.photoDataUrl,
                state.plantHistory.plantName || t("plants.photo"),
                state.plantHistory.plantName || t("plants.photo"));
        }
        if (target.dataset.deleteLocation) {
            if (!confirmDelete("confirm.deleteLocation")) return;
            await withButtonLoading(target, "loading.deleting", async () => {
                await gardenApi.deleteLocation(target.dataset.deleteLocation);
                await loadGarden();
            });
        }
        if (target.dataset.editLocation) {
            openLocationDialog(target.dataset.editLocation);
        }
        if (target.dataset.editPlant) {
            await withButtonLoading(target, "loading.plant", async () => {
                await openPlantDialog(target.dataset.editPlant);
            });
        }
        if (target.dataset.publicPlant) {
            openPublicPlantDialog(target.dataset.publicPlant);
        }
        if (target.dataset.deletePlant) {
            const plant = state.plants.find((item) => item.id === target.dataset.deletePlant);
            openDeletePlantDialog(target.dataset.deletePlant, plant?.name || "");
        }
        if (target.dataset.deleteNote) {
            if (!confirmDelete("confirm.deleteNote")) return;
            const plantId = state.plantNotes.plantId;
            if (!plantId) return;

            await withButtonLoading(target, "loading.deleting", async () => {
                await plantsApi.deleteNote(plantId, target.dataset.deleteNote);
                await loadPlantNotes(plantId, state.plantNotes.page);
                if (!state.plantNotes.items.length && state.plantNotes.page > 1) {
                    await loadPlantNotes(plantId, state.plantNotes.page - 1);
                }
                await loadGarden();
                toast(t("toast.done"));
            });
        }
        if (target.dataset.toggleNoteReminder) {
            const plantId = state.plantNotes.plantId;
            if (!plantId) return;

            await withButtonLoading(target, "loading.saving", async () => {
                await plantsApi.updateNoteReminderStatus(plantId, target.dataset.toggleNoteReminder, { isResolved: target.dataset.reminderResolved === "true" });
                await loadPlantNotes(plantId, state.plantNotes.page);
                await loadGarden();
                toast(t("toast.saved"));
            });
        }
        if (target.dataset.historyDeleteNote) {
            if (!confirmDelete("confirm.deleteNote")) return;
            const plantId = state.plantHistory.plantId;
            if (!plantId) return;

            await withButtonLoading(target, "loading.deleting", async () => {
                await plantsApi.deleteNote(plantId, target.dataset.historyDeleteNote);
                await loadPlantHistory(plantId);
                await loadPlantNotes(plantId, state.plantNotes.page);
                await loadGarden();
                toast(t("toast.done"));
            });
        }
        if (target.dataset.historyToggleReminder) {
            const plantId = state.plantHistory.plantId;
            if (!plantId) return;

            await withButtonLoading(target, "loading.saving", async () => {
                await plantsApi.updateNoteReminderStatus(plantId, target.dataset.historyToggleReminder, { isResolved: target.dataset.reminderResolved === "true" });
                await loadPlantHistory(plantId);
                await loadPlantNotes(plantId, state.plantNotes.page);
                await loadGarden();
                toast(t("toast.saved"));
            });
        }
        if (target.dataset.deleteWatering) {
            if (!confirmDelete("confirm.deleteWatering")) return;
            const plantId = state.plantHistory.plantId;
            if (!plantId) return;

            await withButtonLoading(target, "loading.deleting", async () => {
                await plantsApi.deleteWatering(plantId, target.dataset.deleteWatering);
                await loadPlantHistory(plantId);
                await loadGarden();
                toast(t("toast.done"));
            });
        }
        if (target.dataset.deletePhoto) {
            if (!confirmDelete("confirm.deletePhoto")) return;
            const plantId = state.plantHistory.plantId;
            if (!plantId) return;

            await withButtonLoading(target, "loading.deleting", async () => {
                await plantsApi.deletePhoto(plantId, target.dataset.deletePhoto);
                await loadPlantHistory(plantId);
                await loadGarden();
                toast(t("toast.done"));
            });
        }
        if (target.dataset.copy) {
            await navigator.clipboard.writeText(target.dataset.copy);
            toast(target.dataset.copy);
        }
    });

    document.body.addEventListener("submit", async (event) => {
        const form = event.target.closest("[data-note-date-form]");
        if (!form) return;

        event.preventDefault();
        const plantId = state.plantHistory.plantId;
        if (!plantId) return;

        await withButtonLoading(event.submitter, "loading.saving", async () => {
            await plantsApi.updateNoteDate(plantId, form.dataset.noteDateForm, { createdOn: form.elements.createdOn.value });
            await loadPlantHistory(plantId);
            await loadPlantNotes(plantId, state.plantNotes.page);
            toast(t("toast.saved"));
        });
    });
}

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
