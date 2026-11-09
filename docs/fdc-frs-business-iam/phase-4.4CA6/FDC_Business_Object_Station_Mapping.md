# FDC Business Object Station Mapping

Station.Id lấy từ canonical catalog; IED, disturbance record, collector và record file dùng StationId trong schema có FK. File lookup ghép RecordId + StationId, không tin file path hoặc station client. Station filter là phép giao với scope. Chưa có Lab permit object mapping đã duyệt. Không suy scope theo tên hoặc legacy ID.
