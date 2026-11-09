# IAM Audit Implementation

Browser login/MFA và logout tạo hai sự kiện fixture cùng outbox trong SQL transaction. Assert linkage và correlation. Không log proof. Archive, tamper, restore và retention chưa được phê duyệt hay nghiệm thu.
