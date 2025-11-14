export function getElementHeight(element) {
    return String(element.scrollHeight);
}

export function focusNextElement(element, isReverse){
    var focusable = [].slice.call(document.querySelectorAll("a, button, input, select, textarea, [tabindex], [contenteditable]")).filter(function($e){
        if($e.disabled || ($e.getAttribute("tabindex") && parseInt($e.getAttribute("tabindex"))<0) && !$e.visible) return false;
        return true;
    }).sort(function($a, $b){
        return (parseFloat($a.getAttribute("tabindex") || 99999) || 99999) - (parseFloat($b.getAttribute("tabindex") || 99999) || 99999);
    });
    var next = focusable.indexOf(element ? element : document.activeElement) + (isReverse ? -1 : 1); 
    if(focusable[next])
        focusable[next].focus();
    else
        focusable[0].focus();
};