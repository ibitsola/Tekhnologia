// ==========================================
// API HELPER FUNCTIONS FOR TEKHNOLOGIA
// ==========================================

const API_BASE = '';  // Empty since we're on same domain

// Generic API call function
async function apiCall(endpoint, options = {}) {
    const defaultOptions = {
        headers: {
            'Content-Type': 'application/json',
        },
        credentials: 'include',  // Important for cookies
    };

    const finalOptions = {
        ...defaultOptions,
        ...options,
        headers: {
            ...defaultOptions.headers,
            ...options.headers,
        },
    };

    try {
        const response = await fetch(`${API_BASE}${endpoint}`, finalOptions);
        
        // Handle different response types
        const contentType = response.headers.get('content-type');
        let data;
        
        if (contentType && contentType.includes('application/json')) {
            data = await response.json();
        } else {
            data = await response.text();
        }

        if (!response.ok) {
            const error = new Error(data.message || data || 'API request failed');
            error.status = response.status;
            error.data = data;
            throw error;
        }

        return data;
    } catch (error) {
        console.error('API Error:', error);
        throw error;
    }
}

// ===================
// AUTH API CALLS
// ===================

async function login(email, password) {
    return await apiCall('/api/auth/login', {
        method: 'POST',
        body: JSON.stringify({ email, password }),
    });
}

async function register(name, email, password, confirmPassword) {
    return await apiCall('/api/auth/register', {
        method: 'POST',
        body: JSON.stringify({ name, email, password, confirmPassword }),
    });
}

async function logout() {
    return await apiCall('/api/auth/logout', {
        method: 'POST',
    });
}

// ===================
// USER API CALLS
// ===================

async function getCurrentUser() {
    return await apiCall('/api/user/current');
}

async function getUserProfile(userId) {
    return await apiCall(`/api/user/${userId}`);
}

async function updateUserProfile(userId, name) {
    return await apiCall(`/api/user/${userId}`, {
        method: 'PUT',
        body: JSON.stringify({ name }),
    });
}

async function updateUserPassword(userId, oldPassword, newPassword) {
    return await apiCall(`/api/user/${userId}/password`, {
        method: 'PUT',
        body: JSON.stringify({ oldPassword, newPassword }),
    });
}

async function deleteUser(userId, email, password) {
    return await apiCall(`/api/user/${userId}`, {
        method: 'DELETE',
        body: JSON.stringify({ email, password }),
    });
}

// ===================
// GOAL API CALLS
// ===================

async function getUserGoals(userId) {
    return await apiCall(`/api/goal/user/${userId}`);
}

async function createGoal(userId, goalData) {
    return await apiCall('/api/goal', {
        method: 'POST',
        body: JSON.stringify({
            userId,
            ...goalData
        }),
    });
}

async function updateGoal(goalId, userId, goalData) {
    return await apiCall(`/api/goal/${goalId}`, {
        method: 'PUT',
        body: JSON.stringify({
            userId,
            ...goalData
        }),
    });
}

async function markGoalComplete(goalId, userId) {
    return await apiCall(`/api/goal/${goalId}/complete`, {
        method: 'PUT',
        body: JSON.stringify({ userId }),
    });
}

async function deleteGoal(goalId, userId) {
    return await apiCall(`/api/goal/${goalId}?userId=${userId}`, {
        method: 'DELETE',
    });
}

// ===================
// ADMIN API CALLS
// ===================

async function getAllUsers() {
    return await apiCall('/api/admin/users');
}

async function promoteToAdmin(userId) {
    return await apiCall(`/api/admin/promote/${userId}`, {
        method: 'POST',
    });
}

async function deleteUserAsAdmin(userId) {
    return await apiCall(`/api/admin/user/${userId}`, {
        method: 'DELETE',
    });
}

// ===================
// JOURNAL API CALLS
// ===================

async function getUserJournalEntries(userId) {
    return await apiCall(`/api/journal/user/${userId}`);
}

async function createJournalEntry(userId, content, isPublic = false) {
    return await apiCall('/api/journal', {
        method: 'POST',
        body: JSON.stringify({ userId, content, isPublic }),
    });
}

async function updateJournalEntry(entryId, userId, content, isPublic) {
    return await apiCall(`/api/journal/${entryId}`, {
        method: 'PUT',
        body: JSON.stringify({ userId, content, isPublic }),
    });
}

async function deleteJournalEntry(entryId, userId) {
    return await apiCall(`/api/journal/${entryId}?userId=${userId}`, {
        method: 'DELETE',
    });
}

// ===================
// VISION BOARD API CALLS
// ===================

async function getUserVisionBoardItems(userId) {
    return await apiCall(`/api/visionboard/user/${userId}`);
}

async function createVisionBoardItem(userId, formData) {
    // For file uploads, don't set Content-Type header
    return await apiCall('/api/visionboard', {
        method: 'POST',
        headers: {},  // Let browser set multipart/form-data
        body: formData,
    });
}

async function deleteVisionBoardItem(itemId, userId) {
    return await apiCall(`/api/visionboard/${itemId}?userId=${userId}`, {
        method: 'DELETE',
    });
}

// Export all functions for use in other scripts
window.api = {
    // Auth
    login,
    register,
    logout,
    
    // User
    getCurrentUser,
    getUserProfile,
    updateUserProfile,
    updateUserPassword,
    deleteUser,
    
    // Goals
    getUserGoals,
    createGoal,
    updateGoal,
    markGoalComplete,
    deleteGoal,
    
    // Admin
    getAllUsers,
    promoteToAdmin,
    deleteUserAsAdmin,
    
    // Journal
    getUserJournalEntries,
    createJournalEntry,
    updateJournalEntry,
    deleteJournalEntry,
    
    // Vision Board
    getUserVisionBoardItems,
    createVisionBoardItem,
    deleteVisionBoardItem,
};
