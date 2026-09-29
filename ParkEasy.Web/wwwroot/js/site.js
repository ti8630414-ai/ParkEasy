// ParkEasy JavaScript Core

// Real-time SignalR Connection
const setupSignalR = () => {
    if (typeof signalR === 'undefined') return;

    const connection = new signalR.HubConnectionBuilder()
        .withUrl("/parkingHub")
        .withAutomaticReconnect()
        .build();

    connection.on("SlotStatusChanged", (spaceId, slotId, status, isAvailable) => {
        console.log(`[SignalR] Slot ${slotId} in space ${spaceId} changed to ${status}`);
        const slotEl = document.querySelector(`[data-slot-id="${slotId}"]`);
        if (slotEl) {
            slotEl.classList.remove('available', 'occupied', 'maintenance');
            if (isAvailable) {
                slotEl.classList.add('available');
            } else {
                slotEl.classList.add('occupied');
            }
        }
    });

    connection.on("NewNotificationReceived", (data) => {
        showToast(data.title, data.message, "info");
        updateNotificationBadge();
    });

    connection.start().catch(err => console.log("SignalR Connection Note:", err.toString()));
};

// Toast notification helper
function showToast(title, message, type = 'info') {
    const container = document.getElementById('toast-container');
    if (!container) return;

    const toastId = 'toast-' + Date.now();
    const bgClass = type === 'success' ? 'bg-success text-white' :
                    type === 'danger' || type === 'error' ? 'bg-danger text-white' :
                    type === 'warning' ? 'bg-warning text-dark' : 'bg-primary text-white';

    const toastHtml = `
        <div id="${toastId}" class="toast align-items-center ${bgClass} border-0 shadow-lg" role="alert" aria-live="assertive" aria-atomic="true">
            <div class="d-flex">
                <div class="toast-body">
                    <strong>${title}</strong><br/>
                    ${message}
                </div>
                <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast" aria-label="Close"></button>
            </div>
        </div>
    `;

    container.insertAdjacentHTML('beforeend', toastHtml);
    const toastEl = document.getElementById(toastId);
    if (toastEl && typeof bootstrap !== 'undefined') {
        const bsToast = new bootstrap.Toast(toastEl, { delay: 5000 });
        bsToast.show();
        toastEl.addEventListener('hidden.bs.toast', () => toastEl.remove());
    }
}

// Update notification badge count
async function updateNotificationBadge() {
    try {
        const res = await fetch('/Notification/GetUnreadCount');
        if (res.ok) {
            const data = await res.json();
            const badge = document.getElementById('notif-badge');
            if (badge) {
                if (data.count > 0) {
                    badge.innerText = data.count > 99 ? '99+' : data.count;
                    badge.classList.remove('d-none');
                } else {
                    badge.classList.add('d-none');
                }
            }
        }
    } catch (e) { }
}

document.addEventListener('DOMContentLoaded', () => {
    setupSignalR();
    updateNotificationBadge();
});
