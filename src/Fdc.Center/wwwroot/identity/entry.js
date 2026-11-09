'use strict';
const screens={login:['Đăng nhập','Đăng nhập chưa được mở. Vui lòng liên hệ quản trị để xác nhận chính sách truy cập.'],mfa:['Xác minh MFA','MFA chưa được mở theo chính sách hiện tại. Không có mã xác minh minh họa.'],recovery:['Khôi phục mật khẩu','Khôi phục tài khoản chưa được mở. Vui lòng liên hệ bộ phận quản trị theo quy trình.'],profile:['Hồ sơ và bảo mật','Hồ sơ và các thao tác bảo mật chưa được mở trong bản tích hợp này.']};
function render(){const screen=screens[location.hash.slice(1)]||screens.login;document.getElementById('screen-title').textContent=screen[0];document.getElementById('policy-status').textContent=screen[1];}
addEventListener('hashchange',render);render();
