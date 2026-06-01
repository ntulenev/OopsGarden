export function toDateInputValue(value) {
    if (!value) return "";
    return new Date(value).toISOString().slice(0, 10);
}

export function todayKey() {
    const now = new Date();
    return `${now.getFullYear()}-${String(now.getMonth() + 1).padStart(2, "0")}-${String(now.getDate()).padStart(2, "0")}`;
}

export function toMonthKey(value) {
    const date = value ? new Date(value) : new Date();
    return `${date.getFullYear()}-${String(date.getMonth() + 1).padStart(2, "0")}`;
}

export function isReminderOverdue(note) {
    return Boolean(note?.isReminder && !note.isReminderResolved && note.reminderDate && note.reminderDate < todayKey());
}

export function reminderStateClass(note) {
    if (!note?.isReminder) return "";
    if (note.isReminderResolved) return " reminder-resolved";
    return isReminderOverdue(note) ? " reminder-overdue" : " reminder-active";
}

export function reminderMeta(note, dependencies) {
    if (!note?.isReminder) return "";

    const { escapeHtml, t } = dependencies;
    const stateKey = note.isReminderResolved
        ? "notes.reminderResolved"
        : isReminderOverdue(note)
            ? "notes.reminderOverdue"
            : "notes.reminderActive";
    return `<span class="reminder-meta">${t(stateKey)}: ${escapeHtml(note.reminderDate || "")}</span>`;
}
