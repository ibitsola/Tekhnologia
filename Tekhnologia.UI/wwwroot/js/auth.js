window.auth = {
    /* ---------- SIGN-IN ---------- */
    signin: async model => {
        // normalize model to ensure API receives Email and Password properties
        const payload = {
            Email: model?.Email ?? model?.email ?? '',
            Password: model?.Password ?? model?.password ?? ''
        };

        console.log('auth.signin payload:', payload);

        const resp = await fetch('/api/auth/login', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(payload),
            credentials: 'include'
        });

        if (!resp.ok) {
            // attempt to parse JSON error list, otherwise fallback to text
            let text = await resp.text();
            try {
                const json = JSON.parse(text);
                if (json?.errors) text = json.errors.join('; ');
            } catch (e) {
                // not JSON, keep text
            }
            console.error('auth.signin failed:', text);
            throw text;
        }

        console.log('auth.signin success, delaying redirect to ensure cookie is processed');
        // small delay so the browser has time to process Set-Cookie headers
        await new Promise(resolve => setTimeout(resolve, 200));
        location.href = '/';
    },

    /* ---------- SIGN-OUT ---------- */
    signout: async () => {
        const resp = await fetch('/api/auth/logout', {
            method: 'POST',
            credentials: 'include'
        });

        if (!resp.ok) {
            console.warn('Logout failed:', resp.status);
        }

        // Force-clear Blazor circuit cookie
        document.cookie = '.AspNetCore.Components.Server.Circuit=; max-age=0; path=/;';

        location.href = '/';
    }
};

// Defensive fallback: if Blazor event wiring fails for any reason,
// attach a delegated click handler to elements with `.auth-button`.
(function () {
    function getInputValue(id) {
        const el = document.getElementById(id);
        return el ? el.value : '';
    }

    function handleFallbackClick(ev) {
        const target = ev.target.closest && ev.target.closest('.auth-button');
        if (!target) return;
        // Only act when it's the signin button (text content match is best-effort)
        const text = (target.textContent || '').toLowerCase();
        if (!/sign ?in|signin/.test(text)) return;

        try {
            const payload = { Email: getInputValue('email'), Password: getInputValue('password') };
            console.log('auth.js fallback click — payload:', payload);
            // call auth.signin but don't await to avoid blocking UI thread
            if (window.auth && typeof window.auth.signin === 'function') {
                window.auth.signin(payload).catch(e => console.error('fallback signin error', e));
            }
        } catch (e) {
            console.error('auth.js fallback handler error', e);
        }
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', () => document.body.addEventListener('click', handleFallbackClick));
    } else {
        document.body.addEventListener('click', handleFallbackClick);
    }
})();
