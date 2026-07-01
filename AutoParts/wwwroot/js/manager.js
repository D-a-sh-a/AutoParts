$(document).ready(function () {
    function updateOrderUI(selectElement) {
        var status = $(selectElement).val();
        var form = $(selectElement).closest('form');
        var ttnContainer = form.find('.ttn-container');
        var cancelContainer = form.find('.cancel-container');

        ttnContainer.toggleClass('d-none', status !== 'Shipped');
        cancelContainer.toggleClass('d-none', status !== 'Cancelled');

        ttnContainer.find('input').prop('required', status === 'Shipped');
        cancelContainer.find('select').prop('required', status === 'Cancelled');
    }

    $(document).on('change', '.status-select', function () {
        updateOrderUI(this);
    });

    $('.status-select').each(function () {
        updateOrderUI(this);
    });

    $(document).on('change', '.reason-select', function () {
        var form = $(this).closest('form');
        form.find('.custom-reason-input').remove();
        if ($(this).val() === 'Other') {
            $(this).after('<input type="text" name="customReason" class="form-control mt-2 custom-reason-input" placeholder="Вкажіть причину..." required>');
        }
    });

    $(document).on('click', '.delete-item-btn', function () {
        var btn = $(this);
        if (confirm('Видалити цей товар із замовлення?')) {
            $.post('/Manager/RemoveOrderItem', { orderItemId: btn.data('id') }, function (res) {
                if (res.success) {
                    btn.closest('tr').fadeOut(400, function () {
                        $(this).remove();
                        location.reload();
                    });
                } else {
                    alert("Не вдалося видалити товар.");
                }
            }).fail(function () {
                alert("Помилка з'єднання з сервером.");
            });
        }
    });

    $('#inventory-form').on('submit', function (e) {
        e.preventDefault();
        var form = $(this);
        var btn = form.find('button[type="submit"]');
        var originalContent = btn.html();

        btn.html('<i class="fa-solid fa-spinner fa-spin me-2"></i> Зберігаю...').prop('disabled', true);

        $.post(form.attr('action'), form.serialize(), function (res) {
            if (res.success) {
                alert(res.message);
            }
        }).always(function () {
            btn.html(originalContent).prop('disabled', false);
        });
    });

    $(document).on('submit', '.update-stock-form', function (e) {
        e.preventDefault();
        var form = $(this);
        var submitBtn = form.find('button[type="submit"]');
        var originalText = submitBtn.html();

        submitBtn.html('<i class="fa-solid fa-spinner fa-spin"></i>').prop('disabled', true);

        $.post(form.attr('action'), form.serialize(), function (res) {
            submitBtn.html('<i class="fa-solid fa-check"></i>');
            setTimeout(() => submitBtn.html(originalText), 2000);
            submitBtn.prop('disabled', false);

            if (!res.success) alert(res.message);
        }).fail(function () {
            alert("Помилка з'єднання з сервером.");
            submitBtn.html(originalText).prop('disabled', false);
        });
    });

    function handleModalSubmit(selector, targetSelectId, modalId) {
        $(document).on('submit', selector, function (e) {
            e.preventDefault();
            var form = $(this);
            $.post(form.attr('action'), form.serialize(), function (res) {
                if (res.success) {
                    if (targetSelectId) {
                        var text = res.fullName ? `${res.fullName} (${res.email})` : res.name;
                        $(targetSelectId).append(new Option(text, res.id, true, true)).trigger('change');
                    }
                    $(modalId).modal('hide');
                    form[0].reset();
                } else {
                    alert(res.message);
                }
            }).fail(function () {
                alert("Помилка при виконанні запиту.");
            });
        });
    }

    handleModalSubmit('#categoryModal form', '#CategoryId', '#categoryModal');
    handleModalSubmit('#brandModal form', '#BrandId', '#brandModal');
    handleModalSubmit('#customerForm', '#CustomerId', '#customerModal');

    $('select[name="statusFilter"]').on('change', function () {
        var status = $(this).val();
        var url = new URL(window.location.href);
        status ? url.searchParams.set('statusFilter', status) : url.searchParams.delete('statusFilter');
        window.location.href = url.toString();
    });

    $('form').not('#inventory-form, .update-stock-form').on('submit', function () {
        $(this).find('.d-none input, .d-none select').prop('disabled', true);
    });
    $('.select2-search').select2({ theme: 'bootstrap-5' });

    $('.select2-multiple').select2({
        theme: 'bootstrap-5',
        placeholder: "-- Оберіть сумісні автомобілі --",
        allowClear: true
    });

    $('#addPartBtn').on('click', function () {
        var index = Date.now();
        var html = `
        <div class="row g-2 mb-2 item-row">
            <input type="hidden" name="Items.Index" value="${index}" />
            
            <div class="col-7">
                <select name="Items[${index}].PartId" class="form-select part-select" required>
                    <option value="">-- Оберіть товар --</option>
                    ${$('#parts-template').html()}
                </select>
            </div>
            <div class="col-3">
                <input type="number" name="Items[${index}].Quantity" class="form-control" value="1" min="1" placeholder="К-сть">
            </div>
            <div class="col-2">
                <button type="button" class="btn btn-danger btn-sm remove-item-btn"><i class="fa-solid fa-trash"></i></button>
            </div>
        </div>`;

        var $newRow = $(html);
        $('#itemsList').append($newRow);
        $newRow.find('.part-select').select2({ theme: 'bootstrap-5' });
    });
    $(document).on('click', '.remove-item-btn', function () {
        $(this).closest('.item-row').remove();
    });

    $(document).on('change', '#CustomerId', function () {
        var customerId = $(this).val();
        var $addressInput = $('input[name="ShippingAddress"]');

        if (!customerId) {
            $addressInput.val('');
            return;
        }


        $.get('/Manager/GetLastShippingAddress', { customerId: customerId }, function (res) {
            if (res.address) {
                $addressInput.val(res.address);
                $addressInput.addClass('is-valid');
                setTimeout(() => $addressInput.removeClass('is-valid'), 1500);
            } else {
                $addressInput.val('');
            }
        }).fail(function () {
            console.error("Не вдалося завантажити адресу доставки.");
        });
    });
});