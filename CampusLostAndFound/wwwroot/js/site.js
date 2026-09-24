document.addEventListener("DOMContentLoaded", () => {
    const workspaces = document.querySelectorAll(
        "[data-found-items-workspace]"
    );

    workspaces.forEach((workspace) => {
        const panel = workspace.querySelector(
            "[data-found-item-detail-panel]"
        );

        const openButtons = Array.from(
            workspace.querySelectorAll("[data-found-item-open]")
        );

        const details = Array.from(
            workspace.querySelectorAll("[data-found-item-detail]")
        );

        if (!panel || openButtons.length === 0) {
            return;
        }

        const closePanel = () => {
            workspace.classList.remove("is-detail-open");
            panel.setAttribute("aria-hidden", "true");

            openButtons.forEach((button) => {
                button.classList.remove("is-selected");
                button.setAttribute("aria-expanded", "false");
            });

            window.setTimeout(() => {
                if (!workspace.classList.contains("is-detail-open")) {
                    details.forEach((detail) => {
                        detail.hidden = true;
                    });
                }
            }, 320);
        };

        const openPanel = (itemId, selectedButton) => {
            const selectedDetail = details.find((detail) => {
                return detail.dataset.foundItemDetail === itemId;
            });

            if (!selectedDetail) {
                return;
            }

            details.forEach((detail) => {
                detail.hidden = detail !== selectedDetail;
            });

            openButtons.forEach((button) => {
                const isSelected = button === selectedButton;

                button.classList.toggle(
                    "is-selected",
                    isSelected
                );

                button.setAttribute(
                    "aria-expanded",
                    isSelected ? "true" : "false"
                );
            });

            panel.setAttribute("aria-hidden", "false");
            workspace.classList.add("is-detail-open");
            panel.scrollTop = 0;
        };

        openButtons.forEach((button) => {
            button.addEventListener("click", () => {
                const itemId = button.dataset.foundItemOpen;

                if (!itemId) {
                    return;
                }

                openPanel(itemId, button);
            });
        });

        workspace.addEventListener("click", (event) => {
            const closeButton = event.target.closest(
                "[data-found-item-close]"
            );

            if (closeButton) {
                closePanel();
            }
        });

        document.addEventListener("keydown", (event) => {
            if (
                event.key === "Escape" &&
                workspace.classList.contains("is-detail-open")
            ) {
                closePanel();
            }
        });
    });
});


document.addEventListener("DOMContentLoaded", () => {
    const claimCards = Array.from(
        document.querySelectorAll("[data-my-claim-card]")
    );

    if (claimCards.length === 0) {
        return;
    }

    const closeClaim = (card) => {
        const toggle = card.querySelector(
            "[data-my-claim-toggle]"
        );

        const detail = card.querySelector(
            "[data-my-claim-detail]"
        );

        card.classList.remove("is-open");

        if (toggle) {
            toggle.setAttribute("aria-expanded", "false");
        }

        if (detail) {
            detail.hidden = true;
        }
    };

    const openClaim = (selectedCard) => {
        claimCards.forEach((card) => {
            if (card !== selectedCard) {
                closeClaim(card);
            }
        });

        const toggle = selectedCard.querySelector(
            "[data-my-claim-toggle]"
        );

        const detail = selectedCard.querySelector(
            "[data-my-claim-detail]"
        );

        selectedCard.classList.add("is-open");

        if (toggle) {
            toggle.setAttribute("aria-expanded", "true");
        }

        if (detail) {
            detail.hidden = false;
        }
    };

    claimCards.forEach((card) => {
        const toggle = card.querySelector(
            "[data-my-claim-toggle]"
        );

        if (!toggle) {
            return;
        }

        toggle.addEventListener("click", () => {
            const isOpen = card.classList.contains("is-open");

            if (isOpen) {
                closeClaim(card);
            } else {
                openClaim(card);
            }
        });
    });

    document.addEventListener("keydown", (event) => {
        if (event.key !== "Escape") {
            return;
        }

        claimCards.forEach((card) => {
            closeClaim(card);
        });
    });
});