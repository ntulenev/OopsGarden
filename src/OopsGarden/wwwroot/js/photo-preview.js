export function createPhotoPreviewController(dependencies) {
    const { state, defaultPlantPhotoUrl, t, $ } = dependencies;

    function openPhotoPreview(src, title, alt = title, options = {}) {
        const items = options.items?.length
            ? options.items
            : [{ src: src || defaultPlantPhotoUrl, uploadedAt: options.uploadedAt || null, alt }];
        const requestedIndex = Number.isInteger(options.index)
            ? options.index
            : Math.max(0, items.findIndex((item) => item.src === src));
        state.photoPreview = {
            items,
            index: requestedIndex < 0 ? 0 : requestedIndex
        };
        $("photoPreviewTitle").textContent = title || t("plants.photo");
        $("photoPreviewImage").alt = alt || "";
        renderPhotoPreview();
        $("photoPreviewDialog").hidden = false;
    }

    function openPlantPhotoPreview(plantId, src, title, alt = title) {
        const photos = getPlantPhotoPreviewItems(plantId, src, alt);
        const index = Math.max(0, photos.findIndex((item) => item.src === src));
        openPhotoPreview(src, title, alt, { items: photos, index });
    }

    function closePhotoPreviewDialog() {
        $("photoPreviewDialog").hidden = true;
        $("photoPreviewImage").src = defaultPlantPhotoUrl;
        $("photoPreviewImage").alt = "";
        $("photoPreviewDate").textContent = "";
        state.photoPreview = { items: [], index: 0 };
    }

    function shiftPhotoPreview(delta) {
        const count = state.photoPreview.items.length;
        if (count <= 1) return;
        state.photoPreview.index = (state.photoPreview.index + delta + count) % count;
        renderPhotoPreview();
    }

    function getPlantPhotoPreviewItems(plantId, currentSrc, alt) {
        const photos = state.plantHistory.plantId === plantId
            ? state.plantHistory.items
                .filter((item) => item.type === "photo" && item.photoDataUrl)
                .map((item) => ({
                    id: item.id,
                    src: item.photoDataUrl,
                    uploadedAt: item.occurredAt,
                    alt
                }))
            : [];
        if (currentSrc && !photos.some((item) => item.src === currentSrc)) {
            return [{ src: currentSrc, uploadedAt: null, alt }, ...photos];
        }

        return photos.length ? photos : [{ src: currentSrc || defaultPlantPhotoUrl, uploadedAt: null, alt }];
    }

    function renderPhotoPreview() {
        const items = state.photoPreview.items.length
            ? state.photoPreview.items
            : [{ src: defaultPlantPhotoUrl, uploadedAt: null, alt: "" }];
        const index = Math.min(Math.max(state.photoPreview.index, 0), items.length - 1);
        state.photoPreview.index = index;
        const item = items[index];
        $("photoPreviewImage").src = item.src || defaultPlantPhotoUrl;
        $("photoPreviewImage").alt = item.alt || "";
        $("photoPreviewDate").textContent = item.uploadedAt
            ? new Date(item.uploadedAt).toLocaleString()
            : t("common.none");
        $("previousPhotoPreview").disabled = items.length <= 1;
        $("nextPhotoPreview").disabled = items.length <= 1;
    }

    return {
        closePhotoPreviewDialog,
        openPhotoPreview,
        openPlantPhotoPreview,
        shiftPhotoPreview
    };
}
