export function trackBlur(element, menuView){
    if (!element) return  // prevent race-condition from IsDrawerVisible state change during calling this JS function
    element.addEventListener('focusout', event => {
        if (!element.contains(event.relatedTarget))
            menuView.invokeMethodAsync('OnBlur');
    });
}