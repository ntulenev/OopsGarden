import { api } from "./api-client.js?v=20260602-1";

export const gardenApi = {
    getSummary: () => api("/api/garden/summary"),
    getLocations: () => api("/api/garden/locations"),
    getPublicGarden: (publicGardenId) => api(`/api/public/gardens/${publicGardenId}`),
    saveLocation: (id, command) => api(id ? `/api/garden/locations/${id}` : "/api/garden/locations", {
        method: id ? "PUT" : "POST",
        body: JSON.stringify(command)
    }),
    deleteLocation: (id) => api(`/api/garden/locations/${id}`, { method: "DELETE" })
};
