import { api } from "./api-client.js";

export const plantsApi = {
    savePlant: (id, command) => api(id ? `/api/garden/plants/${id}` : "/api/garden/plants", {
        method: id ? "PUT" : "POST",
        body: JSON.stringify(command)
    }),
    deletePlant: (id) => api(`/api/garden/plants/${id}`, { method: "DELETE" }),
    waterPlant: (plantId) => api(`/api/garden/plants/${plantId}/water`, { method: "POST" }),
    createWatering: (plantId, command) => api(`/api/garden/plants/${plantId}/waterings`, {
        method: "POST",
        body: JSON.stringify(command)
    }),
    deleteWatering: (plantId, wateringId) => api(`/api/garden/plants/${plantId}/waterings/${wateringId}`, { method: "DELETE" }),
    getNotes: ({ plantId, publicGardenId, isPublic, mode, page, pageSize }) => {
        const notesPath = mode === "overdue" ? "notes/overdue" : "notes";
        const url = isPublic
            ? `/api/public/gardens/${publicGardenId}/plants/${plantId}/notes?page=${page}&pageSize=${pageSize}`
            : `/api/garden/plants/${plantId}/${notesPath}?page=${page}&pageSize=${pageSize}`;
        return api(url);
    },
    createNote: (plantId, command) => api(`/api/garden/plants/${plantId}/notes`, {
        method: "POST",
        body: JSON.stringify(command)
    }),
    deleteNote: (plantId, noteId) => api(`/api/garden/plants/${plantId}/notes/${noteId}`, { method: "DELETE" }),
    updateNoteReminderStatus: (plantId, noteId, command) => api(`/api/garden/plants/${plantId}/notes/${noteId}/reminder-status`, {
        method: "PUT",
        body: JSON.stringify(command)
    }),
    updateNoteDate: (plantId, noteId, command) => api(`/api/garden/plants/${plantId}/notes/${noteId}/date`, {
        method: "PUT",
        body: JSON.stringify(command)
    }),
    getHistory: ({ plantId, publicGardenId, isPublic }) => {
        const url = isPublic
            ? `/api/public/gardens/${publicGardenId}/plants/${plantId}/history`
            : `/api/garden/plants/${plantId}/history`;
        return api(url);
    },
    deletePhoto: (plantId, photoId) => api(`/api/garden/plants/${plantId}/photos/${photoId}`, { method: "DELETE" })
};
