export function trackBlur(element, menuView){
    element.addEventListener('focusout', event => {
        if (!element.contains(event.relatedTarget))
            menuView.invokeMethodAsync('OnBlur');
    });
}

window.rzBlazor = { trackBlur };