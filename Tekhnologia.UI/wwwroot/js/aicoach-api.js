export async function fetchConversations() {
  const res = await fetch('/api/conversations', { credentials: 'include' });
  if (!res.ok) throw new Error('Failed to load conversations');
  return await res.json();
}

export async function fetchConversation(id) {
  const res = await fetch(`/api/conversations/${id}`, { credentials: 'include' });
  if (!res.ok) throw new Error('Failed to load conversation');
  return await res.json();
}

export async function deleteConversation(id) {
  const res = await fetch(`/api/conversations/${id}`, { method: 'DELETE', credentials: 'include' });
  return { ok: res.ok, status: res.status };
}

export async function postConversation(payload) {
  const res = await fetch('/api/conversations', {
    method: 'POST',
    credentials: 'include',
    headers: { 'Content-Type': 'application/json' },
    body: payload
  });
  if (!res.ok) throw new Error('Failed to save conversation');
  return await res.json();
}

export async function updateConversation(id, payload) {
  const res = await fetch(`/api/conversations/${id}`, {
    method: 'PUT',
    credentials: 'include',
    headers: { 'Content-Type': 'application/json' },
    body: payload
  });
  if (!res.ok) throw new Error('Failed to update conversation');
  return await res.json();
}
