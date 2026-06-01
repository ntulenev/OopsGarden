export function createAdminController(dependencies) {
    const {
        $,
        adminApi,
        confirmDelete,
        escapeHtml,
        loadingState,
        setRegionLoading,
        showError,
        state,
        t,
        withButtonLoading
    } = dependencies;

    async function loadAdmin() {
        if (loadingState.has("admin")) return;
        loadingState.add("admin");
        setRegionLoading("inviteList", true, "loading.admin");
        setRegionLoading("userList", true, "loading.admin");
        let invites;
        let users;
        try {
            [invites, users] = await Promise.all([adminApi.getInvites(), adminApi.getUsers()]);
        } catch (error) {
            $("inviteList").setAttribute("aria-busy", "false");
            $("userList").setAttribute("aria-busy", "false");
            $("inviteList").innerHTML = `<p class="muted">${t("toast.error")}</p>`;
            $("userList").innerHTML = `<p class="muted">${t("toast.error")}</p>`;
            showError(error);
            return;
        } finally {
            loadingState.delete("admin");
        }

        renderInvites(invites);
        renderUsers(users);
    }

    function renderInvites(invites) {
        $("toggleUsedInvitesBtn").textContent = state.hideUsedInvites
            ? t("admin.showUsedInvites")
            : t("admin.hideUsedInvites");
        const inviteList = $("inviteList");
        inviteList.setAttribute("aria-busy", "false");
        inviteList.innerHTML = "";
        for (const invite of invites.filter((invite) => !state.hideUsedInvites || !invite.usedAt)) {
            const url = `${location.origin}/?invite=${invite.code}`;
            const status = invite.usedAt
                ? t("admin.inviteUsed")
                : invite.isRevoked
                    ? t("admin.inviteRevoked")
                    : t("admin.inviteOpen");
            const row = document.createElement("div");
            row.className = "list-row";
            const canDelete = !invite.usedAt;
            row.innerHTML = `<div><strong>${status}</strong>
                <div class="muted">${url}</div></div>
                <div class="row-actions">
                    <button class="ghost" data-copy="${url}">${t("actions.copy")}</button>
                    ${canDelete ? `<button class="danger" data-delete-invite="${invite.id}">${t("actions.delete")}</button>` : ""}
                </div>`;
            inviteList.append(row);
        }
    }

    function renderUsers(users) {
        const userList = $("userList");
        userList.setAttribute("aria-busy", "false");
        userList.innerHTML = "";
        for (const user of users) {
            const row = document.createElement("div");
            row.className = "list-row";
            row.innerHTML = `<div><strong>${escapeHtml(user.displayName)}</strong>
                <div class="muted">${escapeHtml(user.email)} &middot; ${user.plants} ${t("admin.userPlants")}</div></div>
                <div class="row-actions">
                    <button class="ghost" data-block-user="${user.id}" data-block-value="${!user.isBlocked}">${user.isBlocked ? t("common.unblock") : t("common.block")}</button>
                    <button class="danger" data-delete-user="${user.id}">${t("actions.delete")}</button>
                </div>`;
            userList.append(row);
        }
    }

    function wireEvents() {
        $("createInviteBtn").addEventListener("click", async (event) => {
            await withButtonLoading(event.currentTarget, "loading.saving", async () => {
                await adminApi.createInvite();
                await loadAdmin();
            });
        });

        $("toggleUsedInvitesBtn").addEventListener("click", async (event) => {
            state.hideUsedInvites = !state.hideUsedInvites;
            await withButtonLoading(event.currentTarget, "loading.generic", loadAdmin);
        });
    }

    async function handleButtonClick(target) {
        if (target.dataset.deleteInvite) {
            if (!confirmDelete("confirm.deleteInvite")) return true;
            await withButtonLoading(target, "loading.deleting", async () => {
                await adminApi.deleteInvite(target.dataset.deleteInvite);
                await loadAdmin();
            });
            return true;
        }

        if (target.dataset.blockUser) {
            await withButtonLoading(target, "loading.saving", async () => {
                await adminApi.blockUser(target.dataset.blockUser, { isBlocked: target.dataset.blockValue === "true" });
                await loadAdmin();
            });
            return true;
        }

        if (target.dataset.deleteUser) {
            if (!confirmDelete("confirm.deleteUser")) return true;
            await withButtonLoading(target, "loading.deleting", async () => {
                await adminApi.deleteUser(target.dataset.deleteUser);
                await loadAdmin();
            });
            return true;
        }

        return false;
    }

    return {
        handleButtonClick,
        loadAdmin,
        wireEvents
    };
}
