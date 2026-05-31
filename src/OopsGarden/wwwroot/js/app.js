import { createPhotoPreviewController } from "./photo-preview.js";

const state = {
    me: null,
    lang: localStorage.getItem("oopsGarden.lang") || "en",
    theme: localStorage.getItem("oopsGarden.theme") || "greenhouse",
    view: "garden",
    route: location.pathname.toLowerCase(),
    publicGarden: null,
    hideUsedInvites: false,
    dict: {},
    locations: [],
    plants: [],
    plantNotes: {
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
    },
    plantNotesRequestId: 0,
    plantHistory: {
        plantId: null,
        plantName: "",
        isPublic: false,
        publicGardenId: null,
        isLoading: false,
        items: []
    },
    wateringCalendarMonth: null,
    plantHistoryRequestId: 0,
    plantDialogBaseline: null,
    photoPreview: {
        items: [],
        index: 0
    }
};
const defaultAvatarUrl = "/img/garden-user.png?v=20260531-8";
const defaultPlantPhotoUrl = "/img/default-plant.png?v=20260531-8";
const resourceVersion = "20260531-8";
const maxUploadImageSide = 1080;
const loadingState = new Set();

const $ = (id) => document.getElementById(id);
const qs = (selector) => document.querySelector(selector);
const qsa = (selector) => [...document.querySelectorAll(selector)];

async function api(url, options = {}) {
    const response = await fetch(url, {
        headers: { "Content-Type": "application/json", ...(options.headers || {}) },
        ...options
    });
    if (!response.ok) {
        const text = await response.text();
        const error = new Error(text || response.statusText);
        error.status = response.status;
        throw error;
    }
    if (response.status === 204) return null;
    const text = await response.text();
    return text ? JSON.parse(text) : null;
}

async function loadLanguage(lang) {
    const response = await fetch(`/resources/${lang}.json?v=${resourceVersion}`);
    state.dict = await response.json();
    state.lang = lang;
    document.documentElement.lang = lang;
    localStorage.setItem("oopsGarden.lang", lang);
    $("languageSelect").value = lang;
    qsa("[data-i18n]").forEach((el) => {
        el.textContent = t(el.dataset.i18n);
    });
    qsa("[data-i18n-placeholder]").forEach((el) => {
        el.placeholder = t(el.dataset.i18nPlaceholder);
    });
    qsa("[data-i18n-aria-label]").forEach((el) => {
        el.setAttribute("aria-label", t(el.dataset.i18nAriaLabel));
    });
}

function applyTheme(theme) {
    state.theme = theme === "dark-forest" ? "dark-forest" : "greenhouse";
    document.documentElement.dataset.theme = state.theme;
    localStorage.setItem("oopsGarden.theme", state.theme);
    $("themeSelect").value = state.theme;
}

function t(key) {
    return state.dict[key] || key;
}

const photoPreview = createPhotoPreviewController({ state, defaultPlantPhotoUrl, t, $ });

function toast(message) {
    const el = $("toast");
    el.textContent = message;
    el.hidden = false;
    clearTimeout(toast.timer);
    toast.timer = setTimeout(() => { el.hidden = true; }, 2600);
}

function showError(error) {
    toast(error?.message || t("toast.error"));
}

function loadingMarkup(messageKey = "loading.generic") {
    return `<div class="loading-state" role="status" aria-live="polite">
        <span class="notes-spinner" aria-hidden="true"></span>
        <span>${t(messageKey)}</span>
    </div>`;
}

function setRegionLoading(elementOrId, isLoading, messageKey = "loading.generic") {
    const element = typeof elementOrId === "string" ? $(elementOrId) : elementOrId;
    if (!element) return;
    element.setAttribute("aria-busy", isLoading ? "true" : "false");
    if (isLoading) {
        element.innerHTML = loadingMarkup(messageKey);
    }
}

function setBusyOverlay(elementOrId, isLoading, messageKey = "loading.generic") {
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

function setButtonLoading(button, isLoading, messageKey = "loading.generic") {
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

async function withButtonLoading(button, messageKey, action) {
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

function confirmDelete(key) {
    return window.confirm(t(key));
}

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

function toDateInputValue(value) {
    if (!value) return "";
    return new Date(value).toISOString().slice(0, 10);
}

function todayKey() {
    const now = new Date();
    return `${now.getFullYear()}-${String(now.getMonth() + 1).padStart(2, "0")}-${String(now.getDate()).padStart(2, "0")}`;
}

function isReminderOverdue(note) {
    return Boolean(note?.isReminder && !note.isReminderResolved && note.reminderDate && note.reminderDate < todayKey());
}

function reminderStateClass(note) {
    if (!note?.isReminder) return "";
    if (note.isReminderResolved) return " reminder-resolved";
    return isReminderOverdue(note) ? " reminder-overdue" : " reminder-active";
}

function reminderMeta(note) {
    if (!note?.isReminder) return "";
    const stateKey = note.isReminderResolved
        ? "notes.reminderResolved"
        : isReminderOverdue(note)
            ? "notes.reminderOverdue"
            : "notes.reminderActive";
    return `<span class="reminder-meta">${t(stateKey)}: ${escapeHtml(note.reminderDate || "")}</span>`;
}

function toMonthKey(value) {
    const date = value ? new Date(value) : new Date();
    return `${date.getFullYear()}-${String(date.getMonth() + 1).padStart(2, "0")}`;
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

async function fileToDataUrl(file) {
    if (!file) return null;
    if (file.type.startsWith("image/")) {
        return resizeImageToDataUrl(file, maxUploadImageSide);
    }

    return new Promise((resolve, reject) => {
        const reader = new FileReader();
        reader.onload = () => resolve(reader.result);
        reader.onerror = () => reject(reader.error);
        reader.readAsDataURL(file);
    });
}

async function resizeImageToDataUrl(file, maxSide) {
    const image = await loadImage(file);
    const scale = Math.min(1, maxSide / Math.max(image.naturalWidth, image.naturalHeight));
    if (scale === 1) {
        return readFileAsDataUrl(file);
    }

    const canvas = document.createElement("canvas");
    canvas.width = Math.round(image.naturalWidth * scale);
    canvas.height = Math.round(image.naturalHeight * scale);
    const context = canvas.getContext("2d");
    context.drawImage(image, 0, 0, canvas.width, canvas.height);
    return canvas.toDataURL(file.type === "image/png" ? "image/png" : "image/jpeg", 0.86);
}

function loadImage(file) {
    return new Promise((resolve, reject) => {
        const image = new Image();
        image.onload = () => {
            URL.revokeObjectURL(image.src);
            resolve(image);
        };
        image.onerror = () => {
            URL.revokeObjectURL(image.src);
            reject(new Error("Image could not be loaded."));
        };
        image.src = URL.createObjectURL(file);
    });
}

function readFileAsDataUrl(file) {
    return new Promise((resolve, reject) => {
        const reader = new FileReader();
        reader.onload = () => resolve(reader.result);
        reader.onerror = () => reject(reader.error);
        reader.readAsDataURL(file);
    });
}

function formData(form) {
    return Object.fromEntries(new FormData(form).entries());
}

async function refreshMe() {
    try {
        state.me = await api("/api/me");
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
    if (isAdmin && activeView === "admin") loadAdmin();
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

async function loadGarden() {
    if (loadingState.has("garden")) return;
    loadingState.add("garden");
    setRegionLoading("plantList", true, "loading.garden");
    try {
        [state.plants, state.locations] = await Promise.all([
            api("/api/garden/summary"),
            api("/api/garden/locations")
        ]);
        $("plantList").setAttribute("aria-busy", "false");
        renderPlantGroups($("plantList"), state.plants, { isPublic: false });
    } catch (error) {
        $("plantList").setAttribute("aria-busy", "false");
        $("plantList").innerHTML = `<p class="muted">${t("toast.error")}</p>`;
        showError(error);
    } finally {
        loadingState.delete("garden");
    }
}

function renderPlantGroups(list, plants, options) {
    list.innerHTML = "";
    const groups = groupPlantsByLocation(plants, options);
    if (!groups.length) {
        list.innerHTML = `<p class="muted">${options.isPublic ? t("empty.publicGarden") : t("empty.garden")}</p>`;
        return;
    }

    for (const group of groups) {
        const section = document.createElement("section");
        section.className = "location-section";
        const title = !options.isPublic && group.id
            ? `<button type="button" class="location-title-button" data-edit-location="${group.id}">${escapeHtml(group.name)}</button>`
            : `<h2>${escapeHtml(group.name)}</h2>`;
        section.innerHTML = `
            <div class="location-head">
                ${title}
                <span>${group.plants.length}</span>
            </div>
            <div class="plant-tile-grid"></div>`;
        const grid = section.querySelector(".plant-tile-grid");

        for (const plant of group.plants) {
            const tile = document.createElement("article");
            tile.className = `plant-card${plant.hasOverdueReminders ? " has-overdue-reminders" : ""}`;
            tile.innerHTML = `
                <button type="button" class="plant-photo-button"${options.isPublic ? ` data-public-plant="${plant.id}"` : ` data-edit-plant="${plant.id}"`}>
                    <img alt="" src="${plant.photoDataUrl || defaultPlantPhotoUrl}">
                    ${plant.hasOverdueReminders ? `<span class="plant-warning" aria-label="${t("notes.overdue")}">!</span>` : ""}
                </button>
                <div class="plant-body">
                    <h3>${escapeHtml(plant.name)}</h3>
                    <p>${escapeHtml(plant.description || "")}</p>
                    ${options.isPublic
                        ? ""
                        : `<div class="meta">
                            <span class="chip">${t("plants.plantedOn")}: ${plant.plantedOn || t("common.none")}</span>
                            <span class="chip">${t("plants.lastWatered")}: ${plant.lastWateredAt ? new Date(plant.lastWateredAt).toLocaleDateString() : t("common.never")}</span>
                        </div>
                        <button data-water="${plant.id}">${t("actions.water")}</button>`}
                </div>`;
            grid.append(tile);
        }

        list.append(section);
    }
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
    state.publicGarden = await api(`/api/public/gardens/${publicGardenId}`);
    renderShell();
    return true;
}

function groupPlantsByLocation(plants, options = {}) {
    const groups = new Map();
    if (!options.isPublic) {
        for (const location of state.locations) {
            groups.set(location.id, { id: location.id, name: location.name, plants: [] });
        }
    }

    for (const plant of plants) {
        const id = plant.location?.id || "";
        const name = plant.location?.name || t("common.none");
        if (!groups.has(id)) {
            groups.set(id, { id, name, plants: [] });
        }

        groups.get(id).plants.push(plant);
    }

    return [...groups.values()]
        .map((group) => ({
            ...group,
            plants: group.plants.sort((left, right) => left.name.localeCompare(right.name))
        }))
        .sort((left, right) => left.name.localeCompare(right.name));
}

async function openPlantDialog(id) {
    resetPlantDialogMode();
    $("plantEditDialog").hidden = false;
    setBusyOverlay("plantDialogForm", true, "loading.plant");
    try {
        if (!state.locations.length) {
            state.locations = await api("/api/garden/locations");
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
            state.locations = await api("/api/garden/locations");
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

function setReminderDateFieldVisibility() {
    const isReminder = $("plantNoteIsReminder").checked;
    $("plantNoteReminderDateField").hidden = !isReminder;
    $("plantNoteReminderDate").hidden = !isReminder;
    $("plantNoteReminderDate").required = isReminder;
}

function setPlantNotesMode(mode) {
    state.plantNotes.mode = mode === "overdue" ? "overdue" : "all";
    $("plantNotesAllTab").classList.toggle("is-active", state.plantNotes.mode === "all");
    $("plantNotesOverdueTab").classList.toggle("is-active", state.plantNotes.mode === "overdue");
    const hideComposer = state.plantNotes.mode === "overdue" || state.plantNotes.isPublic;
    $("plantNoteComposer").hidden = hideComposer;
    $("plantNoteComposerActions").hidden = hideComposer;
}

async function loadPlantNotes(plantId, page = state.plantNotes.page, options = {}) {
    const isPublic = Boolean(options.isPublic ?? state.plantNotes.isPublic);
    const publicGardenId = options.publicGardenId ?? state.plantNotes.publicGardenId;
    const mode = isPublic ? "all" : (options.mode ?? state.plantNotes.mode ?? "all");
    const requestId = state.plantNotesRequestId + 1;
    state.plantNotesRequestId = requestId;
    state.plantNotes = {
        plantId,
        publicGardenId,
        isPublic,
        isLoading: true,
        mode,
        page,
        pageSize: state.plantNotes.pageSize,
        total: 0,
        items: [],
        hasPrevious: false,
        hasNext: false
    };
    renderPlantNotes();

    const notesPath = mode === "overdue" ? "notes/overdue" : "notes";
    const url = isPublic
        ? `/api/public/gardens/${publicGardenId}/plants/${plantId}/notes?page=${page}&pageSize=${state.plantNotes.pageSize}`
        : `/api/garden/plants/${plantId}/${notesPath}?page=${page}&pageSize=${state.plantNotes.pageSize}`;
    let result;
    try {
        result = await api(url);
    } catch (error) {
        if (requestId === state.plantNotesRequestId) {
            state.plantNotes = {
                ...state.plantNotes,
                isLoading: false,
                total: 0,
                items: [],
                hasPrevious: false,
                hasNext: false
            };
            renderPlantNotes();
        }
        throw error;
    }

    if (requestId !== state.plantNotesRequestId) {
        return;
    }

    state.plantNotes = {
        plantId,
        publicGardenId,
        isPublic,
        isLoading: false,
        mode,
        page: result.page,
        pageSize: result.pageSize,
        total: result.total,
        items: result.items || [],
        hasPrevious: result.hasPrevious,
        hasNext: result.hasNext
    };
    renderPlantNotes();
}

function renderPlantNotes() {
    const list = $("plantNotesList");
    list.innerHTML = "";
    setPlantNotesMode(state.plantNotes.mode);
    $("plantNotesCount").textContent = String(state.plantNotes.total);
    $("plantNotesPage").textContent = t("notes.page").replace("{page}", state.plantNotes.page);
    $("previousPlantNotesPage").disabled = !state.plantNotes.hasPrevious;
    $("nextPlantNotesPage").disabled = !state.plantNotes.hasNext;

    if (state.plantNotes.isLoading) {
        $("plantNotesCount").textContent = "";
        $("previousPlantNotesPage").disabled = true;
        $("nextPlantNotesPage").disabled = true;
        list.innerHTML = `
            <div class="notes-loading" role="status" aria-live="polite">
                <span class="notes-spinner" aria-hidden="true"></span>
                <span>${t("notes.loading")}</span>
            </div>`;
        return;
    }

    if (!state.plantNotes.items.length) {
        list.innerHTML = `<p class="muted">${state.plantNotes.mode === "overdue" ? t("notes.overdueEmpty") : t("notes.empty")}</p>`;
        return;
    }

    for (const note of state.plantNotes.items) {
        const row = document.createElement("article");
        row.className = `plant-note ${note.isAutomatic ? "automatic-note" : "user-note"}${reminderStateClass(note)}`;
        row.innerHTML = `
            <div>
                <time>${new Date(note.createdAt).toLocaleString()}</time>
                ${reminderMeta(note)}
                <p>${escapeHtml(note.text)}</p>
            </div>
            ${state.plantNotes.isPublic
                ? ""
                : `<div class="note-actions">
                    ${note.isReminder
                        ? `<button type="button" class="ghost compact" data-toggle-note-reminder="${note.id}" data-reminder-resolved="${note.isReminderResolved ? "false" : "true"}">${note.isReminderResolved ? t("notes.reopen") : t("notes.resolve")}</button>`
                        : ""}
                    <button type="button" class="note-delete" data-delete-note="${note.id}" aria-label="${t("actions.delete")}">&times;</button>
                </div>`}`;
        list.append(row);
    }
}

async function openPlantHistoryPage() {
    const plantId = state.plantNotes.plantId || $("plantDialogForm").elements.id.value;
    if (!plantId) return;
    if (!confirmDiscardPlantChanges()) return;

    const isPublic = $("plantDialogForm").dataset.public === "true";
    const plant = isPublic
        ? state.publicGarden?.plants?.find((item) => item.id === plantId)
        : state.plants.find((item) => item.id === plantId);
    const plantName = plant?.name || $("plantDialogTitle").textContent || t("history.title");
    state.plantHistory = {
        plantId,
        plantName,
        isPublic,
        publicGardenId: isPublic ? state.publicGarden?.id : null,
        isLoading: true,
        items: []
    };
    closePlantDialog();
    state.view = "plantHistory";
    renderShell();
    await loadPlantHistory(plantId, {
        plantName,
        isPublic,
        publicGardenId: isPublic ? state.publicGarden?.id : null
    });
}

async function loadPlantHistory(plantId = state.plantHistory.plantId, options = {}) {
    if (!plantId) return;

    const requestId = state.plantHistoryRequestId + 1;
    state.plantHistoryRequestId = requestId;
    state.plantHistory = {
        ...state.plantHistory,
        plantId,
        plantName: options.plantName ?? state.plantHistory.plantName,
        isPublic: Boolean(options.isPublic ?? state.plantHistory.isPublic),
        publicGardenId: options.publicGardenId ?? state.plantHistory.publicGardenId,
        isLoading: true,
        items: []
    };
    if (options.renderPage !== false) {
        renderPlantHistory();
    }

    const isPublic = Boolean(options.isPublic ?? state.plantHistory.isPublic);
    const publicGardenId = options.publicGardenId ?? state.plantHistory.publicGardenId;
    const url = isPublic
        ? `/api/public/gardens/${publicGardenId}/plants/${plantId}/history`
        : `/api/garden/plants/${plantId}/history`;
    const result = await api(url);
    if (requestId !== state.plantHistoryRequestId) {
        return;
    }

    state.plantHistory = {
        ...state.plantHistory,
        isLoading: false,
        items: result || []
    };
    if (options.renderCalendar) {
        renderWateringCalendar(state.plantHistory.items);
        renderPlantTimelineWarning();
    }
    if (options.renderPage !== false) {
        renderPlantHistory();
    }
}

function renderPlantHistory() {
    $("plantHistoryTitle").textContent = state.plantHistory.plantName
        ? `${t("history.title")}: ${state.plantHistory.plantName}`
        : t("history.title");
    renderPlantHistoryDetails();

    const list = $("plantHistoryList");
    const readOnly = Boolean(state.plantHistory.isPublic);
    if (state.plantHistory.isLoading) {
        list.innerHTML = `
            <div class="notes-loading" role="status" aria-live="polite">
                <span class="notes-spinner" aria-hidden="true"></span>
                <span>${t("history.loading")}</span>
            </div>`;
        return;
    }

    if (!state.plantHistory.items.length) {
        list.innerHTML = `<p class="muted">${t("history.empty")}</p>`;
        return;
    }

    list.innerHTML = "";
    for (const item of state.plantHistory.items) {
        const isNote = item.type === "note";
        const isPhoto = item.type === "photo";
        const row = document.createElement("article");
        row.className = `history-item ${isPhoto ? "history-photo-shot" : isNote ? "history-note" : "history-watering"}${item.isAutomatic ? " history-automatic" : ""}${reminderStateClass(item)}`;
        row.innerHTML = `
            <div>
                <h3>${isPhoto ? t("history.photoTaken") : isNote && item.isReminder ? t("history.reminder") : isNote ? t("history.note") : t("history.watering")}</h3>
                <time>${new Date(item.occurredAt).toLocaleString()}</time>
                ${isNote ? reminderMeta(item) : ""}
                ${isPhoto
                    ? `<button type="button" class="history-photo-preview image-preview-button" data-history-photo-preview="${item.id}" aria-label="${t("history.photoTaken")}">
                        <img src="${item.photoDataUrl || defaultPlantPhotoUrl}" alt="">
                    </button>`
                    : isNote ? `<p>${escapeHtml(item.text)}</p>` : ""}
            </div>
            <div class="history-actions">
                ${readOnly
                    ? ""
                    : isPhoto
                        ? `<button type="button" class="note-delete" data-delete-photo="${item.id}" aria-label="${t("actions.delete")}">&times;</button>`
                        : isNote
                        ? `<form class="history-date-form" data-note-date-form="${item.id}">
                        <label>
                            ${t("history.date")}
                            <input name="createdOn" type="date" value="${toDateInputValue(item.occurredAt)}" required>
                        </label>
                        <button type="submit" class="ghost">${t("actions.save")}</button>
                    </form>
                    ${item.isReminder ? `<button type="button" class="ghost compact" data-history-toggle-reminder="${item.id}" data-reminder-resolved="${item.isReminderResolved ? "false" : "true"}">${item.isReminderResolved ? t("notes.reopen") : t("notes.resolve")}</button>` : ""}
                    <button type="button" class="note-delete" data-history-delete-note="${item.id}" aria-label="${t("actions.delete")}">&times;</button>`
                        : `<button type="button" class="note-delete" data-delete-watering="${item.id}" aria-label="${t("actions.delete")}">&times;</button>`}
            </div>`;
        list.append(row);
    }
}

function renderPlantHistoryDetails() {
    const plant = state.plantHistory.isPublic
        ? state.publicGarden?.plants?.find((item) => item.id === state.plantHistory.plantId)
        : state.plants.find((item) => item.id === state.plantHistory.plantId);
    $("plantHistoryDetails").hidden = !plant;
    if (!plant) {
        return;
    }

    $("historyPlantName").textContent = plant.name || "";
    $("historyPlantDescription").textContent = plant.description || "";
    $("historyPlantSoil").textContent = plant.soil || "";
    $("historyPlantLocation").textContent = plant.location?.name || t("common.none");
    $("historyPlantPlantedOn").textContent = plant.plantedOn || t("common.none");
    $("historyPlantPhoto").src = plant.photoDataUrl || defaultPlantPhotoUrl;
    $("historyPlantPhoto").alt = plant.name || "";
}

function renderWateringCalendar(items) {
    const calendar = $("wateringHeatmap");
    calendar.innerHTML = "";
    const isEditable = isWateringCalendarEditable();

    const [year, month] = (state.wateringCalendarMonth || toMonthKey(new Date()))
        .split("-")
        .map((part) => Number(part));
    const monthDate = new Date(year, month - 1, 1);
    const daysInMonth = new Date(year, month, 0).getDate();
    const counts = new Map();
    for (const item of items.filter((entry) => entry.type === "watering")) {
        const key = toDateInputValue(item.occurredAt);
        counts.set(key, (counts.get(key) || 0) + 1);
    }

    const head = document.createElement("div");
    head.className = "watering-calendar-head";
    head.innerHTML = `
        <button type="button" class="ghost" data-calendar-shift="-1" aria-label="${t("notes.previous")}">&lt;</button>
        <strong>${monthDate.toLocaleDateString(undefined, { month: "long", year: "numeric" })}</strong>
        <button type="button" class="ghost" data-calendar-shift="1" aria-label="${t("notes.next")}">&gt;</button>`;
    calendar.append(head);

    const grid = document.createElement("div");
    grid.className = "watering-calendar-grid";
    const firstWeekday = (monthDate.getDay() + 6) % 7;
    for (let blank = 0; blank < firstWeekday; blank += 1) {
        const spacer = document.createElement("span");
        spacer.className = "watering-calendar-spacer";
        grid.append(spacer);
    }

    for (let dayNumber = 1; dayNumber <= daysInMonth; dayNumber += 1) {
        const key = `${year}-${String(month).padStart(2, "0")}-${String(dayNumber).padStart(2, "0")}`;
        const count = counts.get(key) || 0;
        const cell = document.createElement(isEditable ? "button" : "span");
        cell.className = `watering-calendar-day${count ? " watered" : ""}${count > 1 ? " multiple" : ""}`;
        cell.textContent = String(dayNumber);
        cell.title = `${key}: ${count ? `${count} ${t("history.watering").toLowerCase()}` : t("common.none")}`;
        if (isEditable) {
            cell.type = "button";
            cell.dataset.calendarWaterDate = key;
            cell.setAttribute("aria-label", `${t("actions.water")} ${key}`);
        }
        grid.append(cell);
    }
    calendar.append(grid);
}

function isWateringCalendarEditable() {
    const form = $("plantDialogForm");
    return !$("plantEditDialog").hidden
        && form.dataset.public !== "true"
        && Boolean(form.elements.id.value)
        && $("plantEditMode").checked;
}

function shiftWateringCalendarMonth(delta) {
    const [year, month] = (state.wateringCalendarMonth || toMonthKey(new Date()))
        .split("-")
        .map((part) => Number(part));
    const next = new Date(year, month - 1 + delta, 1);
    state.wateringCalendarMonth = toMonthKey(next);
    renderWateringCalendar(state.plantHistory.items);
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

function renderLocationSelect(select, selectedValue) {
    select.innerHTML = `<option value="">${t("common.none")}</option>`;
    for (const location of state.locations) {
        select.append(new Option(location.name, location.id));
    }
    select.value = selectedValue || "";
}

function getLocationName(locationId) {
    if (!locationId) {
        return t("common.none");
    }

    return state.locations.find((location) => location.id === locationId)?.name || t("common.none");
}

function renderChangeNote(templateKey, fromValue, toValue) {
    return t(templateKey)
        .replace("{from}", fromValue)
        .replace("{to}", toValue);
}

async function loadAdmin() {
    if (loadingState.has("admin")) return;
    loadingState.add("admin");
    setRegionLoading("inviteList", true, "loading.admin");
    setRegionLoading("userList", true, "loading.admin");
    let invites;
    let users;
    try {
        [invites, users] = await Promise.all([api("/api/admin/invites"), api("/api/admin/users")]);
    } catch (error) {
        $("inviteList").setAttribute("aria-busy", "false");
        $("userList").setAttribute("aria-busy", "false");
        $("inviteList").innerHTML = `<p class="muted">${t("toast.error")}</p>`;
        $("userList").innerHTML = `<p class="muted">${t("toast.error")}</p>`;
        showError(error);
        return;
    } finally {
        loadingState.delete("admin");
    }
    $("toggleUsedInvitesBtn").textContent = state.hideUsedInvites
        ? t("admin.showUsedInvites")
        : t("admin.hideUsedInvites");
    const inviteList = $("inviteList");
    inviteList.setAttribute("aria-busy", "false");
    inviteList.innerHTML = "";
    for (const invite of invites.filter((invite) => !state.hideUsedInvites || !invite.usedAt)) {
        const url = `${location.origin}/?invite=${invite.code}`;
        const status = invite.usedAt
            ? t("admin.inviteUsed")
            : invite.isRevoked
                ? t("admin.inviteRevoked")
                : t("admin.inviteOpen");
        const row = document.createElement("div");
        row.className = "list-row";
        const canDelete = !invite.usedAt;
        row.innerHTML = `<div><strong>${status}</strong>
            <div class="muted">${url}</div></div>
            <div class="row-actions">
                <button class="ghost" data-copy="${url}">${t("actions.copy")}</button>
                ${canDelete ? `<button class="danger" data-delete-invite="${invite.id}">${t("actions.delete")}</button>` : ""}
            </div>`;
        inviteList.append(row);
    }

    const userList = $("userList");
    userList.setAttribute("aria-busy", "false");
    userList.innerHTML = "";
    for (const user of users) {
        const row = document.createElement("div");
        row.className = "list-row";
        row.innerHTML = `<div><strong>${escapeHtml(user.displayName)}</strong>
            <div class="muted">${escapeHtml(user.email)} · ${user.plants} ${t("admin.userPlants")}</div></div>
            <div class="row-actions">
                <button class="ghost" data-block-user="${user.id}" data-block-value="${!user.isBlocked}">${user.isBlocked ? t("common.unblock") : t("common.block")}</button>
                <button class="danger" data-delete-user="${user.id}">${t("actions.delete")}</button>
            </div>`;
        userList.append(row);
    }
}

function escapeHtml(value) {
    const div = document.createElement("div");
    div.textContent = value ?? "";
    return div.innerHTML;
}

function wireEvents() {
    $("themeSelect").addEventListener("change", (event) => {
        applyTheme(event.target.value);
    });

    $("languageSelect").addEventListener("change", async (event) => {
        await loadLanguage(event.target.value);
        if (state.me?.authenticated && state.me.role !== "Admin") {
            await api("/api/auth/settings", {
                method: "POST",
                body: JSON.stringify({
                    displayName: state.me.name || t("user.defaultName"),
                    language: state.lang,
                    avatarDataUrl: state.me.avatar || null,
                    isGardenPublic: Boolean(state.me.isGardenPublic)
                })
            });
        }
        renderShell();
    });

    $("loginForm").addEventListener("submit", async (event) => {
        event.preventDefault();
        await withButtonLoading(event.submitter, "loading.login", async () => {
            await api("/api/auth/login", { method: "POST", body: JSON.stringify(formData(event.currentTarget)) });
            await refreshMe();
        });
    });

    $("adminLoginForm").addEventListener("submit", async (event) => {
        event.preventDefault();
        await withButtonLoading(event.submitter, "loading.login", async () => {
            await api("/api/auth/admin-login", { method: "POST", body: JSON.stringify(formData(event.currentTarget)) });
            await refreshMe();
        });
    });

    $("registerForm").addEventListener("submit", async (event) => {
        event.preventDefault();
        await withButtonLoading(event.submitter, "loading.saving", async () => {
            const data = formData(event.currentTarget);
            data.language = state.lang;
            await api("/api/auth/register", { method: "POST", body: JSON.stringify(data) });
            await refreshMe();
        });
    });

    $("logoutBtn").addEventListener("click", async (event) => {
        await withButtonLoading(event.currentTarget, "loading.generic", async () => {
            await api("/api/auth/logout", { method: "POST" });
            await refreshMe();
        });
    });

    $("settingsForm").addEventListener("submit", async (event) => {
        event.preventDefault();
        await withButtonLoading(event.submitter, "loading.saving", async () => {
            const form = event.currentTarget;
            const avatarDataUrl = form.dataset.avatarPreview || state.me.avatar || null;
            await api("/api/auth/settings", {
                method: "POST",
                body: JSON.stringify({
                    displayName: form.displayName.value,
                    language: state.lang,
                    avatarDataUrl,
                    isGardenPublic: form.elements.isGardenPublic.checked
                })
            });
            await refreshMe();
            resetAvatarPreview();
            toast(t("toast.saved"));
        });
    });

    qs("#settingsForm [name=avatar]").addEventListener("change", async (event) => {
        const form = $("settingsForm");
        const avatarDataUrl = await fileToDataUrl(event.target.files[0]);
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
                await api(id ? `/api/garden/locations/${id}` : "/api/garden/locations", {
                    method: id ? "PUT" : "POST",
                    body: JSON.stringify({ name: form.elements.name.value })
                });
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
            await api(`/api/garden/locations/${id}`, { method: "DELETE" });
            closeLocationDialog();
            await loadGarden();
            toast(t("toast.done"));
        });
    });

    qs("#plantDialogForm [name=photo]").addEventListener("change", async (event) => {
        const form = $("plantDialogForm");
        const photoDataUrl = await fileToDataUrl(event.target.files[0]);
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
                const photoDataUrl = await fileToDataUrl(form.photo.files[0]);
                const id = form.elements.id.value;
                const previousPlant = id ? state.plants.find((plant) => plant.id === id) : null;
                const previousLocationId = previousPlant?.location?.id || previousPlant?.locationId || "";
                const nextLocationId = form.elements.locationId.value || "";
                const nextName = form.elements.name.value;
                const nextDescription = form.elements.description.value;
                const nextSoil = form.elements.soil.value;
                const payload = {
                    name: nextName,
                    description: nextDescription,
                    soil: nextSoil,
                    locationId: nextLocationId || null,
                    plantedOn: form.elements.plantedOn.value || null,
                    lastWateredOn: null,
                    photoDataUrl: form.dataset.photoPreview || photoDataUrl || form.dataset.photo || null
                };
                await api(id ? `/api/garden/plants/${id}` : "/api/garden/plants", {
                    method: id ? "PUT" : "POST",
                    body: JSON.stringify(payload)
                });
                if (id && previousPlant) {
                    const changeNotes = [];
                    if ((previousPlant.name || "") !== nextName) {
                        changeNotes.push(renderChangeNote("notes.nameChanged", previousPlant.name || "", nextName));
                    }
                    if ((previousPlant.description || "") !== nextDescription) {
                        changeNotes.push(renderChangeNote("notes.descriptionChanged", previousPlant.description || "", nextDescription));
                    }
                    if ((previousPlant.soil || "") !== nextSoil) {
                        changeNotes.push(renderChangeNote("notes.soilChanged", previousPlant.soil || "", nextSoil));
                    }
                    if (previousLocationId !== nextLocationId) {
                        changeNotes.push(renderChangeNote(
                            "notes.locationChanged",
                            getLocationName(previousLocationId),
                            getLocationName(nextLocationId)));
                    }

                    for (const text of changeNotes) {
                        await api(`/api/garden/plants/${id}/notes`, {
                            method: "POST",
                            body: JSON.stringify({ text, isAutomatic: true })
                        });
                    }
                }
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
        if (!confirmDelete("confirm.deletePlant")) return;
        await withButtonLoading(event.currentTarget, "loading.deleting", async () => {
            await api(`/api/garden/plants/${id}`, { method: "DELETE" });
            closePlantDialog();
            await loadGarden();
            toast(t("toast.done"));
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
            await api(`/api/garden/plants/${plantId}/notes`, {
                method: "POST",
                body: JSON.stringify({ text, isAutomatic: false, isReminder, reminderDate: isReminder ? reminderDate : null })
            });
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

    $("createInviteBtn").addEventListener("click", async (event) => {
        await withButtonLoading(event.currentTarget, "loading.saving", async () => {
            await api("/api/admin/invites", { method: "POST" });
            await loadAdmin();
        });
    });

    $("toggleUsedInvitesBtn").addEventListener("click", async (event) => {
        state.hideUsedInvites = !state.hideUsedInvites;
        await withButtonLoading(event.currentTarget, "loading.generic", loadAdmin);
    });

    document.body.addEventListener("click", async (event) => {
        const target = event.target.closest("button");
        if (!target) return;
        if (target.dataset.water) {
            await withButtonLoading(target, "loading.saving", async () => {
                await api(`/api/garden/plants/${target.dataset.water}/water`, { method: "POST" });
                await loadGarden();
            });
        }
        if (target.dataset.calendarShift) {
            shiftWateringCalendarMonth(Number(target.dataset.calendarShift));
        }
        if (target.dataset.calendarWaterDate) {
            const plantId = $("plantDialogForm").elements.id.value;
            if (!plantId || !isWateringCalendarEditable()) return;

            target.disabled = true;
            try {
                await api(`/api/garden/plants/${plantId}/waterings`, {
                    method: "POST",
                    body: JSON.stringify({ wateredOn: target.dataset.calendarWaterDate })
                });
                await loadGarden();
                const plant = state.plants.find((item) => item.id === plantId);
                $("plantDialogForm").elements.lastWateredOn.value = toDateInputValue(plant?.lastWateredAt);
                await loadPlantHistory(plantId, { renderPage: false, renderCalendar: true });
                toast(t("toast.saved"));
            } catch (error) {
                showError(error);
            } finally {
                target.disabled = false;
            }
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
                await api(`/api/garden/locations/${target.dataset.deleteLocation}`, { method: "DELETE" });
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
            if (!confirmDelete("confirm.deletePlant")) return;
            await withButtonLoading(target, "loading.deleting", async () => {
                await api(`/api/garden/plants/${target.dataset.deletePlant}`, { method: "DELETE" });
                await loadGarden();
            });
        }
        if (target.dataset.deleteNote) {
            if (!confirmDelete("confirm.deleteNote")) return;
            const plantId = state.plantNotes.plantId;
            if (!plantId) return;

            await withButtonLoading(target, "loading.deleting", async () => {
                await api(`/api/garden/plants/${plantId}/notes/${target.dataset.deleteNote}`, { method: "DELETE" });
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
                await api(`/api/garden/plants/${plantId}/notes/${target.dataset.toggleNoteReminder}/reminder-status`, {
                    method: "PUT",
                    body: JSON.stringify({ isResolved: target.dataset.reminderResolved === "true" })
                });
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
                await api(`/api/garden/plants/${plantId}/notes/${target.dataset.historyDeleteNote}`, { method: "DELETE" });
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
                await api(`/api/garden/plants/${plantId}/notes/${target.dataset.historyToggleReminder}/reminder-status`, {
                    method: "PUT",
                    body: JSON.stringify({ isResolved: target.dataset.reminderResolved === "true" })
                });
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
                await api(`/api/garden/plants/${plantId}/waterings/${target.dataset.deleteWatering}`, { method: "DELETE" });
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
                await api(`/api/garden/plants/${plantId}/photos/${target.dataset.deletePhoto}`, { method: "DELETE" });
                await loadPlantHistory(plantId);
                await loadGarden();
                toast(t("toast.done"));
            });
        }
        if (target.dataset.copy) {
            await navigator.clipboard.writeText(target.dataset.copy);
            toast(target.dataset.copy);
        }
        if (target.dataset.deleteInvite) {
            if (!confirmDelete("confirm.deleteInvite")) return;
            await withButtonLoading(target, "loading.deleting", async () => {
                await api(`/api/admin/invites/${target.dataset.deleteInvite}`, { method: "DELETE" });
                await loadAdmin();
            });
        }
        if (target.dataset.blockUser) {
            await withButtonLoading(target, "loading.saving", async () => {
                await api(`/api/admin/users/${target.dataset.blockUser}/block`, {
                    method: "POST",
                    body: JSON.stringify({ isBlocked: target.dataset.blockValue === "true" })
                });
                await loadAdmin();
            });
        }
        if (target.dataset.deleteUser) {
            if (!confirmDelete("confirm.deleteUser")) return;
            await withButtonLoading(target, "loading.deleting", async () => {
                await api(`/api/admin/users/${target.dataset.deleteUser}`, { method: "DELETE" });
                await loadAdmin();
            });
        }
    });

    document.body.addEventListener("submit", async (event) => {
        const form = event.target.closest("[data-note-date-form]");
        if (!form) return;

        event.preventDefault();
        const plantId = state.plantHistory.plantId;
        if (!plantId) return;

        await withButtonLoading(event.submitter, "loading.saving", async () => {
            await api(`/api/garden/plants/${plantId}/notes/${form.dataset.noteDateForm}/date`, {
                method: "PUT",
                body: JSON.stringify({ createdOn: form.elements.createdOn.value })
            });
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
