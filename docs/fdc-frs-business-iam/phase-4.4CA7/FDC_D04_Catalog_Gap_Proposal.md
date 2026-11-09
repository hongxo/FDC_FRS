# FDC D04 Catalog Gap Proposal

PROPOSED / AWAITING_APPROVAL. Đề nghị xem xét collector.status.view, lab.test.view, lab.test.prepare, lab.test.review, lab.test.approve và lab.test.execute. Mỗi code bind object+station. View thấp hơn prepare/review; approve/execute critical, independent PersonReference và permit/time/safety gates. OT/Security/Business ký catalog diff và audit actions; DBA chỉ apply additive versioned migration sau phê duyệt. Không seed trong A.7.
