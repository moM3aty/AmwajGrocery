
let cart = {};


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

function updateCartUI() {
    const count = Object.values(cart).reduce((a, b) => a + b.qty, 0);
    const cartCountEl = document.querySelector('.cart-count');
    if (cartCountEl) cartCountEl.textContent = count;
}


function changeProductUnit(domId, unit, piecePrice, cartonPrice) {
    const priceDisplay = document.getElementById(`display-price-${domId}`);
    const currency = isArabic ? 'ر.ع' : 'OMR';

    if (unit === 'carton') {
        priceDisplay.textContent = `${parseFloat(cartonPrice).toFixed(3)} ${currency}`;
        const oldPrice = document.getElementById(`old-price-${domId}`);
        if (oldPrice) oldPrice.style.display = 'none';
    } else {
        priceDisplay.textContent = `${parseFloat(piecePrice).toFixed(3)} ${currency}`;
        const oldPrice = document.getElementById(`old-price-${domId}`);
        if (oldPrice) oldPrice.style.display = 'block';
    }
}

// دالة تحديث الكمية
function updateQuantity(domId, action) {
    const display = document.getElementById(`qty-${domId}`);
    if (!display) return;
    let val = parseInt(display.textContent);
    if (action === 'inc') val++;
    if (action === 'dec' && val > 1) val--;
    display.textContent = val;
}

function addToCartDynamic(domId, productId, name) {
    const qtyDisplay = document.getElementById(`qty-${domId}`);
    const qty = qtyDisplay ? parseInt(qtyDisplay.textContent || 1) : 1;

    const unitSelect = document.getElementById(`unit-select-${domId}`);
    const unit = unitSelect ? unitSelect.value : 'piece';

    const priceText = document.getElementById(`display-price-${domId}`).textContent;
    const price = parseFloat(priceText.replace(/[^\d.]/g, ''));


    const cartItemId = `${productId}-${unit}`;

    let unitLabel = "";
    if (unit === 'carton') unitLabel = isArabic ? "(كرتون)" : "(Carton)";
    const finalName = `${name} ${unitLabel}`;

    if (cart[cartItemId]) {
        cart[cartItemId].qty += qty;
    } else {
        cart[cartItemId] = { name: finalName, price: price, qty: qty, originalId: productId, unit: unit };
    }

    saveCart();
    const msg = isArabic ? `تم إضافة ${qty} من ${name} للسلة` : `Added ${qty} of ${name} to cart`;
    showToast(msg);

    if (qtyDisplay) qtyDisplay.textContent = 1;
    updateCartUI();
}
let currentProduct = null;
let currentModalQty = 1;

function openProductDetails(product) {
    currentProduct = product;
    currentModalQty = 1;
    const currency = isArabic ? 'ر.ع' : 'OMR';

    document.getElementById('modal-img').src = product.image;
    document.getElementById('modal-title').textContent = product.name;
    document.getElementById('modal-desc').innerHTML = product.desc;
    document.getElementById('modal-qty-val').textContent = 1;


    const unitWrapper = document.getElementById('modal-unit-wrapper');
    const priceEl = document.getElementById('modal-price');
    unitWrapper.innerHTML = '';


    window.updateModalPrice = function (unit) {
        if (unit === 'carton') {
            priceEl.textContent = `${parseFloat(product.priceCarton).toFixed(3)} ${currency}`;
        } else {
            priceEl.textContent = `${parseFloat(product.price).toFixed(3)} ${currency}`;
        }
    };

    if (product.priceCarton && parseFloat(product.priceCarton) > 0) {
        unitWrapper.innerHTML = `
            <select id="modal-unit-select" onchange="updateModalPrice(this.value)" 
                    style="width: 100%; padding: 10px; border-radius: 10px; border: 1px solid #ddd; font-weight:bold;">
                <option value="piece">${isArabic ? "بالقطعة" : "Per Piece"}</option>
                <option value="carton">${isArabic ? "بالكرتون" : "Carton"}</option>
            </select>
        `;
        updateModalPrice('piece');
    } else {
        unitWrapper.innerHTML = `<div style="color:#777; font-weight:bold;">${isArabic ? "بالقطعة" : "Per Piece"}</div>`;
        priceEl.textContent = `${parseFloat(product.price).toFixed(3)} ${currency}`;
    }

    document.getElementById('modal-qty-inc').onclick = () => { currentModalQty++; document.getElementById('modal-qty-val').textContent = currentModalQty; };
    document.getElementById('modal-qty-dec').onclick = () => { if (currentModalQty > 1) currentModalQty--; document.getElementById('modal-qty-val').textContent = currentModalQty; };

    document.getElementById('modal-add-btn').onclick = () => {
        const unitSelect = document.getElementById('modal-unit-select');
        const unit = unitSelect ? unitSelect.value : 'piece';
        const finalPrice = unit === 'carton' ? product.priceCarton : product.price;
        addToCartFromModal(product.id, product.name, parseFloat(finalPrice), currentModalQty, unit);
        closeProductModal(null);
    };

    document.getElementById('product-modal').style.display = 'flex';
}

function closeProductModal(e) {
    if (e === null || e.target.id === 'product-modal') {
        document.getElementById('product-modal').style.display = 'none';
    }
}

function addToCartFromModal(id, name, price, qty, unit) {
    const cartItemId = `${id}-${unit}`;
    let unitLabel = "";
    if (unit === 'carton') unitLabel = isArabic ? "(كرتون)" : "(Carton)";
    const finalName = `${name} ${unitLabel}`;

    if (cart[cartItemId]) {
        cart[cartItemId].qty += qty;
    } else {
        cart[cartItemId] = { name: finalName, price: price, qty: qty, originalId: id, unit: unit };
    }

    saveCart();
    const msg = isArabic ? `تم إضافة ${qty} من ${name} للسلة` : `Added ${qty} of ${name} to cart`;
    showToast(msg);
    updateCartUI();
}


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
                    <button onclick="removeFromCart('${id}')" style="background:none; border:none; color:red; cursor:pointer;"><i class="fas fa-trash"></i></button>
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
    if (e === null || e.target.id === 'cart-modal') {
        document.getElementById('cart-modal').style.display = 'none';
    }
}

function removeFromCart(id) {
    delete cart[id];
    saveCart();
    openCartModal();
}


async function sendOrderToWhatsApp() {
    if (Object.keys(cart).length === 0) {
        showToast(isArabic ? "السلة فارغة!" : "Cart is empty!");
        return;
    }

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

    const orderData = { totalAmount: total, items: orderItems };

    const btn = document.querySelector('.cart-footer .add-to-cart-btn');
    const originalText = btn.innerHTML;
    btn.innerHTML = '<i class="fas fa-spinner fa-spin"></i> ...';
    btn.disabled = true;

    try {
        const response = await fetch('/Admin/CreateOrder', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(orderData)
        });

        if (response.ok) {
            const result = await response.json();
            const orderId = result.orderId;

            let message = "";
            if (isArabic) {
                message = `✨ *طلب جديد #${orderId}* ✨\n تكرما أرجو تجهيز الطلب الآتي 🛒\n\n🧾 *تفاصيل الطلب:*\n`;
                for (let id in cart) {
                    let item = cart[id];
                    let itemTotal = item.price * item.qty;
                    message += `🔹 ${item.name}\n   الكمية: ${item.qty}\n   السعر: ${item.price.toFixed(3)} ${currency}\n   المجموع: ${itemTotal.toFixed(3)} ${currency}\n\n`;
                }
                message += `💰 *المجموع الإجمالي: ${total.toFixed(3)} ${currency}*\n\n`;
                message += `🙏 شكراً بقالة أمواج صلالة وموعدنا معكم في طلب قادم بإذن الله 💙`;
            } else {
                message = `✨ *New Order #${orderId}* ✨\n Please prepare the following order 🛒\n\n🧾 *Order Details:*\n`;
                for (let id in cart) {
                    let item = cart[id];
                    let itemTotal = item.price * item.qty;
                    message += `🔹 ${item.name}\n   Qty: ${item.qty}\n   Price: ${item.price.toFixed(3)} ${currency}\n   Total: ${itemTotal.toFixed(3)} ${currency}\n\n`;
                }
                message += `💰 *Grand Total: ${total.toFixed(3)} ${currency}*\n\n`;
                message += `🙏 Thank you Amwaj Salalah Grocery, see you in the next order! 💙`;
            }

            const phone = "96896755118";
            window.open(`https://api.whatsapp.com/send?phone=${phone}&text=${encodeURIComponent(message)}`, '_blank');

            cart = {};
            saveCart();
            openCartModal();
            showToast(isArabic ? "تم إرسال الطلب بنجاح!" : "Order sent successfully!");
        } else {
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

    searchTimeout = setTimeout(() => {
        fetch(`/Home/LiveSearch?q=${encodeURIComponent(query)}`)
            .then(response => response.json())
            .then(data => {
                resultsDiv.innerHTML = '';
                if (data.length > 0) {
                    data.forEach(item => {
                        const div = document.createElement('div');
                        div.className = 'search-result-item';

                        let imgUrl = item.image;
                        if (!imgUrl) imgUrl = defaultImg;
                        else if (!imgUrl.startsWith('http') && !imgUrl.startsWith('/')) {
                            if (imgUrl.startsWith('images/')) imgUrl = '/' + imgUrl;
                            else imgUrl = '/images/' + imgUrl;
                        }

                        let displayName = isArabic ? item.name : item.nameEn;
                        if (!displayName || displayName.trim() === "") displayName = item.name;

                        div.innerHTML = `
                            <img src="${imgUrl}" alt="${displayName}" onerror="this.src='${defaultImg}'">
                            <div class="info">
                                <div class="name">${displayName}</div>
                                <div class="price">${item.price.toFixed(3)} ${currency}</div>
                            </div>
                        `;
                        div.onclick = () => {
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

document.addEventListener('click', function (e) {
    const resultsDiv = document.getElementById('live-search-results');
    const searchInput = document.getElementById('product-search');
    if (!searchInput.contains(e.target) && !resultsDiv.contains(e.target)) {
        resultsDiv.style.display = 'none';
    }
});

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

let deferredPrompt;
const installBtn = document.getElementById('pwa-install-btn');

window.addEventListener('beforeinstallprompt', (e) => {
    e.preventDefault();
    deferredPrompt = e;
    if (installBtn) installBtn.style.display = 'inline-flex';
});

if (installBtn) {
    installBtn.addEventListener('click', async () => {
        if (deferredPrompt) {
            deferredPrompt.prompt();
            const { outcome } = await deferredPrompt.userChoice;
            if (outcome === 'accepted') { console.log('User accepted the install prompt'); }
            deferredPrompt = null;
            installBtn.style.display = 'none';
        }
    });
}

window.addEventListener('appinstalled', () => {
    if (installBtn) installBtn.style.display = 'none';
});



document.addEventListener('keyup', (e) => {
    if (e.key == 'PrintScreen') {
        navigator.clipboard.writeText('');
        showScreenshotOverlay();
    }
});

document.addEventListener('keydown', function (e) {
    if ((e.ctrlKey && e.key === 'p') ||
        (e.metaKey && e.shiftKey && (e.key === '3' || e.key === '4')) ||
        (e.key === 'Meta' && e.shiftKey && e.key === 's')) {
        e.preventDefault();
        showScreenshotOverlay();
    }
});

document.addEventListener('contextmenu', event => event.preventDefault());


window.addEventListener('blur', () => {
    document.body.style.filter = 'blur(20px)';
    const overlay = document.getElementById('screenshot-overlay');
    if (overlay) overlay.style.display = 'flex';
});

window.addEventListener('focus', () => {
    setTimeout(() => {
        document.body.style.filter = 'none';
        const overlay = document.getElementById('screenshot-overlay');
        if (overlay) overlay.style.display = 'none';
    }, 200);
});

function showScreenshotOverlay() {
    const overlay = document.getElementById('screenshot-overlay');
    if (overlay) {
        overlay.style.display = 'flex';
        setTimeout(() => { overlay.style.display = 'none'; }, 2000);
    }
    if (isArabic) alert('عذراً، يمنع التقاط صور للشاشة!');
}