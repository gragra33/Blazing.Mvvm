/**
 * Gets the scroll height of the specified DOM element as a string.
 * @param {HTMLElement} element - The DOM element to measure.
 * @returns {string} The scroll height of the element.
 */
export function getElementHeight(element) {
    return String(element.scrollHeight);
}

/**
 * Moves focus to the next or previous focusable element in the document.
 * @param {HTMLElement} element - The current element to start from.
 * @param {boolean} isReverse - If true, moves focus in reverse order.
 */
export function focusNextElement(element, isReverse){
    var focusable = [].slice.call(document.querySelectorAll("a, button, input, select, textarea, [tabindex], [contenteditable]"))
        .filter(function($e){
            if($e.disabled || ($e.getAttribute("tabindex") && parseInt($e.getAttribute("tabindex"))<0) && !$e.visible) return false;
            return true;
        })
        .sort(function($a, $b){
            return (parseFloat($a.getAttribute("tabindex") || 99999) || 99999) - (parseFloat($b.getAttribute("tabindex") || 99999) || 99999);
        });
    var next = focusable.indexOf(element ? element : document.activeElement) + (isReverse ? -1 : 1); 
    if(focusable[next])
        focusable[next].focus();
    else
        focusable[0].focus();
};