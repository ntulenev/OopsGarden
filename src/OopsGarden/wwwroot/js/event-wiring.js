export function createEventWiring({
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
}) {
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

    return { wireEvents };
}
