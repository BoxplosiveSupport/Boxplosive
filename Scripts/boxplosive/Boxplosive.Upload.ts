module Boxplosive {
    export class Upload {
        protected element: HTMLElement;
        protected $element: JQuery;
        protected upload: string;
        protected $progressbar: JQuery;
        protected $preview: JQuery;
        protected $status: JQuery;
        protected url: string;
        protected fileUrlField: string;
        protected reloadPage: boolean;
        constructor(element: HTMLElement) {
            this.element = element;
            this.$element = $(element);
            this.upload = this.$element.data('upload');
            this.$progressbar = this.$element.find('[data-progress]');
            this.$preview = this.$element.find('[data-preview]');
            this.$status = this.$element.find('[data-status]');
            this.fileUrlField = this.$element.data('file-url-field');
            this.reloadPage = this.$element.data('reload-page');
        }
        init() {
            var self = this,
                previewMaxWidth = this.$preview.outerWidth(),
                previewMaxHeight = this.$preview.outerHeight();
            this.$element.prop('disabled', !$.support.fileInput).fileupload({
                //previewMaxWidth: previewMaxWidth,
                //previewMaxHeight: previewMaxHeight,
                done: function (e, data) {
                    if (self.$preview.length) {
                        $.each(data.files, function () {
                            if (self.upload == 'multiitem') {
                                var $multiitemselects = self.$element.find('#multiitemselects');
                                $multiitemselects.empty();

                                self.$element.find('#multiitemresults').show();
                                self.$element.find('#multiitemresults .items:first').text(data.result['MultiItemCount']);
                                self.$element.find('#multiitemresults .new-items:first').text(data.result['MultiItemNewCount']);

                                if (data.result['MultiItemNewCount'] == 0)
                                    self.$element.find('#multiitemresults .new-items-row').hide();
                                else
                                    self.$element.find('#multiitemresults .new-items-row').show();

                                self.$element.find('#multiitemresults .errors:first').text(data.result['MultiItemErrorCount']);

                                var addMultiItemSelect = function (select) {
                                    return '' +
                                        '<li>' +
                                        '<span class="id">' + select[0] + '</span>' +
                                        '<label class="label">' + select[1] + '</label>' +
                                        '</li>';
                                };
                                var addMultiItemError = function (error) {
                                    var multiItemErrorType = 'Duplicate';
                                    if (error[1] == 'U') {
                                        multiItemErrorType = 'Unknown';
                                    }

                                    return '' +
                                        '<div class="row" style="margin-bottom:0">' +
                                        '<span class="col-sm-6">' + error[0] + '</span>' +
                                        '<span class="col-sm-3">' + multiItemErrorType + ':</span>' +
                                        '<span class="col-sm-3">' + error[2] + '</span>' +
                                        '</div>';
                                };

                                var $multiitemerrors = self.$element.find('#multiitemerrors');
                                var $multiitemerrorsbody = $multiitemerrors.find('.errors');
                                $multiitemerrors.hide();
                                $multiitemerrorsbody.empty();

                                self.$element.find('#multiitembtnok').prop("disabled", true);

                                if (data.result['MultiItemErrorCount'] > 0) {
                                    $.each(data.result['MultiItemErrors'], function (index, value) {
                                        $multiitemerrorsbody.append(addMultiItemError(value));
                                    });

                                    $multiitemerrors.show();
                                }
                                else {
                                    $multiitemselects.append('<ul></ul>')

                                    var $multiitemselectslist = $multiitemselects.children('ul');
                                    if (data.result['MultiItemCount'] < data.result['MultiItemDisplayThreshold']) {
                                        $.each(data.result['MultiItemSelected'], function (index, value) {
                                            $multiitemselectslist.append(addMultiItemSelect(value));
                                        });
                                    }
                                    else {
                                        $multiitemselectslist.append('<li><span class="multiitemcount">' + data.result['MultiItemCount'] + '</span></li>');
                                    }

                                    self.$element.find('#multiitembtnok').prop('disabled', false);
                                }
                            }
                            else if (self.upload == 'single') {
                                self.$preview.html(this.preview);
                            } else {
                                self.$preview.append(this.preview);
                            }
                        });
                    }

                    // Set file name / URL
                    var uploadDataKey = self.fileUrlField;
                    if (data.result[self.fileUrlField] === undefined) { // Split if upload field is nested, e.g. Model.SelectedRule.Image
                        var uploadFiledNameParts = self.fileUrlField.split('_');
                        uploadDataKey = uploadFiledNameParts[uploadFiledNameParts.length - 1];
                    }
                    var $uploadField = self.$element.find('#' + self.fileUrlField);
                    if ($uploadField.length == 0) { // If element is not found, search on the entire page
                        $uploadField = $('#' + self.fileUrlField);
                    }
                    $uploadField.val(data.result[uploadDataKey]);

                    setTimeout(function () {
                        self.$progressbar.css('width', '0%').parent().hide();
                        if (self.reloadPage) {
                            location.reload();
                        }
                    }, 200);
                },
                change: function (e, data) {
                    var error = '';
                    $.each(data.files, function () {
                        if (this.error) {
                            error += error.length ? '\n' + this.error : this.error;
                        }
                    });
                    if (error.length) {
                        self.$status.html(error).show();
                    } else {
                        self.$status.html('').hide();
                    }
                },
                progress: function (e, data) {
                    var progress = data.loaded / data.total * 100;
                    self.$progressbar.parent().show();
                    setTimeout(function () {
                        self.$progressbar.css('width', progress + '%');
                    }, 100);
                }
            })
        }
    }
}

// Init regular image uploads (i.e. not from templates)
// Somehow this is not required for every image upload; Promotions and loyalties work fine without, News Articles do require it.
$(() => {
    $('[data-upload]').each(function () {
        new Boxplosive.Upload(this).init();
    });
});

// Init all image uploads part of templates
var boxplosive = boxplosive || {};
boxplosive.initUpload = function () {
    $('[data-upload]').each(function () {
        new Boxplosive.Upload(this).init();
    });
}

var boxplosive = boxplosive || {};
boxplosive.setupMultiItemBrowser = function () {
    $('.modal.browser.multiitem').on('hidden.bs.modal', function () {
        // Reset errors (messages)
        $('.modal-error').hide();

        // Reset browse elements
        $(this).find('#MultiItemFileName').val('');
        $(this).find('.progress').hide();
        $(this).find('.progress-bar').removeAttr('style');

        // Reset select elements
        $(this).find('#multiitemselects').empty();

        // Reset result elements
        var resultGroup = $(this).find('#multiitemresults');
        $(resultGroup).find('.items, .errors, .new-items').empty();
        $(resultGroup).hide();

        // Reset error elements
        var errorGroup = $(this).find('#multiitemerrors')
        $(errorGroup).find('.errors').empty();
        $(errorGroup).hide();

        $(this).find('#multiitembtnok').prop('disabled', true); // By default the OK button is disabled
    })

    // Disable multi item browser OK button while file is uploading (processing)
    $("[data-upload='multiitem']").change(function () {
        var $modaldialog = $(this).closest('.modal-dialog');
        $modaldialog.find('#multiitembtnok').prop('disabled', true);

        // Reset errors (messages)
        $modaldialog.find('.modal-error').hide();

        // Reset browse elements
        $modaldialog.find('#MultiItemFileName').val('');
        $modaldialog.find('.progress').hide();
        $modaldialog.find('.progress-bar').removeAttr('style');
    });

    // Fire event on browser OK button click
    $('.modal.browser.multiitem').find('.btn-success').click(function () {
        var $modal = $(this).closest('.modal.browser.multiitem');
        var actionUrl = $(this).data('actionurl');
        var callbackFunction = '' + $(this).data('callbackfunction');
        if (actionUrl) {
            $.ajax({
                type: 'POST',
                url: actionUrl,
                contentType: false,
                processData: false,
                data: {},
                success: function (result) {
                    $('#ErrorMessage').hide();

                    // Callback function
                    var fn = window['boxplosive'][callbackFunction];
                    if (typeof fn === 'function') {
                        fn.call(this, $modal, result);
                    }

                    $modal.modal('hide');
                },
                error: function (result) {
                    $('#ErrorMessage').show();
                }
            });
        }
        else {
            $modal.modal('hide');
        }
    });
}