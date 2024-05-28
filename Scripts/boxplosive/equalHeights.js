// Small plugin to set equal heights to all selected elements.
// This is only to use when there is no decent CSS solution possible.
// A future solution could be the yet to be browser supported CSS flexbox module.
// callback: if true, execute
// correction: pixels
$.fn.equalHeights = function (callback, correction) {
    var currentTallest = 0,
        $elem = $(this),
        $singleElem;
    $elem.each(function () {
        $singleElem = $(this);
        if ($singleElem.height() > currentTallest) { currentTallest = $singleElem.height(); }
    });
    currentTallest = (correction === false || correction === undefined) ? currentTallest : currentTallest + correction;
    $elem.css({ 'min-height': currentTallest });
    if (callback !== undefined) {
        callback();
    }
    return this;
};