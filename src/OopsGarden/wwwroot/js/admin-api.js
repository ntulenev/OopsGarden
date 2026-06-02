import { api } from "./api-client.js?v=20260602-1";

export const adminApi = {
    getInvites: () => api("/api/admin/invites"),
    getUsers: () => api("/api/admin/users"),
    createInvite: () => api("/api/admin/invites", { method: "POST" }),
    deleteInvite: (id) => api(`/api/admin/invites/${id}`, { method: "DELETE" }),
    blockUser: (id, command) => api(`/api/admin/users/${id}/block`, {
        method: "POST",
        body: JSON.stringify(command)
    }),
    deleteUser: (id) => api(`/api/admin/users/${id}`, { method: "DELETE" })
};
