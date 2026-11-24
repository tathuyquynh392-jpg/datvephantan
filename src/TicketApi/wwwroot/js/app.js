
const API_URL = "";

// ===============================
// 1. KẾT NỐI SIGNALR
// ===============================

const connection = new signalR.HubConnectionBuilder()
    .withUrl("/hubs/bookings")
    .withAutomaticReconnect()
    .build();

window.connection = connection;

// Nhận thông báo khi đơn đặt vé thay đổi trạng thái
connection.on("BookingStatusChanged", (message) => {
    console.log("SignalR notification:", message);

    alert(
        `Đơn đặt vé đã được xác nhận!\nMã đơn: ${message.bookingId}`
    );

    // Tự động tải lại danh sách vé, không cần F5
    loadBookings(false);
});

// Kết nối SignalR
async function startSignalR() {
    try {
        await connection.start();
        console.log("SignalR connected successfully!");
    } catch (error) {
        console.error("SignalR connection error:", error);
        setTimeout(startSignalR, 5000);
    }
}

startSignalR();

// ===============================
// 2. TẢI DANH SÁCH SỰ KIỆN
// ===============================

async function loadEvents() {
    const container = document.getElementById("event-list");

    try {
        const response = await fetch("/api/events");

        if (!response.ok) {
            throw new Error("Không thể tải danh sách sự kiện");
        }

        const events = await response.json();

        container.innerHTML = "";

        if (events.length === 0) {
            container.innerHTML = "<p>Chưa có sự kiện nào.</p>";
            return;
        }

        events.forEach(event => {
            const card = document.createElement("div");
            card.className = "event-card";

            card.innerHTML = `
                <div class="event-info">
                    <h3>${escapeHtml(event.name)}</h3>
                    <p>📍 ${escapeHtml(event.location)}</p>
                    <p>📅 ${new Date(event.eventDate).toLocaleString("vi-VN")}</p>
                    <p>🎟️ Còn ${event.availableTickets} vé</p>
                    <p class="event-price">
                        ${Number(event.price).toLocaleString("vi-VN")} VNĐ
                    </p>
                    <button class="primary-button"
                        onclick="bookEvent('${event.id}')">
                        Đặt vé ngay
                    </button>
                </div>
            `;

            container.appendChild(card);
        });

    } catch (error) {
        console.error("Lỗi tải sự kiện:", error);
        container.innerHTML =
            "<p>Không thể tải sự kiện. Vui lòng thử lại.</p>";
    }
}

// ===============================
// 3. ĐẶT VÉ
// ===============================

async function bookEvent(eventId) {
    const userId = prompt("Nhập User ID:");

    if (!userId || !userId.trim()) return;

    const quantity = Number(prompt("Nhập số lượng vé:"));

    if (!Number.isInteger(quantity) || quantity <= 0) {
        alert("Số lượng vé không hợp lệ!");
        return;
    }

    try {
        const response = await fetch("/api/bookings", {
            method: "POST",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify({
                eventId: eventId,
                userId: userId.trim(),
                quantity: quantity
            })
        });

        const result = await response.json();

        if (!response.ok) {
            alert(result.message || "Đặt vé thất bại!");
            return;
        }

        alert(
            `Đặt vé thành công!\nTrạng thái: ${result.status}`
        );

        // Đăng ký theo dõi đơn đặt vé
        if (connection.state === signalR.HubConnectionState.Connected) {
            await connection.invoke(
                "WatchBooking",
                result.bookingId
            );
        } else {
            console.error("SignalR chưa kết nối, không thể theo dõi đơn.");
        }

        // Điền User ID vào ô tra cứu
        document.getElementById("user-id").value = userId.trim();

        // Hiển thị đơn Pending ban đầu
        await loadBookings(false);

    } catch (error) {
        console.error("Lỗi đặt vé:", error);
        alert("Không thể đặt vé. Vui lòng thử lại!");
    }
}

// ===============================
// 4. TRA CỨU ĐƠN ĐẶT VÉ
// ===============================

async function loadBookings(showPrompt = true) {
    const userInput = document.getElementById("user-id");
    const container = document.getElementById("booking-list");

    let userId = userInput.value.trim();

    if (!userId && showPrompt) {
        alert("Vui lòng nhập User ID để tra cứu.");
        userInput.focus();
        return;
    }

    if (!userId) return;

    try {
        const response = await fetch(
            `/api/users/${encodeURIComponent(userId)}/bookings`
        );

        if (!response.ok) {
            throw new Error("Không thể tải danh sách đơn đặt vé");
        }

        const bookings = await response.json();

        container.innerHTML = "";

        if (bookings.length === 0) {
            container.innerHTML = "<p>Chưa có đơn đặt vé.</p>";
            return;
        }

        bookings.forEach(booking => {
            const card = document.createElement("div");
            card.className = "booking-card";

            card.innerHTML = `
                <p><strong>Mã đơn:</strong> ${booking.bookingId}</p>
                <p><strong>Mã sự kiện:</strong> ${booking.eventId}</p>
                <p><strong>Số lượng vé:</strong> ${booking.quantity}</p>
                <p><strong>Trạng thái:</strong>
                    <span class="status ${booking.status.toLowerCase()}">
                        ${escapeHtml(booking.status)}
                    </span>
                </p>
            `;

            container.appendChild(card);
        });

    } catch (error) {
        console.error("Lỗi tra cứu đơn:", error);
        container.innerHTML =
            "<p>Không thể tải đơn đặt vé.</p>";
    }
}

// ===============================
// 5. HÀM BẢO VỆ HTML
// ===============================

function escapeHtml(value) {
    return String(value).replace(/[&<>"']/g, character => ({
        "&": "&amp;",
        "<": "&lt;",
        ">": "&gt;",
        '"': "&quot;",
        "'": "&#39;"
    })[character]);
}

// ===============================
// 6. KHỞI ĐỘNG GIAO DIỆN
// ===============================

window.bookEvent = bookEvent;
window.loadBookings = loadBookings;
window.loadEvents = loadEvents;

loadEvents();
