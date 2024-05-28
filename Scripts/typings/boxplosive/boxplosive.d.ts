interface JQuery {
  equalHeights(max?: number, callback?: any, correction?: number): JQuery;
  validator: JQuery;
  boxSpinner: any;
  boxAutocomplete: any;
  wymeditor: any;
  initEditor: any;
  initChart: any;
}

interface JQuerySupport {
  fileInput?: boolean;
}

interface FixedFormData {
  prototype: FormData;
  new (form?: HTMLElement): FormData;
}

declare var wym: any;
