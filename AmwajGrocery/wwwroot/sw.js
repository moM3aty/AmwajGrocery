const CACHE_NAME = 'amwaj-cache-v1';
const urlsToCache = [
  '/',
  '/css/style.css',
  '/js/main.js',
  '/images/logo-removebg-preview (3).png',
  '/manifest.json'
];

self.addEventListener('install', event => {
  event.waitUntil(
    caches.open(CACHE_NAME)
      .then(cache => cache.addAll(urlsToCache))
  );
});

self.addEventListener('fetch', event => {
  event.respondWith(
    caches.match(event.request)
      .then(response => {
        if (response) {
          return response;
        }
        return fetch(event.request);
      })
  );
});