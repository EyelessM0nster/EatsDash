(function () {
    const token = document.querySelector('meta[name="request-verification-token"]')?.content;

    function openModal(id) {
        const modal = document.getElementById(id);
        if (!modal) return;
        modal.classList.add("open");
        modal.setAttribute("aria-hidden", "false");
        document.body.style.overflow = "hidden";
    }

    function closeModal(modal) {
        modal.classList.remove("open");
        modal.setAttribute("aria-hidden", "true");
        if (!document.querySelector(".modal.open")) {
            document.body.style.overflow = "";
        }
    }

    document.querySelectorAll("[data-open-modal]").forEach((btn) => {
        btn.addEventListener("click", () => openModal(btn.dataset.openModal));
    });

    document.querySelectorAll("[data-close-modal]").forEach((el) => {
        el.addEventListener("click", () => {
            const modal = el.closest(".modal");
            if (modal) closeModal(modal);
        });
    });

    document.addEventListener("keydown", (e) => {
        if (e.key === "F1") {
            e.preventDefault();
            openModal("helpModal");
            return;
        }

        if (e.key === "Escape") {
            document.querySelectorAll(".modal.open").forEach(closeModal);
        }
    });

    /* Auth tabs */
    const authModal = document.getElementById("authModal");
    if (authModal) {
        const tabs = authModal.querySelectorAll("[data-auth-tab]");
        const panels = authModal.querySelectorAll("[data-panel]");
        const title = authModal.querySelector(".modal-title");

        function switchAuthTab(name) {
            tabs.forEach((t) => t.classList.toggle("active", t.dataset.authTab === name));
            panels.forEach((p) => p.classList.toggle("active", p.dataset.panel === name));
            if (title) {
                title.textContent = name === "register" ? "Регистрация" : "Вход";
            }
        }

        tabs.forEach((tab) => {
            tab.addEventListener("click", () => switchAuthTab(tab.dataset.authTab));
        });

        switchAuthTab(document.body.dataset.authTab || "login");

        if (document.body.dataset.openAuth === "true") {
            openModal("authModal");
        }
    }

    if (document.body.dataset.openReview === "true") {
        openModal("reviewModal");
    }

    if (document.body.dataset.openReport === "true") {
        openModal("reportModal");
    }

    /* Password visibility */
    document.querySelectorAll(".toggle-password").forEach((btn) => {
        btn.addEventListener("click", () => {
            const wrap = btn.closest(".password-wrap");
            const input = wrap?.querySelector("input");
            if (!input) return;

            const show = input.type === "password";
            input.type = show ? "text" : "password";
            btn.classList.toggle("visible", show);
            btn.setAttribute("aria-label", show ? "Скрыть пароль" : "Показать пароль");
        });
    });

    /* Star pickers */
    document.querySelectorAll("[data-rating-picker]").forEach((picker) => {
        const field = picker.closest(".field");
        const input = field?.querySelector(".rating-input");
        if (!input) return;

        const buttons = picker.querySelectorAll(".star-btn");

        function setRating(value) {
            input.value = value;
            buttons.forEach((btn, i) => {
                btn.classList.toggle("filled", i < value);
            });
        }

        buttons.forEach((btn) => {
            btn.addEventListener("click", (e) => {
                e.preventDefault();
                setRating(Number(btn.dataset.rating));
            });
        });

        if (input.value) {
            setRating(Number(input.value));
        }
    });

    /* Edit review */
    document.querySelectorAll("[data-open-edit]").forEach((btn) => {
        btn.addEventListener("click", () => {
            const id = btn.dataset.reviewId;
            const name = btn.dataset.authorName || "";
            const rating = Number(btn.dataset.rating) || 5;
            let text = "";
            try {
                text = JSON.parse(btn.dataset.text || '""');
            } catch {
                text = btn.dataset.text || "";
            }

            document.getElementById("editReviewId").value = id;
            document.getElementById("editAuthorName").value = name;
            document.getElementById("editReviewText").value = text;

            const courierSelect = document.getElementById("editCourierId");
            if (courierSelect && btn.dataset.courierId) {
                courierSelect.value = btn.dataset.courierId;
            }

            const ratingInput = document.getElementById("editRatingInput");
            ratingInput.value = rating;

            const picker = document.getElementById("editStarPicker");
            picker?.querySelectorAll(".star-btn").forEach((star, i) => {
                star.classList.toggle("filled", i < rating);
            });

            openModal("editReviewModal");
        });
    });

    /* Report review */
    const reportReason = document.getElementById("reportReasonKey");
    const customField = document.getElementById("customReasonField");
    const customInput = document.getElementById("reportCustomReason");

    function updateCustomReason() {
        if (!reportReason || !customField) return;
        const isOther = reportReason.value === "other";
        customField.classList.toggle("hidden", !isOther);
        if (customInput) {
            customInput.required = isOther;
        }
    }

    reportReason?.addEventListener("change", updateCustomReason);
    updateCustomReason();

    document.querySelectorAll("[data-open-report]").forEach((btn) => {
        btn.addEventListener("click", () => {
            document.getElementById("reportReviewId").value = btn.dataset.reviewId;
            if (reportReason) reportReason.value = "";
            if (customInput) customInput.value = "";
            updateCustomReason();
            openModal("reportModal");
        });
    });

    /* Likes / dislikes via AJAX */
    function updateReactionButtons(card, data) {
        const likeBtn = card.querySelector('.react-btn[data-reaction="like"]');
        const dislikeBtn = card.querySelector('.react-btn[data-reaction="dislike"]');

        if (likeBtn) {
            likeBtn.querySelector(".react-count").textContent = data.likesCount;
            likeBtn.classList.toggle("active-like", data.userReaction === "like");
            likeBtn.setAttribute("aria-pressed", data.userReaction === "like");
        }
        if (dislikeBtn) {
            dislikeBtn.querySelector(".react-count").textContent = data.dislikesCount;
            dislikeBtn.classList.toggle("active-dislike", data.userReaction === "dislike");
            dislikeBtn.setAttribute("aria-pressed", data.userReaction === "dislike");
        }
    }

    document.querySelectorAll(".react-btn").forEach((btn) => {
        btn.addEventListener("click", async () => {
            if (!token) return;

            const reviewId = btn.dataset.reviewId;
            const reaction = btn.dataset.reaction;
            const card = btn.closest(".review-card");

            btn.disabled = true;

            try {
                const body = new URLSearchParams();
                body.append("reviewId", reviewId);
                body.append("reaction", reaction);
                body.append("__RequestVerificationToken", token);

                const response = await fetch("/Home/React", {
                    method: "POST",
                    headers: {
                        "Content-Type": "application/x-www-form-urlencoded",
                        RequestVerificationToken: token
                    },
                    body: body.toString()
                });

                if (!response.ok) {
                    const err = await response.json().catch(() => ({}));
                    alert(err.error || "Не удалось поставить реакцию. Попробуйте обновить страницу.");
                    return;
                }

                const data = await response.json();
                if (card) updateReactionButtons(card, data);
            } catch {
                alert("Ошибка сети. Проверьте подключение.");
            } finally {
                btn.disabled = false;
            }
        });
    });

    /* Close review menu on outside click */
    document.addEventListener("click", (e) => {
        if (!e.target.closest(".review-menu")) {
            document.querySelectorAll(".review-menu[open]").forEach((m) => m.removeAttribute("open"));
        }
    });

    /* Auto-dismiss toast notifications */
    const toastDismissMs = 2500;
    document.querySelectorAll(".toast").forEach((toast) => {
        setTimeout(() => {
            toast.classList.add("toast--hide");
            const remove = () => toast.remove();
            toast.addEventListener("transitionend", remove, { once: true });
            setTimeout(remove, 450);
        }, toastDismissMs);
    });
})();
