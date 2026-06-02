export const state = {
    me: null,
    lang: localStorage.getItem("oopsGarden.lang") || "en",
    theme: localStorage.getItem("oopsGarden.theme") || "greenhouse",
    view: "garden",
    route: location.pathname.toLowerCase(),
    publicGarden: null,
    hideUsedInvites: false,
    dict: {},
    locations: [],
    plants: [],
    plantNotes: {
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
    },
    plantNotesRequestId: 0,
    plantHistory: {
        plantId: null,
        plantName: "",
        isPublic: false,
        publicGardenId: null,
        isLoading: false,
        items: []
    },
    wateringCalendarMonth: null,
    plantHistoryRequestId: 0,
    plantDialogBaseline: null,
    photoPreview: {
        items: [],
        index: 0
    }
};

export const defaultAvatarUrl = "/img/garden-user.png?v=20260602-2";
export const defaultPlantPhotoUrl = "/img/default-plant.png?v=20260602-2";
export const resourceVersion = "20260602-2";
export const maxUploadImageSide = 1080;
export const loadingState = new Set();
