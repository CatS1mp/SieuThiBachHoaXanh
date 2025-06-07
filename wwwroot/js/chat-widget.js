let isChatOpen = false;
let chatHistory = JSON.parse(localStorage.getItem('chatHistory')) || [];

document.addEventListener('DOMContentLoaded', () => {
    loadChatHistory();
    toggleChat(false);
});

function toggleChat(toggle = true) {
    const chatWidget = document.getElementById('chat-widget');
    const toggleBtn = document.getElementById('chat-toggle-btn');
    if (toggle) {
        isChatOpen = !isChatOpen;
    }
    chatWidget.classList.toggle('hidden', !isChatOpen);
    if (toggleBtn) {
        toggleBtn.style.display = isChatOpen ? 'none' : 'block';
    }
    if (isChatOpen) {
        scrollToBottom();
    }
}

function handleKeyPress(event) {
    if (event.key === 'Enter' && !event.shiftKey) {
        event.preventDefault();
        sendMessage();
    }
}

async function sendMessage() {
    const input = document.getElementById('chat-input');
    const message = input.value.trim();
    if (!message) return;

    appendMessage(message, 'user-message');
    chatHistory.push({ role: 'user', content: message });
    saveChatHistory();

    input.value = '';
    input.disabled = true;

    showTypingIndicator(true);

    try {
        const response = await fetch('/api/chat', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ userInput: message })
        });

        const result = await response.json();

        showTypingIndicator(false);
        input.disabled = false;

        if (result.success) {
            appendMessage(result.html, 'bot-message', true);
            chatHistory.push({ role: 'bot', content: result.html });
            saveChatHistory();
            
            // Add click handlers for product links
            const productLinks = document.querySelectorAll('.bot-message a');
            productLinks.forEach(link => {
                link.addEventListener('click', (e) => {
                    e.preventDefault();
                    window.open(link.href, '_blank');
                });
            });
        } else {
            appendMessage(`Lỗi: ${result.message}`, 'bot-message');
            chatHistory.push({ role: 'bot', content: `Lỗi: ${result.message}` });
            saveChatHistory();
        }
    } catch (error) {
        showTypingIndicator(false);
        input.disabled = false;

        appendMessage(`Lỗi: ${error.message}`, 'bot-message');
        chatHistory.push({ role: 'bot', content: `Lỗi: ${error.message}` });
        saveChatHistory();
    }

    scrollToBottom();
}

function appendMessage(content, className, isHtml = false) {
    const messagesDiv = document.getElementById('chat-messages');
    const messageDiv = document.createElement('div');
    messageDiv.className = `message ${className}`;

    if (isHtml) {
        messageDiv.innerHTML = content;
        
        // Style product images
        const images = messageDiv.querySelectorAll('img');
        images.forEach(img => {
            img.style.maxWidth = '200px';
            img.style.borderRadius = '8px';
            img.style.margin = '10px 0';
        });
        
        // Style product links
        const links = messageDiv.querySelectorAll('a');
        links.forEach(link => {
            link.style.color = '#00a550';
            link.style.textDecoration = 'none';
            link.style.fontWeight = 'bold';
        });
    } else {
        const p = document.createElement('p');
        p.textContent = content;
        messageDiv.appendChild(p);
    }

    messagesDiv.appendChild(messageDiv);
}

function showTypingIndicator(show) {
    const typingIndicator = document.getElementById('typing-indicator');
    typingIndicator.style.display = show ? 'flex' : 'none';
}

function scrollToBottom() {
    const messagesDiv = document.getElementById('chat-messages');
    messagesDiv.scrollTop = messagesDiv.scrollHeight;
}

function saveChatHistory() {
    localStorage.setItem('chatHistory', JSON.stringify(chatHistory.slice(-50)));
}

function loadChatHistory() {
    // Only load the last 10 messages
    const recentMessages = chatHistory.slice(-10);
    recentMessages.forEach(message => {
        appendMessage(message.content, message.role === 'user' ? 'user-message' : 'bot-message', message.role === 'bot');
    });
    scrollToBottom();
}