// ============================================================
// Service Worker — EngineeCode
// - يخزن الملفات الثابتة (css/js/صور) عشان يفتح التطبيق بسرعة
//   ويشتغل حتى لو الاتصال ضعيف.
// - أي صفحة أو استدعاء API بيروح دايمًا على الشبكة الأول
//   عشان البيانات (منتجات، طلبات، إلخ) تفضل محدثة.
// ============================================================

const CACHE_NAME = "enginee-code-cache-v1";

const PRECACHE_URLS = [
    "/",
    "/css/site.css",
    "/js/site.js",
    "/images/logo.png",
    "/images/placeholder.png",
    "/images/icons/icon-192.png",
    "/images/icons/icon-512.png",
    "/manifest.json"
];

self.addEventListener("install", (event) => {
    event.waitUntil(
        caches.open(CACHE_NAME).then((cache) => cache.addAll(PRECACHE_URLS).catch(() => { }))
    );
    self.skipWaiting();
});

self.addEventListener("activate", (event) => {
    event.waitUntil(
        caches.keys().then((keys) =>
            Promise.all(keys.filter((k) => k !== CACHE_NAME).map((k) => caches.delete(k)))
        )
    );
    self.clients.claim();
});

self.addEventListener("fetch", (event) => {
    const req = event.request;

    // اطلب من الشبكة بس (مش هيتخزن) لأي حاجة GET من الـ API أو أي POST/PUT/DELETE
    if (req.method !== "GET" || req.url.includes("/api/")) {
        return; // سيب الطلب يمشي عادي من غير تدخل الـ Service Worker
    }

    // للصفحات (navigation): جرب الشبكة الأول، ولو فشلت استخدم النسخة المخزنة (offline fallback)
    if (req.mode === "navigate") {
        event.respondWith(
            fetch(req).catch(() => caches.match(req).then((res) => res || caches.match("/")))
        );
        return;
    }

    // للملفات الثابتة (css/js/صور): استخدم النسخة المخزنة فورًا، وحدّثها في الخلفية
    event.respondWith(
        caches.match(req).then((cached) => {
            const network = fetch(req)
                .then((res) => {
                    if (res && res.status === 200) {
                        caches.open(CACHE_NAME).then((cache) => cache.put(req, res.clone()));
                    }
                    return res;
                })
                .catch(() => cached);
            return cached || network;
        })
    );
});
