$(document).ready(function () {

    function loadInitialCounts() {
        $.ajax({
            url: '/Cart/GetCartAndFavoritesCount',
            type: 'GET',
            success: function (data) {
                var cartBadge = $('#cart-badge');
                if (cartBadge.length) {
                    cartBadge.text(data.cartCount);
                }
                var favBadge = $('#favorites-badge');
                if (favBadge.length) {
                    favBadge.text(data.favoritesCount);
                }

                if (window.location.pathname.toLowerCase().includes('favorites')) {
                    $('.favorite-btn i').removeClass('fa-regular').addClass('fa-solid text-danger');
                } else if (data.favoriteIds && data.favoriteIds.length > 0) {
                    $('.favorite-btn').each(function () {
                        var button = $(this);
                        var partId = button.data('id');

                        if (data.favoriteIds.includes(partId)) {
                            button.find('i').removeClass('fa-regular').addClass('fa-solid text-danger');
                        } else {
                            button.find('i').removeClass('fa-solid text-danger').addClass('fa-regular');
                        }
                    });
                }
            },
            error: function () {
                console.log("Не вдалося завантажити початкові лічильники або синхронізувати улюблені.");
            }
        });
    }

    loadInitialCounts();


    $('.favorite-btn').click(function (e) {
        e.preventDefault();

        var button = $(this);
        var partId = button.data('id');

        $.ajax({
            url: '/Favorites/ToggleFavorite',
            type: 'POST',
            data: { partId: partId },
            success: function (response) {
                if (response.success) {
                    var icon = button.find('i');

                    if (response.isAdded) {
                        icon.removeClass('fa-regular').addClass('fa-solid text-danger');
                    } else {
                        icon.removeClass('fa-solid text-danger').addClass('fa-regular');

                        if (window.location.pathname.toLowerCase().includes('favorites')) {
                            button.closest('.col').fadeOut(300, function () {
                                $(this).remove();
                            });
                        }
                    }

                    var badge = $('#favorites-badge');
                    if (badge.length) {
                        badge.text(response.count);
                    }
                }
            },
            error: function () {
                console.log("Помилка під час обробки асинхронного запиту улюблених.");
            }
        });
    });


    $('.add-to-cart-btn').click(function (e) {
        e.preventDefault();

        var button = $(this);
        var partId = button.data('id');

        $.ajax({
            url: '/Cart/AddToCart',
            type: 'POST',
            data: { partId: partId, quantity: 1 },
            success: function (response) {
                if (response.success) {
                    var cartBadge = $('#cart-badge');
                    if (cartBadge.length) {
                        cartBadge.text(response.count);
                    }

                    button.addClass('btn-success').removeClass('btn-light');
                    setTimeout(function () {
                        button.addClass('btn-light').removeClass('btn-success');
                    }, 800);
                }
            },
            error: function () {
                console.log("Помилка під час додавання товару в кошик.");
            }
        });
    });

});