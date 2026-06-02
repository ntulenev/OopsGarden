export function createWaterPlantDialogController({ $, t }) {
    function openWaterPlantDialog(id, name, date = "") {
        if (!id || !name) return;

        const form = $("waterPlantForm");
        form.reset();
        form.elements.id.value = id;
        form.elements.date.value = date;
        $("waterPlantPrompt").textContent = t("plants.waterPrompt")
            .replace("{name}", name)
            .replace("{date}", date || t("common.today"));
        $("waterPlantDialog").hidden = false;
        $("confirmWaterPlant").focus();
    }

    function closeWaterPlantDialog() {
        const form = $("waterPlantForm");
        form.reset();
        $("waterPlantDialog").hidden = true;
    }

    return {
        closeWaterPlantDialog,
        openWaterPlantDialog
    };
}
