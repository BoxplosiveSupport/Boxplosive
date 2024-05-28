// Extends bootstrap tab
(function ($) {
    // set original functions from prototype to call later as super
    var _originalShow = $.fn.modal.Constructor.prototype.show;
    var _originalHide = $.fn.modal.Constructor.prototype.hide;

    $.extend($.fn.modal.Constructor.prototype, {
        show: function (_relatedTarget) {
            this.relatedTarget = _relatedTarget;
            // Find element with browserentry data attribute and check if boxAutocomplete instance is attached
            var $browserentry = this.$element.find('[data-browserentry]');
            if ($browserentry.length && !$browserentry.data('customBoxAutocomplete')) {
                // Trigger parentselect event to force first ajax call to show initial result
                $browserentry.trigger('parentselect');
            }
            // call super
            return _originalShow.call(this, _relatedTarget);
        },
        hide: function (_relatedTarget) {
            // If locked return to prevent hiding
            if (this.isLocked) return;
            // call super
            return _originalHide.call(this, _relatedTarget);
        },
        // Function to lock modal, for example while sending a ajax request to be validated
        lock: function (_relatedTarget) {
            this.isLocked = true;
            // Create new lock event object
            e = $.Event('lock.bs.modal', {
                relatedTarget: _relatedTarget
            });
            // Trigger lock event
            this.$element.trigger(e);
        },
        unlock: function (_relatedTarget) {
            this.isLocked = false;
            // Create new unlock event object
            e = $.Event('unlock.bs.modal', {
                relatedTarget: _relatedTarget
            });
            // Trigger unlock event
            this.$element.trigger(e);
        }
    });
})(jQuery);

// Bind events to boxautocompletes inside browsermodals, to read values and transfer to target.
$(document).on('boxautocompleteselect', '[data-browsermodal] [data-boxautocomplete]:not([data-disableselect=true])', function (e, ui) {
    e.preventDefault();
    var $this = $(this),
        $parent = $this.closest('[data-browsermodal]'),
        relatedTarget = $parent.data('bs.modal').relatedTarget;
    // If the browser had a relatedTarget and the type is singleselect, find repeatable and add new entry
    if (relatedTarget && $parent.data('browsermodal') == 'singleselect') {
        $parent.modal('hide');
        var $relatedParent = $(relatedTarget).closest('[data-repeatable]');
        if ($relatedParent.length) {
            new Boxplosive.Repeatable($relatedParent.get(0), 'add', ui.item);
        }
    }
});

$(function () {
    // Dynamic binding of modals with no id attachment
    if (typeof $.fn.modal != 'undefined') {
        $(document).on('click', '[data-toggle=modal]:not([data-target])', function () {
            var modal = $(this).next('.modal');
            if (modal.length) {
                modal.modal('show');
            }
        });
        // Bind ajax subit on modal tagged with post attribute
        $(document).on('click', '.modal[data-post] [type=submit]', function (e) {
            e.preventDefault();
            var $this = $(this),
                $form = $this.parents('form'),
                $modal = $this.parents('[data-post]'),
                postURL = $modal.data('post'),
                $close = $modal.find('.close');
            if ($form.length && $form.valid() && postURL) {
                var $formelements = $modal.find('input, select, textarea, button');
                $modal.modal('lock');
                $formelements.prop('disabled', true);
                $close.addClass('loading');
                setTimeout(function () {
                    $.ajax({
                        url: postURL,
                        type: 'POST',
                        data: $form.serializeArray(),
                        success: function (data) {
                            $modal.modal('unlock');
                            $modal.modal('hide');
                        },
                        error: function () {
                            $modal.modal('unlock');
                        },
                        complete: function () {
                            $formelements.prop('disabled', false);
                            $close.removeClass('loading');
                        }
                    })
                }, 500);
            }
        });
    }
});

$(document).ready(function () {
    $('.icon-remove').click(function () {
        var newText = "You are about to delete " + $(this).attr('data-type').toLowerCase() + " " + $(this).attr('data-subject');
        $('.modal-body').children('div').children('strong').text(newText);
    });
});