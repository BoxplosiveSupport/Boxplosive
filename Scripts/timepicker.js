// Adds jquery.timepicker to element if it doesn't have the time picker yet
function addTimePicker(element)
{
	var jQueryElement = $(element);

	if (!jQueryElement.hasClass("ui-timepicker-input")) // check if element has time picker
	{
		jQueryElement.timepicker({
			'timeFormat': 'H:i'
		});
		jQueryElement.removeAttr("onclick"); // remove addTimePicker onclick
		element.click(); // raise onclick to show time picker
	}
}