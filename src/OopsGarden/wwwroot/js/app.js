const state = {
    me: null,
    lang: localStorage.getItem("oopsGarden.lang") || "en",
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
        page: 1,
        pageSize: 5,
        total: 0,
        items: [],
        hasPrevious: false,
        hasNext: false
    },
    plantNotesRequestId: 0
};
const defaultAvatarUrl = "/img/garden-user.png";
const defaultPlantPhotoUrl = "/img/default-plant.png";
const maxUploadImageSide = 1080;

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
    const response = await fetch(`/resources/${lang}.json`);
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
}

function t(key) {
    return state.dict[key] || key;
}

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

function confirmDelete(key) {
    return window.confirm(t(key));
}

function toDateInputValue(value) {
    if (!value) return "";
    return new Date(value).toISOString().slice(0, 10);
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
    const userViews = ["garden", "settings"];
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
    $("settingsView").hidden = !isUser || activeView !== "settings";
    $("publicGardenView").hidden = true;
    $("adminView").hidden = !isAdmin;
    if (isUser) {
        renderUserIdentity();
        qs("#settingsForm [name=displayName]").value = state.me.name || "";
        qs("#settingsForm [name=isGardenPublic]").checked = Boolean(state.me.isGardenPublic);
        $("sharePublicGardenLink").hidden = !state.me.isGardenPublic;
    }
    if (activeView === "garden") loadGarden();
    if (activeView === "admin") loadAdmin();
}

function renderPublicGarden() {
    qsa("[data-auth]").forEach((el) => { el.hidden = true; });
    qsa("[data-user]").forEach((el) => { el.hidden = true; });
    qsa("[data-admin]").forEach((el) => { el.hidden = true; });
    $("authView").hidden = true;
    $("adminAuthView").hidden = true;
    $("gardenView").hidden = true;
    $("settingsView").hidden = true;
    $("adminView").hidden = true;
    $("publicGardenView").hidden = false;

    const ownerName = state.publicGarden.name || t("user.defaultName");
    const avatarUrl = state.publicGarden.avatar || defaultAvatarUrl;
    $("publicGardenTitle").textContent = t("public.title").replace("{name}", ownerName);
    document.title = $("publicGardenTitle").textContent || t("garden.title");
    $("publicGardenOwnerName").textContent = ownerName;
    $("publicGardenAvatar").src = avatarUrl;
    $("publicGardenAvatar").alt = ownerName;
    renderPlantGroups($("publicPlantList"), state.publicGarden.plants || [], { isPublic: true });
}

function setView(view) {
    const isAdmin = state.me?.role === "Admin";
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
    [state.plants, state.locations] = await Promise.all([
        api("/api/garden/summary"),
        api("/api/garden/locations")
    ]);
    renderPlantGroups($("plantList"), state.plants, { isPublic: false });
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
            tile.className = "plant-card";
            tile.innerHTML = `
                <button type="button" class="plant-photo-button"${options.isPublic ? ` data-public-plant="${plant.id}"` : ` data-edit-plant="${plant.id}"`}>
                    <img alt="" src="${plant.photoDataUrl || defaultPlantPhotoUrl}">
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
    setPlantDialogPublicMode(true);
    $("plantEditDialog").hidden = false;
    loadPlantNotes(plant.id, 1, { isPublic: true, publicGardenId: state.publicGarden.id });
}

function closePublicPlantDialog() {
    closePlantDialog();
}

async function initPublicGardenFromUrl() {
    const publicGardenId = new URLSearchParams(location.search).get("publicGarden");
    if (!publicGardenId) {
        return false;
    }

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
    if (!state.locations.length) {
        state.locations = await api("/api/garden/locations");
    }

    const plant = state.plants.find((item) => item.id === id);
    if (!plant) return;

    const form = $("plantDialogForm");
    form.elements.id.value = plant.id;
    form.elements.name.value = plant.name;
    form.elements.description.value = plant.description || "";
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
    setPlantEditMode(false);
    $("plantEditDialog").hidden = false;
    await loadPlantNotes(plant.id, 1);
}

async function openCreatePlantDialog() {
    resetPlantDialogMode();
    if (!state.locations.length) {
        state.locations = await api("/api/garden/locations");
    }

    const form = $("plantDialogForm");
    form.reset();
    form.elements.id.value = "";
    form.dataset.photo = "";
    delete form.dataset.photoPreview;
    form.elements.lastWateredOn.value = "";
    $("plantPhotoPreview").src = defaultPlantPhotoUrl;
    $("plantPhotoPreview").alt = t("plants.add");
    $("plantPhotoPreviewLabel").textContent = t("plants.photo");
    renderLocationSelect(form.elements.locationId, "");
    $("plantDialogTitle").textContent = t("plants.add");
    $("lastWateredField").hidden = true;
    $("deletePlantFromDialog").hidden = true;
    $("plantNotesPanel").hidden = true;
    setPlantEditMode(true);
    $("plantEditDialog").hidden = false;
}

function closePlantDialog() {
    const form = $("plantDialogForm");
    state.plantNotesRequestId += 1;
    resetPlantDialogMode();
    form.reset();
    form.dataset.photo = "";
    delete form.dataset.photoPreview;
    $("plantPhotoPreview").src = defaultPlantPhotoUrl;
    $("plantPhotoPreviewLabel").textContent = t("plants.photo");
    $("lastWateredField").hidden = true;
    $("deletePlantFromDialog").hidden = true;
    $("plantNotesPanel").hidden = true;
    $("plantNoteText").value = "";
    setPlantEditMode(true);
    state.plantNotes = {
        plantId: null,
        publicGardenId: null,
        isPublic: false,
        isLoading: false,
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
    form.elements.locationId.disabled = !effectiveEnabled;
    form.elements.plantedOn.readOnly = !effectiveEnabled;
    form.elements.lastWateredOn.readOnly = !effectiveEnabled;
    form.elements.photo.disabled = !effectiveEnabled;

    qsa(".plant-edit-command").forEach((element) => {
        element.hidden = !effectiveEnabled;
    });
    $("deletePlantFromDialog").hidden = !effectiveEnabled || !isExistingPlant;
    qsa("[data-plant-photo-field]").forEach((element) => {
        element.hidden = !effectiveEnabled;
    });
}

function setPlantDialogPublicMode(enabled) {
    const form = $("plantDialogForm");
    form.dataset.public = enabled ? "true" : "false";
    $("plantEditMode").closest("label").hidden = enabled;
    $("plantEditMode").checked = false;
    form.elements.name.readOnly = true;
    form.elements.description.readOnly = true;
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
    document.querySelector('label[for="plantNoteText"]').hidden = true;
}

function resetPlantDialogMode() {
    const form = $("plantDialogForm");
    form.dataset.public = "false";
    $("plantEditMode").closest("label").hidden = false;
    $("addPlantNote").hidden = false;
    $("plantNoteText").hidden = false;
    document.querySelector('label[for="plantNoteText"]').hidden = false;
}

async function loadPlantNotes(plantId, page = state.plantNotes.page, options = {}) {
    const isPublic = Boolean(options.isPublic ?? state.plantNotes.isPublic);
    const publicGardenId = options.publicGardenId ?? state.plantNotes.publicGardenId;
    const requestId = state.plantNotesRequestId + 1;
    state.plantNotesRequestId = requestId;
    state.plantNotes = {
        plantId,
        publicGardenId,
        isPublic,
        isLoading: true,
        page,
        pageSize: state.plantNotes.pageSize,
        total: 0,
        items: [],
        hasPrevious: false,
        hasNext: false
    };
    renderPlantNotes();

    const url = isPublic
        ? `/api/public/gardens/${publicGardenId}/plants/${plantId}/notes?page=${page}&pageSize=${state.plantNotes.pageSize}`
        : `/api/garden/plants/${plantId}/notes?page=${page}&pageSize=${state.plantNotes.pageSize}`;
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
        list.innerHTML = `<p class="muted">${t("notes.empty")}</p>`;
        return;
    }

    for (const note of state.plantNotes.items) {
        const row = document.createElement("article");
        row.className = "plant-note";
        row.innerHTML = `
            <div>
                <time>${new Date(note.createdAt).toLocaleString()}</time>
                <p>${escapeHtml(note.text)}</p>
            </div>
            ${state.plantNotes.isPublic
                ? ""
                : `<button type="button" class="note-delete" data-delete-note="${note.id}" aria-label="${t("actions.delete")}">×</button>`}`;
        list.append(row);
    }
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

async function loadAdmin() {
    const [invites, users] = await Promise.all([api("/api/admin/invites"), api("/api/admin/users")]);
    $("toggleUsedInvitesBtn").textContent = state.hideUsedInvites
        ? t("admin.showUsedInvites")
        : t("admin.hideUsedInvites");
    const inviteList = $("inviteList");
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
        await api("/api/auth/login", { method: "POST", body: JSON.stringify(formData(event.currentTarget)) });
        await refreshMe();
    });

    $("adminLoginForm").addEventListener("submit", async (event) => {
        event.preventDefault();
        await api("/api/auth/admin-login", { method: "POST", body: JSON.stringify(formData(event.currentTarget)) });
        await refreshMe();
    });

    $("registerForm").addEventListener("submit", async (event) => {
        event.preventDefault();
        const data = formData(event.currentTarget);
        data.language = state.lang;
        await api("/api/auth/register", { method: "POST", body: JSON.stringify(data) });
        await refreshMe();
    });

    $("logoutBtn").addEventListener("click", async () => {
        await api("/api/auth/logout", { method: "POST" });
        await refreshMe();
    });

    $("settingsForm").addEventListener("submit", async (event) => {
        event.preventDefault();
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
    $("createPlantBtn").addEventListener("click", openCreatePlantDialog);

    $("locationDialogForm").addEventListener("submit", async (event) => {
        event.preventDefault();
        try {
            const form = event.currentTarget;
            const id = form.elements.id.value;
            await api(id ? `/api/garden/locations/${id}` : "/api/garden/locations", {
                method: id ? "PUT" : "POST",
                body: JSON.stringify({ name: form.elements.name.value })
            });
            closeLocationDialog();
            await loadGarden();
            toast(t("toast.saved"));
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

    $("deleteLocationFromDialog").addEventListener("click", async () => {
        const id = $("locationDialogForm").elements.id.value;
        if (!id) return;
        if (!confirmDelete("confirm.deleteLocation")) return;
        await api(`/api/garden/locations/${id}`, { method: "DELETE" });
        closeLocationDialog();
        await loadGarden();
        toast(t("toast.done"));
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
            const form = event.currentTarget;
            const photoDataUrl = await fileToDataUrl(form.photo.files[0]);
            const id = form.elements.id.value;
            const payload = {
                name: form.elements.name.value,
                description: form.elements.description.value,
                locationId: form.elements.locationId.value || null,
                plantedOn: form.elements.plantedOn.value || null,
                lastWateredOn: id ? form.elements.lastWateredOn.value || null : null,
                photoDataUrl: form.dataset.photoPreview || photoDataUrl || form.dataset.photo || null
            };
            await api(id ? `/api/garden/plants/${id}` : "/api/garden/plants", {
                method: id ? "PUT" : "POST",
                body: JSON.stringify(payload)
            });
            closePlantDialog();
            await loadGarden();
            toast(t("toast.saved"));
        } catch (error) {
            showError(error);
        }
    });

    $("deletePlantFromDialog").addEventListener("click", async () => {
        const id = $("plantDialogForm").elements.id.value;
        if (!id) return;
        if (!confirmDelete("confirm.deletePlant")) return;
        await api(`/api/garden/plants/${id}`, { method: "DELETE" });
        closePlantDialog();
        await loadGarden();
        toast(t("toast.done"));
    });

    $("clearLastWatered").addEventListener("click", () => {
        $("plantDialogForm").elements.lastWateredOn.value = "";
    });

    $("plantEditMode").addEventListener("change", (event) => {
        setPlantEditMode(event.currentTarget.checked);
    });

    $("addPlantNote").addEventListener("click", async () => {
        const plantId = state.plantNotes.plantId;
        const text = $("plantNoteText").value.trim();
        if (!plantId || !text) return;

        await api(`/api/garden/plants/${plantId}/notes`, {
            method: "POST",
            body: JSON.stringify({ text })
        });
        $("plantNoteText").value = "";
        await loadPlantNotes(plantId, 1);
        toast(t("toast.saved"));
    });

    $("previousPlantNotesPage").addEventListener("click", async () => {
        if (!state.plantNotes.plantId || !state.plantNotes.hasPrevious) return;
        await loadPlantNotes(state.plantNotes.plantId, state.plantNotes.page - 1);
    });

    $("nextPlantNotesPage").addEventListener("click", async () => {
        if (!state.plantNotes.plantId || !state.plantNotes.hasNext) return;
        await loadPlantNotes(state.plantNotes.plantId, state.plantNotes.page + 1);
    });

    $("closePlantDialog").addEventListener("click", closePlantDialog);
    qsa("[data-close-dialog]").forEach((button) => button.addEventListener("click", closePlantDialog));
    $("plantEditDialog").addEventListener("click", (event) => {
        if (event.target.id === "plantEditDialog") {
            closePlantDialog();
        }
    });
    $("closePublicPlantDialog").addEventListener("click", closePublicPlantDialog);
    $("publicPlantDialog").addEventListener("click", (event) => {
        if (event.target.id === "publicPlantDialog") {
            closePublicPlantDialog();
        }
    });

    $("createInviteBtn").addEventListener("click", async () => {
        await api("/api/admin/invites", { method: "POST" });
        await loadAdmin();
    });

    $("toggleUsedInvitesBtn").addEventListener("click", async () => {
        state.hideUsedInvites = !state.hideUsedInvites;
        await loadAdmin();
    });

    document.body.addEventListener("click", async (event) => {
        const target = event.target.closest("button");
        if (!target) return;
        if (target.dataset.water) {
            await api(`/api/garden/plants/${target.dataset.water}/water`, { method: "POST" });
            await loadGarden();
        }
        if (target.dataset.deleteLocation) {
            if (!confirmDelete("confirm.deleteLocation")) return;
            await api(`/api/garden/locations/${target.dataset.deleteLocation}`, { method: "DELETE" });
            await loadGarden();
        }
        if (target.dataset.editLocation) {
            openLocationDialog(target.dataset.editLocation);
        }
        if (target.dataset.editPlant) {
            await openPlantDialog(target.dataset.editPlant);
        }
        if (target.dataset.publicPlant) {
            openPublicPlantDialog(target.dataset.publicPlant);
        }
        if (target.dataset.deletePlant) {
            if (!confirmDelete("confirm.deletePlant")) return;
            await api(`/api/garden/plants/${target.dataset.deletePlant}`, { method: "DELETE" });
            await loadGarden();
        }
        if (target.dataset.deleteNote) {
            if (!confirmDelete("confirm.deleteNote")) return;
            const plantId = state.plantNotes.plantId;
            if (!plantId) return;

            await api(`/api/garden/plants/${plantId}/notes/${target.dataset.deleteNote}`, { method: "DELETE" });
            await loadPlantNotes(plantId, state.plantNotes.page);
            if (!state.plantNotes.items.length && state.plantNotes.page > 1) {
                await loadPlantNotes(plantId, state.plantNotes.page - 1);
            }
            toast(t("toast.done"));
        }
        if (target.dataset.copy) {
            await navigator.clipboard.writeText(target.dataset.copy);
            toast(target.dataset.copy);
        }
        if (target.dataset.deleteInvite) {
            if (!confirmDelete("confirm.deleteInvite")) return;
            await api(`/api/admin/invites/${target.dataset.deleteInvite}`, { method: "DELETE" });
            await loadAdmin();
        }
        if (target.dataset.blockUser) {
            await api(`/api/admin/users/${target.dataset.blockUser}/block`, {
                method: "POST",
                body: JSON.stringify({ isBlocked: target.dataset.blockValue === "true" })
            });
            await loadAdmin();
        }
        if (target.dataset.deleteUser) {
            if (!confirmDelete("confirm.deleteUser")) return;
            await api(`/api/admin/users/${target.dataset.deleteUser}`, { method: "DELETE" });
            await loadAdmin();
        }
    });
}

async function initInviteFromUrl() {
    const invite = new URLSearchParams(location.search).get("invite");
    if (invite) {
        qs("#registerForm [name=inviteCode]").value = invite;
    }
}

wireEvents();
await loadLanguage(state.lang);
await initInviteFromUrl();
if (await initPublicGardenFromUrl()) {
    // Public gardens do not need an authenticated session.
} else {
await refreshMe();
}
