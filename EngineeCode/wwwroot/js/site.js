/* ============================================================
   EngineeCode — site.js v6
   API: https://enginecodeapi.runasp.net
   🔧 Fix: unified theme key to 'ec_theme'
============================================================ */

const API_BASE_URL = 'https://enginecodeapi.runasp.net';
const CART_KEY = 'ec_cart';

// ============================================================
//  API HELPERS
// ============================================================
async function apiGet(endpoint) {
    const res = await fetch(`${API_BASE_URL}${endpoint}`, {
        headers: { 'Accept': 'application/json' }
    });
    if (!res.ok) throw new Error(`HTTP ${res.status}`);
    const json = await res.json();
    const inner = json.data ?? json;
    return inner.items ?? inner;
}

async function apiPost(endpoint, body) {
    const res = await fetch(`${API_BASE_URL}${endpoint}`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json', 'Accept': 'application/json' },
        body: JSON.stringify(body)
    });
    const json = await res.json();
    return { ok: res.ok, data: json.data, message: json.message };
}

// ============================================================
//  THEME — مفتاح موحد 'ec_theme'
// ============================================================
(function () {
    const saved = localStorage.getItem('ec_theme') || 'dark';
    document.documentElement.setAttribute('data-theme', saved);
})();

function toggleTheme() {
    const html = document.documentElement;
    const next = html.getAttribute('data-theme') === 'dark' ? 'light' : 'dark';
    html.setAttribute('data-theme', next);
    localStorage.setItem('ec_theme', next);
}

// ============================================================
//  MOBILE NAV
// ============================================================
function toggleMobile() {
    document.getElementById('mobileNav')?.classList.toggle('open');
}

// ============================================================
//  TOAST
// ============================================================
function showToast(text, icon = '✅') {
    const t = document.getElementById('toast');
    if (!t) return;
    document.getElementById('toastText').textContent = text;
    document.getElementById('toastIcon').textContent = icon;
    t.classList.add('show');
    setTimeout(() => t.classList.remove('show'), 3500);
}

// ============================================================
//  COUNTER + REVEAL
// ============================================================
function animateCounters() {
    document.querySelectorAll('[data-count]').forEach(el => {
        const target = parseInt(el.dataset.count);
        let current = 0;
        const step = Math.ceil(target / 50);
        const timer = setInterval(() => {
            current = Math.min(current + step, target);
            el.textContent = (current === target && target >= 100) ? '+' + current : current;
            if (current >= target) clearInterval(timer);
        }, 30);
    });
}

function setupReveal() {
    const obs = new IntersectionObserver(entries => {
        entries.forEach(e => { if (e.isIntersecting) e.target.classList.add('visible'); });
    }, { threshold: 0.1 });
    document.querySelectorAll('.reveal:not(.visible)').forEach(el => obs.observe(el));
}

// ============================================================
//  AUTH
// ============================================================
function getCustomer() {
    try { return JSON.parse(localStorage.getItem('ec-customer')); } catch { return null; }
}

function logout() {
    fetch('/api/proxy/logout', { method: 'POST' })
        .then(() => {
            localStorage.removeItem('ec-customer');
            window.location.href = '/';
        });
}

function requireLogin(returnUrl) {
    window.location.href = `/Login?returnUrl=${encodeURIComponent(returnUrl || window.location.pathname)}`;
}

function updateNavAuth() {
    updateCartCount();
}

function toggleCustomerMenu() {
    document.getElementById('customerDropdown')?.classList.toggle('open');
}

// ============================================================
//  CART
// ============================================================
function getCart() {
    try { return JSON.parse(localStorage.getItem(CART_KEY)) || []; } catch { return []; }
}

function saveCart(cart) {
    localStorage.setItem(CART_KEY, JSON.stringify(cart));
    updateCartCount();
}

function updateCartCount() {
    const cart = getCart();
    const total = cart.reduce((s, i) => s + i.qty, 0);
    const badge = document.getElementById('cartCount');
    const mobile = document.getElementById('cartCountMobile');
    if (badge) {
        badge.textContent = total;
        badge.style.display = total > 0 ? 'flex' : 'none';
    }
    if (mobile) mobile.textContent = total;
}

function addToCart(product) {
    const cart = getCart();
    const existing = cart.find(i => i.id === product.id);

    // خد الصورة الصح من images
    const realImage = (product.images && product.images.length > 0)
        ? (product.images.find(i => i.isMain)?.imagePath || product.images[0].imagePath)
        : product.imagePath;

    if (existing) {
        if (existing.qty >= product.stock) {
            showToast('وصلت للحد الأقصى المتاح', '⚠️'); return;
        }
        existing.qty++;
    } else {
        if (product.stock === 0) { showToast('المنتج غير متوفر حالياً', '❌'); return; }
        cart.push({
            id: product.id,
            name: product.name,
            price: product.price,
            image: realImage,        // ← الصورة الصح
            imagePath: realImage,    // ← الصورة الصح
            subName: product.subName || '',
            stock: product.stock || 99,
            qty: 1
        });
    }
    saveCart(cart);
    showToast(`تمت الإضافة: ${product.name}`, '🛒');
}
function removeFromCart(productId) {
    saveCart(getCart().filter(i => i.id !== productId));
}

function changeQty(productId, delta) {
    const cart = getCart();
    const item = cart.find(i => i.id === productId);
    if (!item) return;
    item.qty = Math.max(1, Math.min(item.stock, item.qty + delta));
    saveCart(cart);
    loadCartPage();
}

// ============================================================
//  RENDER — Stars
// ============================================================
function starsHTML(rating) {
    const full = Math.floor(rating);
    const half = rating % 1 >= 0.5;
    let s = '<div class="stars"><div class="stars-icons">';
    for (let i = 1; i <= 5; i++) {
        if (i <= full) s += '<span class="star filled">★</span>';
        else if (i === full + 1 && half) s += '<span class="star half">★</span>';
        else s += '<span class="star">★</span>';
    }
    return s + '</div></div>';
}

// ============================================================
//  RENDER — Product Card
// ============================================================
function productCardHTML(p) {
    // خد الصورة من images لو موجودة، وإلا استخدم imagePath
    const imgSrc = (p.images && p.images.length > 0)
        ? p.images.find(i => i.isMain)?.imagePath || p.images[0].imagePath
        : p.imagePath;

    const disc = p.discountPercent ? `<div class="badge-discount">خصم ${p.discountPercent}%</div>` : '';
    const oldP = p.oldPrice ? `<div class="price-old">${p.oldPrice} جنيه</div>` : '';
    const stock = p.stock <= 5 && p.stock > 0
        ? `<div style="font-size:11px;color:var(--red);margin-bottom:8px;">⚠️ آخر ${p.stock} قطع فقط!</div>` : '';
    return `
    <div class="pcard" data-cat="${p.category}" onclick="goToProduct(${p.id})" style="cursor:pointer;">
        <div class="pcard-img">
           <img src="${getImageSrc(imgSrc)}" alt="${p.name}" loading="lazy" onerror="this.src='/images/placeholder.png'">
            <div class="badge">${p.badge}</div>${disc}
        </div>
        <div class="pcard-body">
            <div class="pcard-name">${p.name}</div>
            <div class="pcard-sub">${p.subName}</div>
            ${starsHTML(p.rating)}
            <div style="font-size:11px;color:var(--text-muted);margin-bottom:8px;">
                🔥 ${p.salesCount} وحدة مباعة &nbsp;|&nbsp; (${p.reviewsCount} تقييم)
            </div>
            ${stock}
            <div class="pcard-footer">
                <div class="price-wrap">
                    ${oldP}
                    <div class="price-new">${p.price} <small>جنيه</small></div>
                </div>
                <button class="buy-btn" onclick="event.stopPropagation(); addToCart(${JSON.stringify(p).replace(/"/g, '&quot;')})">
                    🛒 أضف للعربة
                </button>
            </div>
        </div>
    </div>`;
}

function goToProduct(id) {
    window.location.href = `/ProductDetail?id=${id}`;
}

// ============================================================
//  RENDER — Service Card
// ============================================================
function serviceCardHTML(s) {
    return `
    <div class="srv-card">
        <div class="srv-icon">${s.icon}</div>
        <div class="srv-title">${s.title}</div>
        <div class="srv-desc">${s.description}</div>
        <div class="srv-price">💰 ${s.priceLabel}</div>
    </div>`;
}

// ============================================================
//  UI HELPERS
// ============================================================
function setLoading(id, msg = '⏳ جاري التحميل...') {
    const el = document.getElementById(id);
    if (el) el.innerHTML = `<div style="grid-column:1/-1;text-align:center;padding:60px;color:var(--text-muted);">${msg}</div>`;
}
function setError(id, msg) {
    const el = document.getElementById(id);
    if (el) el.innerHTML = `<div style="grid-column:1/-1;text-align:center;padding:60px;color:var(--text-muted);">${msg}</div>`;
}

// ============================================================
//  HOME PAGE
// ============================================================
async function loadHomePage() {
    const featuredGrid = document.getElementById('featuredGrid');
    if (featuredGrid) {
        setLoading('featuredGrid');
        try {
            const products = await apiGet('/api/products?featured=true&limit=4');
            featuredGrid.innerHTML = products.length
                ? products.map(productCardHTML).join('')
                : '<p style="grid-column:1/-1;text-align:center;color:var(--text-muted)">لا توجد منتجات مميزة</p>';
            setupReveal();
        } catch (e) { setError('featuredGrid', '😕 تعذر تحميل المنتجات'); }
    }
    const srvHome = document.getElementById('srvHome');
    if (srvHome) {
        try {
            const services = await apiGet('/api/services?limit=4');
            srvHome.innerHTML = services.map(serviceCardHTML).join('');
        } catch (e) { console.error(e); }
    }
}

// ============================================================
//  PRODUCTS PAGE
// ============================================================
let allProducts = [];
let activeFilter = 'all';
let searchTerm = '';
let sortMode = 'default';
let minPrice = 0;
let maxPrice = 999999;

async function loadProductsPage() {
    const grid = document.getElementById('allProductsGrid');
    if (!grid) return;
    setLoading('allProductsGrid', '⏳ جاري تحميل المنتجات...');
    try {
        allProducts = await apiGet('/api/products');
        renderFiltered();
    } catch (e) { setError('allProductsGrid', '😕 تعذر تحميل المنتجات'); }
}

function renderFiltered() {
    let list = [...allProducts];
    if (activeFilter !== 'all') list = list.filter(p => p.category === activeFilter);
    if (searchTerm) list = list.filter(p =>
        p.name.toLowerCase().includes(searchTerm.toLowerCase()) ||
        p.subName.toLowerCase().includes(searchTerm.toLowerCase()));
    list = list.filter(p => p.price >= minPrice && p.price <= maxPrice);
    if (sortMode === 'price-asc') list.sort((a, b) => a.price - b.price);
    else if (sortMode === 'price-desc') list.sort((a, b) => b.price - a.price);
    else if (sortMode === 'rating') list.sort((a, b) => b.rating - a.rating);
    else if (sortMode === 'sales') list.sort((a, b) => b.salesCount - a.salesCount);
    const grid = document.getElementById('allProductsGrid');
    if (!grid) return;
    grid.innerHTML = list.length
        ? list.map(productCardHTML).join('')
        : '<div style="text-align:center;color:var(--text-muted);padding:60px;grid-column:1/-1;">لا توجد نتائج 😕</div>';
    setupReveal();
}

function initProductFilters() {
    document.querySelectorAll('#filterBar .flt').forEach(btn => {
        btn.addEventListener('click', () => {
            document.querySelectorAll('#filterBar .flt').forEach(b => b.classList.remove('active'));
            btn.classList.add('active');
            activeFilter = btn.dataset.cat;
            renderFiltered();
        });
    });
    document.getElementById('searchInput')?.addEventListener('input', e => { searchTerm = e.target.value; renderFiltered(); });
    document.getElementById('sortSel')?.addEventListener('change', e => { sortMode = e.target.value; renderFiltered(); });

    // Price range
    document.getElementById('minPrice')?.addEventListener('input', e => {
        minPrice = parseInt(e.target.value);
        document.getElementById('minValDisplay').textContent = minPrice.toLocaleString();
        renderFiltered();
    });
    document.getElementById('maxPrice')?.addEventListener('input', e => {
        maxPrice = parseInt(e.target.value);
        document.getElementById('maxValDisplay').textContent = maxPrice.toLocaleString();
        renderFiltered();
    });
}

// ============================================================
//  PRODUCT DETAIL PAGE
// ============================================================
let detailImages = [];
let detailImageIndex = 0;
let currentDetailProduct = null;

async function loadProductDetailPage() {
    const wrap = document.getElementById('productDetailWrap');
    if (!wrap) return;
    const id = new URLSearchParams(window.location.search).get('id');
    if (!id) { wrap.innerHTML = '<div style="text-align:center;padding:80px;">❌ لم يتم تحديد المنتج</div>'; return; }
    try {
        const res = await fetch(`${API_BASE_URL}/api/products/${id}`, { headers: { 'Accept': 'application/json' } });
        const json = await res.json();
        const p = json.data ?? json;
        currentDetailProduct = p;
        document.title = `${p.name} — EngineeCode`;
        detailImages = (p.images && p.images.length > 0) ? p.images.map(i => i.imagePath) : [p.imagePath];
        detailImageIndex = 0;
        renderProductDetail(p);
    } catch (e) {
        wrap.innerHTML = '<div style="text-align:center;padding:80px;color:var(--text-muted);">😕 تعذر تحميل المنتج</div>';
    }
}

function renderProductDetail(p) {
    const wrap = document.getElementById('productDetailWrap');
    if (!wrap) return;
    const disc = p.discountPercent ? `<span class="badge-discount">خصم ${p.discountPercent}%</span>` : '';
    const oldP = p.oldPrice ? `<div class="price-old" style="font-size:18px;">${p.oldPrice} جنيه</div>` : '';
    const stockHTML = p.stock === 0
        ? `<div class="stock-badge out">❌ غير متوفر</div>`
        : p.stock <= 5
            ? `<div class="stock-badge low">⚠️ آخر ${p.stock} قطع فقط!</div>`
            : `<div class="stock-badge in">✅ متوفر (${p.stock} قطعة)</div>`;
    const arrows = detailImages.length > 1 ? `
        <button class="img-arrow left"  onclick="prevDetailImage()">&#8249;</button>
        <button class="img-arrow right" onclick="nextDetailImage()">&#8250;</button>` : '';
    const thumbs = detailImages.length > 1 ? `
    <div style="width:100%; overflow-x:auto; overflow-y:hidden; display:flex; flex-wrap:nowrap; gap:8px; margin-top:12px; padding-bottom:8px;">
        ${detailImages.map((img, i) => `
            <img src="${getImageSrc(img)}"
                 style="width:56px; height:56px; min-width:56px; flex-shrink:0; object-fit:contain; border-radius:10px; padding:4px; cursor:pointer; border:2px solid ${i === 0 ? 'var(--primary)' : 'transparent'}; background:var(--bg2);"
                 onclick="switchDetailImage(${i})"
                 onerror="this.src='/images/placeholder.png'">`
    ).join('')}
    </div>` : '';
    wrap.innerHTML = `
<div class="detail-grid">
    <div class="detail-img-wrap" style="overflow:hidden;">
        ${arrows}
        <img id="detailMainImg" src="${getImageSrc(detailImages[0])}" alt="${p.name}" onerror="this.src='/images/placeholder.png'">
        ${thumbs}
    </div>
        <div class="detail-info">
            <div class="modal-badge-row">
                <span class="badge">${p.badge}</span>${disc}
            </div>
            <h1 class="detail-name">${p.name}</h1>
            <div class="detail-sub">${p.subName}</div>
            <div class="modal-rating-row">
                ${starsHTML(p.rating)}
                <span style="color:var(--text-muted);font-size:14px;">${p.rating}/5 | ${p.reviewsCount} تقييم</span>
            </div>
            <div style="font-size:14px;color:var(--text-muted);margin-bottom:16px;">🔥 ${p.salesCount} وحدة مباعة</div>
            ${stockHTML}
            <div class="detail-price-wrap">
                ${oldP}
                <div class="price-new" style="font-size:32px;">${p.price} <small>جنيه</small></div>
            </div>
            <button class="buy-btn" style="width:100%;padding:16px;font-size:17px;margin-top:8px;" ${p.stock === 0 ? 'disabled' : ''}
                onclick="addToCart(currentDetailProduct); setTimeout(()=>window.location.href='/Cart', 800)">
                🛒 أضف للعربة
            </button>
            <div class="detail-meta">
                <div>📦 الفئة: <strong>${p.category}</strong></div>
                <div>🏷️ كود المنتج: <strong>#${p.id}</strong></div>
            </div>
        </div>
    </div>`;
}

function switchDetailImage(index) {
    detailImageIndex = index;
    document.getElementById('detailMainImg').src = getImageSrc(detailImages[index]);
    document.querySelectorAll('.detail-img-wrap img:not(#detailMainImg)').forEach((t, i) => {
        t.style.border = i === index ? '2px solid var(--primary)' : '2px solid transparent';
    });
}
function nextDetailImage() { switchDetailImage((detailImageIndex + 1) % detailImages.length); }
function prevDetailImage() { switchDetailImage((detailImageIndex - 1 + detailImages.length) % detailImages.length); }

// ============================================================
//  CART PAGE
// ============================================================
function loadCartPage() {
    const layout = document.getElementById('cartLayout');
    if (!layout) return;

    const cart = getCart();
    if (cart.length === 0) {
        layout.innerHTML = `
            <div style="text-align:center;padding:80px;color:var(--text-muted);">
                <div style="font-size:60px;margin-bottom:16px;">🛒</div>
                <div style="font-size:20px;margin-bottom:24px;">العربة فارغة</div>
                <a href="/Products" class="buy-btn" style="display:inline-block;padding:12px 32px;text-decoration:none;">
                    تصفح المنتجات
                </a>
            </div>`;
        return;
    }

    const subtotal = cart.reduce((s, i) => s + i.price * i.qty, 0);

    layout.innerHTML = `
    <div class="cart-grid">
        <div class="cart-items">
            ${cart.map(item => `
            <div class="cart-item">
               <img src="${getImageSrc(item.image || item.imagePath)}"  alt="${item.name}" onerror="this.src='/images/placeholder.png'">
                <div class="cart-item-info">
                    <div class="cart-item-name">${item.name}</div>
                    <div class="cart-item-price">${item.price} جنيه</div>
                </div>
                <div class="cart-item-qty">
                    <button onclick="changeQty(${item.id}, -1)">−</button>
                    <span>${item.qty}</span>
                    <button onclick="changeQty(${item.id}, +1)">+</button>
                </div>
                <div class="cart-item-total">${(item.price * item.qty).toFixed(2)} جنيه</div>
                <button class="cart-remove" onclick="removeFromCart(${item.id}); loadCartPage();">✕</button>
            </div>`).join('')}
        </div>
        <div class="cart-summary">
            <h3 style="margin:0 0 20px;font-size:18px;">ملخص الطلب</h3>
            <div class="summary-row"><span>المجموع الفرعي</span><span>${subtotal.toFixed(2)} جنيه</span></div>
            <div class="summary-row"><span>رسوم التوصيل</span><span style="color:var(--green);">يُحدد لاحقاً</span></div>
            <div class="summary-row total"><span>الإجمالي</span><span>${subtotal.toFixed(2)} جنيه</span></div>
            <button class="buy-btn" style="width:100%;padding:14px;font-size:16px;margin-top:16px;"
                onclick="goToCheckout()">
                تأكيد الطلب ✅
            </button>
            <a href="/Products" style="display:block;text-align:center;margin-top:12px;color:var(--text-muted);font-size:14px;">
                ← متابعة التسوق
            </a>
        </div>
    </div>`;
}

function goToCheckout() {
    if (getCart().length === 0) { showToast('العربة فارغة!', '⚠️'); return; }
    window.location.href = '/Checkout';
}

// ============================================================
//  SERVICES PAGE
// ============================================================
async function loadServicesPage() {
    const grid = document.getElementById('srvFull');
    if (!grid) return;
    try {
        const services = await apiGet('/api/services');
        grid.innerHTML = services.map(serviceCardHTML).join('');
        setupReveal();
    } catch (e) { setError('srvFull', '😕 تعذر تحميل الخدمات'); }
}

// ============================================================
//  CONTACT FORM
// ============================================================
function initContactForm() {
    const form = document.getElementById('contactForm');
    if (!form) return;
    form.addEventListener('submit', async function (e) {
        e.preventDefault();
        const btn = form.querySelector('button[type="submit"]');
        btn.disabled = true; btn.textContent = '⏳ جاري الإرسال...';
        try {
            const result = await apiPost('/api/contact', {
                name: form.querySelector('[name="Name"]')?.value || '',
                phone: form.querySelector('[name="Phone"]')?.value || '',
                subject: form.querySelector('[name="Subject"]')?.value || 'other',
                message: form.querySelector('[name="Message"]')?.value || ''
            });
            if (result.ok) { showToast('تم إرسال رسالتك! 🚀', '✅'); form.reset(); }
            else { showToast(result.message || 'حدث خطأ', '❌'); }
        } catch { showToast('تعذر الاتصال بالسيرفر 😕', '❌'); }
        finally { btn.disabled = false; btn.textContent = 'إرسال الرسالة 🚀'; }
    });
}

// ============================================================
//  INIT
// ============================================================
document.addEventListener('DOMContentLoaded', async () => {
    setupReveal();
    animateCounters();
    updateCartCount();

    document.addEventListener('click', e => {
        if (!e.target.closest('.customer-menu')) {
            document.getElementById('customerDropdown')?.classList.remove('open');
        }
    });

    const path = window.location.pathname.toLowerCase();
    const isHome = path === '/' || path === '/index' || path.endsWith('/');
    const isProducts = path.includes('products') && !path.includes('productdetail');
    const isDetail = path.includes('productdetail');
    const isCart = path.includes('cart');
    const isServices = path.includes('service');
    const isContact = path.includes('contact');

    if (isHome) await loadHomePage();
    if (isProducts) { initProductFilters(); await loadProductsPage(); }
    if (isDetail) await loadProductDetailPage();
    if (isCart) loadCartPage();
    if (isServices) await loadServicesPage();
    if (isContact) initContactForm();
});
function getImageSrc(path) {
    if (!path) return '/images/placeholder.png';
    if (path.startsWith('http://') || path.startsWith('https://')) return path;
    if (path.startsWith('/')) return path;
    return `/images/${path}`;
}
