(() => {
    const header = document.querySelector('header > div:last-child');
    const events = document.createElement('button'); events.textContent = 'Sự kiện SOE'; events.onclick = () => window.open('/events.html', '_blank'); header.append(events);
    const observer = new MutationObserver(() => {
        document.querySelectorAll('#records button[data-info]').forEach(button => {
            if (button.parentElement.querySelector('[data-wave]')) return;
            const wave = document.createElement('button'); wave.textContent = 'Dạng sóng'; wave.dataset.wave = button.dataset.info;
            wave.onclick = () => window.open('/waveform.html?id=' + encodeURIComponent(button.dataset.info), '_blank'); button.parentElement.append(wave);
        });
    });
    observer.observe(document.getElementById('records'), { childList: true, subtree: true });
})();
