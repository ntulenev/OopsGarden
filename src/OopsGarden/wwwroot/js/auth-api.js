import { api } from "./api-client.js?v=20260602-1";

export const authApi = {
    getMe: () => api("/api/me"),
    login: (credentials) => api("/api/auth/login", {
        method: "POST",
        body: JSON.stringify(credentials)
    }),
    adminLogin: (credentials) => api("/api/auth/admin-login", {
        method: "POST",
        body: JSON.stringify(credentials)
    }),
    register: (command) => api("/api/auth/register", {
        method: "POST",
        body: JSON.stringify(command)
    }),
    logout: () => api("/api/auth/logout", { method: "POST" }),
    updateSettings: (settings) => api("/api/auth/settings", {
        method: "POST",
        body: JSON.stringify(settings)
    })
};
