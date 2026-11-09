# Runtime architecture

Human loopback HTTPS dùng Fdc.Human và SQL session reader mỗi request, no-store, không tin proxy headers. Machine intake giữ listener/mTLS và station binding riêng. Static landing chỉ có trạng thái. Management shell yêu cầu session và menu dùng read authority. Mọi read tiếp tục kiểm tra scope riêng trên server. Không dùng trạng thái công khai làm feature flag cấp quyền.

Dashboard/Stations/IED/COMTRADE/Collector/Lab vẫn hiển thị chờ station mapping trên Human lane. Chưa tích hợp dữ liệu vận hành hoặc mở tuyến legacy từ menu.
