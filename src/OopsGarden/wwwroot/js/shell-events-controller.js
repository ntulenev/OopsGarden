export function createShellEventsController({
    $,
    applyTheme,
    authApi,
    fileToDataUrl,
    formData,
    loadLanguage,
    maxUploadImageSide,
    qs,
    refreshMe,
    renderPublicGardenLink,
    renderShell,
    resetAvatarPreview,
    setView,
    state,
    t,
    toast,
    withButtonLoading
}) {
    function wireEvents() {
        $("themeSelect").addEventListener("change", (event) => {
            applyTheme(event.target.value);
        });

        $("languageSelect").addEventListener("change", async (event) => {
            await loadLanguage(event.target.value);
            if (state.me?.authenticated && state.me.role !== "Admin") {
                await authApi.updateSettings({
                    displayName: state.me.name || t("user.defaultName"),
                    language: state.lang,
                    avatarDataUrl: state.me.avatar || null,
                    isGardenPublic: Boolean(state.me.isGardenPublic)
                });
            }
            renderShell();
        });

        $("loginForm").addEventListener("submit", async (event) => {
            event.preventDefault();
            await withButtonLoading(event.submitter, "loading.login", async () => {
                await authApi.login(formData(event.currentTarget));
                await refreshMe();
            });
        });

        $("adminLoginForm").addEventListener("submit", async (event) => {
            event.preventDefault();
            await withButtonLoading(event.submitter, "loading.login", async () => {
                await authApi.adminLogin(formData(event.currentTarget));
                await refreshMe();
            });
        });

        $("registerForm").addEventListener("submit", async (event) => {
            event.preventDefault();
            await withButtonLoading(event.submitter, "loading.saving", async () => {
                const data = formData(event.currentTarget);
                data.language = state.lang;
                await authApi.register(data);
                await refreshMe();
            });
        });

        $("logoutBtn").addEventListener("click", async (event) => {
            await withButtonLoading(event.currentTarget, "loading.generic", async () => {
                await authApi.logout();
                await refreshMe();
            });
        });

        $("settingsForm").addEventListener("submit", async (event) => {
            event.preventDefault();
            await withButtonLoading(event.submitter, "loading.saving", async () => {
                const form = event.currentTarget;
                const avatarDataUrl = form.dataset.avatarPreview || state.me.avatar || null;
                await authApi.updateSettings({
                    displayName: form.displayName.value,
                    language: state.lang,
                    avatarDataUrl,
                    isGardenPublic: form.elements.isGardenPublic.checked
                });
                await refreshMe();
                resetAvatarPreview();
                toast(t("toast.saved"));
            });
        });

        qs("#settingsForm [name=avatar]").addEventListener("change", async (event) => {
            const form = $("settingsForm");
            const avatarDataUrl = await fileToDataUrl(event.target.files[0], maxUploadImageSide);
            if (!avatarDataUrl) {
                resetAvatarPreview();
                return;
            }

            form.dataset.avatarPreview = avatarDataUrl;
            $("newAvatarPreview").src = avatarDataUrl;
            $("newAvatarPreview").alt = t("settings.newAvatar");
            $("newAvatarPreviewSlot").hidden = false;
            $("cancelAvatarChange").hidden = false;
        });

        $("cancelAvatarChange").addEventListener("click", resetAvatarPreview);

        $("sharePublicGardenLink").addEventListener("click", async () => {
            await navigator.clipboard.writeText(renderPublicGardenLink());
            toast(t("toast.done"));
        });

        $("settingsShortcut").addEventListener("click", () => setView("settings"));
        $("settingsShortcut").addEventListener("keydown", (event) => {
            if (event.key === "Enter" || event.key === " ") {
                event.preventDefault();
                setView("settings");
            }
        });
    }

    return { wireEvents };
}
