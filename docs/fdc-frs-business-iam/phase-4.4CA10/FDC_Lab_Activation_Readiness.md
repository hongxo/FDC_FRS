# FDC Lab Activation Readiness

BLOCKED_BY_POLICY. D01/D04/D07/D08/D09/D10 trong Decision_Records.json đều PENDING, không có selected option, named approver hoặc signed evidence. Không có release SHA/environment/station scope/validity/revocation proof đã được chủ thể có thẩm quyền phê duyệt.

G1/G2 được đánh giá theo slice kỹ thuật và evidence, còn các giới hạn OCSP, runtime/DPAPI và queue recovery cần disposition. G3/G4 BLOCKED cho tới review và release có chữ ký. Các quyền runtime fixture không có hiệu lực trên shared Lab. Cần một lệnh activation riêng với exact target, release SHA và cửa sổ hợp lệ trước bất kỳ thao tác triển khai nào.
