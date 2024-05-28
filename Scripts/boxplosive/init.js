$(function () {
    // Extending bootstrap affix spy with calculations http://getbootstrap.com/javascript/#affix
    if (typeof $.fn.affix != 'undefined') {
        var $sticky = $('nav.sticky');
        $sticky.affix({
            offset: {
                top: function () {
                    $sticky.width($sticky.parent().width());
                    return (this.top = $sticky.offset().top);
                }
            }
        });
        // Fix width of scrolling navigation because of position fixed
        $(window).on('resize', function () {
            $sticky.width($sticky.parent().width());
        });
    }

    // Clickabe table rows
    $(document).on('click', 'tr[data-rowclick]', function (e) {
        window.location.href = $(this).data('rowclick');
    });

    // Prevent rowclick after clicking on form input, anchor, button etc.
    $(document).on('click', 'tr[data-rowclick] a, tr[data-rowclick] button, tr[data-rowclick] input, tr[data-rowclick] textarea, tr[data-rowclick] select, tr[data-rowclick] .actions', function (e) {
        e.stopPropagation();
    });

    $('form').submit(function () {
        var buttons = $('[type="submit"], button:not([type])');
        if ($(this).valid()) {
            buttons.each(function (btn) {
                $(buttons[btn]).prop('disabled', true);
            });
        }
    });

    if (typeof $.fn.validate != 'undefined') {
        // Extend jquery validation with necessary bootstrap styles
        $.validator.setDefaults({
            ignore: [":hidden:not(.wymtextarea)", ".ck"],
            errorElement: 'label',
            errorClass: 'text-danger',
            highlight: function (element, errorClass, validClass) {
                $(element).parent().addClass('has-error');
            },
            unhighlight: function (element, errorClass, validClass) {
                $(element).parent().removeClass('has-error');
            },
            errorPlacement: function (error, element) {
                var inputGroup = element.parents('.input-group'),
                    btn = element.parents('.btn'),
                    parent = inputGroup.length ? inputGroup : btn,
                    element = parent.length ? parent : element;
                error.insertAfter(element);
            },
            submitHandler: function (form) {
                form.submit();
            }
        });

        // Override default date with ISO validation, to prevent issues in IE8
        $.validator.addMethod('date', function (value, element) {
            return this.optional(element) || /^\d{4}[\/\-](0?[1-9]|1[012])[\/\-](0?[1-9]|[12][0-9]|3[01])$/.test(value);
        });

        // Enabled jquery validation for forms
        $('form').each(function () {
            $(this).validate();
        });
        // Validate range inputs
        $(document).on('focus blur', '[data-range] [data-from], [data-range] [data-to]', function () {
            var $this = $(this),
                $parent = $this.parents('[data-range]'),
                $from = $parent.find('[data-from]'),
                $to = $parent.find('[data-to]');
            if ($from.length && $to.length && $from.val().length && $to.val().length) {
                var min = $from.val();
                var range = $parent.data('range');
                if ((range) && ($from.attr('type') === 'time' || $from.attr('type') === 'time12h' || $from.attr('type') === 'time24h')) {
                    var dt = new Date('01/01/13 ' + min);
                    dt.setMinutes(dt.getMinutes() + range);
                    min = ("0" + dt.getHours()).slice(-2) + ':' + ("0" + dt.getMinutes()).slice(-2);
                }

                // Fixes validation on custom input type used for time picker
                if ($from.attr('type') === 'time24h') {
                    if ($from.val() <= $to.val)
                        $to.removeAttr('min').valid();
                    else
                        $to.attr('min', min).valid();
                }

                if ($from.valid()) {
                    if (min != $to.attr('min')) {
                        $to.attr('min', min).valid();
                    }
                } else {
                    $to.removeAttr('min').valid();
                }
            } else {
                $to.removeAttr('min').valid();
            }
        });
    }

    // Enable jquery datepicker only in absence of true input type date support
    if (!Modernizr.inputtypes.date && $.fn.datepicker != 'undefined') {
        $.datepicker.setDefaults({
            dateFormat: 'yy-mm-dd',
            showButtonPanel: true
        });
        $(document).on('focus', '[type=date]', function () {
            var $this = $(this),
                options = {
                    minDate: $this.attr('min'),
                    maxDate: $this.attr('max'),
                    beforeShow: function (elem, options) {
                        var $elem = $(elem);
                        options.minDate = $elem.attr('min');
                        options.maxDate = $elem.attr('max');
                        return options;
                    }
                };
            $(this).datepicker(options);
        });
    }

    // Click on cancel button clears input
    $(document).on('click', 'input:not(:disabled) ~ .form-control-cancel', function () {
        var $input = $(this).prevAll('input');
        $idto = $($input.data('idto'));
        if ($input.val().length) {
            $input.val('').trigger('keydown');
            $idto.val('').trigger('keydown');
        }
    });

    // Autosubmit form after click
    $(document).on('change', '[data-autosubmit]', function () {
        this.form.submit();
    });

    // Charcounter, updates all elements referring to input id by charcountfor data attribute.
    $(document).on('keyup change', '[data-charcount]', function () {
        var $this = $(this),
            id = this.id,
            maxlength = $this.attr('maxlength');
        if (id.length) {
            // If a maxlength is specified, the count is shown as a countdown.
            if (parseInt(maxlength, 10)) {
                $('[data-charcountfor=#' + id + ']').html((parseInt(maxlength, 10) - $(this).val().length).toString());
            } else {
                $('[data-charcountfor=#' + id + ']').html($(this).val().length);
            }
        }
    });

    // Protect inputs with a maxlength when needed
    $(document).on('input propertychange paste', 'input[data-rule-maxlength]', function () {
        $(this).attr('maxlength', $(this).data('rule-maxlength'));
    });

    // Global ajax failure capture
    $(document).ajaxError(function (xhr, props) {
        if (props.status === 403) {
            location.reload();
        }
        else {
            if ($('body').hasClass('modal-open')) { // pop-up on page
                $('.modal:visible').find('.modal-error').show();
            }
            else {
                $('#ErrorMessage').show();
            }
        }
    });

    $(document).on("click", "[data-buttonmodal]", function (e) {
        e.preventDefault();
        var formId = $(this).data('formid');
        var url = $(this).data('url');
        var subject = $(this).data('subject');

        if (subject) {
            $('.modal-body-subject-replaceable').empty().html(subject);
        }

        if (typeof formId !== "undefined") {
            $('#modal-btn-ok').bind('click',
                function () {
                    $('#' + formId).submit();
                });
        } else {
            $('#modal-btn-ok').bind('click',
                function () {
                    location.href = url;
                });
        }
    });
});

function getFormAntiForgeryToken(formId) {
    // Form data
    var form = $(formId);
    var token = $('input[name="__RequestVerificationToken"]', form).val();
    return token;
}