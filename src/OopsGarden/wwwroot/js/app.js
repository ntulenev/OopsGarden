const state = {
    me: null,
    lang: localStorage.getItem("oopsGarden.lang") || "en",
    dict: {},
    locations: [],
    plants: []
};

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
        throw new Error(text || response.statusText);
    }
    if (response.status === 204) return null;
    const text = await response.text();
    return text ? JSON.parse(text) : null;
}

async function loadLanguage(lang) {
    const response = await fetch(`/resources/${lang}.json`);
    state.dict = await response.json();
    state.lang = lang;
    localStorage.setItem("oopsGarden.lang", lang);
    $("languageSelect").value = lang;
    qsa("[data-i18n]").forEach((el) => {
        el.textContent = t(el.dataset.i18n);
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

async function fileToDataUrl(file) {
    if (!file) return null;
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
    state.me = await api("/api/me");
    renderShell();
}

function renderShell() {
    const authed = Boolean(state.me?.authenticated);
    const isAdmin = state.me?.role === "Admin";
    qsa("[data-auth]").forEach((el) => { el.hidden = !authed; });
    qsa("[data-admin]").forEach((el) => { el.hidden = !isAdmin; });
    $("authView").hidden = authed;
    $("gardenView").hidden = !authed || isAdmin;
    $("workView").hidden = true;
    $("adminView").hidden = !isAdmin;
    if (authed && !isAdmin) {
        qs("#settingsForm [name=displayName]").value = state.me.name || "";
        loadGarden();
    }
    if (isAdmin) {
        loadAdmin();
    }
}

function setView(view) {
    qsa(".tab").forEach((button) => button.classList.toggle("active", button.dataset.view === view));
    $("gardenView").hidden = view !== "garden";
    $("workView").hidden = view !== "work";
    $("adminView").hidden = view !== "admin";
    if (view === "garden") loadGarden();
    if (view === "work") loadWork();
    if (view === "admin") loadAdmin();
}

async function loadGarden() {
    state.plants = await api("/api/garden/summary");
    const list = $("plantList");
    list.innerHTML = "";
    if (!state.plants.length) {
        list.innerHTML = `<p class="muted">${t("empty.garden")}</p>`;
        return;
    }

    for (const plant of state.plants) {
        const card = document.createElement("article");
        card.className = "plant-card";
        card.innerHTML = `
            <img alt="" src="${plant.photoDataUrl || "/img/oops-garden-logo.png"}">
            <div class="plant-body">
                <h3>${escapeHtml(plant.name)}</h3>
                <p>${escapeHtml(plant.description || "")}</p>
                <div class="meta">
                    <span class="chip">${escapeHtml(plant.location?.name || t("common.none"))}</span>
                    <span class="chip">${plant.plantedOn || ""}</span>
                    <span class="chip">${plant.lastWateredAt ? new Date(plant.lastWateredAt).toLocaleString() : t("common.never")}</span>
                </div>
                <button data-water="${plant.id}">${t("actions.water")}</button>
            </div>`;
        list.append(card);
    }
}

async function loadWork() {
    state.locations = await api("/api/garden/locations");
    state.plants = await api("/api/garden/plants");
    renderLocations();
    renderPlantSelect();
    renderPlantEditor();
}

function renderLocations() {
    const list = $("locationList");
    list.innerHTML = "";
    for (const location of state.locations) {
        const row = document.createElement("div");
        row.className = "list-row";
        row.innerHTML = `<span>${escapeHtml(location.name)} <span class="muted">${location.plants}</span></span>
            <button class="ghost danger" data-delete-location="${location.id}">${t("actions.delete")}</button>`;
        list.append(row);
    }
}

function renderPlantSelect() {
    const select = qs("#plantForm [name=locationId]");
    select.innerHTML = `<option value="">${t("common.none")}</option>`;
    for (const location of state.locations) {
        select.append(new Option(location.name, location.id));
    }
}

function renderPlantEditor() {
    const list = $("plantEditorList");
    list.innerHTML = "";
    for (const plant of state.plants) {
        const row = document.createElement("div");
        row.className = "editor-row";
        row.innerHTML = `
            <div>
                <strong>${escapeHtml(plant.name)}</strong>
                <div class="muted">${escapeHtml(plant.locationName || t("common.none"))}</div>
            </div>
            <div class="row-actions">
                <button class="ghost" data-edit-plant="${plant.id}">${t("actions.edit")}</button>
                <button class="danger" data-delete-plant="${plant.id}">${t("actions.delete")}</button>
            </div>`;
        list.append(row);
    }
}

async function loadAdmin() {
    const [invites, users] = await Promise.all([api("/api/admin/invites"), api("/api/admin/users")]);
    const inviteList = $("inviteList");
    inviteList.innerHTML = "";
    for (const invite of invites) {
        const url = `${location.origin}/?invite=${invite.code}`;
        const row = document.createElement("div");
        row.className = "list-row";
        row.innerHTML = `<div><strong>${invite.usedAt ? "Used" : invite.isRevoked ? "Revoked" : "Open"}</strong>
            <div class="muted">${url}</div></div>
            <button class="ghost" data-copy="${url}">Copy</button>`;
        inviteList.append(row);
    }

    const userList = $("userList");
    userList.innerHTML = "";
    for (const user of users) {
        const row = document.createElement("div");
        row.className = "list-row";
        row.innerHTML = `<div><strong>${escapeHtml(user.displayName)}</strong>
            <div class="muted">${escapeHtml(user.email)} · ${user.plants} plants</div></div>
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
                    displayName: state.me.name || "Gardener",
                    language: state.lang,
                    avatarDataUrl: state.me.avatar || null
                })
            });
        }
        renderShell();
    });

    qsa(".tab").forEach((button) => button.addEventListener("click", () => setView(button.dataset.view)));

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
        const avatarDataUrl = await fileToDataUrl(form.avatar.files[0]) || state.me.avatar || null;
        await api("/api/auth/settings", {
            method: "POST",
            body: JSON.stringify({ displayName: form.displayName.value, language: state.lang, avatarDataUrl })
        });
        await refreshMe();
        toast(t("toast.saved"));
    });

    $("locationForm").addEventListener("submit", async (event) => {
        event.preventDefault();
        const form = event.currentTarget;
        await api("/api/garden/locations", { method: "POST", body: JSON.stringify(formData(form)) });
        form.reset();
        await loadWork();
    });

    $("plantForm").addEventListener("submit", async (event) => {
        event.preventDefault();
        const form = event.currentTarget;
        const photoDataUrl = await fileToDataUrl(form.photo.files[0]);
        const payload = {
            name: form.name.value,
            description: form.description.value,
            locationId: form.locationId.value || null,
            plantedOn: form.plantedOn.value || null,
            photoDataUrl: photoDataUrl || form.dataset.photo || null
        };
        const id = form.id.value;
        await api(id ? `/api/garden/plants/${id}` : "/api/garden/plants", {
            method: id ? "PUT" : "POST",
            body: JSON.stringify(payload)
        });
        resetPlantForm();
        await loadWork();
        toast(t("toast.saved"));
    });

    $("cancelPlantEdit").addEventListener("click", resetPlantForm);

    $("createInviteBtn").addEventListener("click", async () => {
        await api("/api/admin/invites", { method: "POST" });
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
            await api(`/api/garden/locations/${target.dataset.deleteLocation}`, { method: "DELETE" });
            await loadWork();
        }
        if (target.dataset.editPlant) {
            editPlant(target.dataset.editPlant);
        }
        if (target.dataset.deletePlant) {
            await api(`/api/garden/plants/${target.dataset.deletePlant}`, { method: "DELETE" });
            await loadWork();
        }
        if (target.dataset.copy) {
            await navigator.clipboard.writeText(target.dataset.copy);
            toast(target.dataset.copy);
        }
        if (target.dataset.blockUser) {
            await api(`/api/admin/users/${target.dataset.blockUser}/block`, {
                method: "POST",
                body: JSON.stringify({ isBlocked: target.dataset.blockValue === "true" })
            });
            await loadAdmin();
        }
        if (target.dataset.deleteUser) {
            await api(`/api/admin/users/${target.dataset.deleteUser}`, { method: "DELETE" });
            await loadAdmin();
        }
    });
}

function editPlant(id) {
    const plant = state.plants.find((item) => item.id === id);
    if (!plant) return;
    const form = $("plantForm");
    form.id.value = plant.id;
    form.name.value = plant.name;
    form.description.value = plant.description || "";
    form.locationId.value = plant.locationId || "";
    form.plantedOn.value = plant.plantedOn || "";
    form.dataset.photo = plant.photoDataUrl || "";
    $("plantFormTitle").textContent = plant.name;
    form.scrollIntoView({ behavior: "smooth", block: "center" });
}

function resetPlantForm() {
    const form = $("plantForm");
    form.reset();
    form.id.value = "";
    form.dataset.photo = "";
    $("plantFormTitle").textContent = t("plants.add");
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
await refreshMe();
