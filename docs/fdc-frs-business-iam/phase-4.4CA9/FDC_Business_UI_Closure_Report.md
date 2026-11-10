# FDC Business UI Closure Report

Business navigation do SQL authority quyết định theo từng operation và grant. Mỗi endpoint vẫn kiểm tra độc lập. Bản paged trả total và items từ cùng transaction sau SQL scope, thứ tự Id ổn định trên fixture tĩnh, giữ response array cũ cho caller hiện hữu. UI có Đầu/Trước/Sau/Cuối. Dashboard có bucket UTC bảy ngày và 20 sự kiện vận hành, scoped trước aggregate/limit. Quan hệ Trạm–IED–Collector–COMTRADE kiểm tra cả permission cha và con tại cùng snapshot, có giới hạn 50 quan hệ mỗi bước.

UI xóa DOM và hủy fetch khi rời trang, kiểm tra navigation khi quay lại và mỗi 15 giây lúc hiển thị. Có xử lý 401/503, nhưng bằng chứng hết hạn IdleUntil/AbsoluteUntil khi dữ liệu đang hiển thị còn thiếu. Navigation ngoài màn hình business cần đối chiếu riêng.
