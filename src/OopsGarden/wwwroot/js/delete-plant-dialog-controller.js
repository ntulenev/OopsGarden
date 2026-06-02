export function createDeletePlantDialogController({ $, t }) {
    function openDeletePlantDialog(id, name) {
        if (!id || !name) return;

        const form = $("deletePlantForm");
        form.reset();
        form.elements.id.value = id;
        form.elements.name.value = name;
        $("deletePlantWarning").textContent = t("plants.deleteWarning").replace("{name}", name);
        $("confirmDeletePlant").disabled = true;
        $("deletePlantDialog").hidden = false;
        form.elements.confirmationName.focus();
    }

    function closeDeletePlantDialog() {
        const form = $("deletePlantForm");
        form.reset();
        $("confirmDeletePlant").disabled = true;
        $("deletePlantDialog").hidden = true;
    }

    function updateDeletePlantConfirmationState() {
        const form = $("deletePlantForm");
        $("confirmDeletePlant").disabled = form.elements.confirmationName.value.trim() !== form.elements.name.value;
    }

    return {
        closeDeletePlantDialog,
        openDeletePlantDialog,
        updateDeletePlantConfirmationState
    };
}
