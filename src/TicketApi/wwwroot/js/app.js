
const API_URL = "";

// Tải danh sách sự kiện từ Backend
async function loadEvents() {
    const eventList = document.getElementById("event-list");

    try {
        const response = await fetch(`${API_URL}/api/events`);

        if (!response.ok) {
            throw new Error("Không thể tải danh sách sự kiện");
        }

        const events = await response.json();

        if (events.length === 0) {
            eventList.innerHTML = '<p class="empty-message">Chưa có sự kiện nào.</p>';
            return;
        }

        eventList.innerHTML = events.map(event => `
            <div class="event-card">
                <div class="event-icon">🎫</div>

                <h3>${escapeHtml(event.name)}</h3>

                <div class="event-info">
                    📍 ${escapeHtml(event.location)}<br>
                    📅 ${new Date(event.eventDate).toLocaleString("vi-VN")}<br>
                    🎟️ Còn ${event.availableTickets} vé
                </div>

                <div class="event-price">
                    ${Number(event.price).toLocaleString("vi-VN")} VNĐ
                </div>

                <button onclick="bookEvent('${event.id}')">
                    Đặt vé ngay
                </button>
            </div>
        `).join("");

    } catch (error) {
        eventList.innerHTML = `
            <p class="empty-message">
                ${escapeHtml(error.message)}. Hãy kiểm tra Backend.
            </p>
        `;
    }
}

// Đặt vé
async function bookEvent(eventId) {
    const userId = prompt("Nhập mã người dùng (ví dụ: user001):");

    if (!userId || !userId.trim()) {
        alert("Vui lòng nhập mã người dùng!");
        return;
    }

    const quantity = Number(prompt("Bạn muốn đặt bao nhiêu vé?"));

    if (!Number.isInteger(quantity) || quantity <= 0) {
        alert("Số lượng vé phải là số nguyên lớn hơn 0!");
        return;
    }

    const bookingRequest = {
        eventId: eventId,
        userId: userId.trim(),
        quantity: quantity
    };

    try {
        const response = await fetch(`${API_URL}/api/bookings`, {
            method: "POST",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify(bookingRequest)
        });

        const result = await response.json();

        if (!response.ok) {
            alert(result.message || "Đặt vé thất bại!");
            return;
        }

        alert(
            "Đặt vé thành công!\n" +
            "Mã đặt vé: " + result.bookingId +
            "\nTrạng thái: " + result.status
        );

        document.getElementById("user-id").value = userId.trim();

        await loadBookings();

    } catch (error) {
        alert("Không thể kết nối đến Backend!");
    }
}

// Tra cứu danh sách vé của người dùng
async function loadBookings() {
    const userId = document.getElementById("user-id").value.trim();
    const bookingList = document.getElementById("booking-list");

    if (!userId) {
        alert("Vui lòng nhập User ID!");
        return;
    }

    try {
        const response = await fetch(
            `${API_URL}/api/users/${encodeURIComponent(userId)}/bookings`
        );

        if (!response.ok) {
            throw new Error("Không thể tra cứu vé");
        }

        const bookings = await response.json();

        if (bookings.length === 0) {
            bookingList.innerHTML =
                '<p class="empty-message">Người dùng chưa có đơn đặt vé.</p>';
            return;
        }

        bookingList.innerHTML = bookings.map(booking => {
            let statusClass = "";

            if (booking.status === "Confirmed") {
                statusClass = "confirmed";
            } else if (booking.status === "Rejected") {
                statusClass = "rejected";
            }

            return `
                <div class="booking-item">
                    <strong>Mã đặt vé:</strong> ${booking.bookingId}<br>
                    <strong>Mã sự kiện:</strong> ${booking.eventId}<br>
                    <strong>Số lượng:</strong> ${booking.quantity} vé<br>
                    <strong>Trạng thái:</strong>
                    <span class="status ${statusClass}">
                        ${escapeHtml(booking.status)}
                    </span>
                </div>
            `;
        }).join("");

    } catch (error) {
        bookingList.innerHTML =
            `<p class="empty-message">${escapeHtml(error.message)}</p>`;
    }
}

// Tránh đưa dữ liệu nhập từ API trực tiếp vào HTML
function escapeHtml(value) {
    return String(value).replace(/[&<>"']/g, char => ({
        "&": "&amp;",
        "<": "&lt;",
        ">": "&gt;",
        '"': "&quot;",
        "'": "&#39;"
    })[char]);
}

// Tự động tải sự kiện khi mở trang
loadEvents();
