// Helper function to get bounding client rect
window.getBoundingClientRect = function(selector) {
    const element = document.querySelector(selector);
    if (element) {
        const rect = element.getBoundingClientRect();
        return {
            left: rect.left,
            top: rect.top,
            right: rect.right,
            bottom: rect.bottom,
            width: rect.width,
            height: rect.height
        };
    }
    return { left: 0, top: 0, right: 0, bottom: 0, width: 0, height: 0 };
};

// Return width/height for an image element identified by data-vision-id attribute
window.getImageSizeForVisionId = function(visionId) {
    try {
        // look for image inside board items with matching data-vision-id
        var img = document.querySelector('[data-vision-id="' + visionId + '"] img');
        if (!img) return null;
        return { width: img.naturalWidth || img.clientWidth, height: img.naturalHeight || img.clientHeight };
    }
    catch (e) {
        return null;
    }
};

// Return bounding rect of the topmost element at a client (x,y) point
window.getRectAtPoint = function(x, y) {
    try {
        var el = document.elementFromPoint(x, y);
        if (!el) return null;
        var rect = el.getBoundingClientRect();
        return { left: rect.left, top: rect.top, right: rect.right, bottom: rect.bottom, width: rect.width, height: rect.height };
    } catch (e) {
        return null;
    }
};

// Start document-level resize listeners and callback into .NET
window.startGlobalResize = function(dotNetRef) {
    if (!dotNetRef) return;

    const onMouseMove = function(e) {
        try {
            dotNetRef.invokeMethodAsync('OnGlobalResizeMove', e.clientX, e.clientY);
        } catch (err) {
            // swallow
        }
    };

    const onMouseUp = function(e) {
        try {
            dotNetRef.invokeMethodAsync('OnGlobalResizeEnd', e.clientX, e.clientY);
        } catch (err) {
            // swallow
        }
        document.removeEventListener('mousemove', onMouseMove);
        document.removeEventListener('mouseup', onMouseUp);
        document.removeEventListener('touchmove', onTouchMove);
        document.removeEventListener('touchend', onTouchEnd);
    };

    const onTouchMove = function(e) {
        if (e.touches && e.touches.length > 0) {
            const t = e.touches[0];
            try { dotNetRef.invokeMethodAsync('OnGlobalResizeMove', t.clientX, t.clientY); } catch (err) {}
        }
    };

    const onTouchEnd = function(e) {
        try {
            // touchend doesn't provide touch coords reliably, just call end
            dotNetRef.invokeMethodAsync('OnGlobalResizeEnd', 0, 0);
        } catch (err) {}
        document.removeEventListener('mousemove', onMouseMove);
        document.removeEventListener('mouseup', onMouseUp);
        document.removeEventListener('touchmove', onTouchMove);
        document.removeEventListener('touchend', onTouchEnd);
    };

    document.addEventListener('mousemove', onMouseMove);
    document.addEventListener('mouseup', onMouseUp);
    document.addEventListener('touchmove', onTouchMove, { passive: false });
    document.addEventListener('touchend', onTouchEnd);
};

