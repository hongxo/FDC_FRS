# FDC Controlled Activation Runbook

PLAN_ONLY. Coordinator thu thập technical acceptance, P0 disposition, independent review có chữ ký, D01/D04/D07/D08/D09/D10 và E dependencies được duyệt. OT owner xác nhận exact Station/IED inventory, operations allowlist, permit/window, stop owner và change ticket. DBA xác nhận backup restore và rollback, SOC xác nhận monitoring và incident contacts.

Activation request riêng phải nêu exact release SHA, environment/target, time window, named operator/approver, rollback owner và checklist đã được ký. Chỉ sau khi người dùng xác nhận hành động riêng và evidence được kiểm tra mới lập phiên triển khai.

Sau activation được ủy quyền, kiểm tra health, Human/Machine boundary, authentication/authorization, station/object scope, required audit/outbox, unexpected traffic và rollback readiness. Owner ghi acceptance record. Bản runbook này chưa được thực thi.

Không có địa chỉ OT, certificate production hoặc thay đổi firewall trong tài liệu.
