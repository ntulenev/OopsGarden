export function createPlantDialogController({
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
    getPlantHistory
}) {
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
        getPlantHistory().renderWateringCalendar([]);
        Promise.all([
            loadPlantNotes(plant.id, 1, { isPublic: true, publicGardenId: state.publicGarden.id, mode: "all" }),
            getPlantHistory().loadPlantHistory(plant.id, {
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
            getPlantHistory().renderWateringCalendar([]);
            renderPlantTimelineWarning();
            await Promise.all([
                loadPlantNotes(plant.id, 1, { mode: "all" }),
                getPlantHistory().loadPlantHistory(plant.id, { plantName: plant.name, isPublic: false, renderPage: false, renderCalendar: true })
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
        $("wateringCalendarHint").hidden = !getPlantHistory().isWateringCalendarEditable();
        if (isExistingPlant && state.plantHistory.plantId === form.elements.id.value) {
            getPlantHistory().renderWateringCalendar(state.plantHistory.items);
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

    return {
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
    };
}
