window.downloadFromDataUrl = function (dataUrl, filename) {
    const link = document.createElement('a');
    link.href = dataUrl;
    link.download = filename || 'download';
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
};

window._tekhnologia = window._tekhnologia || {};
window._tekhnologia.registerVisibilityHandler = function (dotNetRef) {
    function onVisible() {
        if (document.visibilityState === 'visible') {
            try {
                dotNetRef.invokeMethodAsync('OnPageVisible');
            } catch (e) {
                console.error(e);
            }
        }
    }

    // store references so unregister can remove them
    window._tekhnologia._visibilityHandler = onVisible;
    window._tekhnologia._visibilityDotNetRef = dotNetRef;
    document.addEventListener('visibilitychange', onVisible);
};

window._tekhnologia.unregisterVisibilityHandler = function () {
    try {
        const h = window._tekhnologia._visibilityHandler;
        if (h) document.removeEventListener('visibilitychange', h);
        if (window._tekhnologia._visibilityDotNetRef) {
            try { window._tekhnologia._visibilityDotNetRef.dispose(); } catch (e) { }
            window._tekhnologia._visibilityDotNetRef = null;
        }
        window._tekhnologia._visibilityHandler = null;
    } catch (e) { console.error(e); }
};
