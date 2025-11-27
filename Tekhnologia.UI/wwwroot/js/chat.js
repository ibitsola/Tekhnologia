export function scrollToBottom(element) {
  try {
    if (!element) return;
    // smooth scroll to bottom
    element.scrollTo({ top: element.scrollHeight, behavior: 'smooth' });
  } catch (e) {
    // ignore
  }
}

export function focusAndResizeTextArea(textarea) {
  if (!textarea) return;
  textarea.focus();
  // simple autosize: reset then set height
  textarea.style.height = 'auto';
  textarea.style.height = Math.min(textarea.scrollHeight, 200) + 'px';
}
