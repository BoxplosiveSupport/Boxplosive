$(document).on('click', '[data-products]', function (e) {
    var url = $(this).data('post');
    var parentForm = $(this).parents('form:first');
    var token = getFormAntiForgeryToken(parentForm);
    var productsSearchTextbox = $(this).parent().find('#productsearch-input');
    var inputValue = productsSearchTextbox.val();
    var data = { __RequestVerificationToken: token, 'gtin': inputValue };
    var $parent = $(this).closest('[data-repeatable]');
    var args = { 'id': inputValue, 'label': "(" + inputValue + ")", 'value': "(" + inputValue + ")" };

    if (inputValue !== undefined && inputValue != null && inputValue != "") {
        $.ajax({
            url: url,
            type: 'POST',
            data: data,
            dataType: 'json',
            success: function (data) {
                if (data.Name == 'error') {
                    $('#AddProductErrorMessage').show();
                    $('#AddProductErrorMessage').html('<span class="icon icon-danger"></span> <strong>' + data.Description + '</strong>');
                } else {
                    new Boxplosive.Repeatable($parent.get(0), 'add', args);
                    productsSearchTextbox.val("");
                }
            },
            error: function () {
                console.log('error');
            }
        });
    } else {
        $('#AddProductErrorMessage').show();
        $('#AddProductErrorMessage').html('<span class="icon icon-danger"></span> <strong>Input a valid number</strong>');
    }
});

$(document).on('focusin', '#productsearch-input', function (e) {
    if ($('#AddProductErrorMessage').is(':visible')) {
        $('#AddProductErrorMessage').hide();
    }
});