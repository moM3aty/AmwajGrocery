// =========================================
// 1. إدارة السلة (Cart Logic)
// =========================================
let cart = {};

// تحميل السلة من التخزين المحلي عند فتح الموقع
if (localStorage.getItem('amwaj_cart')) {
    try {
        cart = JSON.parse(localStorage.getItem('amwaj_cart'));
        updateCartUI();
    } catch (e) { cart = {}; }
}

function saveCart() {
    localStorage.setItem('amwaj_cart', JSON.stringify(cart));
    updateCartUI();
}

function updateQuantity(id, action) {
    const display = document.getElementById(`qty-${id}`);
    if (!display) return;
    let val = parseInt(display.textContent);
    if (action === 'inc') val++;
    if (action === 'dec' && val > 1) val--;
    display.textContent = val;
}

function addToCart(id, name, price) {
    const qtyDisplay = document.getElementById(`qty-${id}`);
    const qty = qtyDisplay ? parseInt(qtyDisplay.textContent || 1) : 1;

    if (cart[id]) {
        cart[id].qty += qty;
    } else {
        cart[id] = { name: name, price: price, qty: qty };
    }

    saveCart();
    const msg = isArabic ? `تم إضافة ${qty} من ${name} للسلة` : `Added ${qty} of ${name} to cart`;
    showToast(msg);

    // إعادة تعيين العداد لـ 1
    if (qtyDisplay) qtyDisplay.textContent = 1;
}

function updateCartUI() {
    const count = Object.values(cart).reduce((a, b) => a + b.qty, 0);
    const cartCountEl = document.querySelector('.cart-count');
    if (cartCountEl) cartCountEl.textContent = count;
}

function removeFromCart(id) {
    delete cart[id];
    saveCart();
    openCartModal(); // تحديث عرض السلة
}

// =========================================
// 2. نافذة السلة (Cart Modal)
// =========================================
function openCartModal() {
    const modal = document.getElementById('cart-modal');
    const container = document.getElementById('cart-items-container');
    const totalPriceEl = document.getElementById('cart-total-price');

    container.innerHTML = '';
    let total = 0;
    let isEmpty = true;
    const currency = isArabic ? 'ر.ع' : 'OMR';

    for (let id in cart) {
        isEmpty = false;
        let item = cart[id];
        let itemTotal = item.price * item.qty;
        total += itemTotal;

        container.innerHTML += `
            <div class="cart-item" style="display:flex; justify-content:space-between; align-items:center; border-bottom:1px solid #eee; padding:10px 0;">
                <div>
                    <div style="font-weight:bold;">${item.name}</div>
                    <div style="font-size:0.85em; color:#777;">${item.price.toFixed(3)} ${currency} × ${item.qty}</div>
                </div>
                <div style="display:flex; align-items:center; gap:10px;">
                    <span style="font-weight:bold; color:var(--amwaj-accent);">${itemTotal.toFixed(3)}</span>
                    <button onclick="removeFromCart(${id})" style="background:none; border:none; color:red; cursor:pointer;"><i class="fas fa-trash"></i></button>
                </div>
            </div>
        `;
    }

    if (isEmpty) {
        const emptyMsg = isArabic ? 'السلة فارغة، تسوق الآن!' : 'Cart is empty, shop now!';
        container.innerHTML = `<div style="text-align:center; padding:20px; color:#777;"><i class="fas fa-shopping-basket fa-2x"></i><p>${emptyMsg}</p></div>`;
    }

    totalPriceEl.textContent = total.toFixed(3) + ' ' + currency;
    modal.style.display = 'flex';
}

function closeCartModal(e) {
    // يغلق عند الضغط على الخلفية أو زر الإغلاق
    if (e === null || e.target.id === 'cart-modal') {
        document.getElementById('cart-modal').style.display = 'none';
    }
}

// =========================================
// 3. إرسال الطلب (API + WhatsApp)
// =========================================
async function sendOrderToWhatsApp() {
    if (Object.keys(cart).length === 0) {
        showToast(isArabic ? "السلة فارغة!" : "Cart is empty!");
        return;
    }

    // 1. تجهيز البيانات
    let orderItems = [];
    let total = 0;
    const currency = isArabic ? 'ر.ع' : 'OMR';

    for (let id in cart) {
        let item = cart[id];
        total += item.price * item.qty;
        orderItems.push({
            name: item.name,
            qty: item.qty,
            price: item.price
        });
    }

    const orderData = {
        totalAmount: total,
        items: orderItems
    };

    // تغيير حالة الزر أثناء التحميل
    const btn = document.querySelector('.cart-footer .add-to-cart-btn');
    const originalText = btn.innerHTML;
    btn.innerHTML = '<i class="fas fa-spinner fa-spin"></i> ...';
    btn.disabled = true;

    try {
        // 2. إرسال للسيرفر للحفظ في قاعدة البيانات
        const response = await fetch('/Admin/CreateOrder', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(orderData)
        });

        if (response.ok) {
            const result = await response.json();
            const orderId = result.orderId;

            // 3. فتح الواتساب بالرسالة المنسقة
            // ملاحظة: الرسالة باللغة العربية دائماً حسب طلبك المبدئي لتكون موحدة لصاحب المتجر
            let message = `✨ *طلب جديد #${orderId}* ✨%0a تكرما أرجو تجهيز الطلب الآتي 🛒%0a%0a🧾 *تفاصيل الطلب:*%0a`;

            for (let id in cart) {
                let item = cart[id];
                let itemTotal = item.price * item.qty;
                message += `🔹 ${item.name}%0a   الكمية: ${item.qty}%0a   السعر: ${item.price.toFixed(3)} ر.ع%0a   المجموع: ${itemTotal.toFixed(3)} ر.ع%0a%0a`;
            }

            message += `💰 *المجموع الإجمالي: ${total.toFixed(3)} ر.ع*%0a%0a`;
            message += `🙏 شكراً بقالة أمواج صلالة وموعدنا معكم في طلب قادم بإذن الله 💙`;

            const phone = "96896755118";
            window.open(`https://api.whatsapp.com/send?phone=${phone}&text=${message}`, '_blank');

            // تفريغ السلة
            cart = {};
            saveCart();
            openCartModal();
            showToast(isArabic ? "تم إرسال الطلب بنجاح!" : "Order sent successfully!");
        } else {
            console.error("Failed to save order");
            alert(isArabic ? "حدث خطأ أثناء حفظ الطلب." : "Error saving order.");
        }
    } catch (error) {
        console.error("Error:", error);
        alert(isArabic ? "تعذر الاتصال بالسيرفر." : "Connection error.");
    } finally {
        btn.innerHTML = originalText;
        btn.disabled = false;
    }
}

// =========================================
// 4. البحث الفوري (Live Search)
// =========================================
let searchTimeout;
function liveSearch(query) {
    clearTimeout(searchTimeout);
    const resultsDiv = document.getElementById('live-search-results');
    const defaultImg = '/images/logo-removebg-preview (3).png';
    const currency = isArabic ? 'ر.ع' : 'OMR';

    if (!query || query.length < 2) {
        resultsDiv.style.display = 'none';
        return;
    }

    // Debounce: تأخير الطلب 300ms لتقليل الضغط على السيرفر
    searchTimeout = setTimeout(() => {
        fetch(`/Home/LiveSearch?q=${encodeURIComponent(query)}`)
            .then(response => response.json())
            .then(data => {
                resultsDiv.innerHTML = '';
                if (data.length > 0) {
                    data.forEach(item => {
                        const div = document.createElement('div');
                        div.className = 'search-result-item';

                        // معالجة مسار الصورة
                        let imgUrl = item.image;
                        if (!imgUrl) imgUrl = defaultImg;
                        else if (!imgUrl.startsWith('http') && !imgUrl.startsWith('/')) {
                            if (imgUrl.startsWith('images/')) imgUrl = '/' + imgUrl;
                            else imgUrl = '/images/' + imgUrl;
                        }

                        // اختيار الاسم بناءً على اللغة الحالية (isArabic معرف في الـ Layout)
                        let displayName = isArabic ? item.name : item.nameEn;
                        if (!displayName || displayName.trim() === "") displayName = item.name; // Fallback

                        div.innerHTML = `
                            <img src="${imgUrl}" alt="${displayName}" onerror="this.src='${defaultImg}'">
                            <div class="info">
                                <div class="name">${displayName}</div>
                                <div class="price">${item.price.toFixed(3)} ${currency}</div>
                            </div>
                        `;
                        div.onclick = () => {
                            // الانتقال لصفحة البحث لعرض المنتج
                            window.location.href = `/Home/Products?q=${encodeURIComponent(item.name)}`;
                        };
                        resultsDiv.appendChild(div);
                    });
                    resultsDiv.style.display = 'block';
                } else {
                    const noResMsg = isArabic ? 'لا توجد نتائج' : 'No results found';
                    resultsDiv.innerHTML = `<div class="search-result-item" style="justify-content:center; color:#777;">${noResMsg}</div>`;
                    resultsDiv.style.display = 'block';
                }
            })
            .catch(err => console.error(err));
    }, 300);
}

// إخفاء نتائج البحث عند الضغط خارجها
document.addEventListener('click', function (e) {
    const resultsDiv = document.getElementById('live-search-results');
    const searchInput = document.getElementById('product-search');
    if (!searchInput.contains(e.target) && !resultsDiv.contains(e.target)) {
        resultsDiv.style.display = 'none';
    }
});

// =========================================
// 5. أدوات مساعدة (Utilities)
// =========================================
function showToast(msg) {
    const toast = document.getElementById('custom-toast');
    const msgEl = document.getElementById('toast-message');
    if (msgEl) msgEl.textContent = msg;
    toast.classList.add('show');
    setTimeout(() => toast.classList.remove('show'), 3000);
}

function scrollToTop() {
    window.scrollTo({ top: 0, behavior: 'smooth' });
}

// =========================================
// 6. حماية السكرين شوت (Screenshot Protection)
// =========================================
// منع زر PrintScreen
document.addEventListener('keyup', (e) => {
    if (e.key == 'PrintScreen') {
        navigator.clipboard.writeText(''); // مسح الحافظة
        alert(isArabic ? 'التقاط الشاشة غير مسموح به!' : 'Screenshots are not allowed!');
    }
});

// منع اختصارات لوحة المفاتيح الشائعة
document.addEventListener('keydown', function (e) {
    // Windows: Win+Shift+S, Ctrl+P
    // Mac: Cmd+Shift+3, Cmd+Shift+4
    if ((e.ctrlKey && e.key === 'p') ||
        (e.metaKey && e.shiftKey && (e.key === '3' || e.key === '4')) ||
        (e.key === 'Meta' && e.shiftKey && e.key === 's')) {

        e.preventDefault();
        const overlay = document.getElementById('screenshot-overlay');
        if (overlay) {
            overlay.style.display = 'flex';
            setTimeout(() => { overlay.style.display = 'none'; }, 2000);
        }
    }
});

// منع الكليك يمين (اختياري - يمكن إزالته إذا كان مزعجاً)
document.addEventListener('contextmenu', event => event.preventDefault());