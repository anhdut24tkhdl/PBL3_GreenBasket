document.addEventListener("DOMContentLoaded", function () {
    const keywordInput = document.querySelector(".order-filter-form input[name='keyword']");
    const cards = Array.from(document.querySelectorAll(".order-card"));

    if (!keywordInput || cards.length === 0) {
        return;
    }

    keywordInput.addEventListener("input", function () {
        const keyword = (keywordInput.value || "").trim().toLowerCase();
        cards.forEach((card) => {
            const text = (card.getAttribute("data-order-search") || "").toLowerCase();
            card.parentElement.style.display = !keyword || text.includes(keyword) ? "" : "none";
        });
    });
});
