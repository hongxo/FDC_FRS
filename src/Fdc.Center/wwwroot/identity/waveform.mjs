export function appendWaveform(container, recordId, read, signal, invalidate) {
  const section = document.createElement('section');
  section.setAttribute('aria-label', 'Dạng sóng COMTRADE');
  const button = document.createElement('button');
  button.type = 'button';
  button.textContent = 'Xem dạng sóng COMTRADE';
  const feedback = document.createElement('p');
  feedback.setAttribute('role', 'status');
  const content = document.createElement('div');
  section.append(button, feedback, content);
  container.append(section);
  button.addEventListener('click', async () => {
    button.disabled = true;
    content.replaceChildren();
    feedback.textContent = 'Đang đọc CFG/DAT và kiểm tra checksum…';
    try {
      const data = await read('records/' + encodeURIComponent(recordId) + '/waveform', signal);
      if (signal.aborted) return;
      if (!Array.isArray(data.points) || !data.points.length || !Array.isArray(data.analog) || !Array.isArray(data.digital))
        throw new Error('Invalid waveform response');
      feedback.textContent = data.samples + ' mẫu · ' + data.format + ' · ' +
        (data.previewDecimated ? 'Bản xem đã giảm mẫu, có thể không thể hiện đỉnh ngắn.' : 'Hiển thị toàn bộ mẫu.');
      const metadata = document.createElement('p');
      metadata.textContent = 'Bắt đầu: ' + data.startTimeRaw + ' · Trigger: ' + data.triggerTimeRaw +
        ' · Thời gian theo CFG, múi giờ chưa được suy diễn.';
      const label = document.createElement('label');
      label.textContent = 'Kênh hiển thị ';
      const select = document.createElement('select');
      select.setAttribute('aria-label', 'Kênh dạng sóng');
      for (const [type, channels] of [['analog', data.analog], ['digital', data.digital]]) {
        channels.forEach((channel, index) => {
          const option = document.createElement('option');
          option.value = type + ':' + index;
          option.textContent = type === 'analog' ? channel.name + ' (' + channel.unit + ')' : channel + ' (digital)';
          select.append(option);
        });
      }
      label.append(select);
      const chart = document.createElementNS('http://www.w3.org/2000/svg', 'svg');
      chart.setAttribute('viewBox', '0 0 800 240');
      chart.setAttribute('role', 'img');
      chart.setAttribute('aria-label', 'Dạng sóng theo thời gian mili giây');
      chart.style.width = '100%';
      chart.style.maxWidth = '900px';
      const values = document.createElement('p');
      values.setAttribute('data-waveform-summary', '');
      function draw() {
        const [type, index] = select.value.split(':');
        const points = data.points.map(point => [point.timeMs, point[type][Number(index)]]);
        if (points.some(point => point.some(value => !Number.isFinite(value)))) throw new Error('Invalid waveform points');
        const min = Math.min(...points.map(point => point[1]));
        const max = Math.max(...points.map(point => point[1]));
        const start = points[0][0], end = points[points.length - 1][0];
        const line = document.createElementNS('http://www.w3.org/2000/svg', 'polyline');
        line.setAttribute('points', points.map(([time, value]) =>
          (20 + (time - start) * 760 / (end - start || 1)) + ',' + (220 - (value - min) * 200 / (max - min || 1))).join(' '));
        line.setAttribute('fill', 'none');
        line.setAttribute('stroke', '#1565c0');
        line.setAttribute('stroke-width', '2');
        chart.replaceChildren(line);
        values.textContent = 'Thời gian ' + start + '–' + end + ' ms · Min ' + min + ' · Max ' + max;
      }
      select.addEventListener('change', draw);
      content.append(metadata, label, chart, values);
      draw();
    } catch (error) {
      if (signal.aborted) return;
      if (error.status === 401 || error.status === 403 || error.status === 503) {
        invalidate(error);
        return;
      }
      content.replaceChildren();
      feedback.textContent = ({ 401: 'Phiên đã hết hạn.', 403: 'Không có quyền xem dạng sóng.',
        404: 'Không có dữ liệu dạng sóng trong phạm vi được phép.',
        422: 'Cặp CFG/DAT chưa hợp lệ, vượt giới hạn xem hoặc có định dạng chưa hỗ trợ.',
        503: 'Chưa thể xác minh dữ liệu hoặc quyền truy cập.' })[error.status] ?? 'Chưa đọc được dạng sóng.';
    } finally {
      button.disabled = false;
    }
  });
}
