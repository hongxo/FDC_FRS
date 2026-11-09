# FDC UI Authorization Report

Trang /identity/business.html là read-only shell Human cho DB profile. Tab là yêu cầu đọc, không mang quyền. JS xóa kết quả khi đổi tab/rời trang, xử lý 401/403/503, fetch no-store và render textContent. Chưa nối navigation theo effective policy và chưa có browser acceptance A.6.
