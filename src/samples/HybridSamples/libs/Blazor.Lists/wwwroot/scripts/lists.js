/**
 * Scrolls the specified DOM element into view with smooth behavior and nearest alignment.
 * @param {HTMLElement} element - The DOM element to scroll into view.
 */
export function scrollIntoView(element) {
    if (!element) return;
    element.scrollIntoView({ behavior: "smooth", block: "nearest", inline: "nearest" });
}