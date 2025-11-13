window.auth = {
    /* ---------- SIGN-IN ---------- */
    signin: async model => {
        const resp = await fetch('/api/auth/login', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(model),
            credentials: 'include'
        });
        if (!resp.ok) throw await resp.text();
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
