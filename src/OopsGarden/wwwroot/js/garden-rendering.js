export function createGardenRenderer({
    $,
    defaultPlantPhotoUrl,
    escapeHtml,
    gardenApi,
    loadingState,
    setRegionLoading,
    showError,
    state,
    t
}) {
    async function loadGarden() {
        if (loadingState.has("garden")) return;
        loadingState.add("garden");
        setRegionLoading("plantList", true, "loading.garden");
        try {
            [state.plants, state.locations] = await Promise.all([
                gardenApi.getSummary(),
                gardenApi.getLocations()
            ]);
            $("plantList").setAttribute("aria-busy", "false");
            $("gardenPlantTotal").textContent = t("plants.total").replace("{count}", state.plants.length);
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

    function renderLocationSelect(select, selectedValue) {
        select.innerHTML = `<option value="">${t("common.none")}</option>`;
        for (const location of state.locations) {
            select.append(new Option(location.name, location.id));
        }
        select.value = selectedValue || "";
    }

    return {
        loadGarden,
        renderLocationSelect,
        renderPlantGroups
    };
}
