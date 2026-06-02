export function createLocationDialogController({
    $,
    qsa,
    confirmDelete,
    gardenApi,
    loadGarden,
    showError,
    state,
    t,
    toast,
    withButtonLoading
}) {
    function openLocationDialog(id = "") {
        const form = $("locationDialogForm");
        const location = id ? state.locations.find((item) => item.id === id) : null;
        form.reset();
        form.elements.id.value = location?.id || "";
        form.elements.name.value = location?.name || "";
        $("locationDialogTitle").textContent = location ? t("locations.edit") : t("locations.add");
        $("deleteLocationFromDialog").hidden = !location;
        $("locationDialog").hidden = false;
        form.elements.name.focus();
    }

    function closeLocationDialog() {
        $("locationDialogForm").reset();
        $("locationDialog").hidden = true;
    }

    function wireEvents() {
        $("createLocationBtn").addEventListener("click", () => openLocationDialog());

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
    }

    return {
        closeLocationDialog,
        openLocationDialog,
        wireEvents
    };
}
