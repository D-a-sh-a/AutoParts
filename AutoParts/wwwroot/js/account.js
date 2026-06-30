$(document).ready(function () {

    $('.cancel-order-btn').on('click', function () {
        var orderId = $(this).data('order-id');
        $('#cancelOrderId').val(orderId);
        $('#cancelOrderNumber').text('#' + orderId);
    });

    $('#cancelReason').on('change', function () {
        if ($(this).val() === 'Other') {
            $('#customReasonContainer').removeClass('d-none');
            $('#customReason').attr('required', 'required');
        } else {
            $('#customReasonContainer').addClass('d-none');
            $('#customReason').removeAttr('required');
        }
    });

    $('#changePasswordModal form').on('submit', function (e) {
        e.preventDefault();
        var form = $(this);
        var btn = form.find('button[type="submit"]');

        var newPass = form.find('input[name="NewPassword"]').val();
        var confirmPass = form.find('input[name="ConfirmPassword"]').val();

        if (newPass !== confirmPass) {
            alert("Новий пароль та підтвердження не співпадають!");
            return;
        }

        btn.prop('disabled', true).text('Оновлення...');

        $.post(form.attr('action'), form.serialize(), function (res) {
            btn.prop('disabled', false).text('Зберегти пароль');

            if (res.success) {
                alert(res.message);
                $('#changePasswordModal').modal('hide');
                form.trigger('reset');
            } else {
                alert(res.message || "Помилка при зміні пароля");
            }
        });
    });

    $('#cancelOrderForm').on('submit', function (e) {
        e.preventDefault();

        var form = $(this);
        var btn = form.find('button[type="submit"]');

        btn.prop('disabled', true).text('Скасування...');

        $.post('/Account/CancelOrder', form.serialize(), function (res) {
            if (res.success) {
                $('#cancelOrderModal').modal('hide');
                location.reload();
            } else {
                alert(res.message || "Помилка при скасуванні.");
                btn.prop('disabled', false).text('Так, скасувати');
            }
        }).fail(function () {
            alert("Помилка з'єднання з сервером.");
            btn.prop('disabled', false).text('Так, скасувати');
        });
    });

});