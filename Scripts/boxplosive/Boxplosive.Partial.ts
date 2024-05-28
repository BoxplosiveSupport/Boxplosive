module Boxplosive {
    export class Partial {
        protected element: HTMLElement;
        protected $element: JQuery;
        protected getURL: string;
        protected postURL: string;
        protected id: string;
        protected loadingButtons: JQuery;
        private _mode: string;
        private _changed: boolean = false;
        private _loading: boolean = false;
        private _disabledElements: JQuery;
        private _anchorButtons: JQuery;
        constructor(element: HTMLElement) {
            this.element = element;
            this.$element = $(element);
            this.getURL = this.$element.data('get');
            this.postURL = this.$element.data('post');
            this._mode = this.$element.data('mode') || 'display';
            this.id = this.$element.data('partialid');
        }
        init() {
            var self = this;
            this.$element.on('click', '[data-action=edit]', function (e) {
                e.preventDefault();
                e.stopPropagation();
                self.edit(this);
            });
            this.$element.on('click', '[data-action=save]', function (e) {
                e.preventDefault();
                var vars = $(this).data('vars');
                self.save(this, 'display', false, 'POST', vars);
            });
            this.$element.on('click', '[data-action=delete]', function (e) {
                e.preventDefault();
                var vars = $(this).data('vars') || [];
                vars.push({ "name": "actionType", "value": "delete" });
                self.save(this, 'edit', false, 'POST', vars);
            });
            this.$element.on('click', '[data-action=saveget]', function (e) {
                e.preventDefault();
                var vars = $(this).data('vars');
                self.save(this, 'display', false, 'GET', vars);
            });
            this.$element.on('click', '[data-action=forcesaveget]', function (e) {
                e.preventDefault();
                var vars = $(this).data('vars');
                self.save(this, 'display', true, 'GET', vars);
            });
            this.$element.on('click', '[data-action=saveandedit]', function (e) {
                e.preventDefault();
                self.save(this, 'edit');
            });
            this.$element.on('change', '[data-change=save]', function (e) {
                e.preventDefault();
                self.save(this, self._mode);
            });
            this.$element.on('change', '[data-change=saveget]', function (e) {
                e.preventDefault();
                self.save(this, 'display', false, 'GET');
            });
            this.$element.on('change', 'input, select, textarea', function () {
                self._changed = true;
            });
            this.$element.on('change', '[data-change=forcesave]', function (e) {
                e.preventDefault();
                self.save(this, self._mode, true);
            });
            this.$element.on('change', '[data-change=forcesaveget]', function (e) {
                e.preventDefault();
                self.save(this, 'display', true, 'GET');
            });
            this.$element.on('fileuploadsubmit', '[data-upload]', function () {
                self._changed = true;
            });
        }
        update() {
            var self = this;
            this.disable();
            setTimeout(function () {
                self.getView(this._mode);
            }, 500);
        }
        save(element: HTMLElement, mode: string = 'display', force: boolean = false, type: string = 'POST', vars: Object = {}) {
            this.updateEditors();
            var self = this,
                data = this.$element.serializeArray();
            data["__RequestVerificationToken"] = this.$element.find('input[name="__RequestVerificationToken"]:first').val();
            data.push({ name: 'id', value: this.id });
            data.push({ name: 'mode', value: mode });
            if (element) {
                var $submitelem = $(element);
                if ($submitelem.attr('name') && $submitelem.val()) {
                    data.push({ name: $submitelem.attr('name'), value: $submitelem.val() });
                }
            }
            $.merge(data, $.extend([], vars));

            var url = self.getURL;
            if (type === 'POST') {
                if ($(element).data('form-post')) {
                    url = $(element).data('form-post');
                } else {
                    url = self.postURL;
                }
            }

            if (force || this.valid()) {
                this.disable();
                setTimeout(function () {
                    $.ajax({
                        url: url,
                        type: type,
                        data: data,
                        dataType: 'html',
                        success: function (data) {
                            self.updateView(data);
                            self._mode = mode;
                            self._changed = false;
                        },
                        error: function () {
                            self.enable();
                        }
                    })
                }, 500);
            }
        }
        valid() {
            this.updateEditors();
            return this.$element.valid() && !this._loading;
        }
        mode() {
            return this._mode;
        }
        changed() {
            return this._changed;
        }
        edit(submitelem?: HTMLElement) {
            var self = this;
            this.disable();
            setTimeout(function () {
                self.getView('edit', submitelem);
            }, 500);
        }
        protected updateEditors() {
        }
        protected getView(mode: string, submitelem?: HTMLElement) {
            var self = this,
                data = {
                    id: this.id,
                    mode: mode
                };
            if (submitelem) {
                var $submitelem = $(submitelem);
                if ($submitelem.attr('name') && $submitelem.val()) {
                    data[$submitelem.attr('name')] = $submitelem.val();
                }
            }
            $.ajax({
                url: this.getURL,
                type: 'GET',
                data: data,
                dataType: 'html',
                success: function (data) {
                    self.updateView(data);
                    self._mode = mode;
                    self.enable();
                },
                error: function () {
                    self.enable();
                }
            })
        }
        protected updateView(html) {
            this.$element.html(html);
            this.$element.find('[data-boxspinner] input').boxSpinner();
            this.$element.find('[data-upload]').each(function () {
                new Boxplosive.Upload(this).init();
            });
            this._loading = false;
        }
        protected disable() {
            this._loading = true;
            this._disabledElements = this.$element.find('input:not(:disabled), textarea:not(:disabled), select:not(:disabled), button:not(:disabled)').prop('disabled', true);
            this._anchorButtons = this.$element.find('a.btn:not(.disabled)').addClass('disabled');
            this.loadingButtons = this.$element.find('[data-action=save]:not(.loading), .datapanel-heading [data-action=edit]:not(.loading)').addClass('loading');
        }
        protected enable() {
            this._loading = false;
            this._disabledElements.prop('disabled', false);
            this._anchorButtons.removeClass('disabled');
            this.loadingButtons.removeClass('loading');
        }
    }
}

$(() => {
    $('[data-partial="independent"]').each(function () {
        var partial = new Boxplosive.Partial(this);
        partial.init();
    })
});

// manually initialize rich text editors (e.g. when template fields are loaded)
var boxplosive = boxplosive || {};