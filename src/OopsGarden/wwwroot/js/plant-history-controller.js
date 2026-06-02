export function createPlantHistoryController({
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
}) {
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
        const result = await plantsApi.getHistory({ plantId, publicGardenId, isPublic });
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
                    ${isNote ? reminderMeta(item, { escapeHtml, t }) : ""}
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

    return {
        isWateringCalendarEditable,
        loadPlantHistory,
        openPlantHistoryPage,
        renderPlantHistory,
        renderWateringCalendar,
        shiftWateringCalendarMonth
    };
}
