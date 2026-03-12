(function() {
  let tooltip = null;

  function removeTooltip() {
    if (tooltip) {
      tooltip.remove();
      tooltip = null;
    }
  }

  function createTooltip(x, y, text) {
    removeTooltip();
    tooltip = document.createElement('div');
    Object.assign(tooltip.style, {
      position: 'fixed',
      left: x + 'px',
      top: (y - 45) + 'px',
      background: '#4f46e5',
      color: 'white',
      padding: '6px 12px',
      borderRadius: '6px',
      fontSize: '13px',
      cursor: 'pointer',
      zIndex: '999999',
      boxShadow: '0 2px 8px rgba(0,0,0,0.3)',
      fontFamily: 'system-ui, sans-serif',
      whiteSpace: 'nowrap'
    });
    tooltip.textContent = '📚 Save to WordsNote';

    tooltip.addEventListener('click', function(e) {
      e.stopPropagation();
      e.preventDefault();

      try {
        chrome.runtime.sendMessage(
          { action: 'saveToInbox', text: text },
          function(response) {
            if (response && response.success) {
              tooltip.textContent = '✅ Saved!';
              tooltip.style.background = '#16a34a';
            } else {
              tooltip.textContent = '❌ Failed - Login first';
              tooltip.style.background = '#dc2626';
            }
            setTimeout(removeTooltip, 1500);
          }
        );
      } catch (err) {
        tooltip.textContent = '❌ Error';
        setTimeout(removeTooltip, 1500);
      }
    });

    document.body.appendChild(tooltip);
  }

  document.addEventListener('mouseup', function(e) {
    const selection = window.getSelection();
    const text = selection ? selection.toString().trim() : '';

    if (text.length > 0 && text.length < 200) {
      createTooltip(e.clientX, e.clientY, text);
    } else {
      removeTooltip();
    }
  });

  document.addEventListener('mousedown', function(e) {
    if (tooltip && !tooltip.contains(e.target)) {
      removeTooltip();
    }
  });
})();
