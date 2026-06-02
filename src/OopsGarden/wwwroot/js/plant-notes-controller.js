export function createPlantNotesController({
    $,
    escapeHtml,
    plantsApi,
    reminderMeta,
    reminderStateClass,
    state,
    t
}) {
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

        let result;
        try {
            result = await plantsApi.getNotes({
                plantId,
                publicGardenId,
                isPublic,
                mode,
                page,
                pageSize: state.plantNotes.pageSize
            });
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
                    ${reminderMeta(note, { escapeHtml, t })}
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

    return {
        loadPlantNotes,
        renderPlantNotes,
        setPlantNotesMode,
        setReminderDateFieldVisibility
    };
}
