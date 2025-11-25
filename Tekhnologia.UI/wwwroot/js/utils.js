// ==========================================
// UTILITY FUNCTIONS FOR TEKHNOLOGIA
// ==========================================

// Email validation
function validateEmail(email) {
    const regex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    return regex.test(email);
}

// Password validation (for new passwords)
function validatePassword(password) {
    // Min 8 chars, at least one letter, one number, one special char
    const regex = /^(?=.*[A-Za-z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$/;
    return regex.test(password);
}

// Format date for display
function formatDate(dateString) {
    const date = new Date(dateString);
    return date.toLocaleDateString('en-US', { 
        year: 'numeric', 
        month: 'short', 
        day: 'numeric' 
    });
}

// Format datetime for display
function formatDateTime(dateString) {
    const date = new Date(dateString);
    return date.toLocaleDateString('en-US', { 
        year: 'numeric', 
        month: 'long', 
        day: 'numeric',
        hour: '2-digit',
        minute: '2-digit'
    });
}

// Calculate days between two dates
function daysBetween(startDate, endDate) {
    const start = new Date(startDate);
    const end = new Date(endDate);
    const diffTime = Math.abs(end - start);
    const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24));
    return diffDays;
}

// Calculate days into goal (from creation date to now)
function daysIntoGoal(createdDate) {
    const now = new Date();
    const created = new Date(createdDate);
    const diffTime = Math.abs(now - created);
    return Math.floor(diffTime / (1000 * 60 * 60 * 24));
}

// Calculate days remaining until deadline
function daysRemaining(deadline) {
    if (!deadline) return null;
    
    const now = new Date();
    const end = new Date(deadline);
    const diffTime = end - now;
    const days = Math.ceil(diffTime / (1000 * 60 * 60 * 24));
    
    return days;
}

// Calculate progress percentage (for timeline display)
function calculateProgress(createdDate, deadline) {
    if (!deadline) return 0;
    
    const now = new Date();
    const start = new Date(createdDate);
    const end = new Date(deadline);
    
    const totalDays = (end - start) / (1000 * 60 * 60 * 24);
    const daysPassed = (now - start) / (1000 * 60 * 60 * 24);
    
    if (totalDays <= 0) return 100;
    
    const percentage = (daysPassed / totalDays) * 100;
    return Math.min(Math.max(percentage, 0), 100);
}

// Get priority label based on urgency and importance
function getPriorityLabel(urgency, importance) {
    if (urgency === 'Urgent' && importance === 'Important') return 'Critical';
    if (urgency === 'Urgent') return 'High';
    if (importance === 'Important') return 'Medium';
    return 'Low';
}

// Show notification (temporary)
function showNotification(message, type = 'success') {
    const notification = document.createElement('div');
    notification.className = `notification is-${type} notification-toast`;
    notification.style.cssText = 'position: fixed; top: 20px; right: 20px; z-index: 9999; min-width: 300px;';
    
    notification.innerHTML = `
        <button class="delete" onclick="this.parentElement.remove()"></button>
        ${message}
    `;
    
    document.body.appendChild(notification);
    
    // Auto-remove after 5 seconds
    setTimeout(() => {
        if (notification.parentElement) {
            notification.remove();
        }
    }, 5000);
}

// Show/hide loading spinner
function setLoading(elementId, isLoading, loadingText = 'Loading...', originalText = '') {
    const element = document.getElementById(elementId);
    if (!element) return;
    
    if (isLoading) {
        element.disabled = true;
        element.setAttribute('data-original-text', element.textContent);
        element.textContent = loadingText;
    } else {
        element.disabled = false;
        element.textContent = originalText || element.getAttribute('data-original-text') || originalText;
    }
}

// Escape HTML to prevent XSS
function escapeHtml(text) {
    const div = document.createElement('div');
    div.textContent = text;
    return div.innerHTML;
}

// Get URL parameter
function getUrlParameter(name) {
    const params = new URLSearchParams(window.location.search);
    return params.get(name);
}

// Check if user is logged in (by checking for auth cookie or making API call)
async function checkAuth() {
    try {
        const response = await fetch('/api/user/current', {
            credentials: 'include'
        });
        return response.ok;
    } catch {
        return false;
    }
}

// Redirect to signin if not authenticated
async function requireAuth() {
    const isAuthenticated = await checkAuth();
    if (!isAuthenticated) {
        window.location.href = '/signin';
        return false;
    }
    return true;
}
