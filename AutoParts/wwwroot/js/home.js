document.addEventListener("DOMContentLoaded", function () {

    const yearDropdownButton = document.getElementById("yearDropdownButton");
    const yearBtns = document.querySelectorAll(".year-btn");
    const yearBtnText = document.getElementById("yearBtnText");
    const yearValue = document.getElementById("yearValue");

    const makeDropdownButton = document.getElementById("makeDropdownButton");
    const makeBtnText = document.getElementById("makeBtnText");
    const makeDropdownMenu = document.getElementById("makeDropdownMenu");
    const makeValue = document.getElementById("makeValue");

    const modelDropdownButton = document.getElementById("modelDropdownButton");
    const modelBtnText = document.getElementById("modelBtnText");
    const modelDropdownMenu = document.getElementById("modelDropdownMenu");
    const modelValue = document.getElementById("modelValue");

    const bodyDropdownButton = document.getElementById("bodyDropdownButton");
    const bodyBtnText = document.getElementById("bodyBtnText");
    const bodyDropdownMenu = document.getElementById("bodyDropdownMenu");
    const bodyValue = document.getElementById("bodyValue");

    const engineDropdownButton = document.getElementById("engineDropdownButton");
    const engineBtnText = document.getElementById("engineBtnText");
    const engineDropdownMenu = document.getElementById("engineDropdownMenu");
    const engineValue = document.getElementById("engineValue");

    const searchBtn = document.getElementById("searchBtn");
    const finalVehicleId = document.getElementById("finalVehicleId");

    if (yearBtns.length > 0) {
        yearBtns.forEach(btn => {
            btn.addEventListener("click", function () {
                yearBtns.forEach(b => b.classList.remove("active-year"));
                this.classList.add("active-year");

                const selectedYear = this.getAttribute("data-year");
                yearBtnText.innerText = selectedYear;
                yearValue.value = selectedYear;

                resetDropdown(makeDropdownButton, makeBtnText, makeDropdownMenu, makeValue);
                resetDropdown(modelDropdownButton, modelBtnText, modelDropdownMenu, modelValue);
                resetDropdown(bodyDropdownButton, bodyBtnText, bodyDropdownMenu, bodyValue);
                resetDropdown(engineDropdownButton, engineBtnText, engineDropdownMenu, engineValue);
                searchBtn.disabled = true;
                finalVehicleId.value = "";

                if (yearDropdownButton) yearDropdownButton.click();

                fetch(`/Home/GetMakes?year=${selectedYear}`)
                    .then(res => res.json())
                    .then(data => {
                        populateDropdown(makeDropdownMenu, makeDropdownButton, data, function (item) {
                            makeValue.value = item.id;
                            makeBtnText.innerText = item.name;

                            resetDropdown(modelDropdownButton, modelBtnText, modelDropdownMenu, modelValue);
                            resetDropdown(bodyDropdownButton, bodyBtnText, bodyDropdownMenu, bodyValue);
                            resetDropdown(engineDropdownButton, engineBtnText, engineDropdownMenu, engineValue);
                            searchBtn.disabled = true;
                            finalVehicleId.value = "";

                            loadModels(selectedYear, item.id);
                        });
                    })
                    .catch(err => console.error("Помилка завантаження марок:", err));
            });
        });
    }

    function loadModels(year, makeId) {
        fetch(`/Home/GetModels?year=${year}&makeId=${makeId}`)
            .then(res => res.json())
            .then(data => {
                populateDropdown(modelDropdownMenu, modelDropdownButton, data, function (item) {
                    modelValue.value = item.id;
                    modelBtnText.innerText = item.name;

                    resetDropdown(bodyDropdownButton, bodyBtnText, bodyDropdownMenu, bodyValue);
                    resetDropdown(engineDropdownButton, engineBtnText, engineDropdownMenu, engineValue);
                    searchBtn.disabled = true;
                    finalVehicleId.value = "";

                    loadBodies(year, makeId, item.id);
                });
            })
            .catch(err => console.error("Помилка завантаження моделей:", err));
    }

    function loadBodies(year, makeId, modelId) {
        fetch(`/Home/GetBodyTypes?year=${year}&makeId=${makeId}&modelId=${modelId}`)
            .then(res => res.json())
            .then(data => {
                populateDropdown(bodyDropdownMenu, bodyDropdownButton, data, function (item) {
                    bodyValue.value = item.id;
                    bodyBtnText.innerText = item.name;

                    resetDropdown(engineDropdownButton, engineBtnText, engineDropdownMenu, engineValue);
                    searchBtn.disabled = true;
                    finalVehicleId.value = "";

                    loadEngines(year, makeId, modelId, item.id);
                });
            })
            .catch(err => console.error("Помилка завантаження кузовів:", err));
    }

    function loadEngines(year, makeId, modelId, bodyId) {
        fetch(`/Home/GetEngines?year=${year}&makeId=${makeId}&modelId=${modelId}&bodyId=${bodyId}`)
            .then(res => res.json())
            .then(data => {
                populateDropdown(engineDropdownMenu, engineDropdownButton, data, function (item) {
                    engineValue.value = item.id;
                    engineBtnText.innerText = item.name;

                    if (item.vehicleId) {
                        finalVehicleId.value = item.vehicleId;
                        searchBtn.disabled = false;
                    }
                });
            })
            .catch(err => console.error("Помилка завантаження двигунів:", err));
    }

    function populateDropdown(menuElement, buttonElement, items, onClickCallback) {
        menuElement.innerHTML = "";
        if (!items || items.length === 0) return;

        items.forEach(item => {
            const li = document.createElement("li");
            const btn = document.createElement("button");
            btn.type = "button";
            btn.className = "dropdown-item";
            btn.innerText = item.name;
            btn.setAttribute("data-id", item.id);

            btn.addEventListener("click", function () {
                onClickCallback(item);
                buttonElement.click();
            });

            li.appendChild(btn);
            menuElement.appendChild(li);
        });

        buttonElement.disabled = false;
    }

    function resetDropdown(buttonElement, textElement, menuElement, valueElement) {
        if (buttonElement) buttonElement.disabled = true;
        if (textElement) textElement.innerText = "Оберіть...";
        if (menuElement) menuElement.innerHTML = "";
        if (valueElement) valueElement.value = "";
    }
});