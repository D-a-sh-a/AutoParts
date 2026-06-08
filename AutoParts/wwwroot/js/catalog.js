document.addEventListener("DOMContentLoaded", function () {
    var slider = document.getElementById('price-slider');
    if (!slider) return;

    var minInput = document.getElementById('minPriceInput');
    var maxInput = document.getElementById('maxPriceInput');

    var catalogMin = parseInt(slider.getAttribute('data-catalog-min')) || 0;
    var catalogMax = parseInt(slider.getAttribute('data-catalog-max')) || 10000;

    if (catalogMin === catalogMax) {
        catalogMax = catalogMin + 1;
    }

    var startMin = minInput && minInput.value ? parseInt(minInput.value) : catalogMin;
    var startMax = maxInput && maxInput.value ? parseInt(maxInput.value) : catalogMax;

    noUiSlider.create(slider, {
        start: [startMin, startMax],
        connect: true,
        range: {
            'min': catalogMin,
            'max': catalogMax
        },
        step: 1,
        format: {
            to: function (value) { return Math.round(value); },
            from: function (value) { return Number(value); }
        }
    });

    slider.noUiSlider.on('update', function (values, handle) {
        if (handle === 0) {
            if (minInput) minInput.value = values[0];
        } else {
            if (maxInput) maxInput.value = values[1];
        }
    });

    function setSliderFromInputs() {
        slider.noUiSlider.set([minInput.value, maxInput.value]);
    }

    if (minInput) {
        minInput.addEventListener('change', function () {
            setSliderFromInputs();
            document.getElementById('filterForm').submit();
        });
    }

    if (maxInput) {
        maxInput.addEventListener('change', function () {
            setSliderFromInputs();
            document.getElementById('filterForm').submit();
        });
    }

    slider.noUiSlider.on('change', function () {
        var form = document.getElementById('filterForm');
        if (form) form.submit();
    });

    const categoryContainer = document.getElementById("categoryContainer");
    const catScrollLeft = document.getElementById("catScrollLeft");
    const catScrollRight = document.getElementById("catScrollRight");

    if (categoryContainer && catScrollLeft && catScrollRight) {
        const scrollAmount = 250;

        catScrollLeft.addEventListener("click", function () {
            categoryContainer.scrollBy({
                left: -scrollAmount,
                behavior: "smooth"
            });
        });

        catScrollRight.addEventListener("click", function () {
            categoryContainer.scrollBy({
                left: scrollAmount,
                behavior: "smooth"
            });
        });
    }
});